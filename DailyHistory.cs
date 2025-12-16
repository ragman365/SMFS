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
using System.Text;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.Utils.Drawing;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class DailyHistory : DevExpress.XtraEditors.XtraForm
    {
        public static bool reverseSort = true;
        public static bool majorSwitch = true;
        public static bool recalculateHistory = false;
        public static bool showOldDetails = false;
        //        public static DateTime majorDate = new DateTime(2018, 7, 10);
        public static DateTime as400Date = new DateTime(2019, 11, 1);
        public static DateTime majorDate = new DateTime(2019, 11, 1);
        public static DateTime secondDate = new DateTime(2020, 1, 31);
        public static DateTime killSecNatDate = new DateTime(2020, 6, 30);
        public static DateTime kill3rdPartyDate = new DateTime(2020, 12, 31);
        public static DateTime rilesDate = new DateTime(2020, 1, 31);
        public static DateTime interestDate = new DateTime(2021, 7, 1);
        private DataTable policyDt = null;

        private bool doNewFix = false;
        private string workWhat = "";

        private double trust85Max = 0D;
        private double trust85Actual = 0D;

        private double ExpectedPayment = 0D;
        private string workContract = "";
        private string workName = "";
        private double startBalance = 0D;
        private DataRow workdRow = null;
        private DataRow paymentdRow = null;
        private double workPayments = 0D;
        private double dAPR = 0D;
        private int numPayments = 0;
        public static bool calcAmort = false;
        private DateTime lastDate = DateTime.Now;
        private DateTime contractDate = DateTime.Now;
        private bool workPolicy = false;
        private string workPolicyRecord = "";
        private string paymentsFile = "payments";
        private string contractsFile = "contracts";
        private string customersFile = "customers";
        private string workPayer = "";
        private bool foundLocalPreference = false;
        private bool allowDueDateFix = false;
        private bool riles = false;
        private bool loading = true;
        private bool workPDF = false;
        private string workPDFfile = "";
        private bool workJustLoad = false;
        private bool allowCalcRowHeight = false;
        /****************************************************************************************/
        public DailyHistory(string contract )
        {
            InitializeComponent();
            workContract = contract;
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        public DailyHistory(string contract, bool justLoad = false)
        {
            InitializeComponent();
            workContract = contract;
            //SetupTotalsSummary();
            workJustLoad = justLoad;
            workPolicyRecord = "";
            workPolicy = false;
            paymentsFile = "payments";
            contractsFile = "contracts";
            customersFile = "customers";
            DailyHistory_Load(null, null);
        }
        /****************************************************************************************/
        public DailyHistory(string contract, string pdfFileName, bool generatePDF)
        {
            InitializeComponent();
            workContract = contract;
            workPolicyRecord = "";
            workPolicy = false;
            paymentsFile = "payments";
            contractsFile = "contracts";
            customersFile = "customers";
            workPDFfile = pdfFileName;
            workPDF = generatePDF;
            SetupTotalsSummary();
            DailyHistory_Load(null, null);
        }
        /****************************************************************************************/
        public DailyHistory(string contract, string pdfFileName, bool generatePDF, string whatNow = "" )
        {
            InitializeComponent();
            workContract = contract;
            workPolicyRecord = "";
            workPolicy = false;
            paymentsFile = "payments";
            contractsFile = "contracts";
            customersFile = "customers";
            workPDFfile = pdfFileName;
            workPDF = generatePDF;
            workWhat = whatNow;
            SetupTotalsSummary();
            DailyHistory_Load(null, null);
        }
        /****************************************************************************************/
        public DailyHistory( bool isPayer, string payer )
        {
            InitializeComponent();
            loading = true;
            workContract = "";
            workPayer = payer;
            paymentsFile = "ipayments";
            contractsFile = "icontracts";
            customersFile = "icustomers";
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        public DailyHistory(string contract, string policyRecord)
        {
            InitializeComponent();
            workContract = contract;
            workPolicyRecord = policyRecord;
            workPolicy = true;
            paymentsFile = "ipayments";
            contractsFile = "icontracts";
            customersFile = "icustomers";
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        public DailyHistory(string contract, DataRow pDRow = null, DataRow dRow = null, double payments = 0D)
        {
            InitializeComponent();
            workContract = contract;
            workdRow = dRow;
            paymentdRow = pDRow;
            workPayments = payments;
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        public static bool isCemetery(string workContract)
        {
            bool isCemetery = false;
            //if (workContract.ToUpper().IndexOf("NNM") == 0 || workContract.ToUpper().IndexOf ( "HC" ) == 0 )
            //    isCemetery = true;
            return isCemetery;
        }
        /****************************************************************************************/
        public static bool gotCemetery( DataTable dt )
        {
            bool isCemetery = false;
            if ( dt.Rows.Count > 0 )
            {
                if (dt.Rows[0]["edited"].ObjToString().ToUpper() == "CEMETERY")
                    isCemetery = true;
            }
            return isCemetery;
        }
        /****************************************************************************************/
        public static bool gotCemetery(string contractNumber )
        {
            bool isCemetery = false;
            if (contractNumber.Trim().ToUpper().IndexOf("NMM") == 0)
                isCemetery = true;
            else if (contractNumber.Trim().ToUpper().IndexOf("NNM") == 0)
                isCemetery = true;
            else if (contractNumber.Trim().ToUpper().IndexOf("HC") == 0)
                isCemetery = true;
            else if (contractNumber.Trim().ToUpper().IndexOf("NM") == 0)
                isCemetery = true;
            return isCemetery;
        }
        /****************************************************************************************/
        public static bool isRiles(string workContract)
        {
            bool isRiles = false;
            if (workContract.ToUpper().IndexOf("RF") == 0)
                isRiles = true;
            return isRiles;
        }
        /****************************************************************************************/
        public static bool isInsurance(string workContract)
        {
            bool isInsurance = false;
            if (DailyHistory.gotCemetery(workContract))
                isInsurance = false;
            else if (workContract.ToUpper().IndexOf("ZZ") >= 0)
                isInsurance = true;
            else if (workContract.ToUpper().IndexOf("MM") >= 0)
                isInsurance = true;
            else if (workContract.ToUpper().IndexOf("OO") >= 0)
                isInsurance = true;
            return isInsurance;
        }
        /****************************************************************************************/
        private void DailyHistory_Load(object sender, EventArgs e)
        {
            string skinName = "";
            majorSwitch = false;
            recalculateHistory = false;
            if ( !G1.isAdmin() )
                enterTrustAdjustmentToolStripMenuItem.Enabled = false;
            if (paymentdRow == null )
            {
                if ( workWhat.Trim().ToUpper().IndexOf ("PACKET PAYOFF") == 0 )
                    foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, "DailyHistory Primary", ref skinName);
                else
                    foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, "DailyHistory Trust Payment History", ref skinName);
                if (!String.IsNullOrWhiteSpace(skinName))
                {
                    if (skinName != "DevExpress Style")
                        skinForm_SkinSelected("Skin : " + skinName);
                }
            }

            riles = false;
            if (isRiles(workContract))
                riles = true;
            //if ( !LoginForm.RobbyLocal )
            //{
            this.button1.Hide();
            this.button2.Hide();
            this.btnUpdateInt.Hide();

            this.chkDueDate.Hide();
            //}
            if (isInsurance(workContract))
            {
                showOldDetails = true;
                paymentsFile = "ipayments";
                contractsFile = "icontracts";
                customersFile = "icustomers";
                SetupInsuranceColumns();
                txtAsOff.Hide();
                labPayoff.Hide();
                labEqual.Hide();
                txtPayoff.Hide();
                btnDetail.Hide();
                miscToolStripMenuItem.Visible = false;
                btnRecalculate.Hide();
                this.chkDueDate.Show();
                this.chkDueDate.Refresh();
            }
            else
                miscToolStripMenuItem1.Visible = false;

            btnReinstate.Hide();
            btnRequest.Hide();
            btnSummarize.Hide();
            G1.SetupVisibleColumns(gridMain, this.columnsToolStripMenuItem, nmenu_Click);
            chkLoadAll.Hide();
            this.picAgreement.Hide();

            LoadHeader();

            LoadData();

            double trust85 = 0D;
            double balance = ReCalculateBalance(workContract, ref trust85);
            double contractValue = DailyHistory.GetContractValue(workContract);
            double percentage = 0D;
            string str = "";
            //if ( contractValue > 0D)
            //{
            //    percentage = trust85 / contractValue;
            //    str = percentage.ToString("###.00%");
            //}
            //lblTrust85.Text = "Trust85 :$" + G1.ReformatMoney(trust85) + "    " + str;
            if (workPolicy)
                lblTrust85.Hide();
            if (!String.IsNullOrWhiteSpace(workPayer))
            {
                lblTrust85.Hide();
                lblCalcTrust85.Hide();
                lblContractValue.Hide();
            }
            DataTable dt = (DataTable)dgv.DataSource;
            LoadDetailHeader();
            if (workdRow != null)
                LoadNewPayment();
            if (!String.IsNullOrWhiteSpace(workContract))
            {
                if ( !workJustLoad )
                    G1.UpdatePreviousCustomer(workContract, LoginForm.username);
            }
            //if ( !calcAmort)
            //{
            //    gridMain.Columns["newbalance"].Visible = true;
            //    gridMain.Columns["int"].Visible = true;
            //    gridMain.Columns["principal"].Visible = true;
            //}

            dt = (DataTable)dgv.DataSource;

            RecalcTotals();

            //AddSummaryColumn("int");
            //AddSummaryColumn("principal");
            //if (!foundLocalPreference)
                loadGroupCombo(cmbSelectColumns, "DailyHistory", "Primary");

            //G1.loadGroupCombo(cmbSelectColumns, "DailyHistory", "Primary", true, LoginForm.username);
            //cmbSelectColumns.Text = "Original";

            loading = false;

            if (workPDF)
            {
                SetupSelectedColumns("DailyHistory", "Trust Payment History", dgv);
                string name = "DailyHistory " + "Trust Payment History";
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                SetupTotalsSummary();
                gridMain.OptionsView.ShowFooter = true;
            }

            SetupTotalsSummary();
            majorSwitch = true;
            if (!LoginForm.administrator)
                btnRecalculate.Hide();
            if (LoginForm.username.ToUpper() == "ROBBY")
                allowDueDateFix = true;
            if (LoginForm.username.ToUpper() == "CJENKINS")
                allowDueDateFix = true;

            if (allowDueDateFix)
            {
                gridMain.OptionsBehavior.Editable = true;
                gridMain.OptionsBehavior.ReadOnly = false;
                DateTime dueDate8 = DateTime.Now;
                string date = "";
                DataTable ddt = (DataTable)dgv.DataSource;
                if (G1.get_column_number(ddt, "dueDate8Print") < 0)
                    ddt.Columns.Add("dueDate8Print");

                for (int i = 0; i < ddt.Rows.Count; i++)
                {
                    dueDate8 = ddt.Rows[i]["dueDate8"].ObjToDateTime();
                    ddt.Rows[i]["dueDate8Print"] = dueDate8.ToString("MM/dd/yyyy");
                }
                gridMain.Columns["dueDate8"].Visible = false;
                gridMain.Columns["dueDate8Print"].Visible = true;
            }
            gridMain.Columns["dueDate8"].Visible = true;
            gridMain.Columns["dueDate8Print"].Visible = false;
            ScaleCells();

            if (G1.isField() )
                toolStripMenuItem2.Enabled = false;

            if ( workPDF )
            {
                if (String.IsNullOrWhiteSpace(workWhat))
                {
                    printPreviewToolStripMenuItem_Click(null, null);
                    dt = (DataTable)dgv.DataSource;

                    int row = dt.Rows.Count - 1;
                    manualForm = new ManualPayment(workContract, workName, dt, trust85Actual, trust85Max, row, false, true);
                }
                else
                {
                    string waitMessage = "Please Wait!\nGenerating Packet Payoff as of Today!";
                    if ( workWhat.IndexOf ( "-10") > 0 )
                        waitMessage = "Please Wait!\nGenerating Packet Payoff as of 10 Days!";
                    PleaseWait pleaseForm = new PleaseWait( waitMessage );
                    pleaseForm.Show();
                    pleaseForm.Refresh();

                    printPreviewToolStripMenuItem_Click(null, null);

                    doPayoff(workWhat);

                    pleaseForm.FireEvent1();
                    pleaseForm = null;
                }
                this.Close();
            }
        }
        /****************************************************************************************/
        private void LoadData()
        {
            if (doNewFix)
                LoadMainData2();
            else
                LoadMainData();

            string preference = G1.getPreference(LoginForm.username, "DailyHistory", "View Daily History" );
            if (G1.RobbyServer)
                preference = "YES";
            if (preference != "YES")
                dgv.Hide();
            preference = G1.getPreference(LoginForm.username, "DailyHistory", "Allow Screen Configure");
            if (G1.RobbyServer)
                preference = "YES";
            if (preference != "YES")
            {
                btnSelectColumns.Hide();
                lockScreenDetailsToolStripMenuItem.Enabled = false;
                unLockScreenDetailsToolStripMenuItem.Enabled = false;
            }
        }
        /***********************************************************************************************/
        private void loadGroupCombo(System.Windows.Forms.ComboBox cmb, string key, string module )
        {
            string primaryName = "";
            cmb.Items.Clear();
            string cmd = "Select * from procfiles where ProcType = '" + key + "' AND `module` = '" + module + "' group by name;";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Name"].ToString();
                if (name.Trim().ToUpper() == "PRIMARY")
                    primaryName = name;
                cmb.Items.Add(name);
            }
            if (!String.IsNullOrWhiteSpace(primaryName))
                cmb.Text = primaryName;
        }
        /****************************************************************************************/
        private void SetupInsuranceColumns()
        {
            gridMain.Columns["balance"].Visible = false;
            gridMain.Columns["trust100P"].Visible = false;
            gridMain.Columns["trust85P"].Visible = false;
            gridMain.Columns["newbalance"].Visible = false;
            gridMain.Columns["int"].Visible = false;
            gridMain.Columns["principal"].Visible = false;
            gridMain.Columns["days"].Visible = true;
            gridMain.Columns["nextDueDate"].Visible = false;
            gridMain.Columns["creditBalance"].Visible = false;
            gridMain.Columns["calculatedTrust85"].Visible = false;
            gridMain.Columns["calculatedTrust100"].Visible = false;
            gridMain.Columns["method"].Visible = false;
            gridMain.Columns["oldBalance"].Visible = false;
            //            gridMain.Columns[""].Visible = false;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("prince");
            AddSummaryColumn("ovp");
            AddSummaryColumn("trust85P");
            AddSummaryColumn("trust100P");
            AddSummaryColumn("downPayment");
            AddSummaryColumn("ap");
            AddSummaryColumn("dpp");
            AddSummaryColumn("paymentAmount");
            AddSummaryColumn("ccFee");
            AddSummaryColumn("interestPaid");
            AddSummaryColumn("debit");
            AddSummaryColumn("credit");
            AddSummaryColumn("retained");
            AddSummaryColumn("int");
            AddSummaryColumn("principal");
            AddSummaryColumn("calculatedTrust85");
            AddSummaryColumn("calculatedTrust100");
            //AddSummaryColumn("days", gridMain, "{0:0}");
            AddSummaryColumn("NumPayments", gridMain, "{0:0,0.0000}");
            gridMain.Columns["days"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;

        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName)
        {
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            //gridMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "")
        {
            if (gMain == null)
                gMain = gridMain;
            if (String.IsNullOrWhiteSpace(format))
                format = "{0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /***********************************************************************************************/
        void nmenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            string name = menu.Name;
            int index = getGridColumnIndex(name);
            if (index < 0)
                return;
            if (menu.Checked)
            {
                menu.Checked = false;
                gridMain.Columns[index].Visible = false;
            }
            else
            {
                menu.Checked = true;
                gridMain.Columns[index].Visible = true;
            }
            gridMain.RefreshData();
            dgv.Refresh();
            ToolStripMenuItem xmenu = this.columnsToolStripMenuItem;
            xmenu.ShowDropDown();
        }
        /***********************************************************************************************/
        private int getGridColumnIndex(string columnName)
        {
            int index = -1;
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                string name = gridMain.Columns[i].Name;
                if (name == columnName)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
        /****************************************************************************************/
        private void LoadNewPayment()
        {
            if (workdRow == null)
                return;

            double payment = workdRow["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
            ExpectedPayment = payment;
            //            lblPayment.Text = G1.ReformatMoney(payment);
            numPayments = workdRow["numberOfPayments"].ObjToString().ObjToInt32();
            //            lblNumPayments.Text = "# Pmts: " + numPayments.ToString();
            double totalInterest = workdRow["totalInterest"].ObjToString().ObjToDouble();
            //            lblTotalInterest.Text = G1.ReformatMoney(totalInterest);
            string dueDate = workdRow["dueDate8"].ObjToString();
            //            lblDueDate.Text = "Due Date: " + dueDate;
            DateTime iDate = GetIssueDate(workdRow["dueDate8"].ObjToDateTime(), workContract, null);
            string issueDate = iDate.ToString("MM/dd/yyyy");
            lastDate = issueDate.ObjToDateTime();
            //            lblIssueDate.Text = "Issue Date: " + issueDate;
            string apr = workdRow["APR"].ObjToString();
            //            lblAPR.Text = "APR :" + apr + "%";

            dAPR = apr.ObjToDouble() / 100.0D;

            double p2 = CalculateTheMortgage(dAPR, numPayments, startBalance);



            DataTable dt = (DataTable)dgv.DataSource;
            CalculateNewStuff(dt, dAPR, numPayments, startBalance, lastDate);

            if (dt.Rows.Count > 0)
            {
                double creditBalance = dt.Rows[0]["runningCB"].ObjToDouble();
            }

            dt.Columns.Add("sortDate");
            DataRow dRow = dt.NewRow();

            string str = paymentdRow["date"].ObjToString();
            dRow["payDate8"] = G1.DTtoMySQLDT(str);

            double paidAmt = paymentdRow["payment"].ObjToDouble();
            dRow["paymentAmount"] = paidAmt;
            dRow["ccFee"] = paymentdRow["ccFee"].ObjToDouble();

            dRow["dueDate8"] = G1.DTtoMySQLDT(dueDate);
            double balanceDue = workdRow["balanceDue"].ObjToString().ObjToDouble();
            double rate = apr.ObjToDouble() / 100D;
            double interest = G1.RoundValue(balanceDue * rate / 12.0D);
            dRow["interestPaid"] = interest;
            dRow["userId"] = "LKBX";
            dRow["location"] = "LKBX";
            DateTime date = DateTime.Now;
            string depositNumber = "T" + date.Year.ToString("D4") + date.Month.ToString("D2") + date.Day.ToString("D2");
            dRow["depositNumber"] = depositNumber;

            DateTime issue = iDate;
            DateTime testDate = new DateTime(2017, 12, 1);
            if (issue < testDate)
            {
                double pay = payment * 0.85;
                dRow["trust85P"] = pay;
            }
            else
            {
                double pay = paidAmt * 0.85;
                dRow["trust85P"] = pay;
            }

            dt.Rows.Add(dRow);
            DateTime dTime;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dueDate = dt.Rows[i]["payDate8"].ObjToString();
                dTime = dueDate.ObjToDateTime();
                dueDate = dTime.Year.ToString("D4") + dTime.Month.ToString("D2") + dTime.Day.ToString("D2");
                dt.Rows[i]["sortDate"] = dueDate;
            }
            G1.sortTable(dt, "sortDate", "desc");
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /****************************************************************************************/
        private DataTable ReverseOrder(DataTable dt)
        {
            DataView tempview = dt.DefaultView;
            tempview.Sort = "payDate8 asc, record asc";
            dt = tempview.ToTable();
            return dt;
        }
        /****************************************************************************************/
        public static double GetTrust85(DateTime iDate, double payment, double paidAmt)
        {
            double trust85P = 0D;

            DateTime issue = iDate;
            DateTime testDate = new DateTime(2017, 12, 1);
            if (issue < testDate)
            {
                trust85P = payment * 0.85;
            }
            else
            {
                trust85P = paidAmt * 0.85;
            }
            return trust85P;
        }
        /****************************************************************************************/
        public static DateTime GetIssueDate(DateTime oldIssueDate, string contractNumber, DataTable contractDx)
        {
            if (oldIssueDate != null)
            {
                if (oldIssueDate.Year > 1900)
                    return oldIssueDate;
            }
            string cmd = "";
            bool gotit = false;
            DateTime issueDate = DateTime.Now;
            string contractFile = "";
            string customerFile = "";
            Customers.DetermineIfInsurance(contractNumber, ref contractFile, ref customerFile);
            if (contractDx == null)
            {
                cmd = "Select * from `" + contractFile + "` where `contractNumber` = '" + contractNumber + "';";
                contractDx = G1.get_db_data(cmd);
            }
            if (contractDx != null)
            {
                if (contractDx.Rows.Count > 0)
                {
                    issueDate = contractDx.Rows[0]["issueDate8"].ObjToDateTime();
                    if (issueDate.Year > 1900)
                        gotit = true;
                }
            }
            if (!gotit)
            {
                if (contractNumber == "L14081UI")
                {

                }
                cmd = "Select * from `" + customerFile + "` where `contractNumber` = '" + contractNumber + "';";

                contractDx = G1.get_db_data(cmd);
                if (contractDx != null)
                {
                    if (contractDx.Rows.Count > 0)
                    {
                        issueDate = contractDx.Rows[0]["firstPayDate"].ObjToDateTime();
                        if (issueDate.Year > 1900)
                            gotit = true;
                    }
                }
            }
            if (issueDate.Year < 1900)
            {
                string paymentsFile = "payments";
                if (contractFile.ToUpper() == "ICONTRACTS")
                    paymentsFile = "ipayments";
                cmd = "Select * from `" + paymentsFile + "` where `contractNumber` = '" + contractNumber + "' order by `payDate8` DESC, `tmstamp` DESC;";
                contractDx = G1.get_db_data(cmd);
                if (contractDx.Rows.Count > 0)
                    issueDate = contractDx.Rows[0]["payDate8"].ObjToDateTime();
            }
            return issueDate;
        }
        /****************************************************************************************/
        public static void GetIssueDate( string contractNumber, ref DateTime issueDate, ref DateTime dateDpPaid )
        {
            bool gotit = false;
            issueDate = DateTime.Now;
            dateDpPaid = DateTime.Now;
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                issueDate = dx.Rows[0]["issueDate8"].ObjToDateTime();
                dateDpPaid = dx.Rows[0]["dateDpPaid"].ObjToDateTime();
            }
        }
        /****************************************************************************************/
        public static DateTime CheckFirstPayDate(DateTime firstPayDate, DateTime issueDate)
        {
            if (firstPayDate.Year < 1900)
            {
                int increment = 2;
                if (issueDate.Day < 16)
                    increment = 1;
                firstPayDate = issueDate.AddMonths(increment);
                int year = firstPayDate.Year;
                int month = firstPayDate.Month;
                firstPayDate = new DateTime(year, month, 1);
            }
            return firstPayDate;
        }
        /****************************************************************************************/
        public static void CleanupDownPayment ( string contractNumber, DataTable dt )
        {
            if (DailyHistory.isInsurance(contractNumber))
                return;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            DataRow[] dRows = dt.Select("downpayment<>'0.00'");
            if ( dRows.Length > 0 )
            {
                DateTime date = dRows[0]["payDate8"].ObjToDateTime();
                string date1 = G1.DateTimeToSQLDateTime(date);
                string depositNumber = dRows[0]["depositNumber"].ObjToString();
                double downPayment = dRows[0]["downPayment"].ObjToDouble();
                string str = downPayment.ToString("N2");
                string cmd = "Select * from `downpayments`  WHERE `date` >= '" + date1 + "' AND `paymentType` = 'Credit Card' AND `depositNumber` = '" + depositNumber + "' AND `downPayment` = '" + str + "';";
                DataTable dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count > 0 )
                {
                    double ccFee = dx.Rows[0]["ccFee"].ObjToDouble();
                    dRows[0]["ccFee"] = ccFee;
                }
            }
        }
        /****************************************************************************************/
        public static void CalculateNewStuff(DataTable dt, double rate, int numPayments, double startBalance, DateTime lastDate)
        {
            if (dt.Rows.Count <= 0)
                return;

            //dt = FilterDeleted(dt);

            string contract = dt.Rows[0]["contractNumber"].ObjToString();

            //CleanupDownPayment(contract, dt);

            if (G1.get_column_number(dt, "balance") < 0)
                dt.Columns.Add("balance", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "prince") < 0)
                dt.Columns.Add("prince", Type.GetType("System.Double"));

            if (G1.get_column_number(dt, "newbalance") < 0)
            {
                dt.Columns.Add("newbalance", Type.GetType("System.Double"));
                dt.Columns.Add("int", Type.GetType("System.Double"));
                dt.Columns.Add("principal", Type.GetType("System.Double"));
                dt.Columns.Add("days");
            }
            if (G1.get_column_number(dt, "nextDueDate") < 0)
                dt.Columns.Add("nextDueDate");
            if (G1.get_column_number(dt, "creditBalance") < 0)
                dt.Columns.Add("creditBalance", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "calculatedTrust85") < 0)
                dt.Columns.Add("calculatedTrust85", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "calculatedTrust100") < 0)
                dt.Columns.Add("calculatedTrust100", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "method") < 0)
                dt.Columns.Add("method", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "runningCB") < 0)
                dt.Columns.Add("runningCB", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "majorCB") < 0)
                dt.Columns.Add("majorCB", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "ovp") < 0)
                dt.Columns.Add("ovp", Type.GetType("System.Double"));
            if (dt.Rows.Count <= 0)
                return;
            if (isInsurance(contract))
                return;

            DataTable ddt = dt.Copy();
            DataView tempview = ddt.DefaultView;
            tempview.Sort = "payDate8 desc, tmstamp desc, record desc";
            ddt = tempview.ToTable();
            dt.Rows.Clear();
            for (int i = 0; i < ddt.Rows.Count; i++)
                dt.ImportRow(ddt.Rows[i]);

            string cnum = dt.Rows[0]["contractNumber"].ObjToString();
            //if ( isInsurance ( cnum))
            //    return;

            dt = DailyHistory.GetContractPayments(cnum, dt);

            if ( startBalance == 0D)
                startBalance = DailyHistory.GetFinanceValue(cnum);

            double payment = CalculateTheMortgage(rate, numPayments, startBalance);
            payment = G1.RoundValue(payment);
            double expected = payment;

            string cmd = "Select * from `contracts` where `contractNumber` = '" + cnum + "';";
            DataTable dx = G1.get_db_data(cmd);
            expected = 0D;
            double amtOfMonthlyPayt = 0D;
            DateTime contractDueDate8 = DateTime.Now;
            double contractValue = 0D;
            double originalDownPayment = 0D;
            DateTime mainIssueDate = DateTime.Now;
            double trustPercent = 0D;
            if ( dx.Rows.Count > 0 )
                trustPercent = dx.Rows[0]["trustPercent"].ObjToDouble();
            bool riles = DailyHistory.isRiles(cnum);
            bool cemetery = DailyHistory.isCemetery(cnum);
            string lockTrust85 = "";
            if (dx.Rows.Count > 0)
                lockTrust85 = dx.Rows[0]["lockTrust85"].ObjToString();

            if (DailyHistory.isRiles(cnum))
                lockTrust85 = "Y";

            if (dt.Rows.Count > 0)
            {
                originalDownPayment = dt.Rows[0]["downPayment"].ObjToDouble();
                amtOfMonthlyPayt = originalDownPayment;
                contractDueDate8 = dt.Rows[0]["dueDate8"].ObjToDateTime();
                mainIssueDate = dt.Rows[0]["payDate8"].ObjToDateTime();
                contractValue = originalDownPayment;
                expected = originalDownPayment;
            }
            if (dx.Rows.Count > 0)
            {
                expected = dx.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
                amtOfMonthlyPayt = expected;
                contractDueDate8 = dx.Rows[0]["dueDate8"].ObjToDateTime();
                contractValue = GetContractValuePlus(dx.Rows[0]);
                originalDownPayment = dx.Rows[0]["downPayment"].ObjToDouble();

                mainIssueDate = dx.Rows[0]["issueDate8"].ObjToDateTime();
                if (mainIssueDate.Year < 1800)
                    mainIssueDate = GetIssueDate(mainIssueDate, cnum, null);
            }
            if ( trustPercent <= 0D )
            {
                if (mainIssueDate <= new DateTime(2006, 6, 30))
                    trustPercent = 50D;
                else
                    trustPercent = 85D;
            }
            else if ( trustPercent <= 1.0 )
                trustPercent = trustPercent * 100D;

            if ( originalDownPayment <= 0D)
                originalDownPayment = GetDownPaymentFromPayments(cnum);
            if (dt.Rows.Count > 0)
            {
                int lastRow = dt.Rows.Count - 1;
                if (contractDueDate8.Year < 100)
                    contractDueDate8 = dt.Rows[0]["dueDate8"].ObjToDateTime();
            }

            if (isInsurance(cnum))
                return;

            cmd = "Select * from `customers` where `contractNumber` = '" + cnum + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            DateTime dueDate = new DateTime(1850, 1, 1);
            DateTime firstPayDate = new DateTime(1850, 1, 1);
            DateTime saveLastDate = lastDate;

            if (dx.Rows.Count > 0)
            {
                dueDate = dx.Rows[0]["firstPayDate"].ObjToDateTime();
                firstPayDate = dueDate;
            }
            if (dueDate.Year < 1900)
            {
                dueDate = GetIssueDate(dueDate, cnum, null);
                if (dueDate.Year < 1900)
                    return;
            }
            if (firstPayDate.Year < 1900)
            {
                int increment = 2;
                if (dueDate.Day < 18)
                    increment = 1;
                firstPayDate = dueDate.AddMonths(increment);
                int year = firstPayDate.Year;
                int month = firstPayDate.Month;
                firstPayDate = new DateTime(year, month, 1);
                //                dueDate = dueDate.AddMonths(1);
            }

            DateTime issueDate = dueDate;

            string issueDateStr = issueDate.ToString("MM/dd/yyyy");

            double newRate = rate / 12D;
            newRate = rate;

            double principal = 0D;
            double interest = 0D;
            double pastInterest = 0D;
            double balance = G1.RoundValue(startBalance);
            double oldBalance = balance;
            double oldoldBalance = balance;
            double oldDownPayment = 0D;
            double paymentAmount = 0D;
            double downPayment = 0D;
            double ccFee = 0D;
            double debit = 0D;
            double credit = 0D;
            double trust85P = 0D;
            double trust100P = 0D;
            int method = 0;
            DateTime dolp = DateTime.Now;
            if (lastDate < dolp)
                dolp = lastDate;
            DateTime docp = DateTime.Now;
            DateTime nextDueDate = dueDate;
            double creditBalance = 0D;
            double runningCB = 0D;
            double retained = 0D;
            bool first = true;
            bool firstPayment = true;
            DateTime testDate = new DateTime(2018, 5, 7);
            DateTime dueDate8 = DateTime.Now;
            DateTime lastDueDate8 = DateTime.Now;
            string finale = "";
            int finaleCount = 0;
            bool honorFinale = false;
            int months = 0;

            string force = "";

            int days = 0;
            int yearlyDays = 365;
            string location = "";
            string depositNumber = "";
            string pastRecord = "";
            string myFields = "";
            DateTime payDate8 = DateTime.Now;
            double unpaid_interest = 0D;

            double actualBalance = 0D;

            string status = "";

            dueDate8 = dueDate;
            lastDueDate8 = dueDate;
            double testBalance = 0D;
            double lastUnpaidInterest = 0D;
            bool gotDebit = false;
            bool gotCredit = false;
            double adjustedAPR = 0D;
            string lockInterest = "";
            TimeSpan ts = dueDate8 - lastDate;

            bool tryNewMethod = false;
            tryNewMethod = true;

            try
            {
                gotDebit = false;
                gotCredit = false;
                string firstChar = "";
                if (numPayments == 0 && dt.Rows.Count > 0)
                    numPayments = dt.Rows.Count;
                string edited = "";
                bool calcCredit = true;
                runningCB = 0D;
                double saveRetained = 0D;
                double saveTrust85P = 0D;
                double saveTrust100P = 0D;
                DateTime datePaid = DateTime.Now;
                balance = G1.RoundValue(startBalance);
                bool byPassDOLP = false;
                if (tryNewMethod)
                {
                    for (int i = dt.Rows.Count - 1; i >= 0; i--) // Take Care of AS400 data
                    {
                        byPassDOLP = false;
                        datePaid = dt.Rows[i]["payDate8"].ObjToString().ObjToDateTime();
                        docp = datePaid;
                        if ( riles )
                        {
                            if (docp >= rilesDate)
                                break;
                        }
                        else if (docp >= DailyHistory.majorDate)
                            break;

                        ts = datePaid - lastDate;
                        days = ts.Days;

                        credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                        interest = dt.Rows[i]["interestPaid"].ObjToDouble();

                        if (days <= 0)
                        {
                            dolp = docp;
                            days = 0;
                        }

                        if (credit > 0D && interest == 0D)
                            byPassDOLP = true;

                        if (!byPassDOLP)
                        {
                            dt.Rows[i]["days"] = days.ObjToString();
                            lastDate = datePaid;
                        }
                        else
                            dt.Rows[i]["days"] = "0";

                        status = dt.Rows[i]["fill"].ObjToString();
                        if (status == "D")
                            continue;
                        if ( !byPassDOLP )
                            dolp = docp;

                        //payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                        payment = getPayment(dt, i);

                        debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                        credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                        interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                        ccFee = dt.Rows[i]["ccFee"].ObjToDouble();
                        principal = payment - interest + credit - debit;
                        if (debit != 0D)
                            principal = principal - ccFee;
                        principal = G1.RoundDown(principal);
                        dt.Rows[i]["prince"] = principal;
                        balance -= principal;

                        if (balance < 0D)
                            balance = G1.RoundValue(balance);
                        else
                            balance = G1.RoundDown(balance);

                        dt.Rows[i]["balance"] = balance;
                    }
                }
                for (int i = dt.Rows.Count - 1; i >= 0; i--)
                {
                    edited = dt.Rows[i]["edited"].ObjToString().Trim().ToUpper();
                    if (edited == "TRUSTADJ" || edited == "CEMETERY")
                    {
                        trust85P = dt.Rows[i]["trust85P"].ObjToDouble();
                        trust100P = dt.Rows[i]["trust100P"].ObjToDouble();
                        principal = trust100P;
                        if (edited == "TRUSTADJ")
                            principal = 0D;
                        dt.Rows[i]["prince"] = principal;
                        oldoldBalance -= principal;
                        dt.Rows[i]["balance"] = oldoldBalance;
                        continue;
                    }
                    status = dt.Rows[i]["fill"].ObjToString();

                    //payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    payment = getPayment(dt, i);

                    debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                    ccFee = dt.Rows[i]["ccFee"].ObjToDouble();
                    oldDownPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                    principal = 0D;

                    if (oldDownPayment == 0D && payment > 0D)
                        principal = payment - interest + credit - debit;
                    else if (oldDownPayment == 0D && debit != 0D)
                    {
                        principal = ((debit+ccFee) + interest) * -1D;
                    }
                    else if (oldDownPayment == 0D && credit != 0D)
                        principal = (credit - interest);

                    //principal = 0D;
                    if (credit > 0D && payment == 0D && ccFee != 0D )
                        principal = credit - ccFee;
                    else if (debit > 0D && payment == 0D)
                    {
                        if (ccFee != 0D)
                        {
                            principal = debit + ccFee;
                            principal = principal * -1D;
                        }
                    }
                    else
                    {
                        if (oldDownPayment == 0D && payment == 0D && ccFee != 0D)
                            principal = ccFee;
                        if (ccFee < 0D)
                        {
                            dt.Rows[i]["prince"] = 0D;
                            dt.Rows[i]["balance"] = oldoldBalance;
                            continue;
                        }
                    }

                    dt.Rows[i]["prince"] = principal;
                    if ( status != "D" )
                        oldoldBalance -= principal;
                    dt.Rows[i]["balance"] = oldoldBalance;
                }

                oldBalance = balance;
                oldoldBalance = oldBalance;

                DateTime  Date = DateTime.Now;
                DateTime oldLastDate = DateTime.Now;
                string record = "";

                for (int i = dt.Rows.Count - 1; i >= 0; i--)
                {
                    if ( i == 0 )
                    {
                    }
                    //record = dt.Rows[i]["record"].ObjToString();
                    //if ( record == "343629")
                    //{
                    //}
                    status = dt.Rows[i]["fill"].ObjToString();
                    if (status == "D")
                    {
                        dt.Rows[i]["balance"] = oldBalance;
                        continue;
                    }
                    datePaid = dt.Rows[i]["payDate8"].ObjToString().ObjToDateTime();
                    docp = datePaid;
                    if (tryNewMethod)
                    {
                        if (riles)
                        {
                            if (docp < rilesDate)
                                continue;
                        }
                        else
                        {
                            if (docp < DailyHistory.majorDate)
                                continue;
                        }

                        //if (docp < DailyHistory.majorDate)
                        //    continue;
                        //if (docp < DailyHistory.rilesDate)
                        //    continue;
                    }

                    gotDebit = false;
                    gotCredit = false;
                    try
                    {
                        calcCredit = true;
                        honorFinale = false;
                        finale = dt.Rows[i]["new"].ObjToString();
                        if (finale.ToUpper() == "FINALE")
                        {
                            finaleCount++;
                            if ( finaleCount == 1 )
                                honorFinale = true;
                        }
                        edited = dt.Rows[i]["edited"].ObjToString().Trim().ToUpper();
                        if (edited == "TRUSTADJ" || edited == "CEMETERY")
                        {
                            payment = getPayment(dt, i);

                            debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                            credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                            if (debit != 0D || credit != 0D)
                            {
                                interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                                ccFee = dt.Rows[i]["ccFee"].ObjToDouble();
                                oldDownPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                                principal = 0D;
                                if (oldDownPayment == 0D && payment > 0D)
                                    principal = payment - interest + credit - debit;
                                else if (oldDownPayment == 0D && debit != 0D)
                                {
                                    principal = ((debit + ccFee) + interest) * -1D;
                                }
                                else if (oldDownPayment == 0D && credit != 0D)
                                    principal = (credit - interest);
                                trust85P = dt.Rows[i]["trust85P"].ObjToDouble();
                                trust100P = dt.Rows[i]["trust100P"].ObjToDouble();
                                dt.Rows[i]["runningCB"] = runningCB;
                                dt.Rows[i]["calculatedTrust85"] = trust85P;
                                dt.Rows[i]["calculatedTrust100"] = trust100P;
                                dt.Rows[i]["principal"] = principal;
                                dt.Rows[i]["prince"] = principal;
                                dt.Rows[i]["creditBalance"] = 0D;
                            }
                            else
                            {
                                trust85P = dt.Rows[i]["trust85P"].ObjToDouble();
                                trust100P = dt.Rows[i]["trust100P"].ObjToDouble();
                                dt.Rows[i]["runningCB"] = runningCB;
                                dt.Rows[i]["calculatedTrust85"] = trust85P;
                                dt.Rows[i]["calculatedTrust100"] = trust100P;
                                dt.Rows[i]["principal"] = trust85P;
                                dt.Rows[i]["creditBalance"] = 0D;
                            }
                            if (edited == "TRUSTADJ")
                            {
                                interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                                if (interest != 0D)
                                {
                                    dt.Rows[i]["principal"] = -1D * interest;
                                    dt.Rows[i]["prince"] = -1D * interest;
                                }
                                else
                                    dt.Rows[i]["principal"] = 0D;
                                location = DailyHistory.DetermineBox(dt, i, 4);
                                if ( i < (dt.Rows.Count - 1))
                                    dt.Rows[i]["balance"] = dt.Rows[i + 1]["balance"].ObjToDouble();
                            }
                            continue;
                        }
                        location = dt.Rows[i]["location"].ObjToString();
                        depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                        location = DailyHistory.DetermineBox(dt, i, 4);
                        //if ( !String.IsNullOrWhiteSpace(depositNumber) && edited != "MANUAL" )
                        //{
                        //    firstChar = depositNumber.ToUpper().Substring(0, 1);
                        //    if (firstChar == "T" )
                        //        location = "LKBX";
                        //    else if ( firstChar == "A")
                        //        location = "ACH";
                        //    dt.Rows[i]["location"] = location;
                        //}
                        status = dt.Rows[i]["fill"].ObjToString();
                        if (status.ToUpper() == "D")
                            continue;
                        //yearlyDays = 365;
                        //DateTime dueDate = dt.Rows[i]["dueDate8"].ObjToString().ObjToDateTime();
                        //if (DateTime.IsLeapYear(dueDate.Year))
                        //    yearlyDays = 366;

                        datePaid = dt.Rows[i]["payDate8"].ObjToString().ObjToDateTime();
                        docp = datePaid;
                        //if ( docp == new DateTime ( 2019, 12, 10))
                        //{
                        //    dt.Rows[i + 1]["balance"] = 4490.30D;
                        //}
                        ts = dueDate - datePaid; // Doesn't matter
                        ts = datePaid - lastDate;
                        ts = datePaid - dolp; // Try this 2/13/2024
                        days = ts.Days;

                        credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                        if ( credit > 0D)
                        {
                            days = 0;
                        }
                        debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                        if (debit > 0D)
                        {
                            days = 0;
                        }


                        if (days <= 0)
                        {
                            if ( credit == 0D && debit == 0D )
                                dolp = docp;
                            days = 0;
                        }

                        dt.Rows[i]["days"] = days.ObjToString();
                        lastDate = datePaid;

                        if (DateTime.IsLeapYear(datePaid.Year))
                            yearlyDays = 366;

                        double dailyInterest = newRate / (double)(yearlyDays) * (double)(days);
                        adjustedAPR = newRate;
                        lockInterest = dt.Rows[i]["lockInterest"].ObjToString().ToUpper();
                        if ( lockInterest == "Y")
                        {
                            adjustedAPR = dt.Rows[i]["apr"].ObjToDouble();
                            dailyInterest = adjustedAPR / (double)(yearlyDays) * (double)(days);
                        }
                        interest = dailyInterest * balance;
                        interest = G1.RoundDown(interest);
                        
                        if (interest < 0D && docp >= interestDate )
                            interest = 0D;
                        if (credit > 0D)
                            interest = dt.Rows[i]["interestPaid"].ObjToDouble();

                        if ( credit > 0D && interest == 0D )
                        {
                            lastDate = oldLastDate;
                            dt.Rows[i]["days"] = "0";

                        }
                        if (i < (dt.Rows.Count - 1))
                            oldBalance = dt.Rows[i+1]["balance"].ObjToDouble();

                        oldLastDate = lastDate;

                        downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                        //downPayment = DailyHistory.getDownPayment(dt, i);
                        if (downPayment != 0D)
                        {
                            calcCredit = false;
                            if (dt.Rows.Count == 1)
                            {
                                if (originalDownPayment == 0D)
                                {
                                    originalDownPayment = downPayment;
                                    calcCredit = false;
                                }
                                dolp = docp;
                                dt.Rows[i]["newbalance"] = oldBalance;
                                //dt.Rows[i]["newbalance"] = oldoldBalance;
                                //                                dueDate = contractDueDate8.AddMonths(1);
                                dueDate = contractDueDate8;
                                dt.Rows[i]["nextDueDate"] = dueDate.ToString("MM/dd/yyyy");
                                dt.Rows[i]["creditBalance"] = 0D;
                            }
                            else
                            {
                                if (originalDownPayment == 0D)
                                {
                                    originalDownPayment = downPayment;
                                    calcCredit = false;
                                }
                                dolp = docp;
                                dt.Rows[i]["newbalance"] = oldBalance;
                                //dt.Rows[i]["newbalance"] = oldoldBalance;
                                if (first)
                                {
                                    if (nextDueDate != firstPayDate)
                                        nextDueDate = nextDueDate.AddMonths(1);
                                }
                                dueDate = nextDueDate;
                                dt.Rows[i]["nextDueDate"] = dueDate.ToString("MM/dd/yyyy");
                                dt.Rows[i]["creditBalance"] = 0D;
                                if (!first)
                                    dueDate = dueDate.AddMonths(1);
                                //else
                                //    dueDate = firstPayDate.AddMonths(-1);
                            }
                            debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                            credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                            ccFee = dt.Rows[i]["ccFee"].ObjToDouble();

                            first = false;
                            payment = downPayment;
                            principal = downPayment;
                            force = "";
                            //force = dt.Rows[i]["force"].ObjToString();
                            if (edited.ToUpper() == "MANUAL" && trust85P < 0D)
                                force = "Y";
                            else if (debit != 0D || credit != 0D)
                                force = "Y";
                            //                            if (docp > DailyHistory.secondDate || recalculateHistory)
                            if (riles)
                            {
                                if (credit == 0D && debit == 0D)
                                {
                                    if (docp > rilesDate)
                                    {
                                        trust100P = payment;
                                        trust85P = payment * trustPercent / 100D;
                                        dt.Rows[i]["calculatedTrust85"] = trust100P;
                                        dt.Rows[i]["calculatedTrust100"] = trust85P;
                                    }
                                    else
                                    {
                                        dt.Rows[i]["calculatedTrust85"] = payment;
                                        dt.Rows[i]["calculatedTrust100"] = payment;
                                    }
                                }
                            }
                            else if (lockTrust85 != "Y")
                            {
                                //if ((docp >= DailyHistory.majorDate || recalculateHistory) && !honorFinale)
                                if ((docp >= DailyHistory.majorDate || recalculateHistory) && finale.ToUpper() != "FINALE" )
                                    {
                                        method = ImportDailyDeposits.CalcTrust85P(docp, amtOfMonthlyPayt, mainIssueDate.ToString("MM/dd/yyyy"), contractValue, originalDownPayment, numPayments, payment, principal, debit, credit, rate, ref trust85P, ref trust100P, ref retained);
                                    if (force.ToUpper() != "Y")
                                    {
                                        dt.Rows[i]["calculatedTrust85"] = trust85P;
                                        dt.Rows[i]["calculatedTrust100"] = trust100P;
                                    }
                                    dt.Rows[i]["method"] = method.ObjToDouble();
                                }
                                else
                                {
                                    dt.Rows[i]["calculatedTrust85"] = dt.Rows[i]["trust85P"].ObjToDouble();
                                    dt.Rows[i]["calculatedTrust100"] = dt.Rows[i]["trust100P"].ObjToDouble();
                                }
                            }
                            else
                            {
                                dt.Rows[i]["calculatedTrust85"] = dt.Rows[i]["trust85P"].ObjToDouble();
                                dt.Rows[i]["calculatedTrust100"] = dt.Rows[i]["trust100P"].ObjToDouble();
                            }
                            continue;
                        }
                        first = false;

                        //paymentAmount = dt.Rows[i]["paymentAmount"].ObjToDouble();
                        paymentAmount = getPayment(dt, i);

                        payment = paymentAmount;
                        //paymentAmount += creditBalance;
                        debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                        if (debit != 0D)
                        {
                            gotDebit = true;
                        }
                        credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                        if (credit != 0D)
                        {
                            gotCredit = true;
                        }
                        ccFee = dt.Rows[i]["ccFee"].ObjToDouble();

                        interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                        pastInterest = interest;
                        paymentAmount += credit - debit + downPayment;
                        if (payment == 0D && downPayment > 0D)
                            payment = downPayment;

                        if (payment == 0D && debit == 0D && credit == 0D && interest == 0D)
                            continue;

                        //                        trust85P = GetTrust85(issueDate, expected, paymentAmount);
                        if (debit == 0D && credit == 0D)
                        {
                            if (docp >= DailyHistory.majorDate)
                            {
                            }
                            if ( docp.Month == 7 && docp.Year == 2021 )
                            {
                            }
                            testBalance = oldBalance;
                            //testBalance = oldoldBalance;
                            //if (i < (dt.Rows.Count - 1))
                            //    testBalance = dt.Rows[i + 1]["balance"].ObjToDouble();

                            ImportDailyDeposits.CalcPrincipalInterest(testBalance, dolp, docp, paymentAmount, adjustedAPR, ref principal, ref interest, ref days, ref unpaid_interest);

                            if (docp > DailyHistory.majorDate)
                            {
                                if (interest != pastInterest)
                                    dt.Rows[i]["interestPaid"] = interest;
                            }
                        }
                        else
                        {
                            principal = debit + interest;
                            principal = principal * -1D;
                            if (credit > 0D)
                                principal = credit - interest;
                            else if (debit != 0D)
                            {
                                principal = principal - ccFee;
                                //if (ccFee < 0D)
                                //    principal = 0D; // Commented out because of contract M20050LI on 12/3/2024
                            }
                        }
                        saveRetained = dt.Rows[i]["retained"].ObjToDouble();
                        saveTrust85P = dt.Rows[i]["Trust85P"].ObjToDouble();
                        saveTrust100P = dt.Rows[i]["Trust100P"].ObjToDouble();
                        if (saveRetained < 0)
                        {
                            if (Math.Abs(saveRetained) == credit)
                                saveRetained = 0D;
                        }

                        //if ((docp >= DailyHistory.majorDate && edited.ToUpper() != "MANUAL") || recalculateHistory)
                        //if ((docp >= DailyHistory.majorDate || recalculateHistory) && lockTrust85 != "Y" && !honorFinale)
                        //if ((docp >= DailyHistory.majorDate || recalculateHistory) && lockTrust85 != "Y" && finale.ToUpper() != "FINALE")
                         if ((docp >= DailyHistory.majorDate || recalculateHistory) && lockTrust85 != "Y"  )
                            {
                                if (debit == 0D && credit == 0D)
                            {
                                //if ( docp > DailyHistory.secondDate )
                                method = ImportDailyDeposits.CalcTrust85P(docp, amtOfMonthlyPayt, mainIssueDate.ToString("MM/dd/yyyy"), contractValue, originalDownPayment, numPayments, payment, principal, debit, credit, rate, ref trust85P, ref trust100P, ref retained);
                                if (SMFS.activeSystem.ToUpper() == "RILES")
                                    trust85P = trust100P;
                                dt.Rows[i]["trust85P"] = trust85P;
                                dt.Rows[i]["trust100P"] = trust100P;
                                //else
                                //{
                                //    trust85P = dt.Rows[i]["trust85P"].ObjToDouble();
                                //    trust100P = dt.Rows[i]["trust100P"].ObjToDouble();
                                //    retained = payment - trust100P;
                                //    if (debit > 0D)
                                //    {
                                //        retained = debit - Math.Abs(trust100P);
                                //        retained = retained * -1D;
                                //    }
                                if (saveRetained != 0D && contractDueDate8.Year >= 2039)
                                {
                                    retained = saveRetained;
                                    trust85P = saveTrust85P;
                                    trust100P = saveTrust100P;
                                    dt.Rows[i]["trust85P"] = trust85P;
                                    dt.Rows[i]["trust100P"] = trust100P;
                                }
                                //}
                            }
                            else
                            {
                                trust85P = dt.Rows[i]["trust85P"].ObjToDouble();
                                trust100P = dt.Rows[i]["trust100P"].ObjToDouble();
                                retained = payment - trust100P;
                                if (debit > 0D)
                                {
                                    retained = debit - Math.Abs(trust100P);
                                    retained = retained * -1D;
                                }
                                if (saveRetained != 0D)
                                {
                                    retained = saveRetained;
                                    trust85P = saveTrust85P;
                                    trust100P = saveTrust100P;
                                }
                            }
                        }
                        else
                        {
                            trust85P = dt.Rows[i]["trust85P"].ObjToDouble();
                            trust100P = dt.Rows[i]["trust100P"].ObjToDouble();
                            retained = payment - trust100P;
                            if (saveRetained != 0D)
                            {
                                retained = saveRetained;
                                trust85P = saveTrust85P;
                                trust100P = saveTrust100P;
                            }
                            dt.Rows[i]["creditBalance"] = 0D;
                        }


                        //                        method = ImportDailyDeposits.CalcTrust85(mainIssueDate.ToString("MM/dd/yyyy"), contractValue, originalDownPayment, numPayments, paymentAmount, principal, rate, ref trust85P, ref trust100P);


                        if (paymentAmount == downPayment && downPayment == originalDownPayment)
                            principal = 0D;

                        balance = oldBalance - principal;
                        //balance = oldoldBalance - principal;
                        if (balance < 0D)
                            balance = G1.RoundValue(balance);
                        else
                            balance = G1.RoundDown(balance);

                        //ImportDailyDeposits.HandleUnpaidInterest(cnum, payment, ref interest, ref unpaid_interest, ref principal, ref balance);
                        ImportDailyDeposits.HandleUnpaidInterest(payment, lastUnpaidInterest, ref interest, ref unpaid_interest, ref principal, ref balance);

                        lastUnpaidInterest = unpaid_interest;

                        creditBalance = 0D;
                        credit = 0D;
                        debit = 0D;
                        //                        paymentAmount += creditBalance;
                        //paymentAmount += runningCB;
                        if (payment > 0D && firstPayment)
                        {
                            if (calcCredit)
                            {
                                firstPayment = false;
                                dueDate = firstPayDate;
                            }
                        }

                        //                DailyHistory.ReCalculateDueDate(cnum, expected, payment, debit, credit, ref nextDueDate, ref creditBalance);
                        if (!calcCredit && downPayment > 0D)
                            paymentAmount = 0D;
                        //                        creditPayment = paymentAmount + creditBalance;
                        creditBalance = runningCB;
                        months = CalcDueDate(dolp, docp, dueDate, expected, paymentAmount, credit, debit, newRate, balance, ref nextDueDate, ref creditBalance); // RAMMA
                        if (payment == 0D && firstPayment && downPayment > 0D)
                            nextDueDate = firstPayDate;
                        dt.Rows[i]["newbalance"] = balance;
                        dt.Rows[i]["balance"] = balance; // Fixed on 8/1/2021 Not sure why this wasn't there

                        //dt.Rows[i]["newbalance"] = oldoldBalance;
                        dt.Rows[i]["principal"] = principal;
                        dt.Rows[i]["int"] = interest;
                        dt.Rows[i]["nextDueDate"] = nextDueDate.ToString("MM/dd/yyyy");
                        if (balance < 0D)
                            dt.Rows[i]["nextDueDate"] = "12/31/2039";
                        if (calcCredit)
                        {
                            if (creditBalance > 0D)
                            {

                            }
                            dt.Rows[i]["creditBalance"] = creditBalance;
                            //                            runningCB += G1.RoundValue ( creditBalance );
                            runningCB = creditBalance;
                        }
                        else
                            dt.Rows[i]["creditBalance"] = 0D;
                        if (expected > 0D)
                        {
                            runningCB = runningCB % expected;
                            runningCB = G1.RoundValue(runningCB);
                        }
                        else
                            runningCB = 0D;
                        dt.Rows[i]["runningCB"] = runningCB;
                        dt.Rows[i]["calculatedTrust85"] = trust85P;
                        dt.Rows[i]["calculatedTrust100"] = trust100P;
                        dt.Rows[i]["method"] = method.ObjToDouble();
                        oldBalance = oldBalance - principal;
                        //oldoldBalance = oldoldBalance = principal;
                        payDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();
                        if (payDate8 >= majorDate)
                        {
                            //dt.Rows[i]["prince"] = principal;
                        }
                        //dueDate8 = dt.Rows[i]["dueDate8"].ObjToDateTime();
                        //if (payDate8 > majorDate)
                        //{
                        //    CalcDueDate(dolp, docp, lastDueDate8, expected, paymentAmount, credit, debit, newRate, oldoldBalance, ref nextDueDate, ref creditBalance);
                        //}
                        //lastDueDate8 = nextDueDate;
                        if ( !gotDebit && !gotCredit )
                            dolp = docp;
                        dueDate = nextDueDate;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR1*** i=" + i.ToString() + " Contract=" + cnum + " " + ex.Message.ToString());
                    }
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    docp = dt.Rows[i]["payDate8"].ObjToDateTime();
                    if (docp > testDate)
                    {
                        pastInterest = dt.Rows[i]["interestPaid"].ObjToDouble();
                        interest = dt.Rows[i]["int"].ObjToDouble();
                        if (interest != pastInterest)
                        {
                            pastRecord = dt.Rows[i]["record"].ObjToString();
                            myFields = "interestPaid," + interest.ToString();
                            //                            G1.update_db_table("payments", "record", pastRecord, myFields);
                        }
                    }
                }
                finaleCount = 0;
                for (int i = dt.Rows.Count - 1; i >= 0; i--)
                {
                    if ( i == 0 )
                    {

                    }
                    honorFinale = false;
                    finale = dt.Rows[i]["new"].ObjToString().ToUpper();
                    if ( finale == "FINALE")
                    {
                        finaleCount++;
                        if (finaleCount == 1)
                            honorFinale = true;
                    }
                    edited = dt.Rows[i]["edited"].ObjToString().Trim().ToUpper();
                    if (edited == "TRUSTADJ" || edited == "CEMETERY")
                        continue;
                    status = dt.Rows[i]["fill"].ObjToString();
                    if (status.ToUpper() == "D")
                        continue;
                    //force = dt.Rows[i]["force"].ObjToString();
                    //if (force.ToUpper() == "Y")
                    //    continue;
                    datePaid = dt.Rows[i]["payDate8"].ObjToString().ObjToDateTime();
                    docp = datePaid;
                    if ( docp.ToString("yyyy-MM-dd") == "2020-05-06")
                    {
                    }
                    trust85P = dt.Rows[i]["trust85P"].ObjToDouble();
                    if (edited == "MANUAL" && trust85P < 0D)
                        continue;
                    downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                    //downPayment = getDownPayment(dt, i);

                    paymentAmount = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    paymentAmount = getPayment(dt, i);

                    interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                    interest = G1.RoundValue(interest);
                    if (downPayment > 0D)
                    {
                        paymentAmount = downPayment;
                        interest = 0D;
                    }
                    debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    ccFee = dt.Rows[i]["ccFee"].ObjToDouble();
                    principal = 0D;
                    if (downPayment == 0D && paymentAmount != 0D)
                    {
                        principal = paymentAmount - interest;
                        principal = G1.RoundValue(principal);
                        //principal = G1.RoundDown(principal);
                    }
                    if (downPayment > 0D)
                        principal = downPayment;
                    if (docp >= DailyHistory.majorDate && lockTrust85 != "Y")
                    {
                        if (principal == 0D && debit == 0D && credit == 0D && downPayment == 0D )
                        {
                            dt.Rows[i]["trust85P"] = 0D;
                            dt.Rows[i]["trust100P"] = 0D;
                            if ( ccFee == 0D )
                                dt.Rows[i]["prince"] = 0D;
                            dt.Rows[i]["interestPaid"] = 0D;
                            if (i < (dt.Rows.Count - 1))
                                dt.Rows[i]["balance"] = dt.Rows[i + 1]["balance"].ObjToDouble();
                            continue;
                        }
                        if (principal < 0D && debit == 0D && credit == 0D && downPayment == 0D )
                        {
                            interest = paymentAmount;
                            principal = 0D;
                            paymentAmount = 0D;
                            dt.Rows[i]["trust85P"] = 0D;
                            dt.Rows[i]["trust100P"] = 0D;
                            dt.Rows[i]["prince"] = principal;
                            dt.Rows[i]["interestPaid"] = interest;
                        }
                        //}
                        //if (debit == 0D && credit == 0D && docp > DailyHistory.secondDate)
                        //{
                        payment = paymentAmount;
                        //if (edited.ToUpper() != "MANUAL")
                        //{
                        if (debit == 0D && credit == 0D)
                        {
                            if (riles)
                            {
                                if (credit == 0D && debit == 0D)
                                {
                                    if (docp > rilesDate)
                                    {
                                        trust100P = payment;
                                        trust85P = payment * trustPercent / 100D;
                                        dt.Rows[i]["calculatedTrust85"] = trust100P;
                                        dt.Rows[i]["calculatedTrust100"] = trust85P;
                                    }
                                }
                                continue;
                            }

                            saveRetained = dt.Rows[i]["retained"].ObjToDouble();
                            saveTrust85P = dt.Rows[i]["Trust85P"].ObjToDouble();
                            saveTrust100P = dt.Rows[i]["Trust100P"].ObjToDouble();
                            if (saveRetained < 0)
                            {
                                if (Math.Abs(saveRetained) == credit)
                                    saveRetained = 0D;
                            }

                            //if (!honorFinale)
                            if (finale != "FINALE")
                            {
                                if ( docp.Month == 2 && docp.Year == 2021)
                                {
                                }
                                method = ImportDailyDeposits.CalcTrust85P(docp, amtOfMonthlyPayt, mainIssueDate.ToString("MM/dd/yyyy"), contractValue, originalDownPayment, numPayments, payment, principal, debit, credit, rate, ref trust85P, ref trust100P, ref retained);
                                if (SMFS.activeSystem.ToUpper() == "RILES")
                                    trust85P = trust100P;
                                dt.Rows[i]["trust85P"] = trust85P;
                                dt.Rows[i]["trust100P"] = trust100P;
                            }
                            else
                            {
                                retained = saveRetained;
                                trust85P = saveTrust85P;
                                trust100P = saveTrust100P;
                                dt.Rows[i]["trust85P"] = trust85P;
                                dt.Rows[i]["trust100P"] = trust100P;
                            }
                            if (saveRetained != 0D && contractDueDate8.Year >= 2039 )
                            {
                                retained = saveRetained;
                                trust85P = saveTrust85P;
                                trust100P = saveTrust100P;
                                dt.Rows[i]["trust85P"] = trust85P;
                                dt.Rows[i]["trust100P"] = trust100P;
                            }
                            dt.Rows[i]["trust85P"] = trust85P;
                            dt.Rows[i]["trust100P"] = trust100P;
                            dt.Rows[i]["prince"] = principal;
                            //if (!honorFinale && finaleCount > 1)
                            //    dt.Rows[i]["ovp"] = trust85P;
                        }
                        //}
                        //}
                    }
                }

                if (riles)
                    CleanupRilesPaymenrts(dt, trustPercent);

                if (firstPayDate < majorDate)
                    CalcMainDueDates(cnum, dt, originalDownPayment, contractDueDate8, firstPayDate, expected, lastDueDate8, saveLastDate);
                else
                    CalcMainDueDates(cnum, dt, originalDownPayment, contractDueDate8, firstPayDate, expected, lastDueDate8, saveLastDate);

                doSwitch(dt);

                CalculateRetainedInterest(dt, lockTrust85 );

                //AddTrust2013History(cnum, dt);
                RecalcRetained(cnum, dt);

                if ( dt.Rows.Count >= 2 )
                {
                    status = dt.Rows[0]["creditReason"].ObjToString();
                    if ( status.ToUpper() == "TCA")
                    {
                        dt.Rows[0]["balance"] = dt.Rows[1]["balance"].ObjToDouble();
                    }
                }
                if ( riles )
                {
                    for ( int i=0; i<dt.Rows.Count; i++)
                    {
                        //payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                        payment = getPayment(dt, i);

                        if (payment == 0D)
                            payment = dt.Rows[i]["downPayment"].ObjToDouble();
                        dt.Rows[i]["trust85P"] = payment;
                        dt.Rows[i]["trust100P"] = payment;
                    }
                }

                DataRow[] dRow = dt.Select("downPayment>'0'");
                if (dRow.Length > 0)
                {
                    DateTime duedate = dRow[0]["dueDate8"].ObjToDateTime();
                    for (int i = 0; i < dRow.Length; i++)
                    {
                        if (dRow[i]["dueDate8"].ObjToDateTime() < dueDate)
                            dueDate = dRow[i]["dueDate8"].ObjToDateTime();
                    }
                    for (int i = 0; i < dRow.Length; i++)
                    {
                        edited = dRow[i]["edited"].ObjToString();
                        if (edited != "TRUSTADJ")
                        {
                            dRow[i]["principal"] = 0D;
                            dRow[i]["prince"] = 0D;
                        }
                        dRow[i]["dueDate8"] = G1.DTtoMySQLDT(dueDate);
                        dRow[i]["location"] = "";
                    }
                }

                //balance = G1.RoundValue(startBalance);
                //for ( int i=(dt.Rows.Count-1); i>=0; i--)
                //{
                //    principal = dt.Rows[i]["prince"].ObjToDouble();
                //    balance = balance - principal;
                //    if (balance > 0D)
                //        balance = G1.RoundValue(balance);
                //    else
                //        balance = G1.RoundDown(balance);
                //    dt.Rows[i]["balance"] = balance;
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Contract=" + cnum + " " + ex.Message.ToString());
            }
        }
        /****************************************************************************************/
        private static DataTable FilterDeleted(DataTable dt)
        {
            string status = "";
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                status = dt.Rows[i]["fill"].ObjToString();
                if (status.ToUpper() == "D")
                    dt.Rows.RemoveAt(i);
            }
            return dt;
        }
        /****************************************************************************************/
            public static void AddTrust2013History(string workContract, DataTable dx)
        {
            string cmd = "Select * from `trust2013r` where `contractNumber` = '" + workContract + "' ORDER BY `payDate8` DESC;";
            DataTable dt = G1.get_db_data(cmd);

            if (G1.get_column_number(dt, "dailyHistory") < 0)
                dt.Columns.Add("dailyHistory", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "myDate") < 0)
                dt.Columns.Add("myDate");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["myDate"] = dt.Rows[i]["payDate8"].ObjToDateTime().ToString("yyyyMM");
            }
            DateTime date = DateTime.Now;
            DataRow[] dRows = null;
            string date1 = "";
            string oldDate = "";
            double trust85 = 0D;
            string fill = "";
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                fill = dx.Rows[i]["fill"].ObjToString().ToUpper();
                if (fill == "D")
                    continue;
                date = dx.Rows[i]["payDate8"].ObjToDateTime();
                if (date < DailyHistory.majorDate)
                    continue;
                date1 = date.ToString("yyyyMM");
                if (date1 == oldDate)
                    continue;
                oldDate = date1;
                dRows = dt.Select("myDate='" + date1 + "'");
                if (dRows.Length > 0)
                {
                    trust85 = dRows[0]["paymentCurrMonth"].ObjToDouble();
                    dx.Rows[i]["trust85P"] = trust85;
                }
            }
        }
        /****************************************************************************************/
        public static void CleanupRilesPaymenrts(DataTable dt, double trustPercent )
        {
            string edited = "";
            string status = "";
            DateTime docp = DateTime.Now;
            DateTime datePaid = docp;
            double debit = 0D;
            double credit = 0D;
            double payment = 0D;
            double principal = 0D;
            double trust100P = 0D;
            double trust85P = 0D;
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                edited = dt.Rows[i]["edited"].ObjToString().Trim().ToUpper();
                if (edited == "TRUSTADJ" || edited == "CEMETERY")
                    continue;
                status = dt.Rows[i]["fill"].ObjToString();
                if (status.ToUpper() == "D")
                    continue;
                datePaid = dt.Rows[i]["payDate8"].ObjToString().ObjToDateTime();
                docp = datePaid;

                //payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                payment = getPayment(dt, i);

                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                principal = payment;
                principal = G1.RoundDown(principal);
                if (debit == 0D && credit == 0D)
                {
                    if (docp > rilesDate)
                    {
                        trust100P = payment;
                        trust85P = payment * trustPercent / 100D;
                        trust85P = trust100P;
                        dt.Rows[i]["calculatedTrust85"] = trust85P;
                        dt.Rows[i]["calculatedTrust100"] = trust100P;
                        dt.Rows[i]["trust85P"] = trust85P;
                        dt.Rows[i]["trust100P"] = trust100P;
                    }
                }
            }
        }
        /****************************************************************************************/
        public static string DetermineBox ( DataTable dt, int i, int numChars )
        {
            string location = "";
            try
            {
                location = dt.Rows[i]["location"].ObjToString();
                string depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                string edited = dt.Rows[i]["edited"].ObjToString();
                string fill1 = dt.Rows[i]["fill1"].ObjToString();
                string firstChar = "";

                if (!String.IsNullOrWhiteSpace(depositNumber) && edited.ToUpper() != "MANUAL")
                {
                    firstChar = depositNumber.ToUpper().Substring(0, 1);
                    if (firstChar == "T")
                    {
                        if (numChars == 4)
                            location = "LKBX";
                        else
                            location = "LK";
                        if (fill1.ToUpper() == "TFBX")
                        {
                            if (numChars == 4)
                                location = "TFBX";
                            else
                                location = "TF";
                        }
                    }
                    else if (firstChar == "A")
                        location = "ACH";
                    else if (firstChar == "C")
                        dt.Rows[i]["location"] = "CC";
                    dt.Rows[i]["location"] = location;
                }
            }
            catch ( Exception ex)
            {
            }
            return location;
        }
        /****************************************************************************************/
        public static void CalcMainDueDates ( string contractNumber, DataTable dt, double originalDownPayment, DateTime contractDueDate8, DateTime firstPayDate, double expected, DateTime dueDate, DateTime lastDate )
        {
            if (dt.Rows.Count <= 0)
                return;
            bool calcCredit = false;
            string edited = "";
            string status = "";
            DateTime dolp = DateTime.Now;
            DateTime docp = DateTime.Now;
            DateTime nextDueDate = dueDate;
            double creditBalance = 0D;
            double runningCB = 0D;
            bool first = true;
            bool firstPayment = true;
            DateTime testDate = new DateTime(2018, 5, 7);
            DateTime dueDate8 = DateTime.Now;
            DateTime lastDueDate8 = DateTime.Now;
            if (G1.get_column_number(dt, "currentDueDate8") < 0)
                dt.Columns.Add("currentDueDate8" );

            bool insurance = DailyHistory.isInsurance(contractNumber);


            int days = 0;
            DateTime payDate8 = DateTime.Now;

            int months = 0;
            double payment = 0D;
            double interest = 0D;
            double principal = 0D;
            double downPayment = 0D;
            double balance = 0D;
            double paymentAmount = 0D;
            double lastPayment = 0D;
            double lastBalance = 0D;
            double debit = 0D;
            double credit = 0D;
            double lastDebit = 0D;
            double lastCredit = 0D;

            bool gotFinale = false;
            bool notFinale = false;

            int lastRow = dt.Rows.Count - 1;

            lastBalance = dt.Rows[lastRow]["balance"].ObjToDouble();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["new"].ObjToString().ToUpper() == "FINALE")
                    gotFinale = true;
                else
                {
                    if (gotFinale)
                    {
                        notFinale = true;
                        break;
                    }
                }
            }

            DateTime myNextDueDate = DateTime.Now;
            DateTime myFirstDueDate = firstPayDate;

            int calcMonths = 0;
            if (G1.get_column_number(dt, "numPayments") < 0)
                return;

            bool byPassDueDate = false;

            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                try
                {
                    byPassDueDate = false;
                    calcCredit = true;
                    edited = dt.Rows[i]["edited"].ObjToString().Trim().ToUpper();
                    if (edited == "TRUSTADJ" || edited == "CEMETERY" )
                        continue;
                    status = dt.Rows[i]["fill"].ObjToString();
                    if (status.ToUpper() == "D")
                        continue;

                    days = dt.Rows[i]["days"].ObjToInt32();

                    DateTime datePaid = dt.Rows[i]["payDate8"].ObjToString().ObjToDateTime();
                    docp = datePaid;
                    if (days <= 0)
                    {
                        dolp = docp;
                        days = 0;
                    }

                    downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                    downPayment = getDownPayment(dt, i);

                    //payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    payment = getPayment(dt, i);

                    interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                    debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    paymentAmount = payment + credit - debit;
                    if ( interest == 0D && credit > 0D )
                    {
                        //byPassDueDate = true; // Removed because of M23002LI on 10/15/2024
                    }
                    if (datePaid < majorDate && !recalculateHistory)
                    {
                        first = false;
                        firstPayment = false;
                        dolp = docp;
                        dueDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                        lastPayment = paymentAmount;
                        lastBalance = dt.Rows[i]["balance"].ObjToDouble();
                        dt.Rows[i]["majorCB"] = 0D;
                        calcMonths = dt.Rows[i]["numPayments"].ObjToInt32();
                        nextDueDate = dueDate.AddMonths(calcMonths);
                        continue;
                    }
                    if (downPayment != 0D)
                    {
                        if (!insurance)
                        {
                            dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(dueDate);
                            dueDate = firstPayDate;
                            nextDueDate = dueDate;
                        }
                        continue;
                    }

                    if ( !byPassDueDate )
                        lastDate = datePaid;

                    principal = dt.Rows[i]["prince"].ObjToDouble();
                    balance = dt.Rows[i]["balance"].ObjToDouble();
                    if (dueDate.Year == 0)
                        dueDate = docp;

                    creditBalance = runningCB;
//                    paymentAmount += runningCB;
                    if (payment > 0D && firstPayment)
                    {
                        if (calcCredit)
                        {
                            firstPayment = false;
                            dueDate = firstPayDate;
                        }
                    }

                    //if (!calcCredit && downPayment > 0D)
                    //    paymentAmount = 0D;

                    //if ( !majorSwitch )
                    //    CalcDueDate8(dolp, docp, dueDate, expected, lastPayment, lastBalance, ref nextDueDate, ref creditBalance);
                    //else
                    if (first)
                    {
                        dueDate = firstPayDate.AddMonths(-1);
                        //dueDate = firstPayDate;
                        myNextDueDate = firstPayDate;
                    }
                    if (debit > 0D)
                    {
                    }
                    dueDate = nextDueDate;
                    if (!insurance)
                        dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(dueDate);
                    calcMonths = dt.Rows[i]["numPayments"].ObjToInt32();
                    if ( byPassDueDate )
                    {
                        months = 0;
                    }
                    else
                        months = CalcDueDate8(dolp, docp, dueDate, expected, paymentAmount, lastBalance, ref nextDueDate, ref creditBalance);
                    first = false;
                    if ( creditBalance > 0D)
                    {
                    }

                    lastPayment = paymentAmount;
                    lastBalance = balance;
                    if (payment == 0D && firstPayment && downPayment > 0D)
                        nextDueDate = firstPayDate;
                    if (balance < 0D)
                    {
                        if ( gotFinale && !notFinale )
                            nextDueDate = new DateTime(2039, 12, 31);
                    }
                    //if ( !insurance )
                    //    dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(nextDueDate);
                    if (calcCredit)
                        runningCB = creditBalance;
                    //    runningCB += G1.RoundValue(creditBalance);
                    if (expected > 0D)
                    {
                        runningCB = runningCB % expected;
                        runningCB = G1.RoundValue(runningCB);
                    }
                    else
                        runningCB = 0D;
                    dt.Rows[i]["runningCB"] = runningCB;
                    dt.Rows[i]["majorCB"] = runningCB;
                    if (!byPassDueDate)
                    {
                        dolp = docp;
                        dueDate = nextDueDate;
                    }
                    else
                    {
                    }
                    dt.Rows[i]["currentDueDate8"] = nextDueDate.ToString("MM/dd/yyyy");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR1*** i=" + i.ToString() + " Contract=" + contractNumber + " " + ex.Message.ToString());
                }
            }

            bool gotDebit = false;
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                if ( debit != 0D)
                {
                    gotDebit = true;
                    break;
                }
            }
            if (gotDebit)
            {
                DateTime lastDueDate = DateTime.Now;
                int lastMonths = 0;
                first = true;
                //for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                //{
                //    downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                //    if (downPayment != 0D)
                //        continue;
                //    dueDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                //    debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                //    months = dt.Rows[i]["numPayments"].ObjToInt32();
                //    if (first)
                //    {
                //        lastDueDate = dueDate.AddMonths(months);
                //        first = false;
                //        continue;
                //    }
                //    if (months >= 4)
                //    {
                //    }
                //    if (months < 0)
                //    {
                //        dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(lastDueDate);
                //        dueDate = lastDueDate.AddMonths(months);
                //    }
                //    else
                //    {
                //        dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(lastDueDate);
                //        dueDate = lastDueDate.AddMonths(months);
                //    }
                //    lastDueDate = dueDate;
                //    lastMonths = months;
                //}
            }
        }
        /****************************************************************************************/
        public static void CalculateRetainedInterest ( DataTable dt, string lockTrust85, bool recalculate = false )
        {
            if (G1.get_column_number(dt, "retained") < 0)
                dt.Columns.Add("retained", Type.GetType("System.Double"));
            //Last Edit 7/15/2021 Cliff found AS400 Wrong Interest on L08001 So only run on new data
            double retained = 0D;
            double payment = 0D;
            double credit = 0D;
            double debit = 0D;
            double trust100P = 0D;
            double interestPaid = 0D;
            double balance = 0D;
            double ccFee = 0D;
            DateTime docp = DateTime.Now;

            string contract = "";
            bool riles = false;
            if (dt.Rows.Count > 0)
            {
                contract = dt.Rows[0]["contractNumber"].ObjToString();
                riles = DailyHistory.isRiles(contract);
            }
            int days = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    balance = dt.Rows[i]["balance"].ObjToDouble();
                    docp = dt.Rows[i]["payDate8"].ObjToDateTime();
                    if ( riles )
                    {
                        if (docp < DailyHistory.rilesDate)
                            continue;
                    }
                    else if (docp < majorDate)
                        continue;

                    interestPaid = dt.Rows[i]["interestPaid"].ObjToDouble();
                    if ( interestPaid <= 0D)
                    {

                    }
                    retained = dt.Rows[i]["retained"].ObjToDouble();
                    if (retained < 0)
                    {
                        credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                        if (Math.Abs(retained) == credit)
                        {
                            retained = 0D;
                            dt.Rows[i]["retained"] = 0D;
                        }
                    }
                    if (retained == 0D || recalculate )
                    {
                        docp = dt.Rows[i]["payDate8"].ObjToDateTime();
                        if ( docp.ToString("yyyy-MM-dd") == "2020-05-06")
                        {
                        }

                        //payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                        payment = getPayment(dt, i);

                        ccFee = dt.Rows[i]["ccFee"].ObjToDouble();
                        credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                        debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                        trust100P = dt.Rows[i]["trust100P"].ObjToDouble();
                        if (payment != 0D)
                            retained = payment - trust100P;
                        else if (credit != 0D)
                            retained = credit - trust100P;
                        else if (debit != 0D)
                        {
                            retained = debit - Math.Abs(trust100P);
                            retained = retained * -1D;
                        }
                        if (docp >= majorDate)
                        {
                            if (debit != 0D)
                                retained = G1.RoundUp(retained);
                            else
                                retained = G1.RoundDown(retained);
                        }
                        if (ccFee < 0D)
                            retained = 0D;
                        dt.Rows[i]["retained"] = retained;
                        days = dt.Rows[i]["days"].ObjToInt32();
                        if (interestPaid == 0D && days > 0 && debit == 0D && credit == 0D)
                        {
                            if (docp >= DailyHistory.interestDate)
                            {
                                if ( balance >= 0D)
                                    dt.Rows[i]["interestPaid"] = retained;
                            }
                            else
                                dt.Rows[i]["interestPaid"] = retained;
                        }
                    }
                }
                catch ( Exception ex)
                {
                    MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                }
            }
        }
        /****************************************************************************************/
        public static void RecalcRetained ( string contractNumber, DataTable dt )
        {
            if ( DailyHistory.gotCemetery ( dt ))
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["retained"] = 0D;
                return;
            }
            if (!contractNumber.ToUpper().Contains("LI"))
                return;
            if (G1.get_column_number(dt, "retained") < 0)
                return;
            if ( 1 == 1)
            { // As Per CP in July 2021, all LI contracts should have retained interest = zero
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["retained"] = 0D;
                return;
            }
            double paymentAmount = 0D;
            double interestPaid = 0D;
            double retained = 0D;
            double trust85P = 0D;
            double trust100P = 0D;
            double credit = 0D;
            double difference = 0D;
            string finale = "";
            DateTime date = DateTime.Now;
            DateTime startDate = new DateTime(2021, 1, 1);
            //startDate = new DateTime(2018, 1, 1);
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["payDate8"].ObjToDateTime();
                if (date <= startDate)
                    continue;
                finale = dt.Rows[i]["new"].ObjToString().ToUpper();
                interestPaid = dt.Rows[i]["interestPaid"].ObjToDouble();
                retained = dt.Rows[i]["retained"].ObjToDouble();
                trust100P = dt.Rows[i]["trust100P"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                if (credit > 0D)
                {
                    if (credit == retained)
                    {
                        dt.Rows[i]["retained"] = interestPaid;
                        continue;
                    }
                    else if ( trust100P < 0D)
                    {
                        dt.Rows[i]["retained"] = interestPaid;
                    }
                }
                if (finale != "FINALE")
                {
                    difference = interestPaid - retained;
                    if (difference >= -0.02 && difference <= 0.02)
                        dt.Rows[i]["retained"] = interestPaid;
                    continue;
                }
                if (interestPaid != retained)
                {
                    if (paymentAmount == 0D)
                    {
                        if (credit != 0D)
                            paymentAmount = credit;
                    }
                    retained = interestPaid;

                    //paymentAmount = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    paymentAmount = getPayment(dt, i);

                    trust100P = paymentAmount - retained;
                    trust85P = trust100P * 0.85;
                    dt.Rows[i]["retained"] = retained;
                    trust85P = G1.RoundDown(trust85P);
                    trust100P = G1.RoundDown(trust100P);
                    dt.Rows[i]["trust100P"] = trust100P;
                    dt.Rows[i]["trust85P"] = trust85P;
                    dt.Rows[i]["calculatedTrust100"] = trust100P;
                    dt.Rows[i]["calculatedTrust85"] = trust85P;
                }
            }
        }
        /****************************************************************************************/
        public static void RecalcRetained( DataTable dt, string interestColumn = "" )
        {
            if (G1.get_column_number(dt, "retained") < 0)
                return;
            string contractNumber = "";
            double paymentAmount = 0D;
            double credit = 0D;
            double debit = 0D;
            double interestPaid = 0D;
            double retained = 0D;
            double trust85P = 0D;
            double trust100P = 0D;
            double difference = 0D;
            string finale = "";
            string findRecord = "";
            double newInterest = 0D;
            double newTrust85 = 0D;
            double newTrust100 = 0D;
            bool foundLI = false;

            if (String.IsNullOrWhiteSpace(interestColumn))
                interestColumn = "interestPaid";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                foundLI = false;
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                if (contractNumber.ToUpper().Contains("LI") )
                    dt.Rows[i]["retained"] = 0D;
                if ( contractNumber.EndsWith ( "L"))
                    dt.Rows[i]["retained"] = 0D;
                if ( 1 == 1)
                {
                    //dt.Rows[i]["retained"] = 0D;
                    continue;
                }
                if (contractNumber.ToUpper().EndsWith ( "LI"))
                {
                    findRecord = dt.Rows[i]["record"].ObjToString();
                    DailyHistory.CalcPaymentData(contractNumber, findRecord, ref newInterest, ref newTrust85, ref newTrust100);
                    foundLI = true;
                }

                //if ( contractNumber == "B17066LI")
                //{
                //}
                finale = dt.Rows[i]["new"].ObjToString().ToUpper();
                interestPaid = dt.Rows[i][interestColumn].ObjToDouble();
                retained = dt.Rows[i]["retained"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                if (debit != 0D)
                    continue;
                if ( credit > 0D )
                {
                    if (credit == retained)
                    {
                        dt.Rows[i]["retained"] = interestPaid;
                        continue;
                    }
                }
                if (finale != "FINALE")
                {
                    difference = interestPaid - retained;
                    if (difference >= -0.02 && difference <= 0.02)
                        dt.Rows[i]["retained"] = interestPaid;
                    else
                    {
                        retained = interestPaid;
                        //paymentAmount = dt.Rows[i]["paymentAmount"].ObjToDouble();
                        paymentAmount = getPayment(dt, i);

                        if (paymentAmount == 0D)
                        {
                            if (credit != 0D)
                                paymentAmount = credit;
                        }
                        else if (paymentAmount == interestPaid)
                        {
                            //retained = dt.Rows[i]["retained"].ObjToDouble();
                            //if (retained != interestPaid)
                            //{
                            //    interestPaid = retained;
                            //    dt.Rows[i][interestColumn] = interestPaid;
                            //}
                        }
                        trust100P = paymentAmount - interestPaid;
                        trust85P = trust100P * 0.85;
                        dt.Rows[i]["retained"] = interestPaid;
                        trust85P = G1.RoundDown(trust85P);
                        trust100P = G1.RoundDown(trust100P);
                        dt.Rows[i]["trust100P"] = trust100P;
                        dt.Rows[i]["trust85P"] = trust85P;
                    }
                    continue;
                }
                if (interestPaid != retained)
                {
                    retained = interestPaid;
                    //paymentAmount = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    paymentAmount = getPayment(dt, i);

                    if (paymentAmount == 0D)
                    {
                        if (credit != 0D)
                            paymentAmount = credit;
                    }
                    trust100P = paymentAmount - interestPaid;
                    trust85P = trust100P * 0.85;
                    dt.Rows[i]["retained"] = interestPaid;
                    trust85P = G1.RoundDown(trust85P);
                    trust100P = G1.RoundDown(trust100P);
                    dt.Rows[i]["trust100P"] = trust100P;
                    dt.Rows[i]["trust85P"] = trust85P;
                }
                if (foundLI)
                {
                    dt.Rows[i]["retained"] = newInterest;
                    dt.Rows[i]["interestPaid"] = newInterest;
                    dt.Rows[i]["trust100P"] = newTrust100;
                    dt.Rows[i]["trust85P"] = newTrust85;
                }
            }
        }
        /****************************************************************************************/
        public static void doSwitch( DataTable dt)
        {

            if ( majorSwitch && !recalculateHistory )
            {
                if (G1.get_column_number(dt, "balance") < 0)
                    dt.Columns.Add("balance", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "prince") < 0)
                    dt.Columns.Add("prince", Type.GetType("System.Double"));
                //for (int i = 0; i < dt.Rows.Count; i++)
                //{
                //    dt.Rows[i]["trust85P"] = dt.Rows[i]["calculatedTrust85"].ObjToDouble();
                //    dt.Rows[i]["trust100P"] = dt.Rows[i]["calculatedTrust100"].ObjToDouble();
                //    dt.Rows[i]["interestPaid"] = dt.Rows[i]["int"].ObjToDouble();
                //    dt.Rows[i]["prince"] = dt.Rows[i]["principal"].ObjToDouble();
                //    dt.Rows[i]["balance"] = dt.Rows[i]["newbalance"].ObjToDouble();
                //}
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["calculatedTrust85"] = dt.Rows[i]["trust85P"].ObjToDouble();
                    dt.Rows[i]["calculatedTrust100"] = dt.Rows[i]["trust100P"].ObjToDouble();
                    dt.Rows[i]["int"] = dt.Rows[i]["interestPaid"].ObjToDouble();
                    dt.Rows[i]["principal"] = dt.Rows[i]["prince"].ObjToDouble();
                    dt.Rows[i]["newbalance"] = dt.Rows[i]["balance"].ObjToDouble();
                }
            }
            else
            {
                //DateTime payDate8 = DateTime.Now;
                //for (int i = 0; i < dt.Rows.Count; i++)
                //{
                //    payDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();
                //    if ( payDate8 >= majorDate)
                //    {
                //        dt.Rows[i]["trust85P"] = dt.Rows[i]["calculatedTrust85"].ObjToDouble();
                //        dt.Rows[i]["trust100P"] = dt.Rows[i]["calculatedTrust100"].ObjToDouble();
                //    }
                //}
            }
        }
        /****************************************************************************************/
        public static int CalcDueDate8(DateTime dolp, DateTime docp, DateTime dueDate, double monthlyPayment, double currentPayment, double newBalance, ref DateTime nextDueDate, ref double creditBalance)
        {
            nextDueDate = DateTime.Now;
            if (currentPayment == monthlyPayment)
            {
                nextDueDate = dueDate.ObjToDateTime();
                nextDueDate = nextDueDate.AddMonths(1);
                return 0;
            }
            int months = 1;
            double principal = currentPayment + creditBalance;
            //principal = currentPayment;
            double originalPayment = principal;
            double paid = principal;
            int count = 0;
            bool maxedOut = false;
            double nowDue = 0D;

            if (principal > 0D)
            {
                months = 0;
                for (;;)
                {
                    principal = G1.RoundValue(principal);
                    if (principal < monthlyPayment)
                    {
                        nowDue = principal;
                        nowDue = G1.RoundValue(nowDue);
                        creditBalance = nowDue;
                        break;
                    }
                    principal = principal - monthlyPayment;
                    months++;
                    count++;
                    if (count >= 1000)
                    {
                        maxedOut = true;
                        break;
                    }
                }
                nextDueDate = dueDate.ObjToDateTime();
                if (maxedOut)
                    return 0;
                //                nextDueDate = nextDueDate.AddMonths((months - 1));
                nextDueDate = nextDueDate.AddMonths((months));
                if (nextDueDate < dueDate.ObjToDateTime())
                    nextDueDate = dueDate.ObjToDateTime();
            }
            else
            {
                months = 0;
                principal = Math.Abs(principal);
                for (;;)
                {
                    principal = G1.RoundValue(principal);
                    if (principal <= monthlyPayment)
                    {
                        nowDue = principal;
                        nowDue = G1.RoundValue(nowDue);
                        creditBalance = nowDue;
                        //                        creditBalance = 0D;
                        months--;
                        break;
                    }
                    principal = principal - monthlyPayment;
                    months--;
                    count++;
                    if (count >= 1000)
                    {
                        maxedOut = true;
                        break;
                    }
                }
                nextDueDate = dueDate.ObjToDateTime();
                if (maxedOut)
                    return 0;
                //                nextDueDate = nextDueDate.AddMonths((months - 1));
                nextDueDate = nextDueDate.AddMonths((months));
            }
            return months;
        }
        /****************************************************************************************/
        public static int CalcDueDate ( DateTime dolp, DateTime docp, DateTime dueDate, double monthlyPayment, double currentPayment, double credit, double debit, double apr, double newBalance, ref DateTime nextDueDate, ref double creditBalance)
        {
            if (docp < DailyHistory.majorDate && !recalculateHistory )
                creditBalance = 0D;
            nextDueDate = DateTime.Now;
            TimeSpan ts = nextDueDate - dolp;
            int days = ts.Days;
            double interest = CalculateInterest(dolp, days, apr, newBalance);
            interest = G1.RoundValue(interest);
            double tempInt = interest;
            nextDueDate = dolp;
            double nowDue = 0D;
            double creditDue = creditBalance;
            creditDue = G1.RoundValue(creditDue);
            currentPayment = currentPayment + credit - debit;
            currentPayment = G1.RoundValue(currentPayment);
            currentPayment += creditDue;
            currentPayment = G1.RoundValue(currentPayment);
            creditBalance = currentPayment % monthlyPayment;
            creditBalance = G1.RoundValue(creditBalance);
            if (docp < DailyHistory.majorDate && !recalculateHistory )
                creditBalance = 0D;
            if (currentPayment == monthlyPayment)
            {
                nextDueDate = dueDate.ObjToDateTime();
                nextDueDate = nextDueDate.AddMonths(1);
                return 0;
            }
            int months = 1;
            double principal = currentPayment;
            double originalPayment = principal;
            double paid = principal;
            int count = 0;
            bool maxedOut = false;

            if (principal > 0D)
            {
                months = 0;
                for (;;)
                {
                    principal = G1.RoundValue(principal);
                    if (principal < monthlyPayment)
                    {
                        nowDue = principal;
                        nowDue = G1.RoundValue(nowDue);
                        //creditBalance = nowDue;
                        break;
                    }
                    principal = principal - monthlyPayment;
                    months++;
                    count++;
                    if (count >= 1000)
                    {
                        maxedOut = true;
                        break;
                    }
                }
                nextDueDate = dueDate.ObjToDateTime();
                if (maxedOut)
                    return 0;
//                nextDueDate = nextDueDate.AddMonths((months - 1));
                nextDueDate = nextDueDate.AddMonths((months));
                if (nextDueDate < dueDate.ObjToDateTime())
                    nextDueDate = dueDate.ObjToDateTime();
            }
            else
            {
                months = 0;
                principal = Math.Abs(principal);
                for (;;)
                {
                    principal = G1.RoundValue(principal);
                    if (principal <= monthlyPayment)
                    {
                        nowDue = principal;
                        nowDue = G1.RoundValue(nowDue);
                        //creditBalance = nowDue;
//                        creditBalance = 0D;
                        break;
                    }
                    principal = principal - monthlyPayment;
                    months--;
                    count++;
                    if (count >= 1000)
                    {
                        maxedOut = true;
                        break;
                    }
                }
                nextDueDate = dueDate.ObjToDateTime();
                if (maxedOut)
                    return 0;
//                nextDueDate = nextDueDate.AddMonths((months - 1));
                nextDueDate = nextDueDate.AddMonths((months));
            }
            return months;
        }
        /****************************************************************************************/
        public static double CalculateTheMortgage(double yearlyInterestRate, int totalNumberOfMonths, double actualloanAmount)
        {

            // (Loan Value) * (1 + r/12) ^ p = (12x / r) * ((1 + r/12)^p - 1)

            // payment = (((Loan Value) * (1 + r/12) ^ p) * r)/ (12 * ((1 + r/12)^p - 1)));

            double loanAmount = actualloanAmount;                  // price of total mortgage before down payment
            double taxesPerYear = 0D;                   // this will divided by 12 and added to the monthly payment
            double downPayment = 0D;          // down payment will be subtracted from the loan
            double interestRate = yearlyInterestRate;                               // calculate interest from 100%
            double termOfLoan = (double)totalNumberOfMonths;                                 // monthly term
            double propertyTax = 0D;
            double insurance = 0D;

            // plug the values from the input into the mortgage formula

            double payment = (loanAmount - downPayment) * (Math.Pow((1 + interestRate / 12), termOfLoan) * interestRate) / (12 * (Math.Pow((1 + interestRate / 12), termOfLoan) - 1));

            // add on a monthly property tax and insurance

            payment = payment + (propertyTax + insurance) / 12;

            // place the monthly payment calculated into the output text field

            return payment;
        }
        /****************************************************************************************/
        private void LoadHeader()
        {
            string agent = "";
            lblServiceId.Text = "";
            string custDeceasedDate = "";
            string cmd = "Select * from `" + customersFile + "` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                if ( workPolicy || customersFile == "icustomers" )
                {
                    workPayer = dx.Rows[0]["payer"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(workPolicyRecord))
                    {
                        cmd = "Select * from `policies` where `record` = '" + workPolicyRecord + "';";
                        policyDt = G1.get_db_data(cmd);
                    }
                    else if ( !String.IsNullOrWhiteSpace ( workPayer ))
                    {
                        cmd = "Select * from `policies` where `payer` = '" + workPayer + "';";
                        policyDt = G1.get_db_data(cmd);
                    }
                }
                if (isInsurance(workContract))
                    workPayer = dx.Rows[0]["payer"].ObjToString();

                CheckForAgreement(workContract);

                //cmd = "Select * from `pdfimages` where `contractNumber` = '" + workContract + "';";
                //DataTable picDt = G1.get_db_data(cmd);
                //if (picDt.Rows.Count > 0)
                //{
                //    this.picAgreement.Tag = picDt.Rows[0]["record"].ObjToString();
                //    this.picAgreement.Show();
                //}
                //else
                //{
                //    this.picAgreement.Tag = "";
                //    this.picAgreement.Hide();
                //}
                string firstName = dx.Rows[0]["firstName"].ObjToString();
                string lastName = dx.Rows[0]["lastName"].ObjToString();
                workName = firstName + " " + lastName;
                this.Text = "Daily History for (" + workContract + ") " + firstName + " " + lastName;
                agent = dx.Rows[0]["agentCode"].ObjToString();
                contractDate = dx.Rows[0]["contractDate"].ObjToDateTime();
                if (workPolicy)
                    contractDate = policyDt.Rows[0]["issueDate8"].ObjToDateTime();
                custDeceasedDate = dx.Rows[0]["deceasedDate"].ObjToString();
                if (custDeceasedDate.ObjToDateTime().Year < 1900)
                    custDeceasedDate = "";
            }
            lblAgent.Text = "Agent: " + agent;
            cmd = "Select * from `" + contractsFile + "` where `contractNumber` = '" + workContract + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                double serTot = dx.Rows[0]["serviceTotal"].ObjToString().ObjToDouble();
                double merTot = dx.Rows[0]["merchandiseTotal"].ObjToString().ObjToDouble();
                double downPayment = dx.Rows[0]["downPayment"].ObjToString().ObjToDouble();
                if (downPayment == 0D)
                    downPayment = GetDownPaymentFromPayments(workContract);
                double totalPurchase = serTot + merTot - downPayment;
                totalPurchase = DailyHistory.GetFinanceValue(dx.Rows[0]);
                startBalance = totalPurchase;
                double contractValue = DailyHistory.GetContractValuePlus(dx.Rows[0]);
                string lapsed = dx.Rows[0]["lapsed"].ObjToString();
                double balanceDue = dx.Rows[0]["balanceDue"].ObjToString().ObjToDouble();
                labelSerTot.Text = "Service Total: $" + G1.ReformatMoney(serTot);
                labelMerTot.Text = "Merchandise Total: $" + G1.ReformatMoney(merTot);
                labDownPayment.Text = "Down Payment: $" + G1.ReformatMoney(downPayment);
                labRemainingBalance.Text = "Total Finance: $" + G1.ReformatMoney(totalPurchase);
                if (isInsurance(workContract))
                {
                    balanceDue = Policies.CalcMonthlyPremium(workPayer, DateTime.Now);
                    balanceDue = Policies.CalcMonthlyPremium(workPayer);
                }
                labBalanceDue.Text = "$" + G1.ReformatMoney(balanceDue);
                lblLapsed.Text = "";
                if (lapsed.ToUpper() == "Y")
                {
                    lblLapsed.Text = "Lapsed";
                    btnReinstate.Show();
                    btnRequest.Show();
                }
                lblContractValue.Text = "Contract Value: $" + G1.ReformatMoney(contractValue);
                if (workPolicy)
                    lblContractValue.Hide();


                double payment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
                if (isInsurance ( workContract))
                    payment = Policies.CalcMonthlyPremium(workPayer, DateTime.Now);
                ExpectedPayment = payment;
                if (isInsurance(workContract))
                {
                    lblPayment.Text = "Premium :$" + G1.ReformatMoney(payment);

                    DateTime dueDate8 = DateTime.Now;
                    string payerContract = "";
                    DateTime lapseDate8 = DateTime.Now;
                    DateTime reinstateDate8 = DateTime.Now;
                    DateTime deceasedDate = DateTime.Now;

                    lapsed = "";
                    DateTime lastPaidDate = DailyHistory.GetInsuranceLastPaid(workPayer, ref dueDate8, ref payerContract, ref lapseDate8, ref reinstateDate8, ref lapsed, ref deceasedDate );

                    int imonths = G1.GetMonthsBetween(DateTime.Now, dueDate8);

                    double premium = imonths * payment;
                    premium += payment; // Add another month
                    premium = ExpectedPayment;

                    labBalanceDue.Text = "$" + G1.ReformatMoney(premium);
                }
                else
                    lblPayment.Text = "Payment :$" + G1.ReformatMoney(payment);
                numPayments = dx.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
                lblNumPayments.Text = "# Pmts: " + numPayments.ToString();
                double totalInterest = dx.Rows[0]["totalInterest"].ObjToString().ObjToDouble();
                //            lblTotalInterest.Text = G1.ReformatMoney(totalInterest);
                string dueDate = dx.Rows[0]["dueDate8"].ObjToString();
                lblDueDate.Text = "Due Date: " + dueDate;

                DateTime iDate = DateTime.Now;
                string issueDate = dx.Rows[0]["issueDate8"].ObjToString();
                if (workPolicy)
                    issueDate = policyDt.Rows[0]["issueDate8"].ObjToString();
                else
                {
                    iDate = GetIssueDate(dx.Rows[0]["issueDate8"].ObjToDateTime(), workContract, dx);
                    issueDate = iDate.ToString("MM/dd/yyyy");
                    lastDate = issueDate.ObjToDateTime();
                }
                if (issueDate.IndexOf("0000") >= 0)
                    issueDate = contractDate.Month.ToString("D2") + "/" + contractDate.Day.ToString("D2") + "/" + contractDate.Year.ToString("D4");
                lblIssueDate.Text = "Issue Date: " + issueDate;

                string apr = dx.Rows[0]["APR"].ObjToString();
                lblAPR.Text = "APR :" + apr + "%";
                dAPR = apr.ObjToDouble();

                string deadDate = dx.Rows[0]["deceasedDate"].ObjToString();
                iDate = deadDate.ObjToDateTime();
                if (iDate.Year < 1500)
                {
                    if (!String.IsNullOrWhiteSpace(custDeceasedDate))
                        iDate = custDeceasedDate.ObjToDateTime();
                    if ( iDate.Year < 1500 )
                        lblDeadDate.Text = "";
                    else
                        lblDeadDate.Text = "Deceased Date: " + custDeceasedDate;
                }
                else
                    lblDeadDate.Text = "Deceased Date: " + deadDate;

                double cashAdvance = dx.Rows[0]["cashAdvance"].ObjToString().ObjToDouble();
                lblCashAdvance.Text = "Cash Advance: $" + G1.ReformatMoney(cashAdvance);
                if (workPolicy)
                    lblCashAdvance.Hide();
                string serviceId = dx.Rows[0]["ServiceId"].ObjToString();
                if (!String.IsNullOrWhiteSpace(serviceId))
                    lblServiceId.Text = "Service ID : " + serviceId;
                double creditBalance = dx.Rows[0]["creditBalance"].ObjToDouble();
                lblCreditBalance.Text = "Credit Balance : " + G1.ReformatMoney(creditBalance);

                double unpaid_interest = dx.Rows[0]["unpaid_interest"].ObjToDouble();
                lblUnpaidInterest.Text = "Unpaid Interest : " + G1.ReformatMoney(unpaid_interest);
            }
        }
        /****************************************************************************************/
        private void CheckForAgreement ( string contractNumber )
        {
            string cmd = "Select * from `pdfimages` where `contractNumber` = '" + contractNumber + "';";
            DataTable picDt = G1.get_db_data(cmd);
            if (picDt.Rows.Count > 0)
            {
                this.picAgreement.Tag = picDt.Rows[0]["record"].ObjToString();
                this.picAgreement.Show();
            }
            else
            {
                this.picAgreement.Tag = "";
                this.picAgreement.Hide();
            }
        }
        /****************************************************************************************/
        private void LoadDetailHeader()
        {
            string cmd = "Select * from `" + contractsFile + "` a JOIN `" + customersFile + "` b on a.`contractNumber` = b.`contractNumber` where a.`contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;
            DataTable contractDt = dx.Copy();

            double payment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
            ExpectedPayment = payment;
            int numPayments = dx.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
            double totalInterest = dx.Rows[0]["totalInterest"].ObjToString().ObjToDouble();
            string dueDate = dx.Rows[0]["dueDate8"].ObjToString();
            string issueDate = dx.Rows[0]["issueDate8"].ObjToString();
            DateTime iDate = GetIssueDate(dx.Rows[0]["issueDate8"].ObjToDateTime(), workContract, dx);
            issueDate = iDate.ToString("MM/dd/yyyy");
            lastDate = issueDate.ObjToDateTime();
            if (issueDate.IndexOf("0000") >= 0)
                issueDate = contractDate.Month.ToString("D2") + "/" + contractDate.Day.ToString("D2") + "/" + contractDate.Year.ToString("D4");

            DateTime firstPayDate = dx.Rows[0]["firstPayDate"].ObjToDateTime();
            iDate = issueDate.ObjToDateTime();
            firstPayDate = DailyHistory.CheckFirstPayDate(firstPayDate, iDate);
            //            lblDateOfFP.Text = "FPD: " + firstPayDate.ToString("MM/dd/yyyy");
            //lblDateOfFP.Text = "FPD: ";
            txtDOFP.Text = firstPayDate.ToString("MM/dd/yyyy");
            txtDOFP.Enabled = false;
            if (LoginForm.username == "cjenkins" || LoginForm.username == "robby")
                txtDOFP.Enabled = true;
            lblDueDate.Text = "Due Date: " + dueDate;


            string apr = dx.Rows[0]["APR"].ObjToString();

            double dAPR = apr.ObjToDouble() / 100.0D;

            double p2 = CalculateTheMortgage(dAPR, numPayments, startBalance);

            dx = (DataTable)dgv.DataSource;
            if (numPayments <= 0 && dx.Rows.Count > 0)
                numPayments = dx.Rows.Count;
            if (!isInsurance(workContract))
            {
                //majorSwitch = true;
                CalculateNewStuff(dx, dAPR, numPayments, startBalance, lastDate);
                double newbalance = 0D;
                iDate = DailyHistory.getNextDueDate(dx, payment, ref newbalance);
                if (iDate.Year < 100)
                    lblCDD.Hide();
                else
                    lblCDD.Text = "CDD " + iDate.ToString("MM/dd/yyyy");

                //majorSwitch = false;

                if (dx.Rows.Count > 0)
                {
                    double creditBalance = dx.Rows[0]["runningCB"].ObjToDouble();
                    creditBalance = G1.RoundValue(creditBalance);
                    lblCreditBalance.Text = "Credit Balance : " + G1.ReformatMoney(creditBalance);

                    double unpaid_interest = contractDt.Rows[0]["unpaid_interest"].ObjToDouble();
                    unpaid_interest = G1.RoundValue(unpaid_interest);
                    lblUnpaidInterest.Text = "Unpaid Interest : " + G1.ReformatMoney(unpaid_interest);

                    string record = contractDt.Rows[0]["record"].ObjToString();
                    G1.update_db_table(contractsFile, "record", record, new string[] { "creditBalance", creditBalance.ToString() });
                    CustomerDetails.UpdatePayersDetail(workContract);
                }
            }
            else
            {
                double debit = 0D;
                double ccFee = 0D;
                for ( int i=0; i<dx.Rows.Count; i++)
                {
                    ccFee = dx.Rows[i]["ccFee"].ObjToDouble();
                    debit = dx.Rows[i]["debitAdjustment"].ObjToDouble();
                    if ( debit != 0D)
                    {
                        if ( ccFee == 0D )
                            dx.Rows[i]["prince"] = -debit;
                    }
                }
            }

            CheckForAgreement(workContract);

            CalcTrust85Header(contractDt, dx);
        }
        /****************************************************************************************/
        public static double GetDueNow ( string workPayer, double premium = 0D )
        {
            if ( premium == 0D )
                premium = Policies.CalcMonthlyPremium(workPayer, DateTime.Now);

            DateTime dueDate8 = DateTime.Now;
            string payerContract = "";
            DateTime lapseDate8 = DateTime.Now;
            DateTime reinstateDate8 = DateTime.Now;
            DateTime deceasedDate = DateTime.Now;

            string lapsed = "";
            DateTime lastPaidDate = DailyHistory.GetInsuranceLastPaid(workPayer, ref dueDate8, ref payerContract, ref lapseDate8, ref reinstateDate8, ref lapsed, ref deceasedDate );

            int imonths = G1.GetMonthsBetween(DateTime.Now, dueDate8);

            double balanceDue = imonths * premium;
            balanceDue += premium; // Add another month
            return balanceDue;
        }
        /****************************************************************************************/
        private void CalcTrust85Header(DataTable contractDt, DataTable paymentsDt)
        {
            if (workPolicy)
                return;
            DateTime lastPaidDate = DateTime.Now;

            if (isInsurance(workContract))
            {
                double balanceDue = Policies.CalcMonthlyPremium(workPayer, DateTime.Now );

                DateTime dueDate8 = DateTime.Now;
                string payerContract = "";
                DateTime lapseDate8 = DateTime.Now;
                DateTime reinstateDate8 = DateTime.Now;
                DateTime deceasedDate = DateTime.Now;

                string lapsed = "";
                lastPaidDate = DailyHistory.GetInsuranceLastPaid(workPayer, ref dueDate8, ref payerContract, ref lapseDate8, ref reinstateDate8, ref lapsed, ref deceasedDate );

                int imonths = G1.GetMonthsBetween(DateTime.Now, dueDate8);

                double premium = imonths * balanceDue;
                premium += balanceDue; // Add another month
                premium = Policies.CalcMonthlyPremium(workPayer, DateTime.Now );

                labBalanceDue.Text = "$" + G1.ReformatMoney(premium);

                //labBalanceDue.Text = "$" + G1.ReformatMoney(balanceDue);
                labBalanceDue.Refresh();

                lblPayment.Text = "Premium :$" + G1.ReformatMoney(balanceDue);
                lblPayment.Refresh();
            }

            double contractValue = DailyHistory.GetContractValuePlus(contractDt.Rows[0]);
            if (contractValue <= 0D)
                return;
            double trust85P = 0D;
            double trust100P = 0D;
            double financeDays = contractDt.Rows[0]["numberOfPayments"].ObjToDouble();
            double downPayment = contractDt.Rows[0]["downPayment"].ObjToDouble();
            double principal = startBalance + downPayment;

            double payment = contractDt.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
            ExpectedPayment = payment;
            double amtOfMonthlyPayt = payment;
            int numPayments = contractDt.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
            string dueDate = contractDt.Rows[0]["dueDate8"].ObjToString();
            string issueDate = contractDt.Rows[0]["issueDate8"].ObjToString();
            DateTime iDate = GetIssueDate(contractDt.Rows[0]["issueDate8"].ObjToDateTime(), workContract, null);
            issueDate = iDate.ToString("MM/dd/yyyy");
            lastDate = issueDate.ObjToDateTime();
            if (issueDate.IndexOf("0000") >= 0)
                issueDate = contractDate.Month.ToString("D2") + "/" + contractDate.Day.ToString("D2") + "/" + contractDate.Year.ToString("D4");
            string apr = contractDt.Rows[0]["APR"].ObjToString();
            double trustPercent = contractDt.Rows[0]["trustPercent"].ObjToDouble();

            if (trustPercent <= 0D)
            {
                if (lastDate <= new DateTime(2006, 6, 30))
                    trustPercent = 50D;
                else
                    trustPercent = 85D;
            }
            else if (trustPercent <= 1.0)
                trustPercent = trustPercent * 100D;
            if (isRiles(workContract))
                trustPercent = 100D;

            //if (lastDate <= new DateTime(2006, 6, 30) && trustPercent == 0D )
            //    trustPercent = 50D;
            //if (trustPercent <= 1.0D)
            //    trustPercent = trustPercent * 100D;

            double rate = apr.ObjToDouble() / 100.0D;

            ImportDailyDeposits.CalcTrust85(amtOfMonthlyPayt, issueDate, contractValue, downPayment, financeDays, payment, principal, rate, ref trust85P, ref trust100P);

            double beginningBalance = 0D;
            double endingBalance = 0D;

            lastPaidDate = GetTrustLastPaid(workContract, ref beginningBalance, ref endingBalance);
            lblTrust85.Text = "Trust85P:$" + G1.ReformatMoney(endingBalance);

            double Trust85Paid = 0D;
            double Trust85Calc = 0D;
            //trust85P = contractValue * 0.85D;
            trust85P = contractValue * (trustPercent / 100D);
            double totalTrust85 = 0D;
            double dValue = 0D;
            bool done = false;
            DateTime payDate = DateTime.Now;
            for (int i = (paymentsDt.Rows.Count - 1); i >= 0; i--)
            {
                if (paymentsDt.Rows[i]["fill"].ObjToString().ToUpper() == "D")
                    continue;
                payDate = paymentsDt.Rows[i]["payDate8"].ObjToDateTime();
                if (payDate <= lastPaidDate)
                    continue;
                Trust85Paid += paymentsDt.Rows[i]["trust85P"].ObjToDouble();
                totalTrust85 += paymentsDt.Rows[i]["trust85P"].ObjToDouble();
                dValue = paymentsDt.Rows[i]["calculatedTrust85"].ObjToDouble();
                if (done)
                {
                    paymentsDt.Rows[i]["calculatedTrust85"] = 0D;
                    dValue = 0D;
                }
                if (!done)
                {
                    if (Trust85Calc + dValue > trust85P)
                    {
                        rate = trust85P - (Trust85Calc + dValue);
                        if (rate > 0D)
                            dValue = rate;
                        else if (rate < 0D)
                            dValue = trust85P - Trust85Calc;
                        else
                            dValue = 0D;
                        paymentsDt.Rows[i]["calculatedTrust85"] = dValue;
                        done = true;
                    }
                }
                Trust85Calc += dValue;
            }
            Trust85Paid = G1.RoundValue(Trust85Paid);
            totalTrust85 = G1.RoundValue(totalTrust85);
            totalTrust85 = 0D;
            for (int i = (paymentsDt.Rows.Count - 1); i >= 0; i--)
            {
                if (paymentsDt.Rows[i]["fill"].ObjToString().ToUpper() == "D")
                    continue;
                totalTrust85 += paymentsDt.Rows[i]["trust85P"].ObjToDouble();
            }
            totalTrust85 = G1.RoundDown(totalTrust85);
            Trust85Paid = G1.RoundDown(Trust85Paid);
            Trust85Calc = G1.RoundDown(Trust85Calc);

            trust85P = contractValue * 0.85D;
            if (trustPercent > 0D)
            {
                trust85P = contractValue * (trustPercent / 100D);
            }
            if (gotCemetery(workContract))
                trust85P = contractValue;


            double percentage = endingBalance / trust85P;
            string str = percentage.ToString("###.00%");
            lblTrust85.Text = "Trust85P:  $" + G1.ReformatMoney(endingBalance) + " of " + G1.ReformatMoney(trust85P) + " Expected   " + str;
            //            lblTrust85.Text = "";

            double endingBalanceSaved = endingBalance;
            if (endingBalance <= 0D)
            {
                double b = ReCalculateBalance(workContract, ref endingBalance);
            }

            Trust85Paid += endingBalance;

            trust85Actual = Trust85Paid;
            trust85Max = trust85P;

            percentage = Trust85Paid / trust85P;
            if (gotCemetery(workContract))
            {
                Trust85Paid = contractValue;
                percentage = 1D;
            }
            if (endingBalanceSaved <= 0D)
            {
                Trust85Paid = 0D;
                percentage = 0D;
            }
            str = percentage.ToString("###.00%");
            lblCalcTrust85.Text = "Trust85C: $" + G1.ReformatMoney(Trust85Paid) + " of " + G1.ReformatMoney(trust85P) + " Expected   " + str;
        }
        /****************************************************************************************/
        public static DateTime GetTrustLastPaid ( string contractNumber, ref double beginningBalance, ref double endingBalance)
        {
            DateTime payDate8 = DateTime.Now;
            beginningBalance = 0D;
            endingBalance = 0D;
            string cmd = "Select * from `trust2013r` where `contractNumber` = '" + contractNumber + "' ORDER BY `payDate8` DESC LIMIT 1";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count > 0 )
            {
                beginningBalance = dt.Rows[0]["beginningBalance"].ObjToDouble();
                endingBalance = dt.Rows[0]["endingBalance"].ObjToDouble();
                payDate8 = dt.Rows[0]["payDate8"].ObjToDateTime();
            }
            return payDate8;
        }
        /****************************************************************************************/
        public static DateTime GetInsuranceLastPaid(string payer, ref DateTime dueDate8, ref string payerContract, ref DateTime lapseDate8, ref DateTime reinstateDate8, ref string lapsed, ref DateTime deceasedDate )
        {
            DateTime payDate8 = DateTime.Now;
            dueDate8 = DateTime.Now;
            lapseDate8 = DateTime.Now;
            reinstateDate8 = DateTime.Now;
            deceasedDate = DateTime.Now;
            payerContract = "";
            lapsed = "";
            string cmd = "Select * from `payers` where `payer` = '" + payer + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                payDate8 = dt.Rows[0]["lastDatePaid8"].ObjToDateTime();
                dueDate8 = dt.Rows[0]["dueDate8"].ObjToDateTime();
                payerContract = dt.Rows[0]["contractNumber"].ObjToString();
                lapseDate8 = dt.Rows[0]["lapseDate8"].ObjToDateTime();
                reinstateDate8 = dt.Rows[0]["reinstateDate8"].ObjToDateTime();
                deceasedDate = dt.Rows[0]["deceasedDate"].ObjToDateTime();
                lapsed = dt.Rows[0]["lapsed"].ObjToString();
            }
            return payDate8;
        }
        /****************************************************************************************/
        public static double GetTrustEndingBalance(string contractNumber )
        {
            DateTime payDate8 = DateTime.Now;
            double beginningBalance = 0D;
            double endingBalance = 0D;
            string cmd = "Select * from `trust2013r` where `contractNumber` = '" + contractNumber + "' ORDER BY `payDate8` DESC LIMIT 1";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                beginningBalance = dt.Rows[0]["beginningBalance"].ObjToDouble();
                endingBalance = dt.Rows[0]["endingBalance"].ObjToDouble();
                payDate8 = dt.Rows[0]["payDate8"].ObjToDateTime();
            }
            return endingBalance;
        }
        /****************************************************************************************/
        private void LoadMainData()
        {
            bool insurance = false;
            string cmd = "Select * from `" + paymentsFile + "` where `contractNumber` = '" + workContract + "' order by `payDate8` DESC, `tmstamp` DESC;";
            if (paymentsFile.Trim().ToUpper() == "IPAYMENTS" && !String.IsNullOrWhiteSpace(workPayer))
            {
                insurance = true;
                string ccd = "SELECT * from `icustomers` where `payer`= '" + workPayer + "';";
                DataTable ddx = G1.get_db_data(ccd);
                if (ddx.Rows.Count > 0)
                {
                    string list = "";
                    for (int i = 0; i < ddx.Rows.Count; i++)
                    {
                        string contract = ddx.Rows[i]["contractNumber"].ObjToString().Trim();;
                        list += "'" + contract + "',";
                    }
                    list = list.TrimEnd(',');
                    list = "(" + list + ")";
                    cmd = "Select * from `" + paymentsFile + "` where `contractNumber` IN " + list + " order by `payDate8` DESC, `tmstamp` DESC;";
                }
            }
            //            string cmd = "Select * from `payments` where `contractNumber` = '" + workContract + "' order by `dueDate8` DESC;";
            //if ( isInsurance ( workContract )) // Orphans
            //{
            //    cmd = "Select * from `icustomers` where `contractNumber` = '" + workContract + "';";
            //    DataTable ddt = G1.get_db_data(cmd);
            //    if ( ddt.Rows.Count > 0 )
            //    {
            //        string payer = ddt.Rows[0]["payer"].ObjToString();
            //        string firstName = ddt.Rows[0]["firstName"].ObjToString();
            //        string lastName = ddt.Rows[0]["lastName"].ObjToString();
            //        cmd = "Select * from `ipayments` where `firstName` = '" + firstName + "' and `lastName` = '" + lastName + "' order by `payDate8` DESC, `tmstamp` DESC;";

            //    }
            //}
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("balance", Type.GetType("System.Double"));
            dt.Columns.Add("debit", Type.GetType("System.Double"));
            dt.Columns.Add("credit", Type.GetType("System.Double"));
            dt.Columns.Add("prince", Type.GetType("System.Double"));
            dt.Columns.Add("nextDueDate");
            dt.Columns.Add("creditBalance", Type.GetType("System.Double"));
            dt.Columns.Add("reason");
            dt.Columns.Add("NumPayments", Type.GetType("System.Double"));
            DateTime dueDate = DateTime.Now;
            DateTime payDate = DateTime.Now;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dueDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                if (dueDate.Year.ToString("D4").IndexOf("000") >= 0)
                    dt.Rows[i]["dueDate8"] = dt.Rows[i]["payDate8"];
            }

            //DataView tempview = dt.DefaultView;
            //tempview.Sort = "dueDate8 desc";
            ////            tempview.Sort = "loc asc, agentName asc";
            //dt = tempview.ToTable();

            double sBalance = startBalance;
            string status = "";
            bool deleted = false;
            double NumPayments = 0D;
            double numMonthPaid = 0D;
            double payment = 0D;
            double downPayment = 0D;
            double debit = 0D;
            double credit = 0D;
            double interest = 0D;
            double ccFee = 0D;
            double principal = 0D;
            double balance = 0D;
            string reason = "";
            string edited = "";

            DateTime insPayDate8 = DateTime.Now;
            DateTime insDueDate8 = DateTime.Now;

            DateTime pDate = DateTime.Now;
            DateTime dDate = DateTime.Now;
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                //payment = dt.Rows[i]["paymentAmount"].ObjToString().ObjToDouble();
                payment = getPayment(dt, i);

                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                edited = dt.Rows[i]["edited"].ObjToString();
                if (payment == 0D)
                {
                    if (credit > 0D)
                        payment = credit;
                    else if (debit > 0D)
                        payment = debit;
                }
                NumPayments = 0D;
                if (ExpectedPayment > 0D)
                {
                    NumPayments = payment / ExpectedPayment;
                    if (debit > 0D)
                    {
                        NumPayments = NumPayments * -1D;
                        if (edited.ToUpper() == "REFUND")
                            NumPayments = 0D;
                    }

                    if (!String.IsNullOrWhiteSpace(workPayer))
                    {
                        pDate = dt.Rows[i]["payDate8"].ObjToDateTime();
                        dDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                        if ( policyDt == null )
                        {
                            cmd = "Select * from `policies` where `record` = '" + workPolicyRecord + "';";
                            policyDt = G1.get_db_data(cmd);
                        }
                        if ( payment == 8.91D )
                        {

                        }
                        if (pDate <= killSecNatDate)
                            continue;
                        if (pDate > killSecNatDate)
                            ExpectedPayment = GetInsuranceExpectedPayment (policyDt, pDate);
                        double months = CheckMonthsForInsurance(workContract, workPayer, ExpectedPayment, payment, pDate, dDate);
                        if ( months == 2D )
                        {
                        }
                        NumPayments = months;
                        if (edited.ToUpper() == "REFUND")
                            NumPayments = 0D;
                        //nextDueDate = dueDate.ObjToDateTime();
                        //int imonths = (int)months;
                        //nextDueDate = nextDueDate.AddMonths(imonths);
                    }
                }
                dt.Rows[i]["NumPayments"] = NumPayments;
                if (insurance)
                {
                    numMonthPaid = dt.Rows[i]["numMonthPaid"].ObjToDouble();
                    if ( numMonthPaid == 2D)
                    {
                    }
                    if (numMonthPaid != 0D)
                        dt.Rows[i]["NumPayments"] = numMonthPaid;
                }
                debit = dt.Rows[i]["debitAdjustment"].ObjToString().ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToString().ObjToDouble();
                status = dt.Rows[i]["fill"].ObjToString();
                if (status.ToUpper() == "D")
                    deleted = true;
                //if (payment == 0D && debit == 0D && credit == 0D)
                //    dt.Rows.RemoveAt(i);
            }
            if (deleted)
                chkLoadAll.Visible = true;
            string location = "";
            string userId = "";
            int imonths = 0;
            DateTime lastDueDate = DateTime.Now;
            DateTime nextDueDate = DateTime.Now;
            if (isInsurance(workContract) && dt.Rows.Count > 1)
            {
                bool dateFirst = true;
                for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                {
                    status = dt.Rows[i]["fill"].ObjToString();
                    //if (status.ToUpper() == "D" && !chkLoadAll.Checked)
                    //{
                    //    dt.Rows.RemoveAt(i);
                    //    continue;
                    //}
                    insPayDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();
                    insDueDate8 = dt.Rows[i]["dueDate8"].ObjToDateTime();
                    NumPayments = dt.Rows[i]["NumPayments"].ObjToDouble();
                    if (insPayDate8 == insDueDate8)
                        continue;
                    imonths = NumPayments.ObjToInt32();
                    if (dateFirst)
                    {
                        try
                        {
                            insPayDate8 = dt.Rows[i + 1]["dueDate8"].ObjToDateTime();
                            dateFirst = false;
                            lastDueDate = insPayDate8;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    insDueDate8 = lastDueDate.AddMonths(imonths);
                    if (!insurance)
                        dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(insDueDate8);
                    //else
                    //    dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(insDueDate8);
                    lastDueDate = insDueDate8;
                    nextDueDate = lastDueDate.AddMonths(imonths);
                }
                lblCDD.Text = "CDD " + nextDueDate.ToString("MM/dd/yyyy");
            }
            string depositNumber = "";
            string fill1 = "";

            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                status = dt.Rows[i]["fill"].ObjToString();
                //if (status.ToUpper() == "D" && !chkLoadAll.Checked)
                //{
                //    dt.Rows.RemoveAt(i);
                //    continue;
                //}
                location = dt.Rows[i]["location"].ObjToString();
                userId = dt.Rows[i]["userId"].ObjToString();

                //payment = dt.Rows[i]["paymentAmount"].ObjToString().ObjToDouble();
                payment = getPayment(dt, i);
                downPayment = getDownPayment(dt, i);

                debit = dt.Rows[i]["debitAdjustment"].ObjToString().ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToString().ObjToDouble();
                interest = dt.Rows[i]["interestPaid"].ObjToString().ObjToDouble();
                ccFee = dt.Rows[i]["ccFee"].ObjToString().ObjToDouble();
                edited = dt.Rows[i]["edited"].ObjToString();
                if ( credit > 0D )
                {
                }
                if (credit > 0D && payment == 0D && ccFee != 0D )
                    payment = credit - ccFee;
                else if (debit > 0D && payment == 0D)
                {
                    payment = debit + ccFee;
                    payment = payment * -1D;
                }
                else
                {
                    if (downPayment == 0D && payment == 0D && ccFee != 0D)
                        payment = ccFee;
                    else if (credit > 0D && payment == 0D)
                        payment = credit;
                }
                principal = payment - interest;
                balance = sBalance - principal + debit - credit;
                if (status.ToUpper() == "D")
                    balance = sBalance;
                reason = dt.Rows[i]["debitReason"].ObjToString() + " " + dt.Rows[i]["creditReason"].ObjToString();
                if (edited.ToUpper() == "MANUAL")
                    reason = "* " + userId + " " + reason;
                else if (edited.ToUpper() == "TRUSTADJ")
                    reason = "TA-" + userId + " " + reason;
                else if (edited.ToUpper() == "CEMETERY")
                    reason = "CE-" + userId + " " + reason;
                else
                {
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    fill1 = dt.Rows[i]["fill1"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(depositNumber))
                    {
                        if (depositNumber.Substring(0, 1).ToUpper() == "T")
                        {
                            if (fill1.ToUpper() == "TFBX")
                                reason = "TFBX-" + userId;
                            else
                                reason = "LKBX-" + userId;
                        }
                        else if (depositNumber.Substring(0, 1).ToUpper() == "A")
                            reason = "ACH-" + userId;
                    }
                }
                dt.Rows[i]["balance"] = balance;
                dt.Rows[i]["prince"] = G1.RoundValue(principal);
                dt.Rows[i]["debit"] = debit;
                dt.Rows[i]["credit"] = credit;
                dt.Rows[i]["reason"] = reason.Trim();
                sBalance = balance;
            }

            double downPay = GetDownPayment(workContract);

            GetTotals(dt, downPay);

            if (isInsurance(workContract))
            {
                LoadExpectedPremiums(dt, workPayer);
                //CalcInsuranceDueDates(dt, workPayer );
            }

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            gridBand2.Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;

            if (isInsurance(workContract))
                this.bandedGridColumn31.Caption = "Expected Premium";
        }
        /****************************************************************************************/
        private void CalcInsuranceDueDates ( DataTable dt, string payer )
        {
            string cmd = "Select * from `policies` where `payer` = '" + payer + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            DateTime payDate = DateTime.Now;
            DateTime dueDate = DateTime.Now;
            DateTime lastDueDate = DateTime.MinValue;
            double expectedPremium = 0D;
            double payment = 0D;
            double creditBalance = 0D;
            double runningCredit = 0D;
            double months = 0D;
            double remainder = 0D;

            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                payDate = dt.Rows[i]["payDate8"].ObjToDateTime();

                dueDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                if (lastDueDate == DateTime.MinValue)
                    lastDueDate = dueDate;

                if (payDate <= killSecNatDate)
                    continue;
                if (payDate >= secondDate)
                {
                    months = dt.Rows[i]["numPayments"].ObjToDouble();
                    if (months == 2D)
                    {
                    }
                }

                payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                expectedPremium = GetInsuranceExpectedPayment(dx, payDate);
                dt.Rows[i]["retained"] = expectedPremium;

                months = dt.Rows[i]["numPayments"].ObjToDouble();
                months = payment / expectedPremium;
                //dt.Rows[i]["numPayments"] = Convert.ToInt32(months);
                remainder = months % 1D;
                if (remainder != 0D)
                {

                    payment = expectedPremium * remainder;

                    creditBalance = payment;
                    creditBalance = G1.RoundValue(creditBalance);
                    dt.Rows[i]["creditBalance"] = creditBalance;
                    runningCredit += creditBalance;

                    if (runningCredit > expectedPremium)
                        runningCredit = runningCredit % expectedPremium;
                    G1.RoundValue(runningCredit);
                }
                dt.Rows[i]["runningCB"] = runningCredit;
                dt.Rows[i]["NumPayments"] = months;
            }
        }
        /****************************************************************************************/
        private void LoadMainData2()
        {
            bool insurance = false;
            string cmd = "Select * from `" + paymentsFile + "` where `contractNumber` = '" + workContract + "' order by `payDate8` DESC, `tmstamp` DESC;";
            if ( paymentsFile.Trim().ToUpper() == "IPAYMENTS" && !String.IsNullOrWhiteSpace(workPayer ))
            {
                insurance = true;
                string ccd = "SELECT * from `icustomers` where `payer`= '" + workPayer + "';";
                DataTable ddx = G1.get_db_data(ccd);
                if ( ddx.Rows.Count > 0 )
                {
                    string list = "";
                    for ( int i=0; i<ddx.Rows.Count; i++)
                    {
                        string contract = ddx.Rows[i]["contractNumber"].ObjToString().Trim();;
                        list += "'" + contract + "',";
                    }
                    list = list.TrimEnd(',');
                    list = "(" + list + ")";
                    cmd = "Select * from `" + paymentsFile + "` where `contractNumber` IN " + list + " order by `payDate8` DESC, `tmstamp` DESC;";
                }
            }
//            string cmd = "Select * from `payments` where `contractNumber` = '" + workContract + "' order by `dueDate8` DESC;";
            //if ( isInsurance ( workContract )) // Orphans
            //{
            //    cmd = "Select * from `icustomers` where `contractNumber` = '" + workContract + "';";
            //    DataTable ddt = G1.get_db_data(cmd);
            //    if ( ddt.Rows.Count > 0 )
            //    {
            //        string payer = ddt.Rows[0]["payer"].ObjToString();
            //        string firstName = ddt.Rows[0]["firstName"].ObjToString();
            //        string lastName = ddt.Rows[0]["lastName"].ObjToString();
            //        cmd = "Select * from `ipayments` where `firstName` = '" + firstName + "' and `lastName` = '" + lastName + "' order by `payDate8` DESC, `tmstamp` DESC;";

            //    }
            //}
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("balance", Type.GetType("System.Double"));
            dt.Columns.Add("debit", Type.GetType("System.Double"));
            dt.Columns.Add("credit", Type.GetType("System.Double"));
            dt.Columns.Add("prince", Type.GetType("System.Double"));
            dt.Columns.Add("nextDueDate");
            dt.Columns.Add("creditBalance", Type.GetType("System.Double"));
            dt.Columns.Add("reason");
            dt.Columns.Add("NumPayments", Type.GetType("System.Double"));
            DateTime dueDate = DateTime.Now;
            DateTime payDate = DateTime.Now;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                dueDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                if (dueDate.Year.ToString("D4").IndexOf("000") >= 0)
                    dt.Rows[i]["dueDate8"] = dt.Rows[i]["payDate8"];
            }

            //DataView tempview = dt.DefaultView;
            //tempview.Sort = "dueDate8 desc";
            ////            tempview.Sort = "loc asc, agentName asc";
            //dt = tempview.ToTable();

            double sBalance = startBalance;
            string status = "";
            bool deleted = false;
            double NumPayments = 0D;
            double numMonthPaid = 0D;
            double payment = 0D;
            double debit = 0D;
            double credit = 0D;
            double interest = 0D;
            double principal = 0D;
            double balance = 0D;
            string reason = "";
            string edited = "";
            double expected = 0D;
            double remainder = 0D;
            double creditBalance = 0D;
            double runningCredit = 0D;

            cmd = "Select * from `policies` where `payer` = '" + workPayer + "';";
            DataTable policyDx = G1.get_db_data(cmd);


            DateTime insPayDate8 = DateTime.Now;
            DateTime insDueDate8 = DateTime.Now;

            DateTime pDate = DateTime.Now;
            DateTime dDate = DateTime.Now;
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                //payment = dt.Rows[i]["paymentAmount"].ObjToString().ObjToDouble();
                payment = getPayment(dt, i);

                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                if ( payment == 0D)
                {
                    if (credit > 0D)
                        payment = credit;
                    else if (debit > 0D)
                        payment = debit;
                }
                NumPayments = 0D;
                if (ExpectedPayment > 0D)
                {
                    NumPayments = payment / ExpectedPayment;
                    if (debit > 0D)
                        NumPayments = NumPayments * -1D;
                    if ( !String.IsNullOrWhiteSpace(workPayer))
                    {
                        pDate = dt.Rows[i]["payDate8"].ObjToDateTime();
                        dDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                        if (pDate > killSecNatDate)
                            dt.Rows[i]["numMonthPaid"] = 0D;
                        double months = 0D;
                        payment += runningCredit;
                        months = CheckMonthsForInsuranceNew(workContract, workPayer, ExpectedPayment, payment, pDate, dDate );


                        if (pDate > killSecNatDate)
                        {
                            expected = GetInsuranceExpectedPayment(policyDx, pDate);
                            remainder = months % 1D;
                            if (remainder != 0D)
                            {

                                payment = expected * remainder;

                                creditBalance = payment;
                                creditBalance = G1.RoundValue(creditBalance);
                                dt.Rows[i]["creditBalance"] = creditBalance;
                                runningCredit += creditBalance;
                                G1.RoundValue(runningCredit);
                                if (runningCredit > expected)
                                    runningCredit = runningCredit % expected;
                            }
                        }


                        NumPayments = months;
                        //nextDueDate = dueDate.ObjToDateTime();
                        //int imonths = (int)months;
                        //nextDueDate = nextDueDate.AddMonths(imonths);
                    }
                }
                dt.Rows[i]["NumPayments"] = NumPayments;
                if ( insurance )
                {
                    numMonthPaid = dt.Rows[i]["numMonthPaid"].ObjToDouble();
                    if (numMonthPaid != 0D)
                        dt.Rows[i]["NumPayments"] = numMonthPaid;
                }
                debit = dt.Rows[i]["debitAdjustment"].ObjToString().ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToString().ObjToDouble();
                status = dt.Rows[i]["fill"].ObjToString();
                if (status.ToUpper() == "D")
                    deleted = true;
                //if (payment == 0D && debit == 0D && credit == 0D)
                //    dt.Rows.RemoveAt(i);
            }
            if (deleted)
                chkLoadAll.Visible = true;
            string location = "";
            string userId = "";
            int imonths = 0;
            DateTime lastDueDate = DateTime.Now;
            DateTime nextDueDate = DateTime.Now;
            if ( isInsurance ( workContract ) && dt.Rows.Count > 1 )
            {
                bool dateFirst = true;
                for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                {
                    status = dt.Rows[i]["fill"].ObjToString();
                    //if (status.ToUpper() == "D" && !chkLoadAll.Checked)
                    //{
                    //    dt.Rows.RemoveAt(i);
                    //    continue;
                    //}
                    insPayDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();
                    insDueDate8 = dt.Rows[i]["dueDate8"].ObjToDateTime();
                    NumPayments = dt.Rows[i]["NumPayments"].ObjToDouble();
                    if (insPayDate8 == insDueDate8)
                        continue;
                    imonths = NumPayments.ObjToInt32();
                    if (dateFirst)
                    {
                        try
                        {
                            insPayDate8 = dt.Rows[i + 1]["dueDate8"].ObjToDateTime();
                            dateFirst = false;
                            lastDueDate = insPayDate8;
                        }
                        catch ( Exception ex)
                        {

                        }
                    }
                    insDueDate8 = lastDueDate.AddMonths(imonths);
                    if ( !insurance )
                        dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(insDueDate8);
                    lastDueDate = insDueDate8;
                    nextDueDate = lastDueDate.AddMonths(imonths);
                }
                lblCDD.Text = "CDD " + nextDueDate.ToString("MM/dd/yyyy");
            }
            string depositNumber = "";
            string fill1 = "";

            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                status = dt.Rows[i]["fill"].ObjToString();
                //if (status.ToUpper() == "D" && !chkLoadAll.Checked)
                //{
                //    dt.Rows.RemoveAt(i);
                //    continue;
                //}
                location = dt.Rows[i]["location"].ObjToString();
                userId = dt.Rows[i]["userId"].ObjToString();

                //payment = dt.Rows[i]["paymentAmount"].ObjToString().ObjToDouble();
                payment = getPayment(dt, i);

                debit = dt.Rows[i]["debitAdjustment"].ObjToString().ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToString().ObjToDouble();
                interest = dt.Rows[i]["interestPaid"].ObjToString().ObjToDouble();
                edited = dt.Rows[i]["edited"].ObjToString();
                principal = payment - interest;
                balance = sBalance - principal + debit - credit;
                if (status.ToUpper() == "D")
                    balance = sBalance;
                reason = dt.Rows[i]["debitReason"].ObjToString() + " " + dt.Rows[i]["creditReason"].ObjToString();
                if (edited.ToUpper() == "MANUAL")
                    reason = "* " + userId + " " + reason;
                else if ( edited.ToUpper() == "TRUSTADJ" )
                    reason = "TA-" + userId + " " + reason;
                else if (edited.ToUpper() == "CEMETERY")
                    reason = "CE-" + userId + " " + reason;
                else
                {
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    fill1 = dt.Rows[i]["fill1"].ObjToString();
                    if ( !String.IsNullOrWhiteSpace ( depositNumber))
                    {
                        if ( depositNumber.Substring(0,1).ToUpper() == "T")
                        {
                            if (fill1.ToUpper() == "TFBX")
                                reason = "TFBX-" + userId;
                            else
                                reason = "LKBX-" + userId;
                        }
                        else if(depositNumber.Substring(0, 1).ToUpper() == "A")
                            reason = "ACH-" + userId;
                    }
                }
                dt.Rows[i]["balance"] = balance;
                dt.Rows[i]["prince"] = G1.RoundValue(principal);
                dt.Rows[i]["debit"] = debit;
                dt.Rows[i]["credit"] = credit;
                dt.Rows[i]["reason"] = reason.Trim();
                sBalance = balance;
            }

            double downPay = GetDownPayment(workContract);

            GetTotals(dt, downPay );
            if ( isInsurance ( workContract ) )
            {

                LoadExpectedPremiums(dt, workPayer);

                double months = 0D;
                imonths = 0;
                DateTime dueDate8 = DateTime.Now;
                DateTime datePaid = DateTime.Now;
                bool first = true;

                for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                {
                    datePaid = dt.Rows[i]["payDate8"].ObjToDateTime();
                    if ( datePaid < killSecNatDate )
                    {
                        dueDate8 = dt.Rows[i]["dueDate8"].ObjToDateTime();
                        months = dt.Rows[i]["NumPayments"].ObjToDouble();
                        continue;
                    }
                    if ( first )
                    {
                        first = false;
                        imonths = Convert.ToInt32((months));

                        //dueDate8 = dueDate8.AddMonths(imonths);
                        //dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(dueDate8);
                        dueDate8 = dt.Rows[i]["dueDate8"].ObjToDateTime();
                        months = dt.Rows[i]["NumPayments"].ObjToDouble();
                        months = G1.RoundValue(months);
                    }

                    else
                    {
                        imonths = Convert.ToInt32((months));
                        dueDate8 = dueDate8.AddMonths(imonths);

                        dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(dueDate8);
                        months = dt.Rows[i]["NumPayments"].ObjToDouble();
                        months = G1.RoundValue(months);
                    }
                }
            }
            //if (isInsurance(workContract))
            //{
            //    LoadExpectedPremiums(dt, workPayer);
            //}

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            gridBand2.Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;

            if (isInsurance(workContract))
                this.bandedGridColumn31.Caption = "Expected Premium";
        }
        /****************************************************************************************/
        public static void CleanupVisibility (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain )
        {
            try
            {
                gridMain.Columns["paymentAmount"].Caption = "Old Payment";
                gridMain.Columns["paymentAmount"].Visible = false;
                gridMain.Columns["downPayment"].Caption = "Old Down Payment";
                gridMain.Columns["downPayment"].Visible = false;
                gridMain.Columns["ap"].Caption = "Payment";
                gridMain.Columns["dpp"].Caption = "Down Payment";
            }
            catch ( Exception ex )
            {
            }
        }
        /****************************************************************************************/
        public static void AddAP ( DataTable dt, string dpColumn = "" )
        {
            if (G1.get_column_number(dt, "ap") < 0)
                dt.Columns.Add("ap", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "dpp") < 0)
                dt.Columns.Add("dpp", Type.GetType("System.Double"));

            double ap = 0D;
            double paymentAmount = 0D;
            double downPayment = 0D;
            double ccFee = 0D;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                ccFee = dt.Rows[i]["ccFee"].ObjToDouble();
                if (ccFee > 0D)
                {
                    downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                    if (!String.IsNullOrWhiteSpace(dpColumn))
                        downPayment = dt.Rows[i][dpColumn].ObjToDouble();
                    if (downPayment > 0)
                    {
                        downPayment += ccFee;
                        dt.Rows[i]["dpp"] = downPayment;
                        dt.Rows[i]["ap"] = 0D;
                    }
                    else
                    {
                        paymentAmount = dt.Rows[i]["paymentAmount"].ObjToDouble();
                        dt.Rows[i]["ap"] = paymentAmount;
                        paymentAmount -= ccFee;
                        dt.Rows[i]["paymentAmount"] = paymentAmount;
                        dt.Rows[i]["dpp"] = 0D;
                    }
                }
                else
                {
                    paymentAmount = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    dt.Rows[i]["ap"] = paymentAmount;
                    downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                    dt.Rows[i]["dpp"] = downPayment;
                }
            }
        }
        /****************************************************************************************/
        private void GetTotals ( DataTable dt, double downPay )
        {
            double paymentAmount = 0D;
            double payment = 0D;
            double interest = 0D;
            double debit = 0D;
            double credit = 0D;
            double downPayment = 0D;
            string status = "";
            string tdp = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                status = dt.Rows[i]["fill"].ObjToString();
                if (status.ToUpper() == "D" )
                    continue;

                paymentAmount = getPayment(dt, i);
                //payment += dt.Rows[i]["paymentAmount"].ObjToDouble();
                payment += paymentAmount;

                interest += dt.Rows[i]["interestPaid"].ObjToDouble();
                debit += dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit += dt.Rows[i]["creditAdjustment"].ObjToDouble();
                //downPayment += dt.Rows[i]["downPayment"].ObjToDouble();
                tdp = dt.Rows[i]["tdp"].ObjToString();
                if (!String.IsNullOrWhiteSpace(tdp))
                    dt.Rows[i]["depositNumber"] = tdp;
            }
            downPayment = downPay;
            double paid = payment + downPayment - interest + credit - debit;
            paid = G1.RoundValue(paid);
            lblTotalPaid.Text = "Total Paid :$" + G1.ReformatMoney(paid);
        }
        /****************************************************************************************/
        public static string GetLastPaymentDate ( string contractNumber)
        {
            string lastDate = "";
            if (String.IsNullOrWhiteSpace(contractNumber))
                return lastDate;

            string contractFile = "contracts";
            string customerFile = "customers";
            string paymentsFile = "payments";
            if ( isInsurance ( contractNumber ))
            {
                contractFile = "icontracts";
                customerFile = "icustomers";
                paymentsFile = "ipayments";
            }

            string cmd = "Select * from `" + paymentsFile + "` where `contractNumber` = '" + contractNumber + "' ORDER by `payDate8` DESC, `tmstamp` DESC;";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                cmd = "Select * from `" + contractFile + "` where `contractNumber` = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    return lastDate;
                DateTime iDate = GetIssueDate(dx.Rows[0]["issueDate8"].ObjToDateTime(), contractNumber, dx);
                lastDate = iDate.ToString("MM/dd/yyyy");
            }
            else
            {
                lastDate = dx.Rows[0]["payDate8"].ObjToString();
                DateTime dolp = DateTime.MinValue;
                DailyHistory.getDOLP(dx, ref dolp ); // Had to avoid Credit being (DOLP)
                if (dolp.Year > 1000)
                    lastDate = dolp.ToString("MM/dd/yyyy");
            }

            return lastDate;
        }
        /****************************************************************************************/
        public static int GetDaysSinceLastPayment(string contractNumber, DateTime dueDate )
        {
            string date = dueDate.Month.ToString("D2") + "/" + dueDate.Day.ToString("D2") + "/" + dueDate.Year.ToString("D4");
            int days = GetDaysSinceLastPayment(contractNumber, date);
            return days;
        }
        /****************************************************************************************/
        public static int GetDaysSinceLastPayment ( string contractNumber, string currentDate = "" )
        {
            int days = 0;
            string last = "";
            if (String.IsNullOrWhiteSpace(contractNumber))
                return days;

            last = GetLastPaymentDate(contractNumber);
            if (!G1.validate_date(last))
                return days;
            DateTime lastDate = last.ObjToDateTime();

            DateTime datePaid = DateTime.Now;
            if ( !String.IsNullOrWhiteSpace(currentDate))
            {
                if (G1.validate_date(currentDate))
                    datePaid = currentDate.ObjToDateTime();
            }
            TimeSpan ts = datePaid - lastDate;
            days = ts.Days;
            return days;
        }
        /****************************************************************************************/
        public static double GetFinanceValue(string contractNumber)
        {
            double totalFinanced = 0D;
            if (String.IsNullOrWhiteSpace(contractNumber))
                return 0D;
            if (isInsurance ( contractNumber))
                return 0D;
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return 0D;
            totalFinanced = GetFinanceValue(dx.Rows[0]);
            return totalFinanced;
        }
        /****************************************************************************************/
        public static double GetFinanceValue(DataRow dRow)
        {
            double totalFinanced = 0D;
            if (dRow == null)
                return 0D;
            try
            {
                double serviceTotal = dRow["serviceTotal"].ObjToDouble();
                double merchandiseTotal = dRow["merchandiseTotal"].ObjToDouble();
                double allowMerchandise = dRow["allowMerchandise"].ObjToDouble();
                double allowInsurance = dRow["allowInsurance"].ObjToDouble();
                double downpayment = dRow["downPayment"].ObjToDouble();
                if ( downpayment == 0D)
                {
                    string contractNumber = dRow["contractNumber"].ObjToString();
                    string cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' AND `downPayment` > 0.00;";
                    DataTable dx = G1.get_db_data(cmd);
                    if ( dx.Rows.Count > 0 )
                        downpayment = dx.Rows[0]["downpayment"].ObjToDouble();
                }
                double cashAdvance = dRow["cashAdvance"].ObjToDouble();
//                totalContract = serviceTotal + merchandiseTotal + allowMerchandise + allowInsurance - downpayment;
                totalFinanced = serviceTotal + merchandiseTotal - allowMerchandise - allowInsurance - downpayment + cashAdvance;
            }
            catch (Exception ex)
            {

            }
            return totalFinanced;
        }
        /****************************************************************************************/
        public static double GetContractValue(string contractNumber)
        {
            double totalContract = 0D;
            if (String.IsNullOrWhiteSpace(contractNumber))
                return 0D;
            if (isInsurance ( contractNumber))
                return 0D;
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return 0D;
            totalContract = GetContractValue(dx.Rows[0]);
            return totalContract;
        }
        /****************************************************************************************/
        public static double GetMonthlyPayment(string contractNumber)
        {
            double amtOfMonthlyPayt = 0D;
            if (String.IsNullOrWhiteSpace(contractNumber))
                return 0D;
            if (isInsurance(contractNumber))
                return 0D;
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return 0D;
            amtOfMonthlyPayt = dx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
            return amtOfMonthlyPayt;
        }
        /****************************************************************************************/
        public static double GetOriginalDownPayment ( DataRow dRow )
        {
            double downPayment = dRow["downPayment"].ObjToDouble();
            if ( downPayment <= 0D)
            {
                double payment = 0D;
                string fill = "";

                string contractNumber = dRow["contractNumber"].ObjToString();
                string cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' ORDER BY `payDate8` ASC;";
                DataTable dt = G1.get_db_data(cmd);
                for ( int i=0; i<dt.Rows.Count; i++)
                {
                    fill = dt.Rows[i]["fill"].ObjToString();
                    if (fill.ToUpper() == "D")
                        continue;
                   
                    payment = dt.Rows[i]["downPayment"].ObjToDouble();
                    if ( payment > 0D)
                    {
                        downPayment = payment;
                        break;
                    }
                }
            }
            return downPayment;
        }
        /****************************************************************************************/
        public static double GetDownPayment(string contractNumber)
        {
            double downPayment = 0D;
            if (String.IsNullOrWhiteSpace(contractNumber))
                return 0D;
            string contractFile = "contracts";
            if ( isInsurance ( contractNumber))
                contractFile = "icontracts";
            string cmd = "Select * from `" + contractFile + "` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return 0D;
            downPayment = dx.Rows[0]["downPayment"].ObjToDouble();
            if (downPayment == 0D)
                downPayment = GetDownPaymentFromPayments(contractNumber);
            return downPayment;
        }
        /****************************************************************************************/
        public static bool GetDownPaymentFromPayments(string contractNumber, ref double downPayment, ref DateTime downPaymentDate, ref double trust85P, ref double trust100P, ref double ccFee, ref string record2, ref string depositNumber )
        {
            downPayment = 0D;
            ccFee = 0D;
            trust85P = 0D;
            trust100P = 0D;
            record2 = "";
            depositNumber = "";
            bool rtn = false;
            string cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' AND `downPayment` > '0.00';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                downPayment = dx.Rows[0]["downPayment"].ObjToDouble();

                ccFee = dx.Rows[0]["ccFee"].ObjToDouble();

                downPayment += ccFee;

                downPaymentDate = dx.Rows[0]["payDate8"].ObjToDateTime();
                trust85P = dx.Rows[0]["trust85P"].ObjToDouble();
                trust100P = dx.Rows[0]["trust100P"].ObjToDouble();
                depositNumber = dx.Rows[0]["depositNumber"].ObjToString();
                record2 = dx.Rows[0]["record"].ObjToString();
                rtn = true;
            }
            return rtn;
        }
        /****************************************************************************************/
        public static double GetDownPaymentFromPayments ( string contractNumber)
        {
            double downPayment = 0D;
            string cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' AND `downPayment` > '0.00';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
                downPayment = dx.Rows[0]["downPayment"].ObjToDouble();
            return downPayment;
        }
        /****************************************************************************************/
        public static double GetContractValuePlus ( string contractNumber)
        {
            double totalContract = 0D;
            if (String.IsNullOrWhiteSpace(contractNumber))
                return 0D;
            if (isInsurance(contractNumber))
                return 0D;
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return 0D;
            totalContract = GetContractValuePlus(dx.Rows[0]);
            return totalContract;
        }
        /****************************************************************************************/
        public static double GetContractValueMinus(DataRow dRow)
        {
            double totalContract = 0D;
            if (dRow == null)
                return 0D;
            try
            {
                double serviceTotal = dRow["serviceTotal"].ObjToDouble();
                double merchandiseTotal = dRow["merchandiseTotal"].ObjToDouble();
                double allowMerchandise = dRow["allowMerchandise"].ObjToDouble();
                double allowInsurance = dRow["allowInsurance"].ObjToDouble();
                double downpayment = dRow["downPayment"].ObjToDouble();
                double cashAdvance = dRow["cashAdvance"].ObjToDouble();

                totalContract = serviceTotal + merchandiseTotal;
            }
            catch (Exception ex)
            {

            }
            return totalContract;
        }
        /****************************************************************************************/
        public static double GetContractValuePlus(DataRow dRow, bool lessAllow = false )
        {
            double totalContract = 0D;
            if (dRow == null)
                return 0D;
            try
            {
                double serviceTotal = dRow["serviceTotal"].ObjToDouble();
                double merchandiseTotal = dRow["merchandiseTotal"].ObjToDouble();
                double allowMerchandise = dRow["allowMerchandise"].ObjToDouble();
                double allowInsurance = dRow["allowInsurance"].ObjToDouble();
                double downpayment = dRow["downPayment"].ObjToDouble();
                double cashAdvance = dRow["cashAdvance"].ObjToDouble();

//                totalContract = serviceTotal + merchandiseTotal - allowMerchandise - allowInsurance - cashAdvance;
                if ( lessAllow )
                    totalContract = serviceTotal + merchandiseTotal + cashAdvance;
                else
                    totalContract = serviceTotal + merchandiseTotal - allowMerchandise - allowInsurance + cashAdvance;
            }
            catch (Exception ex)
            {

            }
            return totalContract;
        }
        /****************************************************************************************/
        public static double GetTrust85 ( DataRow dRow)
        {
            double contractValue = GetContractValue(dRow);
            double trust85 = contractValue * 0.85D;
            //double downPayment = GetOriginalDownPayment(dRow);
            //double numPayments = dRow["numberOfPayments"].ObjToDouble();
            //if ( numPayments > 0D)
            //{
            //    double trust100 = (contractValue - downPayment);
            //    trust85 = trust100 * 0.85D;
            //}
            return trust85;
        }
        /****************************************************************************************/
        public static double GetContractValue ( DataRow dRow )
        {
            double totalContract = 0D;
            if (dRow == null)
                return 0D;
            try
            {
                double serviceTotal = dRow["serviceTotal"].ObjToDouble();
                double merchandiseTotal = dRow["merchandiseTotal"].ObjToDouble();
                double allowMerchandise = dRow["allowMerchandise"].ObjToDouble();
                double allowInsurance = dRow["allowInsurance"].ObjToDouble();
                double downpayment = dRow["downPayment"].ObjToDouble();
                double cashAdvance = dRow["cashAdvance"].ObjToDouble();

//                totalContract = serviceTotal + merchandiseTotal - allowMerchandise - allowInsurance - cashAdvance;
                totalContract = serviceTotal + merchandiseTotal - allowMerchandise - allowInsurance;
            }
            catch ( Exception ex)
            {

            }
            return totalContract;
        }
        /****************************************************************************************/
        public static bool IsFundedByInsurance(DataRow dRow)
        {
            bool rv = false;
            double totalContract = 0D;
            if (dRow == null)
                return false;
            try
            {
                string contract = dRow["contractNumber"].ObjToString();
                if ( contract == "L21129L")
                {
                }
                double serviceTotal = dRow["serviceTotal"].ObjToDouble();
                double merchandiseTotal = dRow["merchandiseTotal"].ObjToDouble();
                double allowMerchandise = dRow["allowMerchandise"].ObjToDouble();
                double allowInsurance = dRow["allowInsurance"].ObjToDouble();
                double downpayment = dRow["downPayment"].ObjToDouble();
                double cashAdvance = dRow["cashAdvance"].ObjToDouble();

                totalContract = serviceTotal + merchandiseTotal - allowMerchandise - allowInsurance - cashAdvance;
                if (allowInsurance > 0D && (serviceTotal > 0D || merchandiseTotal > 0D))
                {
                    if (allowInsurance >= (serviceTotal + merchandiseTotal))
                    {
                        rv = true;
                        if (downpayment > 0D)
                        {
                            rv = false;
                            dRow["contractValue"] = downpayment;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return rv;
        }
        /****************************************************************************************/
        public static int GetNumPayments(double payment, double expectedPayment )
        {
            int months = (int)(payment / expectedPayment);
            return months;
        }
        /****************************************************************************************/
        public static bool GetNextDueDate ( string contractNumber, DateTime currentDueDate, ref DateTime nextDueDate )
        {
            nextDueDate = DateTime.Now;
            if (String.IsNullOrWhiteSpace(contractNumber))
                return false;
            string contractFile = "contracts";
            if ( isInsurance ( contractNumber))
                contractFile = "icontracts";
            string cmd = "Select * from `" + contractFile + "` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return false;
            double payment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
            if (payment <= 0D)
                return false;
            double balanceDue = dx.Rows[0]["balanceDue"].ObjToDouble();
            int months = (int)(balanceDue / payment);
            nextDueDate = currentDueDate;
            if ( months > 0 )
                nextDueDate = currentDueDate.AddMonths(months);
            return true;
        }
        /****************************************************************************************/
        private void picAgreement_Click(object sender, EventArgs e)
        {
            string filename = this.picAgreement.Tag.ObjToString();
            if (!String.IsNullOrWhiteSpace(filename))
            {
                string title = "Agreement for (" + workContract + ") ";
                string cmd = "Select * from `" + customersFile + "` where `contractNumber` = '" + workContract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    filename = dx.Rows[0]["agreementFile"].ObjToString();
                    string firstName = dx.Rows[0]["firstName"].ObjToString();
                    string lastName = dx.Rows[0]["lastName"].ObjToString();
                    title = "Agreement for (" + workContract + ") " + firstName + " " + lastName;
                    string record = this.picAgreement.Tag.ObjToString();
                    if (record != "-1")
                    {
                        //CustomerDetails.ShowPDfImage(record, title, title, workContract);
                        Customers.ShowPDfImage(record, title, filename);
                    }
                }
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

            printingSystem1.ExportOptions.Xlsx.TextExportMode = DevExpress.XtraPrinting.TextExportMode.Text;

            //Printer.setupPrinterMargins(50, 50, 150, 50);
            Printer.setupPrinterMargins(10, 10, 150, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;
            if ( !workPDF )
            {
                //printableComponentLink1.ShowPreview();
                allowCalcRowHeight = true;
                gridMain.Columns["notes"].ColumnEdit = this.repositoryItemMemoEdit1;
                gridMain.RefreshEditor(true);
                dgv.Refresh();
            }

            printableComponentLink1.CreateDocument();
            bool test = true;
            //if ( test )
            //{
            //    string filename = "c:/ragware/adam.xlsx";
            //    try
            //    {
            //        printableComponentLink1.ExportToXlsx(filename);
            //    }
            //    catch ( Exception ex)
            //    {

            //    }
            //    return;
            //}

            if (workPDF)
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
            {
                //printableComponentLink1.ShowPreview();
                printableComponentLink1.ShowPreviewDialog();
                allowCalcRowHeight = false;
                gridMain.Columns["notes"].ColumnEdit = null;
                gridMain.RefreshData();
                gridMain.RefreshEditor(true);
                dgv.Refresh();
            }
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

            Printer.setupPrinterMargins(10, 10, 150, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            allowCalcRowHeight = true;
            gridMain.Columns["notes"].ColumnEdit = this.repositoryItemMemoEdit1;
            gridMain.RefreshEditor(true);
            dgv.Refresh();

            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();

            allowCalcRowHeight = false;
            gridMain.Columns["notes"].ColumnEdit = null;
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void printableComponentLink1_BeforeCreateAreas(object sender, EventArgs e)
        {
            //DateTime date = DateTime.Now;
            //string dateStr = "";
            DataTable dt = (DataTable)dgv.DataSource;
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            //if (G1.get_column_number(dt, "dueDate8Print") < 0)
            //    dt.Columns.Add("dueDate8Print");
            //if (G1.get_column_number(dt, "payDate8Print") < 0)
            //    dt.Columns.Add("payDate8Print");
            //for ( int i=0; i<dt.Rows.Count; i++)
            //{
            //    date = dt.Rows[i]["dueDate8"].ObjToDateTime();
            //    dateStr = date.ToString("MM/dd/yyyy");
            //    dt.Rows[i]["dueDate8Print"] = dateStr;
            //    date = dt.Rows[i]["payDate8"].ObjToDateTime();
            //    dateStr = date.ToString("MM/dd/yyyy");
            //    dt.Rows[i]["payDate8Print"] = dateStr;
            //}
            //gridMain.Columns["dueDate8"].Visible = false;
            //gridMain.Columns["payDate8"].Visible = false;
            //gridMain.Columns["dueDate8Print"].Visible = true;
            //gridMain.Columns["payDate8Print"].Visible = true;
        }
        /***********************************************************************************************/
        private void printableComponentLink1_AfterCreateAreas(object sender, EventArgs e)
        {
            //gridMain.Columns["dueDate8"].Visible = true;
            //gridMain.Columns["payDate8"].Visible = true;
            //gridMain.Columns["dueDate8Print"].Visible = false;
            //gridMain.Columns["payDate8Print"].Visible = false;
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

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 1, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 1, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 3, 2, 2, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Regular );
            Printer.DrawQuad(6, 3, 2, 3, "Daily Payment Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            font = new Font("Ariel", 7, FontStyle.Regular);
            Printer.DrawQuad(1, 5, 4, 1, "Contract :" + workContract, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(3, 5, 3, 1, "Name :" + workName, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            if ( isInsurance ( workContract))
                Printer.DrawQuad( 11, 5, 3, 1, "Payer :" + workPayer, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.DrawQuadBorder(1, 1, 12, 5, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 6, BorderSide.Right, 1, Color.Black);

            //Printer.SetQuadSize(24, 24);
            //font = new Font("Ariel", 7, FontStyle.Regular);
            Printer.DrawQuad(1, 6, 3, 1, labelSerTot.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(1, 7, 3, 1, labelMerTot.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(1, 8, 3, 1, labDownPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(1, 9, 3, 1, labRemainingBalance.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(1, 10, 3, 1, lblDueDate.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(1, 11, 3, 1, lblAPR.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.DrawQuad(3, 6, 3, 1, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(3, 7, 3, 1, lblIssueDate.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(3, 8, 3, 1, lblNumPayments.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(3, 9, 3, 1, lblTotalPaid.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            if (String.IsNullOrWhiteSpace(workPayer))
            {
                Printer.DrawQuad(3, 10, 3, 1, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                Printer.DrawQuad(3, 11, 3, 1, lblCalcTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            }

            Printer.DrawQuad(5, 6, 3, 1, labBalDue.Text + " " + labBalanceDue.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            if ( !String.IsNullOrWhiteSpace ( lblDeadDate.Text))
                Printer.DrawQuad(5, 7, 3, 1, lblDeadDate.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            if (!String.IsNullOrWhiteSpace(lblServiceId.Text))
                Printer.DrawQuad(5, 8, 3, 1, lblServiceId.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            if (String.IsNullOrWhiteSpace(workPayer))
                Printer.DrawQuad(5, 9, 3, 1, lblContractValue.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);



            //Printer.DrawQuadBorder(1, 1, 12, 11, BorderSide.All, 1, Color.Black);
            ////            Printer.DrawQuadTicks();
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
            else if (e.Column.FieldName.ToUpper() == "DUEDATE8")
            {
                if (e.RowHandle >= 0)
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                }
            }
            else if (e.Column.FieldName.ToUpper() == "PAYDATE8")
            {
                if (e.RowHandle >= 0)
                {
                    if (!riles)
                    {
                        DateTime date = e.DisplayText.ObjToDateTime();
                        e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                        if (date < as400Date && !isInsurance(workContract))
                        {
                            e.Appearance.BackColor = Color.LightGreen;
                            e.Appearance.ForeColor = Color.Black;
                        }
                        else if (date <= killSecNatDate && isInsurance(workContract))
                        {
                            e.Appearance.BackColor = Color.LightGreen;
                            e.Appearance.ForeColor = Color.Black;
                        }
                    }
                    else
                    {
                        DateTime date = e.DisplayText.ObjToDateTime();
                        e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                        if (date < rilesDate )
                        {
                            e.Appearance.BackColor = Color.LightGreen;
                            e.Appearance.ForeColor = Color.Black;
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
        private ManualPayment manualForm = null;
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if ( 1 == 1)
            {
                string preference = G1.getPreference(LoginForm.username, "DailyHistory", "Add Manual Payment", true );
                if (G1.RobbyServer)
                    preference = "YES";
                if (preference != "YES")
                    return;

                DataTable dt = (DataTable)dgv.DataSource;
                manualForm = new ManualPayment(workContract, workName, dt, trust85Actual, trust85Max);
                manualForm.ManualDone += ManualForm_ManualDone;
                manualForm.ShowDialog();
                return;
            }
            try
            {
                dgv.Hide();
                btnAdd.Hide();
                btnDelete.Hide();
                //            G1.ClearTabPageControls(tabDailyHistory);
                if (String.IsNullOrWhiteSpace(workContract))
                    return;
                DataTable dt = (DataTable)dgv.DataSource;
                manualForm = new ManualPayment(workContract, workName, dt, trust85Actual, trust85Max);
                manualForm.ManualDone += ManualForm_ManualDone;
                G1.LoadFormInControl(manualForm, panelBottom);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Loading Manual Payment! " + ex.Message.ToString());
            }
        }
        /****************************************************************************************/
        private void CheckLatePayment ()
        {
            if (paymentsFile.ToUpper() != "PAYMENTS")
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count < 2)
                return;
            int lastRow = dt.Rows.Count - 1;
            try
            {
                DateTime dolp = dt.Rows[lastRow]["payDate8"].ObjToDateTime();
                DateTime previousDate = dt.Rows[lastRow-1]["payDate8"].ObjToDateTime();

                if ( G1.GetMonthsBetween ( dolp, previousDate ) > 6 )
                    Messages.SendLatePayment(workContract);
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void ManualForm_ManualDone(string s)
        {
            try
            {
                if (manualForm != null)
                {
                    if (manualForm.Visible)
                    {
                        manualForm.Hide();
                        G1.ClearControl(panelBottom, "ManualPayment");
                        DailyHistory_Load(null, null);
                        cmbSelectColumns_SelectedIndexChanged(null, null);
                        CheckLatePayment();
//                        dgv.Show();
//                        btnAdd.Show();
//                        btnDelete.Show();
//                        if (s.ToUpper() == "POST")
//                        {
//                            LoadData();
//                            DataTable dt = (DataTable)dgv.DataSource;
//                            CalcNewAmort(dt);
//                            LoadDetailHeader();
//                            RecalcTotals();
////                            LoadHeader();
//                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Loading Manual Payment Close! " + ex.Message.ToString());
            }
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string s);
        public event d_void_eventdone_string ManualDone;
        protected void OnManualDone(string done)
        {
            if (ManualDone != null)
                ManualDone.Invoke(done);
        }
        /***************************************************************************************/
//        public delegate void d_void_eventdone_string(string s);
        public event d_void_eventdone_string SkinChange;
        protected void OnSkinChange(string done)
        {
            if (SkinChange != null)
                SkinChange.Invoke(done);
        }
        /****************************************************************************************/
        private void CalcNewAmort ( DataTable dt )
        {
            double balanceDue = 0D;
            if (workPolicy)
                return;
            string cmd = "Select * from `contracts` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                balanceDue = dx.Rows[0]["balanceDue"].ObjToString().ObjToDouble();
                if (isInsurance(workContract))
                    balanceDue = Policies.CalcMonthlyPremium(workPayer, DateTime.Now );
                double payment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
                numPayments = dx.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
                double totalInterest = dx.Rows[0]["totalInterest"].ObjToString().ObjToDouble();
                string dueDate = dx.Rows[0]["dueDate8"].ObjToString();
                DateTime iDate = GetIssueDate(dx.Rows[0]["issueDate8"].ObjToDateTime(), workContract, dx);
                string issueDate = iDate.ToString("MM/dd/yyyy");
                lastDate = issueDate.ObjToDateTime();
                string apr = dx.Rows[0]["APR"].ObjToString();

                dAPR = apr.ObjToDouble() / 100.0D;
                CalculateNewStuff(dt, dAPR, numPayments, balanceDue, lastDate);
                if (gotCemetery(dt))
                    balanceDue = 0D;
                labBalanceDue.Text = "$" + G1.ReformatMoney(balanceDue);
                if ( !showOldDetails )
                {
                    if ( dt.Rows.Count > 0 )
                    {
                        balanceDue = dt.Rows[0]["newbalance"].ObjToDouble();
                        if (gotCemetery(dt))
                            balanceDue = 0D;
                    }
                }
            }
            return;
        }
        /****************************************************************************************/
        private void btnDelete_Click(object sender, EventArgs e)
        {
            string preference = G1.getPreference(LoginForm.username, "DailyHistory", "Delete Payment", true );
            if (G1.RobbyServer)
                preference = "YES";
            if (preference != "YES")
                return;

            DataTable ddt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string depositNumber = dr["depositNumber"].ObjToString().ToUpper();

            DialogResult result;
            if ( depositNumber.IndexOf ( "TCA") == 0 )
                result = MessageBox.Show("***Warning*** Are you SURE you want to DELETE this TCA?", "Delete TCA Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            else
                result = MessageBox.Show("***Warning*** Are you SURE you want to DELETE this PAYMENT?", "Delete Payment Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if ( result != DialogResult.Yes )
            {
                MessageBox.Show("***INFO*** Okay, Payment not deleted!", "Delete Payment Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string reason = "";
            using (Ask askForm = new Ask("Enter Delete Reason?"))
            {
                askForm.Text = "";
                askForm.ShowDialog();
                if (askForm.DialogResult != DialogResult.OK)
                    return;
                reason = askForm.Answer;
            }

            if ( !LoginForm.administrator )
            {
                MessageBox.Show("***ERROR*** You do not have permission to remove a payment!\nCall Database Administrator!");
                return;
            }

            string record = dr["record"].ObjToString();
            if (depositNumber.ToUpper().IndexOf("TCA") == 0)
            {
                RemoveTCA (dr, record, reason, depositNumber );
                DailyHistory_Load(null, null);
                //LoadData();
                //DataTable dt = (DataTable)dgv.DataSource;
                //CalcNewAmort(dt);
                //RecalcTotals();
                //ResetAgents(record, true);
                return;
            }
            if (!String.IsNullOrWhiteSpace(record))
            {
                ManualPayment.ReversePayment(workContract, record, reason);
                LoadData();
                DataTable dt = (DataTable)dgv.DataSource;
                CalcNewAmort(dt);
                RecalcTotals();
                ResetAgents(record, true );
            }
        }
        /****************************************************************************************/
        private void RemoveTCA ( DataRow dr, string TCA_Record, string reason, string depositNumber )
        {
            try
            {
                string saveDepositNumber = depositNumber;

                DateTime dueDate = dr["dueDate8"].ObjToDateTime();

                string number = depositNumber.ToUpper().Replace("TCA-", "");
                int count = number.ObjToInt32();
                int startCount = count;
                DateTime date = dr["payDate8"].ObjToDateTime();
                string sDate = date.ToString("yyyy-MM-dd");
                string cmd = "Select * from `payments` WHERE `payDate8` >= '" + sDate + "' AND `depositNumber` LIKE 'TCA%' order by `depositNumber`;";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                    return;

                int minimum = count;
                int maximum = count;
                date = new DateTime(date.Year, 1, 1);
                string january = date.ToString("yyyy-MM-dd");

                DataTable dx = null;



                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString().ToUpper();
                    depositNumber = depositNumber.Replace("TCA-", "");
                    count = depositNumber.ObjToInt32();
                    if (count < minimum)
                        minimum = count;
                    if (count > maximum)
                        maximum = count;
                }

                DataRow[] dRows = null;

                for (int i = minimum; i <= maximum; i++)
                {
                    depositNumber = "TCA-" + i.ToString("D4");
                    if (depositNumber == saveDepositNumber)
                        continue;
                    dRows = dt.Select("depositNumber='" + depositNumber + "'");
                    if (dRows.Length == 0)
                    {
                        cmd = "Select * from `payments` where `payDate8` >= '" + january + "' AND `depositNumber` = '" + depositNumber + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            dt.ImportRow(dx.Rows[0]);
                        }
                    }
                }

                DataView tempview = dt.DefaultView;
                tempview.Sort = "depositNumber ASC";
                dt = tempview.ToTable();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString().ToUpper();
                    if (depositNumber == saveDepositNumber)
                    {
                        dt.Rows[i]["depositNumber"] = "";
                        continue;
                    }
                    depositNumber = depositNumber.Replace("TCA-", "");
                    count = depositNumber.ObjToInt32();
                    if (count <= startCount)
                        continue;
                    count = count - 1;
                    depositNumber = "TCA-" + count.ToString("D4");
                    dt.Rows[i]["depositNumber"] = depositNumber;
                }

                string record = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString().ToUpper();
                    record = dt.Rows[i]["record"].ObjToString();
                    if (String.IsNullOrWhiteSpace(depositNumber))
                    {
                        if (record == TCA_Record)
                            G1.delete_db_table("payments", "record", record);
                    }
                    else
                        G1.update_db_table("payments", "record", record, new string[] { "depositNumber", depositNumber });
                }

                fixTCANumber(); //Reduce the System TCA by 1

                cmd = "Select * from `contracts` where `contractNumber` = '" + workContract + "';";
                DataTable ddt = G1.get_db_data(cmd);
                if (ddt.Rows.Count > 0 )
                {
                    record = ddt.Rows[0]["record"].ObjToString();
                    G1.update_db_table("contracts", "record", record, new string[] { "dueDate8", dueDate.ToString("yyyy-MM-dd")});
                }

                G1.AddToAudit(LoginForm.username, "Daily History", "TCA Removed", reason, workContract );

                MessageBox.Show ( "*** INFO *** TCA was successfully removed!\nAll others were renumbered!", "Remove TCA Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR*** Something went wrong!\n" + ex.Message.ToString(), "Delete TCA Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        /***********************************************************************************************/
        private void fixTCANumber()
        {
            string request = "";
            string cmd = "Select * from `options` where `option` = 'TCA Starting Number';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
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
                seq--;

            string str = year.ToString("D4") + "-" + seq.ToString();
            string nextReinstateNumber = str;
            G1.update_db_table("options", "record", reinstateRequestRecord, new string[] { "answer", str });
        }
        /****************************************************************************************/
        private void ResetAgents ( string paymentRecord, bool remove )
        {
            if (String.IsNullOrWhiteSpace(paymentRecord))
                return;
            string cmd = "Select * from `" + paymentsFile + "` where `record` = '" + paymentRecord + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            // double payment = dt.Rows[0]["paymentAmount"].ObjToDouble();
            double payment = getPayment(dt, 0);

            string agent = dt.Rows[0]["agentNumber"].ObjToString();
            string originalAgent = agent;
            if (String.IsNullOrWhiteSpace(agent))
                return;
            cmd = "Select * from `agents` where `agentCode` = '" + agent + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            string record = dx.Rows[0]["record"].ObjToString();
            if (String.IsNullOrWhiteSpace(record))
                return;

            double commission = dx.Rows[0]["commission"].ObjToDouble();
            string splits = dx.Rows[0]["splits"].ObjToString();
            double recapAmount = dx.Rows[0]["recapAmount"].ObjToDouble();

            double commPaid = payment * (commission / 100D);
            commPaid = G1.RoundValue(commPaid);
            if ( splits.IndexOf ( '~') < 0 )
            {
                if ( remove )
                    recapAmount += commPaid;
                else
                    recapAmount -= commPaid;
                recapAmount = G1.RoundValue(recapAmount);
                G1.update_db_table("agents", "record", record, new string[] { "recapAmount", recapAmount.ToString() });
                return;
            }

            string str = "";
            double percent = 0D;
            double totalComm = 0D;
            double value = 0D;
            string[] Lines = splits.Split('~');
            for ( int i=0; i<Lines.Length; i=i+2 )
            {
                agent = Lines[i].Trim();
                if (String.IsNullOrWhiteSpace(agent))
                    continue;
                str = Lines[i + 1].ObjToString();
                if ( G1.validate_numeric ( str ))
                {
                    percent = str.ObjToDouble() / 100D;
                    value = payment * percent;
                    value = G1.RoundValue(value);
                    cmd = "Select * from `agents` where `agentCode` = '" + agent + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                        continue;
                    record = dx.Rows[0]["record"].ObjToString();
                    if (String.IsNullOrWhiteSpace(record))
                        continue;
                    recapAmount = dx.Rows[0]["recapAmount"].ObjToDouble();
                    if (remove)
                        recapAmount += value;
                    else
                        recapAmount -= value;
                    recapAmount = G1.RoundValue(recapAmount);
                    G1.update_db_table("agents", "record", record, new string[] { "recapAmount", recapAmount.ToString() });
                    totalComm += value;
                }
            }
            //double diff = totalComm - commPaid;
            //diff = G1.RoundValue(diff);
            //if ( diff != 0D )
            //{
            //    cmd = "Select * from `agents` where `agentCode` = '" + originalAgent + "';";
            //    dx = G1.get_db_data(cmd);
            //    if (dx.Rows.Count > 0)
            //    {
            //        record = dx.Rows[0]["record"].ObjToString();
            //        if (!String.IsNullOrWhiteSpace(record))
            //        {
            //            recapAmount = dx.Rows[0]["recapAmount"].ObjToDouble();
            //            if (remove)
            //                recapAmount += diff;
            //            else
            //                recapAmount -= diff;
            //        }
            //    }
            //}
        }
        /****************************************************************************************/
        public static double ProcessLapseAmount ( bool updateAgent, string agent, double contractValue, double commission, string splits )
        {
            double percent = 0D;
            string cmd = "";
            string str = "";
            double totalComm = 0D;
            double value = 0D;
            DataTable dx = null;
            string record = "";
            double recapAmount = 0D;
            double payment = contractValue;
            double commPaid = payment * commission;
            commPaid = G1.RoundValue(commPaid);
            if (splits.IndexOf('~') < 0)
            {
                cmd = "Select * from `agents` where `agentCode` = '" + agent + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    return commPaid;
                record = dx.Rows[0]["record"].ObjToString();
                if (String.IsNullOrWhiteSpace(record))
                    return commPaid;
                recapAmount = dx.Rows[0]["recapAmount"].ObjToDouble();
                recapAmount += commPaid;
                recapAmount = G1.RoundValue(recapAmount);
                if ( updateAgent)
                    G1.update_db_table("agents", "record", record, new string[] { "recapAmount", recapAmount.ToString() });
                return commPaid;
            }

            string[] Lines = splits.Split('~');
            for (int i = 0; i < Lines.Length; i = i + 2)
            {
                agent = Lines[i].Trim();
                if (String.IsNullOrWhiteSpace(agent))
                    continue;
                str = Lines[i + 1].ObjToString();
                if (G1.validate_numeric(str))
                {
                    percent = str.ObjToDouble() / 100D;
                    value = payment * percent;
                    value = G1.RoundValue(value);
                    commPaid = value;
                    cmd = "Select * from `agents` where `agentCode` = '" + agent + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                        continue;
                    record = dx.Rows[0]["record"].ObjToString();
                    if (String.IsNullOrWhiteSpace(record))
                        continue;
                    recapAmount = dx.Rows[0]["recapAmount"].ObjToDouble();
                    recapAmount += value;
                    recapAmount = G1.RoundValue(recapAmount);
                    if ( updateAgent)
                        G1.update_db_table("agents", "record", record, new string[] { "recapAmount", recapAmount.ToString() });
                    totalComm += value;
                }
            }
            return commPaid;
        }
        /****************************************************************************************/
        private void RecalcTotals ()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            dgv.DataSource = null;
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            if (!showOldDetails)
            {
                if (dt.Rows.Count > 0)
                {
                    DataView tempview = dt.DefaultView;
                    tempview.Sort = "payDate8 desc, tmstamp desc, record desc";
                    dt = tempview.ToTable();

                    int lastRow = 0;
                    double balanceDue = dt.Rows[lastRow]["newbalance"].ObjToDouble();
                    balanceDue = dt.Rows[lastRow]["balance"].ObjToDouble();
                    if (isInsurance(workContract))
                        balanceDue = Policies.CalcMonthlyPremium(workPayer, DateTime.Now );
                    if (gotCemetery(dt))
                        balanceDue = 0D;
                    labBalanceDue.Text = "$" + G1.ReformatMoney(balanceDue);
                    DateTime now = DateTime.Now;
                    string str = txtAsOff.Text;
                    if (String.IsNullOrWhiteSpace(str))
                        txtAsOff.Text = now.ToString("MM/dd/yyyy");
                    str = txtAsOff.Text;
                    if (G1.validate_date(str))
                        now = str.ObjToDateTime();

                    DateTime dolp = dt.Rows[lastRow]["payDate8"].ObjToDateTime();
                    getDOLP(dt, ref dolp); // This had to be done to avoid a credit being the last date paid (DOLP)

                    double oldBalance = balanceDue;
                    DateTime docp = now;
                    double payment = 0D;
                    str = lblAPR.Text.Trim();
                    str = str.Replace("APR :", "");
                    str = str.Replace("%", "");
                    double apr = str.ObjToDouble();
                    double principal = 0D;
                    double interest = 0D;
                    int days = 0;
                    double unpaid_interest = 0D;
                    ImportDailyDeposits.CalcPrincipalInterest(oldBalance, dolp, docp, payment, apr, ref principal, ref interest, ref days, ref unpaid_interest );
                    double oldInterest = unpaid_interest;
                    ImportDailyDeposits.HandleUnpaidInterest(workContract, payment, ref interest, ref unpaid_interest, ref principal, ref balanceDue);
                    double payoff = balanceDue + oldInterest;
                    txtPayoff.Text = "$" + G1.ReformatMoney(payoff);
                }
            }
            else
            {

            }
            bool chronological = chkChonological.Checked;

            if (G1.get_column_number(dt, "ap") < 0)
                AddAP(dt);

            FixCemeteryView(dt);

            if ( chronological )
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "tmstamp desc, record desc";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                if (reverseSort)
                    dt = ReverseOrder(dt);
                dgv.DataSource = dt;
            }
            else
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "payDate8 desc, tmstamp desc, record desc";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                if (reverseSort)
                    dt = ReverseOrder(dt);
                dgv.DataSource = dt;
            }
        }
        /****************************************************************************************/
        public static bool getDOLP ( DataTable dx, ref DateTime dolp )
        {
            dolp = DateTime.MinValue;
            if (dx.Rows.Count <= 0)
                return false;
            DataTable dt = dx.Copy();

            DataView tempview = dt.DefaultView;
            tempview.Sort = "payDate8 desc, tmstamp desc, record desc";
            dt = tempview.ToTable();

            double credit = 0D;
            double debit = 0D;
            double interest = 0D;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                dolp = dt.Rows[i]["payDate8"].ObjToDateTime();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                if (credit > 0D && interest == 0D)
                    continue;
                else if ( credit > 0D && interest > 0D )
                    break;
                if (credit != 0D || debit != 0D)
                    continue;
                break;
            }
            return true;
        }
        /****************************************************************************************/
        private void FixCemeteryView ( DataTable dt )
        {
            double debit = 0D;
            double credit = 0D;
            if (dt.Rows.Count > 0)
            {
                if ( DailyHistory.gotCemetery ( workContract ))
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                        credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                        if (debit != 0D || credit != 0D)
                        {
                            dt.Rows[i]["oldBalance"] = 0D;
                            dt.Rows[i]["balance"] = 0D;
                            dt.Rows[i]["newbalance"] = 0D;
                        }
                        else
                        {
                            dt.Rows[i]["principal"] = dt.Rows[i]["paymentAmount"].ObjToDouble();
                            dt.Rows[i]["prince"] = dt.Rows[i]["paymentAmount"].ObjToDouble();
                            dt.Rows[i]["oldBalance"] = 0D;
                            dt.Rows[i]["balance"] = 0D;
                            dt.Rows[i]["newbalance"] = 0D;
                        }
                    }
                    gridMain.Columns["trust85P"].Caption = "Trust15P";
                    gridMain.Columns["trust100P"].Caption = "Trust15P";
                    gridMain.Columns["calculatedTrust85"].Caption = "Trust15P";
                    gridMain.Columns["calculatedTrust100"].Caption = "Trust15P";
                    gridMain.Columns["calculatedTrust100"].Visible = false;
                    gridMain.Columns["trust100P"].Visible = false;
                    gridMain.Columns["credit"].Visible = false;
                    gridMain.Columns["debit"].Visible = false;
                    gridMain.Columns["reason"].Visible = false;
                    gridMain.Columns["dpp"].Visible = false;
                    gridMain.Columns["interestPaid"].Visible = false;
                    gridMain.Columns["retained"].Visible = false;
                    gridMain.Columns["dueDate8"].Visible = false;
                    gridMain.Columns["NumPayments"].Visible = false;
                    gridMain.Columns["days"].Visible = false;
                }
            }
        }
        /****************************************************************************************/
        private void chkLoadAll_CheckedChanged(object sender, EventArgs e)
        {
            //LoadData();
            //DataTable dt = (DataTable)dgv.DataSource;
            //CalcNewAmort(dt);
            //dgv.DataSource = dt;
            //RecalcTotals();
            gridMain.RefreshData();
            this.Refresh();
        }
        /****************************************************************************************/
        private void resetPaymentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                G1.update_db_table(paymentsFile, "record", record, new string[] { "fill", "", "debitReason", "" });
                dr["fill"] = "";
                LoadData();
                DataTable dt = (DataTable)dgv.DataSource;
                CalcNewAmort(dt);
                RecalcTotals();
                ResetAgents(record, false);
            }
        }
        /****************************************************************************************/
        public static double CalculatePayout(string contractNumber, DataTable paymentsDt )
        {
            double rv = 0D;
            if ( isInsurance ( contractNumber))
                return rv;
            if ( paymentsDt == null )
            {
                paymentsDt = G1.get_db_data("Select * from `payments` where `contractNumber` = '" + contractNumber + "' order by `payDate8` DESC, `tmstamp` DESC;");
                if (paymentsDt.Rows.Count <= 0)
                    return rv;
            }

            bool found = false;
            double endingBalance = 0D;
            double newBalance = 0D;
            DateTime endingDate = new DateTime(2000, 1, 1);
            string cmd = "Select * from `trust2013` where `contractNumber` = '" + contractNumber + "' order by `payDate8` DESC, `tmstamp` DESC;";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                found = true;
                endingDate = dx.Rows[0]["payDate8"].ObjToDateTime();
            }

            string date = "";
            double payment = 0D;
            double principal = 0D;
            double debit = 0D;
            double credit = 0D;
            double interest = 0D;
            double trust85P = 0D;
            double trust100P = 0D;
            double totalPaid = 0D;
            int days = 0;

            DateTime dolp = DateTime.Now;
            DateTime docp = DateTime.Now;

            cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            dx = G1.get_db_data(cmd);
            double expected = dx.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
            double amtOfMonthlyPayt = expected;
            DateTime contractDueDate8 = dx.Rows[0]["dueDate8"].ObjToDateTime();
            double contractValue = GetContractValuePlus(dx.Rows[0]);
            double originalDownPayment = dx.Rows[0]["downPayment"].ObjToDouble();
            DateTime dueDate = dx.Rows[0]["issueDate8"].ObjToDateTime();
            dueDate = GetIssueDate(dueDate, contractNumber, null);
            string issueDateStr = dueDate.ToString("MM/dd/yyyy");
            double numPayments = dx.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
            double rate = dx.Rows[0]["apr"].ObjToDouble() / 100D;
            double paymentAmount = 0D;
            double downPayment = 0D;
            double retained = 0D;
            string finale = "";

            newBalance = 0D;

            for (int i = 0; i < paymentsDt.Rows.Count; i++)
            {
                if (paymentsDt.Rows[i]["fill"].ObjToString().ToUpper() == "D")
                    continue;
                finale = paymentsDt.Rows[i]["new"].ObjToString().ToUpper();
                docp = paymentsDt.Rows[i]["payDate8"].ObjToDateTime();

                //payment = paymentsDt.Rows[i]["paymentAmount"].ObjToDouble();
                payment = getPayment(paymentsDt, i);

                if (payment == 12.91D)
                {

                }
                debit = paymentsDt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = paymentsDt.Rows[i]["creditAdjustment"].ObjToDouble();
                interest = paymentsDt.Rows[i]["interestPaid"].ObjToDouble();
                //downPayment = paymentsDt.Rows[i]["downPayment"].ObjToDouble();
                downPayment = getDownPayment(paymentsDt, i);
                if (downPayment != 0D)
                {
                    if (paymentsDt.Rows.Count == 1)
                    {
                        if (originalDownPayment == 0D)
                            originalDownPayment = downPayment;
                    }
                    else if (originalDownPayment == 0D)
                        originalDownPayment = downPayment;
                }

                paymentAmount = payment + credit - debit - interest + downPayment;
                principal = paymentAmount - interest;
                if ( finale == "FINALE")
                {
                    trust85P = paymentsDt.Rows[i]["trust85P"].ObjToDouble();
                }
                else
                    ImportDailyDeposits.CalcTrust85P(docp, amtOfMonthlyPayt, issueDateStr, contractValue, originalDownPayment, numPayments, payment, principal, debit, credit, rate, ref trust85P, ref trust100P, ref retained );

//                ImportDailyDeposits.CalcTrust85(issueDateStr, contractValue, originalDownPayment, numPayments, paymentAmount, principal, rate, ref trust85P, ref trust100P);

                dolp = paymentsDt.Rows[i]["payDate8"].ObjToDateTime(); // This is okay (DOLP)
//                if (dolp > endingDate)
                    newBalance += trust85P;
            }

            return newBalance;
        }
        /***********************************************************************************************/
        public static double ReCalculateBalance(string contractNumber)
        {
            double trust85 = 0D;
            return ReCalculateBalance(contractNumber, ref trust85);
        }
        /***********************************************************************************************/
        public static double ReCalculateBalance(string contractNumber, ref double trust85)
        {
            trust85 = 0D;
            if ( isInsurance ( contractNumber ))
                return 0D;
            DataTable dx = G1.get_db_data("Select * from `contracts` where `contractNumber` = '" + contractNumber + "';");
            if (dx.Rows.Count <= 0)
                return 0D;
            double serviceTotal = dx.Rows[0]["serviceTotal"].ObjToDouble();
            double merchandiseTotal = dx.Rows[0]["merchandiseTotal"].ObjToDouble();
            double allowMerchandise = dx.Rows[0]["allowMerchandise"].ObjToDouble();
            double allowInsurance = dx.Rows[0]["allowInsurance"].ObjToDouble();
            double downpayment = dx.Rows[0]["downPayment"].ObjToDouble();
            double financedAmount = serviceTotal + merchandiseTotal - allowMerchandise - allowInsurance - downpayment;
            financedAmount = DailyHistory.GetFinanceValue(dx.Rows[0]);

            double originalBalance = dx.Rows[0]["balanceDue"].ObjToDouble();
            DateTime dueDate = dx.Rows[0]["dueDate8"].ObjToDateTime();
            DateTime future = new DateTime(2039, 12, 31);
            if (dueDate >= future)
            {
                trust85 = CalculatePayout(contractNumber, null);
                financedAmount = originalBalance;
                return financedAmount;
            }


            DataTable dt = G1.get_db_data("Select * from `payments` where `contractNumber` = '" + contractNumber + "' order by `payDate8` asc, `tmstamp` asc;");
            if (dt.Rows.Count <= 0)
            {
                trust85 = CalculatePayout(contractNumber, null);
                return financedAmount;
            }

            string date = "";
            double payment = 0D;
            double principal = 0D;
            double debit = 0D;
            double credit = 0D;
            double interest = 0D;
            double trust85P = 0D;
            double trust100P = 0D;
            double totalPaid = 0D;
            int days = 0;
            double newBalance = financedAmount;

            DateTime dolp = DateTime.Now;
            DateTime docp = DateTime.Now;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                payment = getPayment(dt, i);

                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                newBalance = newBalance - payment + interest - credit + debit;
            }

            trust85 = CalculatePayout(contractNumber, dt);
            return newBalance;
        }
        /***********************************************************************************************/
        public static double CalculatePayoff ( string contractNumber, DateTime payoffDate )
        {
            double totalInterest = 0D;
            string contractFile = "contracts";
            string paymentFile = "payments";
            if (isInsurance(contractNumber))
            {
                contractFile = "icontracts";
                paymentFile = "ipayments";
            }
            DataTable dx = G1.get_db_data("Select * from `" + contractFile + "` where `contractNumber` = '" + contractNumber + "';");
            if (dx.Rows.Count <= 0)
                return 0D;
            double serviceTotal = dx.Rows[0]["serviceTotal"].ObjToDouble();
            double merchandiseTotal = dx.Rows[0]["merchandiseTotal"].ObjToDouble();

            double allowMerchandise = dx.Rows[0]["allowMerchandise"].ObjToDouble();
            double allowInsurance = dx.Rows[0]["allowInsurance"].ObjToDouble();
            double downpayment = dx.Rows[0]["downPayment"].ObjToDouble();
            double financedAmount = serviceTotal + merchandiseTotal - allowMerchandise - allowInsurance - downpayment;
            financedAmount = DailyHistory.GetFinanceValue(dx.Rows[0]);

            double originalBalance = dx.Rows[0]["balanceDue"].ObjToDouble();
            DateTime dueDate = dx.Rows[0]["dueDate8"].ObjToDateTime();
            DateTime future = new DateTime(2039, 12, 31);
            if (dueDate >= future)
            {
                financedAmount = originalBalance;
                totalInterest = dx.Rows[0]["totalInterest"].ObjToDouble();
                return financedAmount;
            }


            DataTable dt = G1.get_db_data("Select * from `" + paymentFile + "` where `contractNumber` = '" + contractNumber + "' order by `payDate8` DESC, `tmstamp` DESC;");
            if (dt.Rows.Count <= 0)
                return financedAmount;

            double creditBalance = 0D;
            int numPayments = dx.Rows[0]["numberOfPayments"].ObjToInt32();
            DateTime issueDate = dx.Rows[0]["issueDate8"].ObjToDateTime();
            issueDate = DailyHistory.GetIssueDate(issueDate, contractNumber, dx);
            double creditDue = dx.Rows[0]["creditBalance"].ObjToDouble();
//            double financedAmount = serviceTotal + merchandiseTotal - allowMerchandise - allowInsurance - downpayment;
            financedAmount = DailyHistory.GetFinanceValue(dx.Rows[0]);
            double apr = dx.Rows[0]["apr"].ObjToDouble();
            apr = apr / 100D;

            double newBalance = dx.Rows[0]["BalanceDue"].ObjToDouble();

            DailyHistory.CalculateNewStuff(dt, apr, numPayments, financedAmount, issueDate);
            if (dt.Rows.Count > 0)
            {
                if (gotCemetery(dt))
                {
                    creditBalance = 0D;
                    newBalance = 0D;
                }
                else
                {
                    creditBalance = dt.Rows[0]["runningCB"].ObjToDouble();

                    DateTime dolp = dt.Rows[0]["payDate8"].ObjToDateTime();
                    getDOLP(dt, ref dolp); // This had to be done to avoid a credit being the last payment (DOLP)

                    DateTime docp = payoffDate;
                    newBalance = dt.Rows[0]["newBalance"].ObjToDouble();
                    double payment = 0D;
                    double principal = 0D;
                    double interest = 0D;
                    double unpaid_interest = 0D;
                    int days = 0;
                    ImportDailyDeposits.CalcPrincipalInterest(newBalance, dolp, docp, payment, apr, ref principal, ref interest, ref days, ref unpaid_interest);
                    newBalance = newBalance + interest - creditBalance;

                    ImportDailyDeposits.HandleUnpaidInterest(contractNumber, payment, ref interest, ref unpaid_interest, ref principal, ref newBalance);
                }
            }
            return newBalance;
        }
        /***********************************************************************************************/
        public static double ReCalculateDetails(string contractNumber, ref double totalInterest)
        {
            totalInterest = 0D;
            string contractFile = "contracts";
            string paymentFile = "payments";
            if ( isInsurance ( contractNumber ))
            {
                contractFile = "icontracts";
                paymentFile = "ipayments";
            }
            DataTable dx = G1.get_db_data("Select * from `" + contractFile + "` where `contractNumber` = '" + contractNumber + "';");
            if (dx.Rows.Count <= 0)
                return 0D;
            double serviceTotal = dx.Rows[0]["serviceTotal"].ObjToDouble();
            double merchandiseTotal = dx.Rows[0]["merchandiseTotal"].ObjToDouble();

            double allowMerchandise = dx.Rows[0]["allowMerchandise"].ObjToDouble();
            double allowInsurance = dx.Rows[0]["allowInsurance"].ObjToDouble();
            double downpayment = dx.Rows[0]["downPayment"].ObjToDouble();
            double financedAmount = serviceTotal + merchandiseTotal - allowMerchandise - allowInsurance - downpayment;
            financedAmount = DailyHistory.GetFinanceValue(dx.Rows[0]);

            double originalBalance = dx.Rows[0]["balanceDue"].ObjToDouble();
            DateTime dueDate = dx.Rows[0]["dueDate8"].ObjToDateTime();
            DateTime future = new DateTime(2039, 12, 31);
            if (dueDate >= future)
            {
                financedAmount = originalBalance;
                totalInterest = dx.Rows[0]["totalInterest"].ObjToDouble();
                return financedAmount;
            }


            DataTable dt = G1.get_db_data("Select * from `" + paymentFile + "` where `contractNumber` = '" + contractNumber + "' order by `payDate8` asc;");
            if (dt.Rows.Count <= 0)
                return financedAmount;

            string date = "";
            double payment = 0D;
            double principal = 0D;
            double debit = 0D;
            double credit = 0D;
            double interest = 0D;
            double trust85P = 0D;
            double trust100P = 0D;
            double totalPaid = 0D;
            int days = 0;
            double newBalance = financedAmount;

            DateTime dolp = DateTime.Now;
            DateTime docp = DateTime.Now;
            string delete = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                delete = dt.Rows[i]["fill"].ObjToString().ToUpper();
                if (delete == "D")
                    continue;
                //payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                payment = getPayment(dt, i);

                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                newBalance = newBalance - payment + interest - credit + debit;
                totalInterest += interest;
            }
            return newBalance;
        }
        /****************************************************************************************/
        private void checkInterestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DateTime toDate = dr["payDate8"].ObjToDateTime();
            double payment = dr["paymentAmount"].ObjToDouble();
            double ccFee = dr["ccFee"].ObjToDouble();
            payment -= ccFee;

            DataTable dt = (DataTable)dgv.DataSource;

            if (row < (dt.Rows.Count))
            {
                int inc = 1;
                if (reverseSort)
                    inc = -1;
                double balance = dt.Rows[row+inc]["balance"].ObjToDouble();
                //if ( recalculateHistory )
                //    balance = dt.Rows[row + inc]["newbalance"].ObjToDouble();
                DateTime fromDate = dt.Rows[row + inc]["payDate8"].ObjToDateTime();
                double apr = dAPR;
                CheckInterest checkForm = new CheckInterest(apr, payment, fromDate, toDate, balance);
                checkForm.Show();
            }
        }
        /****************************************************************************************/
        public static bool ClearLapsed ( string contractNumber )
        {
            bool rv = false;

            string contractFile = "contracts";
            string customerFile = "customers";
            Customers.DetermineIfInsurance(contractNumber, ref contractFile, ref customerFile);

            string cmd = "Select * from `" + contractFile + "` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Cannot find Contract " + contractNumber + "!");
                return rv;
            }

            string record = dx.Rows[0]["record"].ObjToString();
            DateTime lapseDate = dx.Rows[0]["lapseDate8"].ObjToDateTime();
            string date1 = dx.Rows[0]["lapseDate8"].ObjToString();
            //if (date1.IndexOf("0000") >= 0)
            //{
            //    MessageBox.Show("***ERROR*** Contract ( " + contractNumber + ") is not Lapsed! Therefore, it cannot be Cleared!");
            //    return rv;
            //}

            G1.update_db_table(contractFile, "record", record, new string[] { "lapsed", "", "lapseDate8", "0000-00-00" });
            CustomerDetails.UpdatePayersDetail(contractNumber);

            cmd = "Select * from `" + customerFile + "` where `contractNumber` = '" + contractNumber + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Cannot find Customers " + contractNumber + "!");
                return rv;
            }
            record = dx.Rows[0]["record"].ObjToString();
            G1.update_db_table(customerFile, "record", record, new string[] { "lapsed", "" });

            G1.AddToAudit(LoginForm.username, "Customers", "Clear Lapsed", "Reset", contractNumber);

            rv = true;
            return rv;
        }
        /****************************************************************************************/
        public static bool SetLapsed(string contractNumber, string lapseDate )
        {
            bool rv = false;
            if ( !G1.validate_date ( lapseDate))
            {
                MessageBox.Show("***ERROR*** Lapse Date is not a valid Date!");
                return rv;
            }

            string contractFile = "contracts";
            string customerFile = "customers";
            Customers.DetermineIfInsurance(contractNumber, ref contractFile, ref customerFile);

            string cmd = "Select * from `" + contractFile + "` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Cannot find Contract " + contractNumber + "!");
                return rv;
            }

            string record = dx.Rows[0]["record"].ObjToString();

            G1.update_db_table(contractFile, "record", record, new string[] { "lapsed", "Y", "lapseDate8", lapseDate });
            CustomerDetails.UpdatePayersDetail(contractNumber);

            cmd = "Select * from `" + customerFile + "` where `contractNumber` = '" + contractNumber + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Cannot find Customers " + contractNumber + "!");
                return rv;
            }
            record = dx.Rows[0]["record"].ObjToString();
            G1.update_db_table(customerFile, "record", record, new string[] { "lapsed", "Y" });

            G1.AddToAudit(LoginForm.username, "Customers", "Set Lapsed", "Set", contractNumber);

            rv = true;
            return rv;
        }
        /****************************************************************************************/
        public static bool SetReinstate(string contractNumber, string reinstateDate)
        {
            bool rv = false;
            if (!G1.validate_date(reinstateDate))
            {
                MessageBox.Show("***ERROR*** Reinstate Date is not a valid Date!");
                return rv;
            }

            string contractFile = "contracts";
            string customerFile = "customers";
            Customers.DetermineIfInsurance(contractNumber, ref contractFile, ref customerFile);

            string cmd = "Select * from `" + contractFile + "` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Cannot find Contract " + contractNumber + "!");
                return rv;
            }

            string record = dx.Rows[0]["record"].ObjToString();
            G1.update_db_table(contractFile, "record", record, new string[] { "lapsed", "", "lapseDate8", "0000-00-00", "reinstateDate8", reinstateDate });

            CustomerDetails.UpdatePayersDetail(contractNumber);

            cmd = "Select * from `" + customerFile + "` where `contractNumber` = '" + contractNumber + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Cannot find Customers " + contractNumber + "!");
                return rv;
            }
            record = dx.Rows[0]["record"].ObjToString();
            G1.update_db_table(customerFile, "record", record, new string[] { "lapsed", "" });

            G1.AddToAudit(LoginForm.username, "Customers", "Set Lapsed", "Set", contractNumber);

            rv = true;
            return rv;
        }
        /****************************************************************************************/
        private void clearLapsedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to Clear Lapsed for customer (" + workContract + ") ?", "Clear Lapsed Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;
            ClearLapsed(workContract);
        }
        /****************************************************************************************/
        private void btnReinstate_Click(object sender, EventArgs e)
        {
            string contractNumber = workContract;
            this.Cursor = Cursors.Default;
            this.TopMost = false;
            try
            {
                //DialogResult result = DevExpress.XtraEditors.XtraMessageBox.Show ("Are you sure you want to REINSTATE customer (" + workContract + ") ?", "Reinstate Contract Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (MessageBox.Show("Are you sure you want to REINSTATE customer (" + workContract + ") ?", "Reinstate Contract Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.No)
                    return;
            }
            catch ( Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("***ERROR*** " + ex.Message.ToString(), "Reinstate Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            string cmd = "Select * from `" + customersFile + "` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                DateTime today = DateTime.Now;
                string reinstateDate = today.ToString("yyyy-MM-dd");
                string record = dx.Rows[0]["record"].ObjToString();
                G1.update_db_table(customersFile, "record", record, new string[] { "lapsed", "" });

                cmd = "Select * from `" + contractsFile + "` where `contractNumber` = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);

                record = dx.Rows[0]["record"].ObjToString();
                G1.update_db_table(contractsFile, "record", record, new string[] { "lapsed", "", "lapseDate8", "0000-00-00", "reinstateDate8", reinstateDate });

                //ReinstatePolicies(contractNumber);

                CustomerDetails.UpdatePayersDetail(contractNumber);
                G1.AddToAudit(LoginForm.username, "Customers", "Reinstate", "Set", contractNumber);

                this.Cursor = Cursors.WaitCursor;
                ReinstateReport report = new ReinstateReport(contractNumber);
                report.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void ReinstatePolicies ( string contractNumber)
        {
            string cmd = "Select * from `policies` p JOIN `icustomers` d ON p.`contractNumber` = d.`contractNumber` JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
            if (String.IsNullOrWhiteSpace(workPayer))
                return;
            cmd += " WHERE p.`contractNumber` = '" + contractNumber + "' ";
            cmd += ";";

            string record = "";

            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                G1.update_db_table("policies", "record", record, new string[] { "lapsed", "", "lapseDate8", "0000-00-00" });
            }
        }
        /***********************************************************************************************/
        public static bool CalculateDueNow(string contractNumber, ref double dueNow )
        {
            dueNow = 0D;
            string contractsFile = "contracts";
            if ( isInsurance ( contractNumber))
                contractsFile = "icontracts";
            double totalInterest = 0D;
            DataTable dx = G1.get_db_data("Select * from `" + contractsFile + "` where `contractNumber` = '" + contractNumber + "';");
            if (dx.Rows.Count <= 0)
                return false;
            dueNow = dx.Rows[0]["nowDue"].ObjToDouble();
            if (isInsurance ( contractNumber))
                return true;

            double serviceTotal = dx.Rows[0]["serviceTotal"].ObjToDouble();
            double merchandiseTotal = dx.Rows[0]["merchandiseTotal"].ObjToDouble();
            double allowMerchandise = dx.Rows[0]["allowMerchandise"].ObjToDouble();
            double allowInsurance = dx.Rows[0]["allowInsurance"].ObjToDouble();
            double downpayment = dx.Rows[0]["downPayment"].ObjToDouble();
            double financedAmount = serviceTotal + merchandiseTotal - allowMerchandise - allowInsurance - downpayment;
            financedAmount = DailyHistory.GetFinanceValue(dx.Rows[0]);
            double monthlyPayment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
            double apr = dx.Rows[0]["apr"].ObjToDouble();
            apr = apr / 100D;

            double originalBalance = dx.Rows[0]["balanceDue"].ObjToDouble();
            DateTime dueDate = dx.Rows[0]["dueDate8"].ObjToDateTime();
            DateTime future = new DateTime(2039, 12, 31);
            if (dueDate >= future)
            {
                financedAmount = originalBalance;
                totalInterest = dx.Rows[0]["totalInterest"].ObjToDouble();
                return false;
            }


            DataTable dt = G1.get_db_data("Select * from `payments` where `contractNumber` = '" + contractNumber + "' order by `payDate8` asc;");
            if (dt.Rows.Count <= 0)
                return false;

            string date = "";
            double payment = 0D;
            double principal = 0D;
            double debit = 0D;
            double credit = 0D;
            double interest = 0D;
            double trust85P = 0D;
            double trust100P = 0D;
            double totalPaid = 0D;
            double newBalance = financedAmount;

            DateTime dolp = DateTime.Now;
            DateTime docp = DateTime.Now;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                payment = getPayment(dt, i);

                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                newBalance = newBalance - payment + interest - credit + debit;
                totalInterest += interest;
                dolp = dt.Rows[i]["payDate8"].ObjToDateTime();
                docp = dt.Rows[i]["dueDate8"].ObjToDateTime();
            }

            DateTime nextDueDate = DateTime.Now;
            TimeSpan ts = nextDueDate - dolp;
            int days = ts.Days;
            if (days <= 30)
            {
                dueNow = monthlyPayment;
                return true;
            }
            interest = CalculateInterest(dolp, days, apr, newBalance);
            double tempInt = CalculateInterest(dolp, 30, apr, newBalance);
            interest = interest - tempInt;
            nextDueDate = dolp;
            int months = 1;
            dueNow = monthlyPayment + interest;
            for (;;)
            {
                nextDueDate = nextDueDate.AddMonths(1);
                if (nextDueDate >= DateTime.Now)
                    break;
                dueNow += monthlyPayment - tempInt;
                months++;
            }
            return true;
        }
        /***********************************************************************************************/
        public static double CalcMonthsForInsurance (string contractNumber, string payer, double monthlyPayment, double currentPayment, DateTime docp, DateTime dDate, ref double creditBalance, ref DateTime nextDueDate )
        {
            double months = 0D; 
            months = CheckMonthsForInsurance(contractNumber, payer, monthlyPayment, currentPayment, docp, dDate);
            months = G1.RoundValue(months);
            nextDueDate = dDate.ObjToDateTime();
            int imonths = (int)months;
            creditBalance = currentPayment - (imonths.ObjToDouble() * monthlyPayment);
            if (creditBalance < 0D)
            {
                int nMonths = Convert.ToInt32((currentPayment / monthlyPayment));
                creditBalance = currentPayment - (nMonths * monthlyPayment);
                creditBalance = G1.RoundDown(creditBalance);
            }
            creditBalance = G1.RoundValue(creditBalance);
            nextDueDate = nextDueDate.AddMonths(imonths);
            return months;
        }
        /***********************************************************************************************/
        public static bool ReCalculateDueDate(string contractNumber, DateTime docp, double monthlyPayment, double currentPayment, double debitAmount, double creditAmount, ref DateTime nextDueDate, ref double creditBalance, ref double months, ref double newbalance, ref DateTime currentDueDate )
        {
            creditBalance = 0D;
            newbalance = 0D;
            months = 0D;
            double totalInterest = 0D;
            if ( contractNumber == "ZZ0002792")
            {

            }
            bool insurance = false;
            string contractsFile = "contracts";
            string customersFile = "customers";
            string paymentsFile = "payments";
            if ( isInsurance ( contractNumber))
            {
                insurance = true;
                contractsFile = "icontracts";
                customersFile = "icustomers";
                paymentsFile = "ipayments";
                //    return false;
            }
            string cmd = "Select * from `" + contractsFile + "` c JOIN `" + customersFile + "` d ON c.`contractNumber` = d.`contractNumber` where c.`contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return false;
            string payer = dx.Rows[0]["payer"].ObjToString();
            double serviceTotal = dx.Rows[0]["serviceTotal"].ObjToDouble();
            double merchandiseTotal = dx.Rows[0]["merchandiseTotal"].ObjToDouble();
            double allowMerchandise = dx.Rows[0]["allowMerchandise"].ObjToDouble();
            double allowInsurance = dx.Rows[0]["allowInsurance"].ObjToDouble();
            double downpayment = dx.Rows[0]["downPayment"].ObjToDouble();
            int numPayments = dx.Rows[0]["numberOfPayments"].ObjToInt32();
            if (monthlyPayment <= 0D)
                monthlyPayment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
            DateTime issueDate = dx.Rows[0]["issueDate8"].ObjToDateTime();
            issueDate = DailyHistory.GetIssueDate(issueDate, contractNumber, dx);
            double dp = 0D;
            double creditDue = dx.Rows[0]["creditBalance"].ObjToDouble();
            double financedAmount = serviceTotal + merchandiseTotal - allowMerchandise - allowInsurance - downpayment;
            financedAmount = DailyHistory.GetFinanceValue(dx.Rows[0]);
            double apr = dx.Rows[0]["apr"].ObjToDouble();
            apr = apr / 100D;

            double originalBalance = dx.Rows[0]["balanceDue"].ObjToDouble();
            DateTime dueDate = dx.Rows[0]["dueDate8"].ObjToDateTime();
            DateTime future = new DateTime(2039, 12, 31);
            if (dueDate >= future)
            {
                financedAmount = originalBalance;
                totalInterest = dx.Rows[0]["totalInterest"].ObjToDouble();
                nextDueDate = future;
                currentDueDate = future;
                return false;
            }

            DateTime majorNextDueDate = DateTime.Now;
            DateTime pDate = DateTime.Now;
            DateTime dDate = DateTime.Now;

            DataTable dt = G1.get_db_data("Select * from `" + paymentsFile + "` where `contractNumber` = '" + contractNumber + "' order by `paydate8` DESC, `tmstamp` DESC;");
            if (dt.Rows.Count <= 0)
            {
                if (insurance && !String.IsNullOrWhiteSpace(payer))
                {
                    if (currentPayment == monthlyPayment)
                    {
                        currentDueDate = dueDate;
                        nextDueDate = dueDate.ObjToDateTime();
                        nextDueDate = nextDueDate.AddMonths(1);
                        months = 1;
                        return true;
                    }
                    currentDueDate = dueDate;
                    dDate = dueDate.ObjToDateTime();
                    months = CalcMonthsForInsurance(contractNumber, payer, monthlyPayment, currentPayment, docp, dDate, ref creditBalance, ref nextDueDate);
                    //months = CheckMonthsForInsurance(contractNumber, payer, monthlyPayment, currentPayment, docp, dDate );
                    //months = G1.RoundValue(months);
                    //nextDueDate = dueDate.ObjToDateTime();
                    //int imonths = (int)months;
                    //creditBalance = currentPayment - (imonths.ObjToDouble() * monthlyPayment);
                    //if ( creditBalance < 0D)
                    //{
                    //    int nMonths = Convert.ToInt32((currentPayment / monthlyPayment));
                    //    creditBalance = currentPayment - (nMonths * monthlyPayment);
                    //    creditBalance = G1.RoundDown(creditBalance);
                    //}
                    //creditBalance = G1.RoundValue(creditBalance);
                    //nextDueDate = nextDueDate.AddMonths(imonths);
                    return true;
                }
                return false;
            }
            DailyHistory.CalculateNewStuff(dt, apr, numPayments, financedAmount, issueDate);
            if (!insurance)
            {
                dueDate = DailyHistory.getNextDueDate(dt, monthlyPayment, ref newbalance);
                if (currentPayment < monthlyPayment)
                    dueDate = currentDueDate;
            }
            creditDue = 0D;
            if (docp >= majorDate || recalculateHistory )
            {
                DateTime payDate8 = dt.Rows[0]["payDate8"].ObjToDateTime();
                if (payDate8 >= majorDate || recalculateHistory )
                    creditDue = dt.Rows[0]["runningCB"].ObjToDouble();
            }
            double payment = 0D;
            double debit = 0D;
            double credit = 0D;
            //if (dt.Rows.Count > 0)
            //{
            //    newbalance = dt.Rows[0]["newbalance"].ObjToDouble();
            //    newbalance = dt.Rows[0]["balance"].ObjToDouble();
            //    creditBalance = dt.Rows[0]["runningCB"].ObjToDouble(); //??????
            //    creditDue = creditBalance;
            //    majorNextDueDate = dt.Rows[0]["dueDate8"].ObjToDateTime();
            //    payment = dt.Rows[0]["paymentAmount"].ObjToDouble();
            //    credit = dt.Rows[0]["creditAdjustment"].ObjToDouble();
            //    debit = dt.Rows[0]["debitAdjustment"].ObjToDouble();
            //    months = (payment + credit - debit + creditDue) / monthlyPayment;
            //    int imonths = (int)months;
            //    majorNextDueDate = majorNextDueDate.AddMonths(imonths);
            //    dueDate = majorNextDueDate;
            //    currentDueDate = dueDate;
            //}

            DataView tempview = dt.DefaultView;
            tempview.Sort = "payDate8 asc";
            dt = tempview.ToTable();


            string date = "";
            double principal = 0D;
            double interest = 0D;
            double trust85P = 0D;
            double trust100P = 0D;
            double totalPaid = 0D;
            double newBalance = financedAmount;

            DateTime dolp = DateTime.Now;
            DateTime dopp = DateTime.Now;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                payment = getPayment(dt, i);

                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                newBalance = newBalance - payment + interest - credit + debit;

                totalInterest += interest;
                dolp = dt.Rows[i]["payDate8"].ObjToDateTime();
                dopp = dt.Rows[i]["dueDate8"].ObjToDateTime();
            }

            //if ( dopp == dueDate ) // Had to comment out because it caused 2 months to be added to the duedate
            //    dueDate = dueDate.AddMonths(1);

            nextDueDate = DateTime.Now;
            nextDueDate = docp;
            TimeSpan ts = nextDueDate - dolp;
            int days = ts.Days;
            interest = CalculateInterest(dolp, days, apr, newBalance);
            interest = G1.RoundValue(interest);
            double tempInt = interest;
            nextDueDate = dolp;
            double nowDue = 0D;
            double realPayment = 0D;
            //creditDue = 0D; // Ramma Zamma
            creditDue = G1.RoundValue(creditDue);
            currentPayment = currentPayment + creditAmount - debitAmount;
            if (creditAmount > 0D)
                currentPayment = creditAmount;
            if (debitAmount > 0D)
                currentPayment = debitAmount * -1D;
            currentPayment = G1.RoundValue(currentPayment);
            //currentPayment += creditDue; // Ramma Zamma
            currentPayment = G1.RoundValue(currentPayment);
            currentDueDate = dueDate;
            if ( currentPayment == monthlyPayment )
            {
                if ( !isInsurance ( contractNumber ))
                    dueDate = FindMismatches.VerifyDueDate(contractNumber);
                nextDueDate = dueDate.ObjToDateTime();
                nextDueDate = nextDueDate.AddMonths(1);
                months = 1;
                return true;
            }
            //else if ( currentPayment < monthlyPayment )
            //{
            //    nextDueDate = dueDate.ObjToDateTime();
            //    creditBalance = currentPayment;
            //    return true;
            //}

            months = 1D;
            principal = currentPayment;
            double originalPayment = principal;
            double paid = principal;
            int count = 0;
            bool maxedOut = false;

            if (insurance && !String.IsNullOrWhiteSpace(payer))
            {
                currentDueDate = dueDate;
                dDate = dueDate.ObjToDateTime();

                months = CalcMonthsForInsurance(contractNumber, payer, monthlyPayment, currentPayment, docp, dDate, ref creditBalance, ref nextDueDate);

                //months = CheckMonthsForInsurance(contractNumber, payer, monthlyPayment, currentPayment, docp, dDate);
                //double remainder = 0D;
                //currentPayment = months * monthlyPayment;
                //currentPayment = G1.RoundValue(currentPayment);
                //int imonths = (int) (Math.Truncate(months));
                //months = G1.RoundValue(months);
                //nextDueDate = dueDate.ObjToDateTime();
                //imonths = (int)months;
                //creditBalance = currentPayment - (imonths.ObjToDouble() * monthlyPayment);
                //creditBalance = G1.RoundValue(creditBalance);
                //nextDueDate = nextDueDate.AddMonths(imonths);
                return true;
            }

            if (principal > 0D)
            {
                if (interest >= principal)
                    principal = 0D;
                months = 0;
                months = principal / monthlyPayment; // Ramma Zamma
                if ( (months % 1D) > 0D )
                    months += 0.00000000001D;
                //principal = principal % monthlyPayment;
                //principal += 0.0000000001D;
                //nowDue = principal;
                //creditBalance = principal;
                for (; ; )
                {
                    principal = G1.RoundValue(principal);
                    if (principal < monthlyPayment)
                    {
                        nowDue = principal;
                        nowDue = G1.RoundValue(nowDue);
                        creditBalance = nowDue;
                        break;
                    }
                    principal = principal - monthlyPayment;
                    //months++;
                    count++;
                    if (count >= 1000)
                    {
                        maxedOut = true;
                        break;
                    }
                }
                if (!isInsurance(contractNumber))
                    dueDate = FindMismatches.VerifyDueDate(contractNumber);
                nextDueDate = dueDate.ObjToDateTime();
                if (maxedOut)
                    return false;
//                nextDueDate = nextDueDate.AddMonths((months.ObjToInt32() - 1));
                nextDueDate = nextDueDate.AddMonths((months.ObjToInt32()));
                if (nextDueDate < dueDate.ObjToDateTime())
                    nextDueDate = dueDate.ObjToDateTime();
            }
            else
            { // Must be a debit
                months = 0;
                principal = Math.Abs(principal);
                for (;;)
                {
                    principal = G1.RoundValue(principal);
                    if (principal >= monthlyPayment)
                    {
                        nowDue = principal;
                        nowDue = G1.RoundValue(nowDue);
                        creditBalance = nowDue;
                        months--;
                        break;
                    }
                    principal = principal - monthlyPayment;
                    months--;
                    count++;
                    if (count >= 1000)
                    {
                        maxedOut = true;
                        break;
                    }
                }
                nextDueDate = dueDate.ObjToDateTime();
                if (maxedOut)
                    return false;
//                nextDueDate = nextDueDate.AddMonths((months.ObjToInt32() - 1));
                nextDueDate = nextDueDate.AddMonths((months.ObjToInt32()));
            }
//            nextDueDate = majorNextDueDate;
            return true;
        }
        /***********************************************************************************************/
        public static bool DetermineDueDate(string contractNumber, DateTime docp, double monthlyPayment, double currentPayment, double debitAmount, double creditAmount, ref DateTime nextDueDate, ref double creditBalance, ref double months, ref double newbalance, ref DateTime currentDueDate)
        {
            creditBalance = 0D;
            newbalance = 0D;
            months = 0D;
            double totalInterest = 0D;
            if (contractNumber == "ZZ0002792")
            {

            }
            bool insurance = false;
            string contractsFile = "contracts";
            string customersFile = "customers";
            string paymentsFile = "payments";
            if (isInsurance(contractNumber))
            {
                insurance = true;
                contractsFile = "icontracts";
                customersFile = "icustomers";
                paymentsFile = "ipayments";
                //    return false;
            }
            string cmd = "Select * from `" + contractsFile + "` c JOIN `" + customersFile + "` d ON c.`contractNumber` = d.`contractNumber` where c.`contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return false;
            string payer = dx.Rows[0]["payer"].ObjToString();
            double serviceTotal = dx.Rows[0]["serviceTotal"].ObjToDouble();
            double merchandiseTotal = dx.Rows[0]["merchandiseTotal"].ObjToDouble();
            double allowMerchandise = dx.Rows[0]["allowMerchandise"].ObjToDouble();
            double allowInsurance = dx.Rows[0]["allowInsurance"].ObjToDouble();
            double downpayment = dx.Rows[0]["downPayment"].ObjToDouble();
            int numPayments = dx.Rows[0]["numberOfPayments"].ObjToInt32();
            if (monthlyPayment <= 0D)
                monthlyPayment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
            DateTime issueDate = dx.Rows[0]["issueDate8"].ObjToDateTime();
            issueDate = DailyHistory.GetIssueDate(issueDate, contractNumber, dx);
            double dp = 0D;
            double creditDue = dx.Rows[0]["creditBalance"].ObjToDouble();
            double financedAmount = serviceTotal + merchandiseTotal - allowMerchandise - allowInsurance - downpayment;
            financedAmount = DailyHistory.GetFinanceValue(dx.Rows[0]);
            double apr = dx.Rows[0]["apr"].ObjToDouble();
            apr = apr / 100D;

            double originalBalance = dx.Rows[0]["balanceDue"].ObjToDouble();
            DateTime dueDate = dx.Rows[0]["dueDate8"].ObjToDateTime();
            DateTime future = new DateTime(2039, 12, 31);
            if (dueDate >= future)
            {
                financedAmount = originalBalance;
                totalInterest = dx.Rows[0]["totalInterest"].ObjToDouble();
                nextDueDate = future;
                currentDueDate = future;
                return false;
            }

            DateTime majorNextDueDate = DateTime.Now;
            DateTime pDate = DateTime.Now;
            DateTime dDate = DateTime.Now;

            DataTable dt = G1.get_db_data("Select * from `" + paymentsFile + "` where `contractNumber` = '" + contractNumber + "' order by `paydate8` DESC, `tmstamp` DESC;");
            if (dt.Rows.Count <= 0)
            {
                if (insurance && !String.IsNullOrWhiteSpace(payer))
                {
                    if (currentPayment == monthlyPayment)
                    {
                        currentDueDate = dueDate;
                        nextDueDate = dueDate.ObjToDateTime();
                        nextDueDate = nextDueDate.AddMonths(1);
                        months = 1;
                        return true;
                    }
                    currentDueDate = dueDate;
                    dDate = dueDate.ObjToDateTime();
                    months = CalcMonthsForInsurance(contractNumber, payer, monthlyPayment, currentPayment, docp, dDate, ref creditBalance, ref nextDueDate);
                    //months = CheckMonthsForInsurance(contractNumber, payer, monthlyPayment, currentPayment, docp, dDate );
                    //months = G1.RoundValue(months);
                    //nextDueDate = dueDate.ObjToDateTime();
                    //int imonths = (int)months;
                    //creditBalance = currentPayment - (imonths.ObjToDouble() * monthlyPayment);
                    //if ( creditBalance < 0D)
                    //{
                    //    int nMonths = Convert.ToInt32((currentPayment / monthlyPayment));
                    //    creditBalance = currentPayment - (nMonths * monthlyPayment);
                    //    creditBalance = G1.RoundDown(creditBalance);
                    //}
                    //creditBalance = G1.RoundValue(creditBalance);
                    //nextDueDate = nextDueDate.AddMonths(imonths);
                    return true;
                }
                return false;
            }
            DailyHistory.CalculateNewStuff(dt, apr, numPayments, financedAmount, issueDate);
            if (!insurance)
            {
                dueDate = DailyHistory.getNextDueDate(dt, monthlyPayment, ref newbalance);
                if (currentPayment < monthlyPayment)
                    dueDate = currentDueDate;
            }
            creditDue = 0D;
            if (docp >= majorDate || recalculateHistory)
            {
                DateTime payDate8 = dt.Rows[0]["payDate8"].ObjToDateTime();
                if (payDate8 >= majorDate || recalculateHistory)
                    creditDue = dt.Rows[0]["runningCB"].ObjToDouble();
            }
            double payment = 0D;
            double debit = 0D;
            double credit = 0D;
            //if (dt.Rows.Count > 0)
            //{
            //    newbalance = dt.Rows[0]["newbalance"].ObjToDouble();
            //    newbalance = dt.Rows[0]["balance"].ObjToDouble();
            //    creditBalance = dt.Rows[0]["runningCB"].ObjToDouble(); //??????
            //    creditDue = creditBalance;
            //    majorNextDueDate = dt.Rows[0]["dueDate8"].ObjToDateTime();
            //    payment = dt.Rows[0]["paymentAmount"].ObjToDouble();
            //    credit = dt.Rows[0]["creditAdjustment"].ObjToDouble();
            //    debit = dt.Rows[0]["debitAdjustment"].ObjToDouble();
            //    months = (payment + credit - debit + creditDue) / monthlyPayment;
            //    int imonths = (int)months;
            //    majorNextDueDate = majorNextDueDate.AddMonths(imonths);
            //    dueDate = majorNextDueDate;
            //    currentDueDate = dueDate;
            //}

            DataView tempview = dt.DefaultView;
            tempview.Sort = "payDate8 asc";
            dt = tempview.ToTable();


            string date = "";
            double principal = 0D;
            double interest = 0D;
            double trust85P = 0D;
            double trust100P = 0D;
            double totalPaid = 0D;
            double newBalance = financedAmount;

            DateTime dolp = DateTime.Now;
            DateTime dopp = DateTime.Now;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                payment = getPayment(dt, i);

                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                newBalance = newBalance - payment + interest - credit + debit;

                totalInterest += interest;
                dolp = dt.Rows[i]["payDate8"].ObjToDateTime();
                dopp = dt.Rows[i]["dueDate8"].ObjToDateTime();
            }

            //if ( dopp == dueDate ) // Had to comment out because it caused 2 months to be added to the duedate
            //    dueDate = dueDate.AddMonths(1);

            nextDueDate = DateTime.Now;
            nextDueDate = docp;
            TimeSpan ts = nextDueDate - dolp;
            int days = ts.Days;
            interest = CalculateInterest(dolp, days, apr, newBalance);
            interest = G1.RoundValue(interest);
            double tempInt = interest;
            nextDueDate = dolp;
            double nowDue = 0D;
            double realPayment = 0D;
            //          creditDue = 69.96D;
            creditDue = G1.RoundValue(creditDue);
            currentPayment = currentPayment + creditAmount - debitAmount;
            if (creditAmount > 0D)
                currentPayment = creditAmount;
            if (debitAmount > 0D)
                currentPayment = debitAmount * -1D;
            currentPayment = G1.RoundValue(currentPayment);
            currentPayment += creditDue;
            currentPayment = G1.RoundValue(currentPayment);
            currentDueDate = dueDate;
            if (currentPayment == monthlyPayment)
            {
                nextDueDate = dueDate.ObjToDateTime();
                nextDueDate = nextDueDate.AddMonths(1);
                months = 1;
                return true;
            }
            //else if ( currentPayment < monthlyPayment )
            //{
            //    nextDueDate = dueDate.ObjToDateTime();
            //    creditBalance = currentPayment;
            //    return true;
            //}

            months = 1D;
            principal = currentPayment;
            double originalPayment = principal;
            double paid = principal;
            int count = 0;
            bool maxedOut = false;

            if (insurance && !String.IsNullOrWhiteSpace(payer))
            {
                currentDueDate = dueDate;
                dDate = dueDate.ObjToDateTime();

                months = CalcMonthsForInsurance(contractNumber, payer, monthlyPayment, currentPayment, docp, dDate, ref creditBalance, ref nextDueDate);

                //months = CheckMonthsForInsurance(contractNumber, payer, monthlyPayment, currentPayment, docp, dDate);
                //double remainder = 0D;
                //currentPayment = months * monthlyPayment;
                //currentPayment = G1.RoundValue(currentPayment);
                //int imonths = (int) (Math.Truncate(months));
                //months = G1.RoundValue(months);
                //nextDueDate = dueDate.ObjToDateTime();
                //imonths = (int)months;
                //creditBalance = currentPayment - (imonths.ObjToDouble() * monthlyPayment);
                //creditBalance = G1.RoundValue(creditBalance);
                //nextDueDate = nextDueDate.AddMonths(imonths);
                return true;
            }

            if (principal > 0D)
            {
                if (interest >= principal)
                    principal = 0D;
                months = 0;
                for (; ; )
                {
                    principal = G1.RoundValue(principal);
                    if (principal < monthlyPayment)
                    {
                        nowDue = principal;
                        nowDue = G1.RoundValue(nowDue);
                        creditBalance = nowDue;
                        break;
                    }
                    principal = principal - monthlyPayment;
                    months++;
                    count++;
                    if (count >= 1000)
                    {
                        maxedOut = true;
                        break;
                    }
                }
                nextDueDate = dueDate.ObjToDateTime();
                if (maxedOut)
                    return false;
                //                nextDueDate = nextDueDate.AddMonths((months.ObjToInt32() - 1));
                nextDueDate = nextDueDate.AddMonths((months.ObjToInt32()));
                if (nextDueDate < dueDate.ObjToDateTime())
                    nextDueDate = dueDate.ObjToDateTime();
            }
            else
            { // Must be a debit
                months = 0;
                principal = Math.Abs(principal);
                for (; ; )
                {
                    principal = G1.RoundValue(principal);
                    if (principal >= monthlyPayment)
                    {
                        nowDue = principal;
                        nowDue = G1.RoundValue(nowDue);
                        creditBalance = nowDue;
                        months--;
                        break;
                    }
                    principal = principal - monthlyPayment;
                    months--;
                    count++;
                    if (count >= 1000)
                    {
                        maxedOut = true;
                        break;
                    }
                }
                nextDueDate = dueDate.ObjToDateTime();
                if (maxedOut)
                    return false;
                //                nextDueDate = nextDueDate.AddMonths((months.ObjToInt32() - 1));
                nextDueDate = nextDueDate.AddMonths((months.ObjToInt32()));
            }
            //            nextDueDate = majorNextDueDate;
            return true;
        }
        /***********************************************************************************************/
        public static DateTime getNextDueDate ( DataTable dt, double monthlyPayment, ref double balanceDue )
        {
            balanceDue = 0D;
            if (dt.Rows.Count <= 0)
                return DateTime.Now;

            if (monthlyPayment <= 0D)
                return DateTime.Now;

            DataTable ddt = dt.Copy();
            DataView tempview = ddt.DefaultView;
            tempview.Sort = "payDate8 desc, tmstamp desc, record desc";
            ddt = tempview.ToTable();

            double payment = 0D;
            double downPayment = 0D;
            double debit = 0D;
            double credit = 0D;
            string status = "";
            if (ddt.Rows.Count > 0)
            {
                string contractNumber = ddt.Rows[0]["contractNumber"].ObjToString();
                if ( contractNumber == "WM13055UI")
                {
                }
            }
            DateTime nextDueDate = DateTime.Now;
            DateTime payDate8 = DateTime.Now;
            double months = 0D;
            int imonths = 0;
            double creditDue = 0D;
            double interestPaid = 0D;
            for ( int i=0; i< ddt.Rows.Count; i++)
            {
                status = ddt.Rows[i]["fill"].ObjToString();
                if (status.ToUpper() == "D")
                    continue;
                balanceDue = ddt.Rows[i]["balance"].ObjToDouble();
                nextDueDate = ddt.Rows[i]["dueDate8"].ObjToDateTime();
                if (nextDueDate.Year >= 2039)
                    break;
                downPayment = ddt.Rows[i]["downPayment"].ObjToDouble();
                if ( downPayment > 0D && ddt.Rows.Count == 1)
                {
                    nextDueDate = ddt.Rows[i]["nextDueDate"].ObjToDateTime();
                    break;
                }
                payDate8 = ddt.Rows[i]["payDate8"].ObjToDateTime();

                //payment = ddt.Rows[i]["paymentAmount"].ObjToDouble();
                payment = getPayment(ddt, i);

                interestPaid = ddt.Rows[i]["interestPaid"].ObjToDouble();
                credit = ddt.Rows[i]["creditAdjustment"].ObjToDouble();

                if (credit > 0D && interestPaid == 0D) // Don't consider this as (DOLP), I think
                    continue;
                if (credit != 0D)
                    continue;

                debit = ddt.Rows[i]["debitAdjustment"].ObjToDouble();
                if (debit != 0D)
                    continue;

                creditDue = 0D;
                if (payDate8 >= majorDate || recalculateHistory )
                {
                    if ( G1.get_column_number ( ddt, "runningCB") >= 0)
                        creditDue = ddt.Rows[i]["runningCB"].ObjToDouble();
                }
                if (monthlyPayment > 0D && payment == 0D )
                    payment = monthlyPayment;

                months = (payment + credit - debit + creditDue) / monthlyPayment;
                months = G1.RoundValue(months);
                imonths = (int)months;
                string contract = ddt.Rows[i]["contractNumber"].ObjToString().Trim();;
                nextDueDate = nextDueDate.AddMonths(imonths);
                break;
            }
            return nextDueDate;
        }
        /***********************************************************************************************/
        public static double getPayment(DataRow [] dt, int i)
        {
            double paymentAmount = dt[i]["paymentAmount"].ObjToDouble();
            double ccFee = dt[i]["ccFee"].ObjToDouble();
            paymentAmount -= ccFee;
            return paymentAmount;
        }
        /***********************************************************************************************/
        public static double getPayment ( DataTable dt, int i )
        {
            double paymentAmount = dt.Rows[i]["paymentAmount"].ObjToDouble();
            if (paymentAmount == 0D)
                return 0D;
            double ccFee = dt.Rows[i]["ccFee"].ObjToDouble();
            paymentAmount -= ccFee;
            return paymentAmount;
        }
        /***********************************************************************************************/
        public static double getDownPayment(DataTable dt, int i)
        {
            double ccFee = 0D;
            double downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
            if ( downPayment > 0D )
                ccFee = dt.Rows[i]["ccFee"].ObjToDouble();
            downPayment-= ccFee;
            return downPayment;
        }
        /***********************************************************************************************/
        public static bool CheckPayDate ( DateTime payDate, DateTime dueDate )
        {
            //if (1 == 1)
            //    return true; // Force true for now
            bool okay = false;
            if (payDate <= dueDate)
                okay = true;
            if ( !okay )
            { // For Debug
            }
            return okay;
        }
        /***********************************************************************************************/
//        public static double CheckMonthsForInsurance(string contractNumber, string payer, double expected, double paid, DateTime payDate, DateTime dueDate )
//        {
//            if ( payer == "CC5132")
//            {

//            }
//            double months = 0D;
//            if (expected > 0D )
//            { // Last Problem was CC-3256 Switched to Decimal
//                decimal dPaid = (decimal)paid;
//                decimal dExpected = (decimal)expected;
//                decimal dMonths = dPaid / dExpected;
//                months = (double)dMonths;
////                months = paid / expected;
//                months = Math.Truncate(months);
//                //months = G1.RoundValue(months);
//            }
//            try
//            {
//                if (!CheckPayDate ( payDate, dueDate ) )
//                    return months;
//                if (String.IsNullOrWhiteSpace(contractNumber))
//                    return months;
//                if (String.IsNullOrWhiteSpace(payer))
//                    return months;
//                double annualPremium = expected * 12D;
//                string cmd = "Select * from `icontracts` where `contractNumber` = '" + contractNumber + "';";
//                DataTable dx = G1.get_db_data(cmd);
//                if (dx.Rows.Count > 0)
//                {
//                    annualPremium = dx.Rows[0]["annualPremium"].ObjToDouble();
//                    if (annualPremium > 0D && payDate > DailyHistory.killSecNatDate )
//                        annualPremium = expected * 11D;
//                }
//                if ( annualPremium <= 0D)
//                    annualPremium = expected * 12D;
//                annualPremium = G1.RoundValue(annualPremium);
//                if (paid < annualPremium)
//                    return months;

//                double fn = paid / annualPremium;
//                int n = Convert.ToInt32(fn);
//                int additional = n;
//                double fr = Convert.ToDouble(n) * annualPremium;
//                fr = G1.RoundValue(fr);
//                double nfr = (paid - fr) / expected;
//                nfr = G1.RoundValue(nfr);
//                months = Convert.ToDouble((n * 12)) + Math.Truncate(nfr);

//                cmd = "Select * from `policies` where `payer` = '" + payer + "' AND `contractNumber` = '" + contractNumber + "' ORDER BY `contractNumber` DESC;";
//                dx = G1.get_db_data(cmd);
//                if (dx.Rows.Count <= 0)
//                {
//                    cmd = "Select * from `icustomers` where `payer` = '" + payer + "' AND `contractNumber` = '" + contractNumber + "' ORDER BY `contractNumber` DESC;";
//                    dx = G1.get_db_data(cmd);
//                    if (dx.Rows.Count <= 0)
//                        return months;
//                    string fname = dx.Rows[0]["firstName"].ObjToString();
//                    string lname = dx.Rows[0]["lastName"].ObjToString();
//                    cmd = "Select * from `policies` where `payer` = '" + payer + "' AND `firstName` = '" + fname + "' AND `lastName` = '" + lname + "' ORDER BY `contractNumber` DESC;";
//                    dx = G1.get_db_data(cmd);
//                    if (dx.Rows.Count <= 0)
//                    {
//                        cmd = "Select * from `policies` where `payer` = '" + payer + "' AND `policyNumber` = '" + payer + "' ORDER BY `contractNumber` DESC;";
//                        dx = G1.get_db_data(cmd);
//                        //if (paid == annualPremium)
//                        //    months = 12D;
//                        //else if (paid == (annualPremium * 2D))
//                        //    months = 24D;
//                        //months = CheckMonthlyBreak(payer, paid, months, expected, annualPremium);
//                        if ( dx.Rows.Count <= 0 )
//                            return months;
//                    }
//                }
//                if (months < 11D)
//                    return months;
//                string lastName = dx.Rows[0]["lastName"].ObjToString().Trim().ToUpper();
//                string firstName = dx.Rows[0]["firstName"].ObjToString().Trim().ToUpper();
//                string policyType = dx.Rows[0]["type"].ObjToString();
//                if (payer == "EV-090468" && lastName == "PITTS" && firstName == "R L")
//                {
//                    if (paid == 30.36D)
//                        months = 12;
//                }
//                else if (payer == "BB-4035" && lastName == "MANGUM" && firstName == "WILLIAM W")
//                {
//                    if (paid == 30.36D)
//                        months = 12;
//                }
//                else if (policyType == "B")
//                {
//                    string companyCode = dx.Rows[0]["companyCode"].ObjToString().Trim().ToUpper();
//                    if (companyCode != "MI" && companyCode != "MS" && companyCode != "FS")
//                    {
//                        string agentCode = dx.Rows[0]["oldAgentInfo"].ObjToString();
//                        if (agentCode != "P03" && agentCode != "P06" && agentCode != "T02")
//                        {
//                            string firstAgent = "";
//                            string firstPayer = "";
//                            if (agentCode.Length > 0)
//                                firstAgent = agentCode.Substring(0, 1);
//                            if (payer.Length > 0)
//                                firstPayer = payer.Substring(0, 1);
//                            if (firstAgent != "1" && firstAgent != "2" && firstAgent != "7")
//                            {
//                                if (firstPayer.ToUpper() != "V")
//                                {
//                                    months = CheckMonthlyBreak(payer, paid, months, expected, annualPremium);
//                                }
//                                else
//                                {
//                                    //months = CheckMonthlyBreak(payer, paid, months, expected, annualPremium);
//                                }
//                            }
//                        }
//                    }
//                }
//                else
//                {
//                    bool checkMonths = false;
//                    if (policyType == "L")
//                    {
//                        if (payer.Length >= 2)
//                        {
//                            string firstPayer = payer.Substring(0, 2);
//                            if (firstPayer != "VI")
//                                checkMonths = true;
//                        }
//                    }
//                    string companyCode = dx.Rows[0]["companyCode"].ObjToString().Trim().ToUpper();
//                    if (companyCode == "MI" || companyCode == "MS" || companyCode == "FS")
//                        checkMonths = true;
//                    string agentCode = dx.Rows[0]["oldAgentInfo"].ObjToString();
//                    if (agentCode == "P03" || agentCode == "P06" || agentCode == "T02")
//                        checkMonths = true;
//                    string firstAgent = "";
//                    if (agentCode.Length > 0)
//                        firstAgent = agentCode.Substring(0, 1);
//                    if (firstAgent != "1" || firstAgent == "2" || firstAgent == "7")
//                        checkMonths = true;
//                    if ((agentCode == "A00" || agentCode == "V00") && companyCode != "CA")
//                        checkMonths = true;
//                    if (checkMonths)
//                    {
//                        if (months >= 11D)
//                            months = 12D + Math.Truncate(nfr);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//            }
//            return months;
//        }
        /***********************************************************************************************/
        public static double CheckMonthlyBreak ( string payer, double paid, double months, double expected, double annualPremium )
        {
            double percentMoney = expected * 12D * .95D;
            annualPremium = G1.RoundValue(annualPremium);
            if (annualPremium > 0D)
                percentMoney = annualPremium;
            else
                percentMoney = Policies.CalcAnnualPremium(payer);
            percentMoney = G1.RoundDown(percentMoney);
            if (paid >= percentMoney)
            {
                double fn = paid / percentMoney;
                int n = Convert.ToInt32(fn);
                int additional = n;
                double fr = Convert.ToDouble(n) * percentMoney;
                fr = G1.RoundValue(fr);
                double nfr = (paid - fr) / expected;
                nfr = G1.RoundValue(nfr);
                months = Convert.ToDouble((n * 12)) + Math.Truncate ( nfr);

                //double fn = paid / percentMoney;
                //int n = Convert.ToInt32(fn);
                //int addition = n;
                //double fr = Convert.ToDouble(n) * percentMoney;
                //double nfr = (paid - fr) / expected;
                //months = Convert.ToDouble((n * 12) + addition) + G1.RoundDown(nfr);
            }
            return months;
        }
        /***********************************************************************************************/
        public static double GetInsuranceExpectedPayment ( DataTable dx, DateTime payDate )
        {
            double expected = 0D;
            double premium = 0D;
            DateTime deceasedDate = DateTime.Now;
            DateTime lapseDate = DateTime.Now;


            string report = "";
            string companyCode = "";
            bool gotSecNat = false;
            bool got3rdParty = false;
            string lapsed = "";

            for ( int i=0; i<dx.Rows.Count; i++)
            {
                if (payDate > killSecNatDate)
                {
                    report = dx.Rows[i]["report"].ObjToString();
                    if (String.IsNullOrWhiteSpace(report))
                        report = "No Report";
                    companyCode = dx.Rows[i]["companyCode"].ObjToString();
                    gotSecNat = CustomerDetails.isSecNat(companyCode);
                    got3rdParty = false;
                    if (!gotSecNat)
                    {
                        if (payDate > kill3rdPartyDate)
                        {
                            if (report.ToUpper() != "NOT THIRD PARTY")
                                got3rdParty = true;
                        }
                    }
                    if (gotSecNat)
                        continue;
                    if (got3rdParty)
                        continue;
                }
                lapsed = dx.Rows[i]["lapsed"].ObjToString().ToUpper();
                if ( lapsed == "Y")
                {
                }
                deceasedDate = dx.Rows[i]["deceasedDate"].ObjToDateTime();
                lapseDate = dx.Rows[i]["lapsedDate8"].ObjToDateTime();
                premium = dx.Rows[i]["premium"].ObjToDouble();
                if ( deceasedDate.Year > 100 )
                    premium = dx.Rows[i]["historicPremium"].ObjToDouble();
                else if (lapseDate.Year > 100)
                    premium = dx.Rows[i]["historicPremium"].ObjToDouble();
                if (deceasedDate.Year < 100)
                {
                    if (lapsed != "Y")
                        expected += premium;
                    else if (payDate < lapseDate)
                        expected += premium;
                }
                else
                {
                    if (deceasedDate < payDate)
                        continue;
                    if (lapsed != "Y")
                        expected += premium;
                }
            }
            return expected;
        }
        /***********************************************************************************************/
        public static void LoadExpectedPremiums ( DataTable dt, string payer )
        {
            string cmd = "Select * from `policies` where `payer` = '" + payer + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            DateTime payDate = DateTime.Now;
            double expectedPremium = 0D;
            double payment = 0D;
            double debit = 0D;
            double credit = 0D;
            double creditBalance = 0D;
            double runningCredit = 0D;
            double months = 0D;
            double remainder = 0D;
            double money = 0D;
            if (G1.get_column_number(dt, "runningCB") < 0)
                dt.Columns.Add("runningCB", Type.GetType("System.Double"));


            for ( int i=(dt.Rows.Count-1); i>=0; i--)
            {
                payDate = dt.Rows[i]["payDate8"].ObjToDateTime();
                if (payDate <= killSecNatDate)
                    continue;
                if ( payDate >= secondDate )
                {
                    months = dt.Rows[i]["numPayments"].ObjToDouble();
                    if (months == 2D)
                    {
                    }
                }
                payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();

                if (debit != 0D)
                    payment = debit * -1D;
                else if (credit != 0D)
                    payment = credit;

                expectedPremium = GetInsuranceExpectedPayment(dx, payDate);
                dt.Rows[i]["retained"] = expectedPremium;

                months = dt.Rows[i]["numPayments"].ObjToDouble();
                months = payment / expectedPremium;
                //dt.Rows[i]["numPayments"] = Convert.ToInt32(months);
                remainder = months % 1D;
                if (remainder != 0D)
                {

                    payment = expectedPremium * remainder;

                    creditBalance = payment;
                    creditBalance = G1.RoundValue(creditBalance);
                    dt.Rows[i]["creditBalance"] = creditBalance;

                    if ( payment > 0D )
                        runningCredit += creditBalance;
                    else
                    {
                        if (debit != 0D)
                        {
                            money = expectedPremium - (Math.Abs(payment));
                            //money = money * -1D;
                            runningCredit += money;
                        }
                    }

                    if (runningCredit > expectedPremium)
                    {
                        runningCredit = runningCredit % expectedPremium;
                    }
                    else
                    {
                        //money = expectedPremium - (Math.Abs(payment));
                        //money = money * -1D;
                        //    runningCredit += money;
                    }
                    G1.RoundValue(runningCredit);
                }
                dt.Rows[i]["runningCB"] = runningCredit;
                dt.Rows[i]["NumPayments"] = months;
            }
        }
        /***********************************************************************************************/
        public static double CheckMonthsForInsurance(string contractNumber, string payer, double expected, double paid, DateTime payDate, DateTime dueDate )
        {
            if ( payer == "CC5132")
            {

            }
            double months = 0D;
            if (expected > 0D )
            { // Last Problem was CC-3256 Switched to Decimal
                decimal dPaid = (decimal)paid;
                decimal dExpected = (decimal)expected;
                decimal dMonths = dPaid / dExpected;
                months = (double)dMonths;
//                months = paid / expected;
                months = Math.Truncate(months);
                //months = G1.RoundValue(months);
            }
            try
            {
                if (!CheckPayDate ( payDate, dueDate ) )
                    return months;
                if (String.IsNullOrWhiteSpace(contractNumber))
                    return months;
                if (String.IsNullOrWhiteSpace(payer))
                    return months;
                double annualPremium = expected * 12D;
                string cmd = "Select * from `icontracts` where `contractNumber` = '" + contractNumber + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    annualPremium = dx.Rows[0]["annualPremium"].ObjToDouble();
                    if (annualPremium > 0D && payDate > DailyHistory.killSecNatDate )
                        annualPremium = expected * 11D;
                }
                if ( annualPremium <= 0D)
                    annualPremium = expected * 12D;
                annualPremium = G1.RoundValue(annualPremium);
                if (paid < annualPremium)
                    return months;

                double fn = paid / annualPremium;
                int n = Convert.ToInt32(fn);
                int additional = n;
                double fr = Convert.ToDouble(n) * annualPremium;
                fr = G1.RoundValue(fr);
                double nfr = (paid - fr) / expected;
                nfr = G1.RoundValue(nfr);
                months = Convert.ToDouble((n * 12)) + Math.Truncate(nfr);

                cmd = "Select * from `policies` where `payer` = '" + payer + "' AND `contractNumber` = '" + contractNumber + "' ORDER BY `contractNumber` DESC;";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    cmd = "Select * from `icustomers` where `payer` = '" + payer + "' AND `contractNumber` = '" + contractNumber + "' ORDER BY `contractNumber` DESC;";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                        return months;
                    string fname = dx.Rows[0]["firstName"].ObjToString();
                    string lname = dx.Rows[0]["lastName"].ObjToString();
                    cmd = "Select * from `policies` where `payer` = '" + payer + "' AND `firstName` = '" + fname + "' AND `lastName` = '" + lname + "' ORDER BY `contractNumber` DESC;";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                    {
                        cmd = "Select * from `policies` where `payer` = '" + payer + "' AND `policyNumber` = '" + payer + "' ORDER BY `contractNumber` DESC;";
                        dx = G1.get_db_data(cmd);
                        //if (paid == annualPremium)
                        //    months = 12D;
                        //else if (paid == (annualPremium * 2D))
                        //    months = 24D;
                        //months = CheckMonthlyBreak(payer, paid, months, expected, annualPremium);
                        if ( dx.Rows.Count <= 0 )
                            return months;
                    }
                }
                if (months < 11D)
                    return months;
                string lastName = dx.Rows[0]["lastName"].ObjToString().Trim().ToUpper();
                string firstName = dx.Rows[0]["firstName"].ObjToString().Trim().ToUpper();
                string policyType = dx.Rows[0]["type"].ObjToString();
                if (payer == "EV-090468" && lastName == "PITTS" && firstName == "R L")
                {
                    if (paid == 30.36D)
                        months = 12;
                }
                else if (payer == "BB-4035" && lastName == "MANGUM" && firstName == "WILLIAM W")
                {
                    if (paid == 30.36D)
                        months = 12;
                }
                else if (policyType == "B")
                {
                    string companyCode = dx.Rows[0]["companyCode"].ObjToString().Trim().ToUpper();
                    if (companyCode != "MI" && companyCode != "MS" && companyCode != "FS")
                    {
                        string agentCode = dx.Rows[0]["oldAgentInfo"].ObjToString();
                        if (agentCode != "P03" && agentCode != "P06" && agentCode != "T02")
                        {
                            string firstAgent = "";
                            string firstPayer = "";
                            if (agentCode.Length > 0)
                                firstAgent = agentCode.Substring(0, 1);
                            if (payer.Length > 0)
                                firstPayer = payer.Substring(0, 1);
                            if (firstAgent != "1" && firstAgent != "2" && firstAgent != "7")
                            {
                                if (firstPayer.ToUpper() != "V")
                                {
                                    months = CheckMonthlyBreak(payer, paid, months, expected, annualPremium);
                                }
                                else
                                {
                                    //months = CheckMonthlyBreak(payer, paid, months, expected, annualPremium);
                                }
                            }
                        }
                    }
                }
                else
                {
                    bool checkMonths = false;
                    if (policyType == "L")
                    {
                        if (payer.Length >= 2)
                        {
                            string firstPayer = payer.Substring(0, 2);
                            if (firstPayer != "VI")
                                checkMonths = true;
                        }
                    }
                    string companyCode = dx.Rows[0]["companyCode"].ObjToString().Trim().ToUpper();
                    if (companyCode == "MI" || companyCode == "MS" || companyCode == "FS")
                        checkMonths = true;
                    string agentCode = dx.Rows[0]["oldAgentInfo"].ObjToString();
                    if (agentCode == "P03" || agentCode == "P06" || agentCode == "T02")
                        checkMonths = true;
                    string firstAgent = "";
                    if (agentCode.Length > 0)
                        firstAgent = agentCode.Substring(0, 1);
                    if (firstAgent != "1" || firstAgent == "2" || firstAgent == "7")
                        checkMonths = true;
                    if ((agentCode == "A00" || agentCode == "V00") && companyCode != "CA")
                        checkMonths = true;
                    if (checkMonths)
                    {
                        if (months >= 11D)
                            months = 12D + Math.Truncate(nfr);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return months;
        }
        /***********************************************************************************************/
        public static double CheckMonthsForInsuranceNew(string workContract, string payer, double expected, double paid, DateTime payDate, DateTime dueDate)
        {
            double months = 0D;
            if (expected > 0D)
                months = paid / expected;
            try
            {
                //if (!CheckPayDate(payDate, dueDate))
                //    return months;
                if (String.IsNullOrWhiteSpace(payer))
                    return months;
                double annualPremium = expected * 12D;
                string cmd = "Select * from `policies` where `payer` = '" + payer + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    return months;
                //if (months < 11D)
                //    return months;

                expected = GetInsuranceExpectedPayment(dx, payDate);
                if (expected > 0D)
                {
                    months = paid / expected;
                    months = G1.RoundValue(months);
                    annualPremium = expected * 12D;
                }
                if (months < 11D)
                    return months;


                string lastName = dx.Rows[0]["lastName"].ObjToString().Trim().ToUpper();
                string firstName = dx.Rows[0]["firstName"].ObjToString().Trim().ToUpper();
                string policyType = dx.Rows[0]["type"].ObjToString();
                if (payer == "EV-090468" && lastName == "PITTS" && firstName == "R L")
                {
                    if (paid == 30.36D)
                        months = 12;
                }
                else if (payer == "BB-4035" && lastName == "MANGUM" && firstName == "WILLIAM W")
                {
                    if (paid == 30.36D)
                        months = 12;
                }
                else if (policyType == "B")
                {
                    string companyCode = dx.Rows[0]["companyCode"].ObjToString().Trim().ToUpper();
                    if (companyCode != "MI" && companyCode != "MS" && companyCode != "FS")
                    {
                        string agentCode = dx.Rows[0]["oldAgentInfo"].ObjToString();
                        if (agentCode != "P03" && agentCode != "P06" && agentCode != "T02")
                        {
                            string firstAgent = "";
                            string firstPayer = "";
                            if (agentCode.Length > 0)
                                firstAgent = agentCode.Substring(0, 1);
                            if (payer.Length > 0)
                                firstPayer = payer.Substring(0, 1);
                            if (firstAgent != "1" && firstAgent != "2" && firstAgent != "7")
                            {
                                if (firstPayer.ToUpper() != "V")
                                {
                                    annualPremium = G1.RoundValue(annualPremium);
                                    double percentMoney = annualPremium * 0.95D;

                                    percentMoney = Policies.CalcAnnualPremium(payer, payDate );

                                    percentMoney = G1.RoundDown(percentMoney);
                                    if (paid >= percentMoney)
                                    {
                                        double remainder = paid - percentMoney;
                                        double extra = remainder / percentMoney;
                                        months = (paid / percentMoney) * 12D;
//                                        months = G1.RoundValue(months + 0.05D);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    bool checkMonths = false;
                    if (policyType == "L")
                    {
                        if (payer.Length >= 2)
                        {
                            string firstPayer = payer.Substring(0, 2);
                            if (firstPayer != "VI")
                                checkMonths = true;
                        }
                    }
                    string companyCode = dx.Rows[0]["companyCode"].ObjToString().Trim().ToUpper();
                    if (companyCode == "MI" || companyCode == "MS" || companyCode == "FS")
                        checkMonths = true;
                    string agentCode = dx.Rows[0]["oldAgentInfo"].ObjToString();
                    if (agentCode == "P03" || agentCode == "P06" || agentCode == "T02")
                        checkMonths = true;
                    string firstAgent = "";
                    if (agentCode.Length > 0)
                        firstAgent = agentCode.Substring(0, 1);
                    if (firstAgent != "1" || firstAgent == "2" || firstAgent == "7")
                        checkMonths = true;
                    if ((agentCode == "A00" || agentCode == "V00") && companyCode != "CA")
                        checkMonths = true;
                    if (checkMonths)
                    {
                        if (months == 11D)
                            months = 12D;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return months;
        }
        /***************************************************************************************/
        public static double CalculateInterest(DateTime dueDate, int days, double apr, double balance)
        {
            int yearlyDays = 365;
            if (DateTime.IsLeapYear(dueDate.Year))
                yearlyDays = 366;

//            string date = dueDate.Month.ToString("D2") + "/" + dueDate.Day.ToString("D2") + "/" + dueDate.Year.ToString("D4");

//            int days = DailyHistory.GetDaysSinceLastPayment(workContract, date);

            double dailyInterest = apr / (double)(yearlyDays) * (double)(days);
            double interest = dailyInterest * balance;
            interest = G1.RoundDown(interest);
            return interest;
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            string preference = G1.getPreference(LoginForm.username, "DailyHistory", "Add Manual Payment" );
            if (G1.RobbyServer)
                preference = "YES";
            if (preference != "YES")
            {
                MessageBox.Show("You do not have permission to perform this function!", "Edit Payments Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex( rowHandle);
            string contract = workContract;
            string name = workName;
            if (!String.IsNullOrWhiteSpace(contract))
            {
                DataTable dt = (DataTable)dgv.DataSource;
                double downPayment = dt.Rows[row]["downPayment"].ObjToDouble();
                if (downPayment == 0D)
                {
                    string editing = dt.Rows[row]["edited"].ObjToString();
                    if (editing.ToUpper() != "TRUSTADJ" || editing.ToUpper() == "CEMETERY" )
                    {
                        manualForm = new ManualPayment(contract, name, dt, trust85Actual, trust85Max, row);
                        manualForm.TopMost = true;
                        manualForm.ManualDone += ManualForm_ManualDone;
                        manualForm.ShowDialog();
                    }
                    else
                    {
                        AddEditTrustAdjustment(true);
                    }
                }
                else
                {
                    DialogResult result = MessageBox.Show("***Warning*** Do you want to change this DOWN PAYMENT?", "Change Down Payment Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.No)
                    {
                        MessageBox.Show("***INFO*** Okay, Nothing Changed!", "Down Payment Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    manualForm = new ManualPayment(contract, name, dt, downPayment, trust85Max, row, true);
                    manualForm.TopMost = true;
                    manualForm.ManualDone += ManualForm_ManualDone;
                    manualForm.ShowDialog();
                }
            }
        }
        /****************************************************************************************/
        private void btnSummarize_Click(object sender, EventArgs e)
        {
            gridMain.Columns["fill"].Visible = false;
            gridMain.Columns["agentNumber"].Visible = false;
            gridMain.Columns["trust85P"].Visible = false;
            gridMain.Columns["trust100P"].Visible = false;
            gridMain.Columns["downPayment"].Visible = false;
        }
        /****************************************************************************************/
        public static DataTable GetContractPayments(string contractNumber, DataTable dt = null )
        {
            DateTime dolp = DateTime.Now;
            DateTime dueDate = DateTime.Now;
            DateTime lastDOLP = DateTime.Now;
            string paymentFile = "payments";
            if ( isInsurance ( contractNumber))
                paymentFile = "ipayments";
            string cmd = "Select * from `" + paymentFile + "` where `contractNumber` = '" + contractNumber + "' order by `paydate8` DESC, `tmstamp` DESC;";
            DataTable dx = dt;
            if ( dx == null )
                dx = G1.get_db_data(cmd);
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                dueDate = dx.Rows[i]["dueDate8"].ObjToDateTime();
                if (dueDate.Year.ToString("D4").IndexOf("000") >= 0)
                    dx.Rows[i]["dueDate8"] = dx.Rows[i]["payDate8"];
            }

            string status = "";
            bool first = true;
            double credit = 0D;
            double debit = 0D;
            double interest = 0D;
            for ( int i=(dx.Rows.Count-1); i>= 0; i--)
            {
                status = dx.Rows[i]["fill"].ObjToString();
                if (status.ToUpper() == "D")
                    continue;

                credit = dx.Rows[i]["creditAdjustment"].ObjToDouble();
                debit = dx.Rows[i]["debitAdjustment"].ObjToDouble();
                interest = dx.Rows[i]["interestPaid"].ObjToDouble();
                if (credit > 0D && interest == 0D) // This is to avoid zero interest credits from having a (DOLP), because it isn't
                    continue;
                if (credit != 0D || debit != 0D)
                    continue;

                dolp = dx.Rows[i]["payDate8"].ObjToDateTime();
                if ( dolp.Year < 1900 )
                {
                    if ( first )
                    {
                        dolp = GetIssueDate(dolp, contractNumber, null);
                        dx.Rows[i]["payDate8"] = G1.DTtoMySQLDT(dolp);
                    }
                    else
                    {
                        dueDate = dx.Rows[i]["dueDate8"].ObjToDateTime();
                        if ( dueDate.Year > 1900)
                        {
                            dolp = dueDate;
                            dx.Rows[i]["payDate8"] = G1.DTtoMySQLDT(dolp);
                        }
                    }
                    first = false;
                }
                lastDOLP = dolp;
            }
            return dx;
        }
        /****************************************************************************************/
        public static int CalcTrust85Max(DataTable contractDt, DataTable paymentsDt, ref double trust85P, ref double trust100P, ref double oldTrust85P )
        {
            trust85P = 0D;
            trust100P = 0D;
            oldTrust85P = 0D;
            double contractValue = DailyHistory.GetContractValuePlus(contractDt.Rows[0]);
            if (contractValue <= 0D)
                return 0;
            string contractNumber = contractDt.Rows[0]["contractNumber"].ObjToString();
            double financeDays = contractDt.Rows[0]["numberOfPayments"].ObjToDouble();
            double downPayment = contractDt.Rows[0]["downPayment"].ObjToDouble();
            double principal = DailyHistory.GetFinanceValue ( contractDt.Rows[0] ) + downPayment;

            double payment = contractDt.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
            double amtOfMonthlyPayt = payment;
            int numPayments = contractDt.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
            string dueDate = contractDt.Rows[0]["dueDate8"].ObjToString();
            string issueDate = contractDt.Rows[0]["issueDate8"].ObjToString();
            DateTime iDate = GetIssueDate(contractDt.Rows[0]["issueDate8"].ObjToDateTime(), contractNumber, null);
            issueDate = iDate.ToString("MM/dd/yyyy");
            string apr = contractDt.Rows[0]["APR"].ObjToString();

            double rate = apr.ObjToDouble() / 100.0D;

            int method = ImportDailyDeposits.CalcTrust85(amtOfMonthlyPayt, issueDate, contractValue, downPayment, financeDays, payment, principal, rate, ref trust85P, ref trust100P);
            for ( int i=0; i<paymentsDt.Rows.Count; i++)
            {
                oldTrust85P += paymentsDt.Rows[i]["trust85P"].ObjToDouble();
            }
            return method;
        }
        /****************************************************************************************/
        public static int CalcTrust85(DataTable contractDt, ref double trust85P, ref double trust100P )
        {
            trust85P = 0D;
            trust100P = 0D;
            double contractValue = DailyHistory.GetContractValuePlus(contractDt.Rows[0]);
            if (contractValue <= 0D)
                return 0;
            string contractNumber = contractDt.Rows[0]["contractNumber"].ObjToString();
            double financeDays = contractDt.Rows[0]["numberOfPayments"].ObjToDouble();
            double downPayment = contractDt.Rows[0]["downPayment"].ObjToDouble();
            double principal = DailyHistory.GetFinanceValue(contractDt.Rows[0]) + downPayment;

            double payment = contractDt.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
            double amtOfMonthlyPayt = payment;
            int numPayments = contractDt.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
            string dueDate = contractDt.Rows[0]["dueDate8"].ObjToString();
            string issueDate = contractDt.Rows[0]["issueDate8"].ObjToString();
            DateTime iDate = GetIssueDate(contractDt.Rows[0]["issueDate8"].ObjToDateTime(), contractNumber, null);
            issueDate = iDate.ToString("MM/dd/yyyy");
            string apr = contractDt.Rows[0]["APR"].ObjToString();

            double rate = apr.ObjToDouble() / 100.0D;

            int method = ImportDailyDeposits.CalcTrust85(amtOfMonthlyPayt, issueDate, contractValue, downPayment, financeDays, payment, principal, rate, ref trust85P, ref trust100P);
            return method;
        }
        /****************************************************************************************/
        private void btnSelectColumns_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;
            SelectColumns sform = new SelectColumns(dgv, "DailyHistory", "Primary", actualName);
            sform.Done += new SelectColumns.d_void_eventdone(sform_Done);
            sform.Show();


            //string actualName = cmbSelectColumns.Text;
            //if (actualName.ToUpper().IndexOf("(C)") >= 0 && !LoginForm.administrator)
            //{
            //    MessageBox.Show("***Warning*** You do not have permission to modify a Common Display Format!", "Display Format Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //    return;
            //}
            ////            SelectColumns sform = new SelectColumns(dgv, "Funerals", "Primary", actualName);
            //string user = LoginForm.username;
            //SelectDisplayColumns sform = new SelectDisplayColumns(dgv, "DailyHistory", "Primary", actualName, LoginForm.username);
            //sform.Done += new SelectDisplayColumns.d_void_selectionDone(sform_Done);
            //sform.Show();

        }
        /***********************************************************************************************/
        void sform_Done(DataTable dt)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "DailyHistory " + name;
            string skinName = "";
            SetupSelectedColumns("DailyHistory", name, dgv);
            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            gridMain.OptionsView.ShowFooter = true;
            SetupTotalsSummary();
            string field = "";
            string select = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                field = dt.Rows[i]["field"].ObjToString();
                select = dt.Rows[i]["select"].ObjToString();
                if (G1.get_column_number(gridMain, field) >= 0)
                {
                    if (select == "0")
                        gridMain.Columns[field].Visible = false;
                    else
                        gridMain.Columns[field].Visible = true;
                }
            }
            dgv.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        void sform_Done()
        {
            dgv.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void cmbSelectColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            //ComboBox combo = (ComboBox)sender;
            //string comboName = combo.Text;
            string comboName = cmbSelectColumns.Text;
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                SetupSelectedColumns("DailyHistory", comboName, dgv);
                string name = "DailyHistory " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                SetupTotalsSummary();
                gridMain.OptionsView.ShowFooter = true;
            }
            else
            {
                SetupSelectedColumns("DailyHistory", "Primary", dgv);
                string name = "DailyHistory Primary";
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                gridMain.OptionsView.ShowFooter = true;
                SetupTotalsSummary();
            }

            CleanupFieldColumns();

            DataTable dt = (DataTable)dgv.DataSource;

            FixCemeteryView(dt);

            //gridMain.OptionsView.ShowBands = false;
            //gridMain.Columns["paidInFull"].Visible = false;
        }
        /***********************************************************************************************/
        private void CleanupFieldColumns()
        {
            if (LoginForm.classification.ToUpper() != "FIELD")
                return;
            //gridMain.Columns["amountGrowth"].Visible = false;
            //gridMain.Columns["amountDiscount"].Visible = false;
            //gridMain.Columns["contractNumber"].Visible = false;
        }
        /****************************************************************************************/
        //private void cmbSelectColumns_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    SetupSelectedColumns();
        //}
        /***********************************************************************************************/
        private void SetupSelectedColumns()
        {
            string group = cmbSelectColumns.Text.Trim().ToUpper();
            if (group.Trim().Length == 0)
                return;
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = 'DailyHistory' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < gridMain.Columns.Count; i++)
                gridMain.Columns[i].Visible = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                int index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
                    ((GridView)dgv.MainView).Columns[name].Visible = true;
                }
                catch
                {
                }
            }
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns(string procType, string group, DevExpress.XtraGrid.GridControl dgv)
        {
            if (String.IsNullOrWhiteSpace(group))
                return;
            if (String.IsNullOrWhiteSpace(procType))
                procType = "DailyHistory";
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + procType + "' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)dgv.MainView;
            for (int i = 0; i < gridMain.Columns.Count; i++)
                gridMain.Columns[i].Visible = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                int index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
                    ((GridView)dgv.MainView).Columns[name].Visible = true;
                }
                catch
                {
                }
            }
        }
        /***************************************************************************************/
        public bool FireEventReloadHeader()
        {
            //            LoadHeader();
            //DataTable dt = (DataTable)dgv.DataSource;
            //majorSwitch = false;
            //CalcNewAmort(dt);
            //LoadDetailHeader();
            //RecalcTotals();
            DailyHistory_Load(null, null);
            panelTop.Refresh();
            this.Refresh();
            majorSwitch = true;
            return false;
        }
        /****************************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            ReinstateReport report = new ReinstateReport(workContract);
            report.Show();
        }
        /****************************************************************************************/
        private void button2_Click(object sender, EventArgs e)
        {
            ReinstateReport report = new ReinstateReport(workContract, true);
            report.Show();
        }
        /****************************************************************************************/
        private void showOldDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (1 == 1)
            {
                MessageBox.Show("***INFO*** This option is not used at this time!\nCalculations Changed after last Import of Payment Data!");
                return;
            }
//            if ( showOldDetails )
//            {
//                showOldDetails = false;
//                txtAsOff.Show();
//                labPayoff.Show();
//                labEqual.Show();
//                txtPayoff.Show();
//                btnDetail.Show();
////                majorSwitch = true;
//                LoadHeader();
//                LoadData();
//                DataTable dt = (DataTable)dgv.DataSource;
//                LoadDetailHeader();
//                RecalcTotals();
//                showOldDetailsToolStripMenuItem.Text = "Show Old Details";
//            }
//            else
//            {
//                showOldDetails = true;
//                txtAsOff.Hide();
//                labPayoff.Hide();
//                labEqual.Hide();
//                txtPayoff.Hide();
//                btnDetail.Hide();
////                majorSwitch = false;
//                LoadHeader();
//                LoadData();
//                DataTable dt = (DataTable)dgv.DataSource;
//                LoadDetailHeader();
//                RecalcTotals();
//                showOldDetailsToolStripMenuItem.Text = "Show New Details";
//            }
        }
        /****************************************************************************************/
        private void txtAsOff_Enter(object sender, EventArgs e)
        {
            string date = txtAsOff.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                txtAsOff.Text = ddate.ToString("MM/dd/yyyy");
            }
            else
            {
                txtAsOff.Text = "";
            }
        }
        /****************************************************************************************/
        private void txtAsOff_Leave(object sender, EventArgs e)
        {
            string date = txtAsOff.Text;
            if (String.IsNullOrWhiteSpace(date))
            {
                txtPayoff.Text = "";
                return;
            }
            if (G1.validate_date(date))
            {
                DateTime ddate = txtAsOff.Text.ObjToDateTime();
                if (ddate.Year < 1800)
                {
                    MessageBox.Show("***ERROR*** Date Entered Year is less than 1800!");
                    txtPayoff.Text = "";
                    return;
                }
                if (ddate.Year > 100)
                {
                    txtAsOff.Text = ddate.ToString("MM/dd/yyyy");
                    RecalcTotals();
                }
                else
                    txtPayoff.Text = "";
            }
            else
            {
                MessageBox.Show("***ERROR*** Invalid Date!");
            }
        }
        /****************************************************************************************/
        private void txtAsOff_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtAsOff_Leave(sender, e);
            else if (e.KeyCode == Keys.Tab)
                txtAsOff_Leave(sender, e);
        }
        /****************************************************************************************/
        private void btnDetail_Click(object sender, EventArgs e)
        {
            doPayoff("");
        }
        /****************************************************************************************/
        private void doPayoff ( string payWhat )
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            int lastRow = 0;
            if (reverseSort)
                lastRow = dt.Rows.Count - 1;
            double balanceDue = dt.Rows[lastRow]["newbalance"].ObjToDouble();
            balanceDue = dt.Rows[lastRow]["balance"].ObjToDouble();
            if (isInsurance(workContract))
                balanceDue = Policies.CalcMonthlyPremium(workPayer, DateTime.Now);
            //            labBalanceDue.Text = "$" + G1.ReformatMoney(balanceDue);
            DateTime now = DateTime.Now;
            if (payWhat.IndexOf("-10") > 0)
            {
                now = now.AddDays(10);
                payWhat = "Packet Payoff";
                txtAsOff.Text = now.ToString("MM/dd/yyyy");
            }
            string str = txtAsOff.Text;
            if (String.IsNullOrWhiteSpace(str))
                txtAsOff.Text = now.ToString("MM/dd/yyyy");
            str = txtAsOff.Text;
            if (G1.validate_date(str))
                now = str.ObjToDateTime();

            DateTime dolp = dt.Rows[lastRow]["payDate8"].ObjToDateTime();
            getDOLP(dt, ref dolp); // This has to be done to avoid Credit as the (DOLP)

            double oldBalance = balanceDue;
            DateTime docp = now;
            str = lblAPR.Text.Trim();
            str = str.Replace("APR :", "");
            str = str.Replace("%", "");
            double apr = str.ObjToDouble();

            string lblDue = lblDueDate.Text;
            lblDue = lblDue.Replace("Due Date:", "").Trim();
            DateTime dueDate = lblDue.ObjToDateTime();

            this.Cursor = Cursors.WaitCursor;

            saveDt = (DataTable)dgv.DataSource;
            DataTable tempDt = saveDt.Copy();

            if (!String.IsNullOrWhiteSpace(payWhat))
            {
                PayOffDetail payForm = new PayOffDetail(workContract, oldBalance, dolp, docp, apr, dueDate, tempDt, payWhat );
                payForm.Show();
            }
            else
            {
                PayOffDetail payForm = new PayOffDetail(workContract, oldBalance, dolp, docp, apr, dueDate, tempDt);
                payForm.CA_Done += PayForm_CA_Done;
                payForm.Show();
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable saveDt = null;
        /****************************************************************************************/
        private void PayForm_CA_Done()
        {
            dgv.DataSource = saveDt;
            //LoadData();
            DataTable dt = (DataTable)dgv.DataSource;
            CalcNewAmort(dt);
            LoadDetailHeader();
            RecalcTotals();
        }
        /****************************************************************************************/
        private void DailyHistory_FormClosing(object sender, FormClosingEventArgs e)
        {
            showOldDetails = false;
            G1.CleanupDataGrid(ref dgv);
            GC.Collect();
        }
        /****************************************************************************************/
        private void enterTrustAdjustmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string contract = workContract;
            string name = workName;
            DataTable workDt = (DataTable)dgv.DataSource;

            using (TrustAdjustment trustForm = new TrustAdjustment(workContract, workName, DateTime.Now, 0D, 0D, 0D, 0D, "", "", false))
            {
                DialogResult result = trustForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string cmd = "Select * from `customers` where `contractNumber` = '" + workContract + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                    {
                        MessageBox.Show("***ERROR*** Locating Customer for Contract (" + workContract + ")!");
                        return;
                    }
                    string firstName = dx.Rows[0]["firstName"].ObjToString();
                    string lastName = dx.Rows[0]["lastName"].ObjToString();
                    string agent = dx.Rows[0]["agentCode"].ObjToString();
                    string loc = "";
                    string trust = "";
                    string miniContract = Trust85.decodeContractNumber(workContract, ref trust, ref loc);

                    loc = "None";

                    DateTime trustDate = trustForm.TrustDate;
                    double trust100Amount = trustForm.Trust100Amount;
                    double trust85Amount = trustForm.Trust85Amount;
                    double trustRetained = trustForm.TrustRetained;
                    double trustInterest = trustForm.TrustInterest;
                    string trustDepositNumber = trustForm.TrustDepositNumber;
                    string trustReason = trustForm.TrustReason;
                    string record = G1.create_record("payments", "lastName", "-1");
                    if (G1.BadRecord("payments", record))
                        return;
                    //double trust100P = trustAmount / 0.85D;
                    //trust100P = G1.RoundValue(trust100P);
                    G1.update_db_table("payments", "record", record, new string[] { "contractNumber", workContract, "lastName", lastName, "firstName", firstName, "paymentAmount", "0", "interestPaid", "0", "debitAdjustment", "0", "creditAdjustment", "0", "debitReason", "", "creditReason", "" });
                    G1.update_db_table("payments", "record", record, new string[] { "CheckNumber", "", "dueDate8", trustDate.ToString("yyyy-MM-dd"), "payDate8", trustDate.ToString("yyyy-MM-dd"), "trust85P", trust85Amount.ToString(), "trust100P", trust100Amount.ToString(), "retained", trustRetained.ToString(), "interestPaid", trustInterest.ToString(), "location", loc, "agentNumber", agent, "userId", LoginForm.username, "depositNumber", trustDepositNumber, "edited", "TrustAdj" });
                    if (trust100Amount > 0D)
                        G1.update_db_table("payments", "record", record, new string[] { "creditReason", trustReason});
                    else
                        G1.update_db_table("payments", "record", record, new string[] { "debitReason", trustReason });
                    LoadData();
                    DataTable dt = (DataTable)dgv.DataSource;
                    CalcNewAmort(dt);
                    LoadDetailHeader();
                    RecalcTotals();
                }
            }
        }
        /****************************************************************************************/
        private void AddEditTrustAdjustment( bool editing )
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string contract = workContract;
            string name = workName;
            DataTable workDt = (DataTable)dgv.DataSource;

            double trust100Adjustment = 0D;
            double trust85Adjustment = 0D;
            double retained = 0D;
            double interest = 0D;
            string depositNumber = "";
            string reason = "";
            DateTime trustDate = DateTime.Now;
            if ( editing)
            {
                trust100Adjustment = dr["trust100P"].ObjToDouble();
                trust85Adjustment = dr["trust85P"].ObjToDouble();
                if (trust85Adjustment > 0D)
                    reason = dr["creditReason"].ObjToString();
                else
                    reason = dr["debitReason"].ObjToString();
                retained = dr["retained"].ObjToDouble();
                interest = dr["interestPaid"].ObjToDouble();
                trustDate = dr["payDate8"].ObjToDateTime();
                depositNumber = dr["depositNumber"].ObjToString();
            }

            using (TrustAdjustment trustForm = new TrustAdjustment(workContract, workName, trustDate, trust100Adjustment, trust85Adjustment, retained, interest, depositNumber, reason, editing))
            {
                DialogResult result = trustForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string cmd = "Select * from `customers` where `contractNumber` = '" + workContract + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                    {
                        MessageBox.Show("***ERROR*** Locating Customer for Contract (" + workContract + ")!");
                        return;
                    }
                    string firstName = dx.Rows[0]["firstName"].ObjToString();
                    string lastName = dx.Rows[0]["lastName"].ObjToString();
                    string agent = dx.Rows[0]["agentCode"].ObjToString();
                    string loc = "";
                    string trust = "";
                    string miniContract = Trust85.decodeContractNumber(workContract, ref trust, ref loc);

                    loc = "None";

                    trustDate = trustForm.TrustDate;
                    double trust100Amount = trustForm.Trust100Amount;
                    double trust85Amount = trustForm.Trust85Amount;
                    double trustRetained = trustForm.TrustRetained;
                    double trustInterest = trustForm.TrustInterest;
                    string trustDepositNumber = trustForm.TrustDepositNumber;
                    string trustReason = trustForm.TrustReason;
                    string record = "";
                    if (!editing)
                    {
                        record = G1.create_record("payments", "lastName", "-1");
                        if (G1.BadRecord("payments", record))
                            return;
                    }
                    else
                        record = dr["record"].ObjToString();
                    //double trust100P = trustAmount / 0.85D;
                    //trust100P = G1.RoundValue(trust100P);
                    G1.update_db_table("payments", "record", record, new string[] { "contractNumber", workContract, "lastName", lastName, "firstName", firstName, "paymentAmount", "0", "interestPaid", "0", "debitAdjustment", "0", "creditAdjustment", "0", "debitReason", "", "creditReason", "" });
                    G1.update_db_table("payments", "record", record, new string[] { "CheckNumber", "", "dueDate8", trustDate.ToString("yyyy-MM-dd"), "payDate8", trustDate.ToString("yyyy-MM-dd"), "trust85P", trust85Amount.ToString(), "trust100P", trust100Amount.ToString(), "retained", trustRetained.ToString(), "interesstPaid", trustInterest.ToString(), "location", loc, "agentNumber", agent, "userId", LoginForm.username, "depositNumber", trustDepositNumber, "edited", "TrustAdj" });
                    if (trust100Amount > 0D)
                        G1.update_db_table("payments", "record", record, new string[] { "creditReason", trustReason });
                    else
                        G1.update_db_table("payments", "record", record, new string[] { "debitReason", trustReason });
                    LoadData();
                    DataTable dt = (DataTable)dgv.DataSource;
                    CalcNewAmort(dt);
                    LoadDetailHeader();
                    RecalcTotals();
                }
            }
        }
        /****************************************************************************************/
        private void chkChonological_CheckedChanged(object sender, EventArgs e)
        {
            RecalcTotals();
        }
        /****************************************************************************************/
        private void lockScreenDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "DailyHistory " + name;
            G1.SaveLocalPreferences(this, gridMain, LoginForm.username, saveName);

            //G1.SaveLocalPreferences(this, gridMain, LoginForm.username, "DailyHistoryLayout" );
            foundLocalPreference = true;
        }
        /****************************************************************************************/
        private void unLockScreenDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string comboName = cmbSelectColumns.Text;
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                string name = "DailyHistory " + comboName;
                if (String.IsNullOrWhiteSpace(name))
                    name = "Primary";
                string saveName = "DailyHistory " + name;
                G1.RemoveLocalPreferences(LoginForm.username, saveName);
                foundLocalPreference = false;
            }

            //G1.RemoveLocalPreferences(LoginForm.username, "DailyHistoryLayout");
            foundLocalPreference = false;
        }
        /***********************************************************************************************/
        private void skinsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SkinSelect skinForm = new SkinSelect("");
            skinForm.SkinSelected += skinForm_SkinSelected;
            skinForm.Show();
        }
        /***********************************************************************************************/
        void skinForm_SkinSelected(string s)
        {
            if (s.ToUpper().IndexOf("SKIN : ") >= 0)
            {
                string skin = s.Replace ("Skin : ", "");
                if (skin.Trim().Length == 0)
                    skin = "Windows Default";
                if (skin == "Windows Default")
                {
                    this.LookAndFeel.SetSkinStyle(skin);
                    this.gridMain.Appearance.EvenRow.BackColor = System.Drawing.Color.LightGreen;
                    this.gridMain.Appearance.EvenRow.BackColor2 = System.Drawing.Color.LightGreen;
                    this.gridMain.Appearance.SelectedRow.BackColor = System.Drawing.Color.Yellow;
                    this.gridMain.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Black;
                }
                else
                {
                    this.panelTop.BackColor = Color.Transparent;
                    this.menuStrip1.BackColor = Color.Transparent;
                    this.gridMain.PaintStyleName = "Skin";
                    DevExpress.Skins.SkinManager.EnableFormSkins();
                    this.LookAndFeel.UseDefaultLookAndFeel = true;
                    DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle(skin);
                    this.LookAndFeel.SetSkinStyle(skin);
                    this.dgv.LookAndFeel.SetSkinStyle(skin);
                    this.dgv.LookAndFeel.SkinName = skin;
                    gridMain.Appearance.EvenRow.Options.UseBackColor = false;
                    gridMain.Appearance.OddRow.Options.UseBackColor = false;
                    this.panelTop.Refresh();
                    OnSkinChange(skin);

                    //DevExpress.LookAndFeel.UserLookAndFeel.Default.SkinName = skin;
                    //this.LookAndFeel.SetSkinStyle(skin);
                    //this.dgv.LookAndFeel.SetSkinStyle(skin);
                }
            }
            else if (s.ToUpper().IndexOf("COLOR : ") >= 0)
            {
                string color = s.Replace( "Color : ", "");
                this.gridMain.Appearance.EvenRow.BackColor = Color.FromName(color);
                this.gridMain.Appearance.EvenRow.BackColor2 = Color.FromName(color);
                this.gridMain.Appearance.SelectedRow.BackColor = System.Drawing.Color.Yellow;
                this.gridMain.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Black;
                this.gridMain.Appearance.EvenRow.Options.UseBackColor = true;
                this.gridMain.Appearance.OddRow.Options.UseBackColor = true;
            }
            else if (s.ToUpper().IndexOf("NO COLOR ON") >= 0)
            {
                this.gridMain.Appearance.EvenRow.Options.UseBackColor = false;
                this.gridMain.Appearance.OddRow.Options.UseBackColor = false;
            }
            else if (s.ToUpper().IndexOf("NO COLOR OFF") >= 0)
            {
                this.gridMain.Appearance.EvenRow.Options.UseBackColor = true;
                this.gridMain.Appearance.OddRow.Options.UseBackColor = true;
            }
        }
        /****************************************************************************************/
        private void btnUpdateInt_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            string record = "";
            double interest = 0D;

            this.Cursor = Cursors.WaitCursor;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                if (!String.IsNullOrWhiteSpace(record))
                {
                    interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                    G1.update_db_table("payments", "record", record, new string[] { "interestPaid", interest.ToString() });
                }
            }
            this.Cursor = Cursors.Arrow;
        }
        /****************************************************************************************/
        public static bool CalculateNewInterest(string contractNumber, string findRecord, ref double interest, ref double principal)
        {
            interest = 0D;
            principal = 0D;
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return false;

            double payment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
            int numPayments = dx.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
            double totalInterest = dx.Rows[0]["totalInterest"].ObjToString().ObjToDouble();
            string dueDate = dx.Rows[0]["dueDate8"].ObjToString();
            string issueDate = dx.Rows[0]["issueDate8"].ObjToString();
            DateTime iDate = DailyHistory.GetIssueDate(dx.Rows[0]["issueDate8"].ObjToDateTime(), contractNumber, dx);
            issueDate = iDate.ToString("MM/dd/yyyy");
            DateTime lastDate = issueDate.ObjToDateTime();
            if (issueDate.IndexOf("0000") >= 0)
                return false;

            string apr = dx.Rows[0]["APR"].ObjToString();
            double dAPR = apr.ObjToDouble() / 100.0D;

            double startBalance = DailyHistory.GetFinanceValue(dx.Rows[0]);

            cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' order by `paydate8` DESC, `tmstamp` DESC;";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return false;

            DailyHistory.CalculateNewStuff(dx, dAPR, numPayments, startBalance, lastDate);

            if ( DailyHistory.gotCemetery ( dx ))
                return false ;

            bool rtn = false;
            double trust85P = 0D;
            double retained = 0D;
            double paymentAmount = 0D;
            string record = "";
            for (int j = 0; j < dx.Rows.Count; j++)
            {
                if (dx.Rows[j]["fill"].ObjToString().ToUpper() == "D")
                    continue;
                record = dx.Rows[j]["record"].ObjToString();
                if (record == findRecord)
                {
                    interest = dx.Rows[j]["int"].ObjToDouble();
                    interest = G1.RoundValue(interest);
                    trust85P = dx.Rows[j]["calculatedTrust85"].ObjToDouble();

                    //paymentAmount = dx.Rows[j]["paymentAmount"].ObjToDouble();
                    paymentAmount = getPayment(dx, j);

                    if ( interest == 0D && paymentAmount > 0D )
                    {
                        if ( dx.Rows[j]["interestPaid"].ObjToDouble() != 0D )
                        {
                            if (dx.Rows[j]["interestPaid"].ObjToDouble() == paymentAmount)
                                interest = dx.Rows[j]["interestPaid"].ObjToDouble();
                        }
                    }

                    retained = dx.Rows[j]["retained"].ObjToDouble();
                    principal = dx.Rows[j]["principal"].ObjToDouble();
                    if ( paymentAmount == interest && retained != interest && contractNumber.ToUpper().EndsWith ( "LI" ))
                    {
                        if (retained == 0D)
                            retained = interest;
                        interest = retained;
                        principal = paymentAmount - interest;
                    }
                    rtn = true;
                    break;
                }
            }
            return rtn;
        }
        /****************************************************************************************/
        private void reversePaymentMenu_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string contract = workContract;
            string name = workName;
            DialogResult result = MessageBox.Show("Are you sure you want to create a Reverse Payment for customer (" + workContract + ") ?", "Reverse Payment Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;
            if (String.IsNullOrWhiteSpace(contract))
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            string record = dt.Rows[row]["record"].ObjToString();
            double downPayment = dt.Rows[row]["downPayment"].ObjToDouble();
            double payment = dt.Rows[row]["paymentAmount"].ObjToDouble();
            double ccFee = dt.Rows[row]["ccFee"].ObjToDouble();
            double originalPayment = payment;
            double credit = dt.Rows[row]["creditAdjustment"].ObjToDouble();
            double debit = dt.Rows[row]["debitAdjustment"].ObjToDouble();
            double interest = dt.Rows[row]["interestPaid"].ObjToDouble();
            double trust100P = dt.Rows[row]["trust100P"].ObjToDouble();
            double trust85P = dt.Rows[row]["trust85P"].ObjToDouble();
            string loc = dt.Rows[row]["location"].ObjToString();
            double oldBalance = dt.Rows[row]["oldBalance"].ObjToDouble();
            if (oldBalance == 0D && row > 0)
                oldBalance = dt.Rows[row - 1]["balance"].ObjToDouble();
            DateTime oldDueDate = dt.Rows[row]["oldDueDate8"].ObjToDateTime();
            if (oldDueDate.Year <= 2 && row > 0)
                oldDueDate = dt.Rows[row - 1]["currentDueDate8"].ObjToDateTime();
            DateTime oldDOLP = dt.Rows[row]["oldDOLP"].ObjToDateTime();
            if (oldDOLP.Year <= 2 && row > 0 )
                oldDOLP = dt.Rows[row - 1]["payDate8"].ObjToDateTime();

            DataTable dx = dt.Clone();
            G1.copy_dt_row(dt, row, dx, 0);

            downPayment = downPayment * -1D;
            payment = payment * -1D;
            ccFee = ccFee * -1D;
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
            if ( creditReason.ToUpper().IndexOf ( "REVERSAL" ) >= 0 )
            {
                result = MessageBox.Show("You CANNOT Reverse a Reversal for customer (" + workContract + ") !!", "BAD-Reversal Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            creditReason += " Reversal";

            record = G1.create_record(paymentsFile, "lastName", "-1");
            G1.update_db_table(paymentsFile, "record", record, new string[] { "contractNumber", workContract, "lastName", lastName, "firstName", firstName, "paymentAmount", payment.ToString(), "ccFee", ccFee.ToString(), "interestPaid", interest.ToString(), "debitAdjustment", debit.ToString(), "creditAdjustment", credit.ToString(), "debitReason", debitReason, "creditReason", creditReason });
            G1.update_db_table(paymentsFile, "record", record, new string[] { "CheckNumber", checknumber, "dueDate8", dueDate, "payDate8", datePaid, "trust85P", trust85P.ToString(), "trust100P", trust100P.ToString(), "location", location, "agentNumber", agent, "userId", user, "depositNumber", depositNumber, "edited", "Manual" });

            string cmd = "Select * from `" + contractsFile + "` where `contractNumber` = '" + workContract + "';";
            dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count > 0 )
            {
                record = dx.Rows[0]["record"].ObjToString();
                G1.update_db_table(contractsFile, "record", record, new string[] {"balanceDue", oldBalance.ToString(), "dueDate8", oldDueDate.ToString("yyyy-MM-dd"), "lastDatePaid8", oldDOLP.ToString("yyyy-MM-dd")});
            }

            ReverseACH(workContract, depositNumber, originalPayment);

            string audit = "Paid Date: " + datePaid + " Pmt/Credit/Debit: " + payment.ToString() + "/" + credit.ToString() + "/" + debit.ToString();
            G1.AddToAudit(LoginForm.username, "ManualPayment", "Reversal", audit, workContract);

            DailyHistory_Load(null, null);

            //LoadData();
            //dt = (DataTable)dgv.DataSource;
            //CalcNewAmort(dt);
            //LoadDetailHeader();
            //RecalcTotals();
        }
        /****************************************************************************************/
        private void ReverseACH(string contractNumber, string depositNumber, double payment)
        {
            if (String.IsNullOrWhiteSpace(depositNumber))
                return;
            string str = depositNumber.Substring(0, 1);
            if (str.ToUpper() != "A")
                return;
            string cmd = "Select * from `ach` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            string record = dt.Rows[0]["record"].ObjToString();
            int leftPayments = dt.Rows[0]["leftPayments"].ObjToInt32();
            int numPayments = dt.Rows[0]["numPayments"].ObjToInt32();
            double achPayment = dt.Rows[0]["payment"].ObjToDouble();
            if (achPayment == payment)
            {
                leftPayments = leftPayments + 1;
                if ( leftPayments <= numPayments)
                    G1.update_db_table("ach", "record", record, new string[] { "leftPayments", leftPayments.ToString() });
            }
        }
        /****************************************************************************************/
        public static string ParseOutLocations(DataTable dt)
        {
            string locations = "";
            string contractNumber = "";
            string trust = "";
            string loc = "";
            string email = "";
            string emailLocations = "";

            DataTable dx = G1.get_db_data("Select * from `funeralHomes`;");
            if (dx.Rows.Count <= 0)
                return "";
            DataRow[] dRows = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                contractNumber = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                if (!locations.Contains(loc))
                {
                    locations += loc + ",";
                    dRows = dx.Select("keycode='" + loc + "'");
                    if (dRows.Length > 0)
                    {
                        email = dRows[0]["email"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(email))
                            emailLocations += email + ";";
                    }
                }
            }
            emailLocations = emailLocations.TrimEnd(';');
            dx.Dispose();
            dx = null;
            return emailLocations;
        }
        /****************************************************************************************/
        private void btnRequest_Click(object sender, EventArgs e)
        {
            bool insurance = isInsurance(workContract);
            ReinstateRequest reinstateForm = new ReinstateRequest( workContract, insurance, workPayments, paymentdRow );
            reinstateForm.Show();
        }
        /****************************************************************************************/
        private void explainTrust85ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string contract = workContract;
            string name = workName;
            DataTable workDt = (DataTable)dgv.DataSource;
            ExplainTrust85 trustForm = new ExplainTrust85(workContract, workName, workDt, row);
            trustForm.Show();
        }
        /****************************************************************************************/
        public static void RemoveDeletedPayments ( DataTable dt)
        {
            string status = "";
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            if (G1.get_column_number(dt, "fill") < 0)
                return;
            for ( int i=(dt.Rows.Count-1); i>=0; i--)
            {
                status = dt.Rows[i]["fill"].ObjToString();
                if (status.ToUpper() == "D")
                    dt.Rows.RemoveAt(i);
            }
        }
        /****************************************************************************************/
        public static void RemoveTrustAdjustments(DataTable dt, DateTime startDate, DateTime stopDate )
        {
            string status = "";
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            DateTime now = DateTime.Now;
            string contractNumber = "";
            double trust85P = 0D;
            string date = stopDate.ToString("yyyy-MM-dd");
            date += " 23:59:59";
            DateTime newStop = date.ObjToDateTime();
            stopDate = newStop;
            try
            {
                for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                {
                    status = dt.Rows[i]["edited"].ObjToString();
                    if (status.ToUpper() == "TRUSTADJ" )
                    {
                        contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();;
                        //if ( contractNumber == "B17059UI")
                        //{
                        //}
                        trust85P = dt.Rows[i]["trust85P"].ObjToDouble();
                        now = dt.Rows[i]["tmstamp2"].ObjToDateTime();
                        if (now < DailyHistory.interestDate)
                        {
                            if (now.Year > 100)
                            {
                                if (now < startDate || now > stopDate)
                                    dt.Rows.RemoveAt(i);
                            }
                        }
                    }
                }
            }
            catch ( Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            string status = dt.Rows[row]["fill"].ObjToString().ToUpper();
            if (status.ToUpper() == "D" && !chkLoadAll.Checked)
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private void btnRecalculate_Click(object sender, EventArgs e)
        {
            recalculateHistory = true;
            LoadData();
            DataTable dt = (DataTable)dgv.DataSource;
            CalcNewAmort(dt);
            LoadDetailHeader();
            RecalcTotals();
            recalculateHistory = false;
        }
        /****************************************************************************************/
        private void copyPreviousLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string contract = workContract;
            string name = workName;
            DialogResult result = MessageBox.Show("Are you sure you want to Copy Previous Location for customer (" + workContract + ") ?", "Copy Location Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;
            if (String.IsNullOrWhiteSpace(contract))
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            try
            {
                string record = dt.Rows[row]["record"].ObjToString();
                string location = dt.Rows[row - 1]["location"].ObjToString();
                G1.update_db_table(paymentsFile, "record", record, new string[] { "location", location});
            }
            catch ( Exception ex)
            {
                MessageBox.Show("***ERROR*** Problem Copying Previous Location!");
            }
        }
        /****************************************************************************************/
        public static double GetPossibleDBR(string contractNumber)
        {
            string cmd = "Select * from `customers` a JOIN `contracts` b on a.`contractNumber` = b.`contractNumber` where a.`contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return 0D;

            DateTime deceasedDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();

            int deceasedYear = deceasedDate.Year;
            if (deceasedYear <= 1000)
                return 0D;
            int deceasedMonth = deceasedDate.Month;

            DateTime startDate = new DateTime(deceasedYear, deceasedMonth, 1);

            cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "'  AND `payDate8` >= '" + startDate.ToString("yyyy-MM-dd") + "' ORDER BY `payDate8` DESC, `tmstamp` DESC;";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return 0D;

            double dbr = 0D;

            for ( int i=0; i<dx.Rows.Count; i++)
                dbr += dx.Rows[i]["trust100P"].ObjToDouble();

            return dbr;
        }
        /****************************************************************************************/
        public static DataTable GetPaymentData(string contractNumber, DateTime maxDate, double myOriginalDownPayment, bool greater = false )
        {
            string cmd = "Select * from `customers` a JOIN `contracts` b on a.`contractNumber` = b.`contractNumber` where a.`contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return null;

            //double payment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
            //double ExpectedPayment = payment;
            int numPayments = dx.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
            //double totalInterest = dx.Rows[0]["totalInterest"].ObjToString().ObjToDouble();
            //string dueDate = dx.Rows[0]["dueDate8"].ObjToString();
            DateTime contractDate = dx.Rows[0]["contractDate"].ObjToDateTime();
            string issueDate = dx.Rows[0]["issueDate8"].ObjToString();
            DateTime iDate = GetIssueDate(dx.Rows[0]["issueDate8"].ObjToDateTime(), contractNumber, dx);
            issueDate = iDate.ToString("MM/dd/yyyy");
            if (issueDate.IndexOf("0000") >= 0)
                issueDate = contractDate.Month.ToString("D2") + "/" + contractDate.Day.ToString("D2") + "/" + contractDate.Year.ToString("D4");
            DateTime lastDate = issueDate.ObjToDateTime();

            //DateTime firstPayDate = dx.Rows[0]["firstPayDate"].ObjToDateTime();
            //iDate = issueDate.ObjToDateTime();
            //firstPayDate = DailyHistory.CheckFirstPayDate(firstPayDate, iDate);

            string apr = dx.Rows[0]["APR"].ObjToString();

            double dAPR = apr.ObjToDouble() / 100.0D;

            //majorSwitch = true;
            double startBalance = 0D;

            cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "'  AND `payDate8` <= '" + maxDate.ToString("yyyy-MM-dd") + "' ORDER BY `payDate8` DESC, `tmstamp` DESC;";
            if (greater)
            {
                cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "'  AND `payDate8` <= '" + maxDate.ToString("yyyy-MM-dd") + "' ORDER BY `payDate8` DESC, `tmstamp` DESC LIMIT 5;";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    lastDate = dx.Rows[0]["payDate8"].ObjToDateTime();
                    startBalance = dx.Rows[0]["oldBalance"].ObjToDouble();
                    startBalance = startBalance - dx.Rows[0]["paymentAmount"].ObjToDouble() - dx.Rows[0]["ccFee"].ObjToDouble() - dx.Rows[0]["creditAdjustment"].ObjToDouble() + dx.Rows[0]["debitAdjustment"].ObjToDouble() + dx.Rows[0]["interestPaid"].ObjToDouble();
                }
                else
                    lastDate = maxDate;
                cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "'  AND `payDate8` > '" + maxDate.ToString("yyyy-MM-dd") + "' ORDER BY `payDate8` DESC, `tmstamp` DESC;";
            }

            dx = G1.get_db_data(cmd);

            if (numPayments <= 0 && dx.Rows.Count > 0)
                numPayments = dx.Rows.Count;


            CalculateNewStuff(dx, dAPR, numPayments, startBalance, lastDate);
            return dx;
        }
        /****************************************************************************************/
        public static DataTable CalcPaymentData(string contractNumber, string findRecord, ref double interest, ref double trust85, ref double trust100 )
        {
            interest = 0D;
            trust85 = 0D;
            trust100 = 0D;
            string cmd = "Select * from `customers` a JOIN `contracts` b on a.`contractNumber` = b.`contractNumber` where a.`contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return null;

            //double payment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
            //double ExpectedPayment = payment;
            int numPayments = dx.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
            //double totalInterest = dx.Rows[0]["totalInterest"].ObjToString().ObjToDouble();
            //string dueDate = dx.Rows[0]["dueDate8"].ObjToString();
            DateTime contractDate = dx.Rows[0]["contractDate"].ObjToDateTime();
            string issueDate = dx.Rows[0]["issueDate8"].ObjToString();
            DateTime iDate = GetIssueDate(dx.Rows[0]["issueDate8"].ObjToDateTime(), contractNumber, dx);
            issueDate = iDate.ToString("MM/dd/yyyy");
            if (issueDate.IndexOf("0000") >= 0)
                issueDate = contractDate.Month.ToString("D2") + "/" + contractDate.Day.ToString("D2") + "/" + contractDate.Year.ToString("D4");
            DateTime lastDate = issueDate.ObjToDateTime();

            //DateTime firstPayDate = dx.Rows[0]["firstPayDate"].ObjToDateTime();
            //iDate = issueDate.ObjToDateTime();
            //firstPayDate = DailyHistory.CheckFirstPayDate(firstPayDate, iDate);

            string apr = dx.Rows[0]["APR"].ObjToString();

            double dAPR = apr.ObjToDouble() / 100.0D;

            //majorSwitch = true;
            double startBalance = 0D;

            cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' ORDER BY `payDate8` DESC, `tmstamp` DESC;";

            dx = G1.get_db_data(cmd);

            if (numPayments <= 0 && dx.Rows.Count > 0)
                numPayments = dx.Rows.Count;

            if ( contractNumber == "CT19037LI")
            {
            }

            CalculateNewStuff(dx, dAPR, numPayments, startBalance, lastDate);
            if ( String.IsNullOrWhiteSpace ( findRecord ))
            {
            }
            DataRow[] dRows = dx.Select("record='" + findRecord + "'");
            if ( dRows.Length > 0 )
            {
                DataTable dd = dRows.CopyToDataTable();
                interest = dd.Rows[0]["interestPaid"].ObjToDouble();
                trust85 = dd.Rows[0]["trust85P"].ObjToDouble();
                trust100 = dd.Rows[0]["trust100P"].ObjToDouble();
            }
            return dx;
        }
        /****************************************************************************************/
        public static DateTime GetDOLPfromPayments ( string contractNumber)
        {
            DateTime dolp = DateTime.Now;
            DataTable dx = DailyHistory.GetPaymentData(contractNumber);
            if ( dx != null)
            {
                if ( dx.Rows.Count > 0 )
                {
                    dolp = dx.Rows[0]["payDate8"].ObjToDateTime();
                    getDOLP(dx, ref dolp); // This is to avoid Credit being (DOLP)
                }
            }
            return dolp;
        }
        /****************************************************************************************/
        public static DataTable GetPaymentData(string contractNumber )
        {
            bool insurance = false;
            if (DailyHistory.isInsurance(contractNumber))
                insurance = true;

            string cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' ORDER BY `payDate8` DESC, `tmstamp` DESC;";
            if ( insurance )
            {
                string ccd = "SELECT * from `icustomers` where `contractNumber`= '" + contractNumber + "';";
                DataTable ddx = G1.get_db_data(ccd);
                if (ddx.Rows.Count > 0)
                {
                    string workPayer = ddx.Rows[0]["payer"].ObjToString();
                    string list = "";
                    for (int i = 0; i < ddx.Rows.Count; i++)
                    {
                        string contract = ddx.Rows[i]["contractNumber"].ObjToString().Trim();;
                        list += "'" + contract + "',";
                    }
                    list = list.TrimEnd(',');
                    list = "(" + list + ")";
                    cmd = "Select * from `ipayments` where `contractNumber` IN " + list + " order by `payDate8` DESC, `tmstamp` DESC;";
                }
            }
            DataTable dx = G1.get_db_data(cmd);
            return dx;
        }
        /****************************************************************************************/
        private void showOldDetailsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(workPayer))
            {
                MessageBox.Show("***INFO*** This option only works for Insurance Payers");
                return;
            }
            string cmd = "Select * from `old_ipayments` where `contractNumber` = '" + workContract + "' GROUP BY `location`;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Cannot find old payments for this Insurance Payer");
                return;
            }
            string locations = "";
            string location = "";

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                location = dt.Rows[i]["location"].ObjToString().Trim();
                if (String.IsNullOrWhiteSpace(location))
                    continue;
                if (location.ToUpper() == "NONE")
                    continue;
                if (location.ToUpper() == "ACH")
                    continue;
                locations += location + "\n";
            }
            if ( String.IsNullOrWhiteSpace ( locations ))
            {
                MessageBox.Show("***ERROR*** Cannot find old payments for this Insurance Payer");
                return;
            }
            string[] Lines = locations.Split('\n');
            string str = "Old Location Could be :";
            for ( int i=0; i<Lines.Length; i++)
            {
                location = Lines[i].Trim();
                if ( !String.IsNullOrWhiteSpace ( location))
                    str += "\n" + location;
            }
            DialogResult result = MessageBox.Show(str, "Possible Locations Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            if (e.Column.FieldName.Trim() == "dueDate8Print" )
            {
                string dueDate = dr["dueDate8Print"].ObjToString();
                if ( !G1.validate_date ( dueDate))
                {
                    MessageBox.Show("***ERROR*** Invalid Date Entered!\nDue Date Will Not Be Changed!", "Due Date Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                string record = dr["record"].ObjToString();
                string payments = "payments";
                if (isInsurance(workContract))
                    payments = "ipayments";
                G1.update_db_table(payments, "record", record, new string[] { "dueDate8", dueDate });
                DateTime dueDate8 = dueDate.ObjToDateTime();
                dr["dueDate8Print"] = dueDate8.ToString("MM/dd/yyyy");
                dr["dueDate8"] = G1.DTtoMySQLDT(dueDate8);
            }
            else if (e.Column.FieldName.Trim().ToUpper() == "APR")
            {
                decimal apr = dr["apr"].ObjToDecimal();
                string record = dr["record"].ObjToString();
                string payments = "payments";
                if (isInsurance(workContract))
                    payments = "ipayments";
                G1.update_db_table(payments, "record", record, new string[] { "apr", apr.ToString(), "lockInterest", "Y" });
                dr["lockInterest"] = "Y";
            }
            else if (e.Column.FieldName.Trim().ToUpper() == "NOTES")
            {
                string notes = dr["notes"].ObjToString();
                notes = notes.Replace("\n", " ");
                string record = dr["record"].ObjToString();
                string payments = "payments";
                if (isInsurance(workContract))
                    payments = "ipayments";
                G1.update_db_table(payments, "record", record, new string[] { "notes", notes.ToString() });
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
                originalSize = gridMain.Columns["lastName"].AppearanceCell.Font.Size;
                mainFont = gridMain.Columns["lastName"].AppearanceCell.Font;
            }
            double scale = txtScale.Text.ObjToDouble();
            double size = scale / 100D * originalSize;
            Font font = new Font(mainFont.Name, (float)size);
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                gridMain.Columns[i].AppearanceCell.Font = font;
            }
            gridMain.Appearance.GroupFooter.Font = font;
            gridMain.AppearancePrint.FooterPanel.Font = font;
            newFont = font;
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
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
        private void txtDOFP_Enter(object sender, EventArgs e)
        {
            string date = txtDOFP.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                txtDOFP.Text = ddate.ToString("MM/dd/yyyy");
            }
            else
                txtDOFP.Text = "";
        }
        /****************************************************************************************/
        private void txtDOFP_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtDOFP_Leave(sender, e);
            else if (e.KeyCode == Keys.Tab)
                txtDOFP_Leave(sender, e);
        }
        /****************************************************************************************/
        private void txtDOFP_Leave(object sender, EventArgs e)
        {
            string date = txtDOFP.Text;
            if (String.IsNullOrWhiteSpace(date))
                return;
            if (G1.validate_date(date))
            {
                DateTime ddate = txtDOFP.Text.ObjToDateTime();
                if (ddate.Year < 1800)
                {
                    MessageBox.Show("***ERROR*** Date Entered Year is less than 1800!", "Date Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                if (ddate.Year > 100)
                {
                    txtDOFP.Text = ddate.ToString("MM/dd/yyyy");

                    string cmd = "Select * from `" + customersFile + "` where `contractNumber` = '" + workContract + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        string record = dx.Rows[0]["record"].ObjToString();
                        G1.update_db_table(customersFile, "record", record, new string[] { "firstPayDate", ddate.ToString("yyyy-MM-dd") });
                    }
                }
            }
            else
            {
                MessageBox.Show("***ERROR*** Invalid Date!", "Date Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        /***************************************************************************************/
        public DataTable FireEventDailyReturn()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            return dt;
        }
        /****************************************************************************************/
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            PaymentsReport payForm = new PaymentsReport(workContract);
            //payForm.ShowDialog();
        }

        private void chkDueDate_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDueDate.Checked)
                doNewFix = true;
            else
                doNewFix = false;

            DailyHistory_Load(null, null);
        }
        /****************************************************************************************/
        private void addDownPaymentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!G1.isAdmin())
            {
                MessageBox.Show("*** Sorry *** You do not have permission to add a Down Payment!!", "Down Payment Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            DataTable dt = (DataTable)dgv.DataSource;
            DialogResult result = MessageBox.Show("***Warning*** Do you want to add a new DOWN PAYMENT?", "Add Down Payment Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                MessageBox.Show("***INFO*** Okay, Nothing Changed!", "New Down Payment Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string contract = workContract;
            string name = workName;
            manualForm = new ManualPayment(contract, name, dt, trust85Actual, trust85Max, 9999, true);
            manualForm.TopMost = true;
            manualForm.ManualDone += ManualForm_ManualDone;
            manualForm.ShowDialog();
            return;
        }
        /****************************************************************************************/
        private void addTCAToolStripMenuItem_Clickx(object sender, EventArgs e)
        {
            //DataRow dr = gridMain.GetFocusedDataRow();
            //int rowHandle = gridMain.FocusedRowHandle;
            //int row = gridMain.GetDataSourceRowIndex(rowHandle);
            //string contract = workContract;
            //string name = workName;
            //DialogResult result = MessageBox.Show("Are you sure you want to ADD a TCA Payment for customer (" + workContract + ") ?", "Add TCA Payment Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            //if (result == DialogResult.No)
            //    return;
            //if (String.IsNullOrWhiteSpace(contract))
            //    return;
            //DataTable dt = (DataTable)dgv.DataSource;
            ////string record = dt.Rows[row]["record"].ObjToString();
            ////double downPayment = dt.Rows[row]["downPayment"].ObjToDouble();
            ////double payment = dt.Rows[row]["paymentAmount"].ObjToDouble();
            ////double ccFee = dt.Rows[row]["ccFee"].ObjToDouble();
            ////double originalPayment = payment;
            ////double credit = dt.Rows[row]["creditAdjustment"].ObjToDouble();
            ////double debit = dt.Rows[row]["debitAdjustment"].ObjToDouble();
            ////double interest = dt.Rows[row]["interestPaid"].ObjToDouble();
            ////double trust100P = dt.Rows[row]["trust100P"].ObjToDouble();
            ////double trust85P = dt.Rows[row]["trust85P"].ObjToDouble();
            ////string loc = dt.Rows[row]["location"].ObjToString();
            ////double oldBalance = dt.Rows[row]["oldBalance"].ObjToDouble();
            ////if (oldBalance == 0D && row > 0)
            ////    oldBalance = dt.Rows[row - 1]["balance"].ObjToDouble();
            ////DateTime oldDueDate = dt.Rows[row]["oldDueDate8"].ObjToDateTime();
            ////if (oldDueDate.Year <= 2 && row > 0)
            ////    oldDueDate = dt.Rows[row - 1]["currentDueDate8"].ObjToDateTime();
            ////DateTime oldDOLP = dt.Rows[row]["oldDOLP"].ObjToDateTime();
            ////if (oldDOLP.Year <= 2 && row > 0)
            ////    oldDOLP = dt.Rows[row - 1]["payDate8"].ObjToDateTime();

            //string cmd = "Select * from `contracts` where `contractNumber` = '" + workContract + "';";
            //DataTable dx = G1.get_db_data(cmd);
            //if (dx.Rows.Count <= 0)
            //    return;
            //double amtOfMonthlyPayt = dx.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
            //double contractValue = DailyHistory.GetContractValue(workContract);

            //double maxTrust85 = 0D;
            //double totalTrust85 = 0D;
            //bool trustThreshold = false;
            //bool balanceThreshold = false;
            //double trust85P = 0D;

            //bool isPaid = Customers.CheckForcedPayoff(workContract, amtOfMonthlyPayt, numPayments, ref maxTrust85, ref totalTrust85, ref contractValue, ref trustThreshold, ref balanceThreshold, trust85P);

            //string record = G1.create_record("payments", "lastName", "-1");
            //if (G1.BadRecord("payments", record))
            //    return;
            //retained = -1D * paidOffTrust100;

            //string TCADepositNumber = ImportDailyDeposits.getNextTCANumber();
            //TCADepositNumber = "TCA-" + TCADepositNumber;

            //if (workContract.Contains("LI"))
            //{
            //    PutMoney("Interest", retained);
            //    G1.update_db_table(paymentFile, "record", record, new string[] { "contractNumber", workContract, "lastName", lastName, "firstName", firstName, "paymentAmount", "0.00", "interestPaid", retained.ToString(), "ccFee", "0.00", "debitAdjustment", "0.00", "creditAdjustment", "0.00", "debitReason", "", "creditReason", "TCA", "unpaid_interest", "0.00" });
            //    G1.update_db_table(paymentFile, "record", record, new string[] { "CheckNumber", checknumber, "dueDate8", dueDate, "payDate8", datePaid, "trust85P", paidOffTrust85.ToString(), "trust100P", paidOffTrust100.ToString(), "retained", "0.00", "location", location, "agentNumber", agent, "userId", user, "depositNumber", TCADepositNumber, "edited", "TrustAdj", "bank_account", bankAccount });
            //}
            //else
            //{
            //    PutMoney("Interest", 0D);
            //    G1.update_db_table(paymentFile, "record", record, new string[] { "contractNumber", workContract, "lastName", lastName, "firstName", firstName, "paymentAmount", "0.00", "interestPaid", "0.00", "ccFee", "0.00", "debitAdjustment", "0.00", "creditAdjustment", "0.00", "debitReason", "", "creditReason", "TCA", "unpaid_interest", "0.00" });
            //    G1.update_db_table(paymentFile, "record", record, new string[] { "CheckNumber", checknumber, "dueDate8", dueDate, "payDate8", datePaid, "trust85P", paidOffTrust85.ToString(), "trust100P", paidOffTrust100.ToString(), "retained", retained.ToString(), "location", location, "agentNumber", agent, "userId", user, "depositNumber", TCADepositNumber, "edited", "TrustAdj", "bank_account", bankAccount });
            //}



            ////double Trust85Paid = workTrust85C + trust85P;
            ////difference = maxTrust85 - totalTrust85;
            ////difference = G1.RoundValue(difference);
            ////difference = maxTrust85 - Trust85Paid;
            ////difference = G1.RoundValue(difference);
            ////if (difference < 0D)
            ////{
            ////    difference = 0D;
            ////    isPaid = false;
            ////}

            ////txtTrust85Paid.Text = "$" + G1.ReformatMoney(Trust85Paid);
            ////if (isPaid)
            ////{
            ////    txtTrust85Paid.ForeColor = Color.White;
            ////    txtTrust85Paid.BackColor = Color.Pink;
            ////    if (isPaid)
            ////    {
            ////        txtTrust85Paid.BackColor = Color.LimeGreen;
            ////        PutData("Next Due Date", "12/31/2039");
            ////        row = FindRow("trust85P");
            ////        if (row >= 0)
            ////        {
            ////            trust85P = trust85P + difference;
            ////            if (trust85P <= 0D)
            ////                trust85P = 0D;
            ////            p_trust85P = G1.ReformatMoney(trust85P);
            ////            dt.Rows[row]["newpayment"] = p_trust85P;
            ////            txtTrust85Paid.Text = "$" + G1.ReformatMoney(totalTrust85 + difference);
            ////            row = FindRow("trust100P");
            ////            {
            ////                trust100P = trust85P / 0.85D;
            ////                trust100P = G1.RoundValue(trust100P);
            ////                p_trust100P = G1.ReformatMoney(trust100P);
            ////                dt.Rows[row]["newpayment"] = p_trust100P;
            ////            }
            ////        }
            ////        txtTrust85Paid.Refresh();
            ////    }
            ////}
            ////else
            ////{
            ////    txtTrust85Paid.BackColor = Color.White;
            ////    txtTrust85Paid.ForeColor = Color.Black;
            ////    txtTrust85Paid.Refresh();
            ////}

            ////string PaymentDetil = " of " + G1.ReformatMoney(workTrust85Max) + " Expected";
            ////if (difference != 0D && isPaid)
            ////{
            ////    difference = G1.RoundValue(difference);
            ////    double credit100P = difference / 0.85D;
            ////    credit100P = G1.RoundValue(credit100P);
            ////    PaymentDetil += "   Payoff Trust Credit : " + G1.ReformatMoney(difference) + " / " + G1.ReformatMoney(credit100P);
            ////}

        }
        /****************************************************************************************/
        private void addTCAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string preference = G1.getPreference(LoginForm.username, "DailyHistory", "Add Manual Payment", true);
            if (G1.RobbyServer)
                preference = "YES";
            if (preference != "YES")
                return;

            //public ManualPayment(string contract, string name, DataTable dt, double Trust85C, double Trust85Max, int row = -1, bool changeDownPayment = false, bool tcaReport = false, bool justTCA = false)


            DataTable dt = (DataTable)dgv.DataSource;
            manualForm = new ManualPayment(true, workContract, workName, dt, trust85Actual, trust85Max );
            manualForm.ManualDone += ManualForm_TCADone;
            manualForm.ShowDialog();
        }
        /****************************************************************************************/
        private void ManualForm_TCADone(string s)
        {
            try
            {
                if (manualForm != null)
                {
                    if (manualForm.Visible)
                    {
                        manualForm.Hide();
                        G1.ClearControl(panelBottom, "ManualPayment");
                        DailyHistory_Load(null, null);
                        cmbSelectColumns_SelectedIndexChanged(null, null);
                        //                        dgv.Show();
                        //                        btnAdd.Show();
                        //                        btnDelete.Show();
                        //                        if (s.ToUpper() == "POST")
                        //                        {
                        //                            LoadData();
                        //                            DataTable dt = (DataTable)dgv.DataSource;
                        //                            CalcNewAmort(dt);
                        //                            LoadDetailHeader();
                        //                            RecalcTotals();
                        ////                            LoadHeader();
                        //                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Loading Manual Payment Close! " + ex.Message.ToString());
            }
        }
        /****************************************************************************************/
        private void gridMain_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            if (!allowCalcRowHeight)
                return;
            if (!gridMain.Columns["notes"].Visible)
                return;
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                int maxHeight = 0;
                int newHeight = 0;
                bool doit = false;
                string name = "";
                string str = "";
                int count = 0;
                string[] Lines = null;
                foreach (GridColumn column in gridMain.Columns)
                {
                    name = column.FieldName.ToUpper();
                    if (name == "NOTES")
                        doit = true;
                    if (doit)
                    {
                        using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
                        {
                            using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
                            {
                                str = gridMain.GetRowCellValue(e.RowHandle, column.FieldName).ObjToString();
                                if (!String.IsNullOrWhiteSpace(str))
                                {
                                    Lines = str.Split('\n');
                                    count = Lines.Length + 1;
                                }
                                viewInfo.EditValue = gridMain.GetRowCellValue(e.RowHandle, column.FieldName);
                                viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv.Height);
                                using (Graphics graphics = dgv.CreateGraphics())
                                using (GraphicsCache cache = new GraphicsCache(graphics))
                                {
                                    viewInfo.CalcViewInfo(graphics);
                                    var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
                                    newHeight = Math.Max(height, maxHeight);
                                    if (newHeight > maxHeight)
                                    {
                                        maxHeight = newHeight * count;
                                    }
                                }
                            }
                        }
                    }
                }

                if (maxHeight > 0 && maxHeight > e.RowHeight)
                    e.RowHeight = maxHeight;
            }
        }
        /****************************************************************************************/
        private void lockScreenDetailsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "DailyHistory " + name;
            G1.SaveLocalPreferences(this, gridMain, LoginForm.username, saveName);

            //G1.SaveLocalPreferences(this, gridMain, LoginForm.username, "DailyHistoryLayout" );
            foundLocalPreference = true;
        }
        /****************************************************************************************/
        private void unlockScreenDetailsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string comboName = cmbSelectColumns.Text;
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                string name = "DailyHistory " + comboName;
                if (String.IsNullOrWhiteSpace(name))
                    name = "Primary";
                string saveName = "DailyHistory " + name;
                G1.RemoveLocalPreferences(LoginForm.username, saveName);
                foundLocalPreference = false;
            }

            //G1.RemoveLocalPreferences(LoginForm.username, "DailyHistoryLayout");
            foundLocalPreference = false;
        }
        /****************************************************************************************/
        private void reverseTCAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string contract = workContract;
            string name = workName;
            string depositNumber = dr["depositNumber"].ObjToString();
            if ( depositNumber.ToUpper().IndexOf ( "TCA") < 0 )
            {
                MessageBox.Show("***ERROR*** This entry does not appear to be a TCA!", "Reverse TCA Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            DialogResult result = MessageBox.Show("Are you sure you want to create a Reverse TCA for customer (" + workContract + ") ?", "Reverse TCA Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No)
                return;
            if (String.IsNullOrWhiteSpace(contract))
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            string record = dt.Rows[row]["record"].ObjToString();
            double downPayment = dt.Rows[row]["downPayment"].ObjToDouble();
            double payment = dt.Rows[row]["paymentAmount"].ObjToDouble();
            double ccFee = dt.Rows[row]["ccFee"].ObjToDouble();
            double originalPayment = payment;
            double credit = dt.Rows[row]["creditAdjustment"].ObjToDouble();
            double debit = dt.Rows[row]["debitAdjustment"].ObjToDouble();
            double interest = dt.Rows[row]["interestPaid"].ObjToDouble();
            double trust100P = dt.Rows[row]["trust100P"].ObjToDouble();
            double trust85P = dt.Rows[row]["trust85P"].ObjToDouble();
            string loc = dt.Rows[row]["location"].ObjToString();
            double oldBalance = dt.Rows[row]["oldBalance"].ObjToDouble();
            if (oldBalance == 0D && row > 0)
                oldBalance = dt.Rows[row - 1]["balance"].ObjToDouble();
            DateTime oldDueDate = dt.Rows[row]["oldDueDate8"].ObjToDateTime();
            if (oldDueDate.Year <= 2 && row > 0)
                oldDueDate = dt.Rows[row - 1]["currentDueDate8"].ObjToDateTime();
            DateTime oldDOLP = dt.Rows[row]["oldDOLP"].ObjToDateTime();
            if (oldDOLP.Year <= 2 && row > 0)
                oldDOLP = dt.Rows[row - 1]["payDate8"].ObjToDateTime();

            DataTable dx = dt.Clone();
            G1.copy_dt_row(dt, row, dx, 0);

            downPayment = downPayment * -1D;
            payment = payment * -1D;
            ccFee = ccFee * -1D;
            credit = credit * -1D;
            debit = debit * -1D;
            interest = interest * -1D;
            trust100P = trust100P * -1D;
            trust85P = trust85P * -1D;

            DateTime today = DateTime.Now;
            string datePaid = today.ToString("MM/dd/yyyy");
            string dueDate = dt.Rows[row]["dueDate8"].ObjToDateTime().ToString("MM/dd/yyyy");
            string lastName = dt.Rows[row]["lastName"].ObjToString();
            string firstName = dt.Rows[row]["firstName"].ObjToString();
            string checknumber = dt.Rows[row]["checkNumber"].ObjToString();
            string location = dt.Rows[row]["location"].ObjToString();
            string agent = dt.Rows[row]["agentNumber"].ObjToString();
            string user = dt.Rows[row]["userId"].ObjToString();
            user = LoginForm.username;
            string debitReason = dt.Rows[row]["debitReason"].ObjToString();
            string creditReason = dt.Rows[row]["creditReason"].ObjToString();
            if (creditReason.ToUpper().IndexOf("REVERS") >= 0 )
            {
                result = MessageBox.Show("You CANNOT Reverse a Reversal for customer (" + workContract + ") !!", "BAD-Reversal Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            creditReason = "Reverse TCA";

            record = G1.create_record(paymentsFile, "lastName", "-1");
            G1.update_db_table(paymentsFile, "record", record, new string[] { "contractNumber", workContract, "lastName", lastName, "firstName", firstName, "paymentAmount", payment.ToString(), "ccFee", ccFee.ToString(), "interestPaid", interest.ToString(), "debitAdjustment", debit.ToString(), "creditAdjustment", credit.ToString(), "debitReason", debitReason, "creditReason", creditReason });
            G1.update_db_table(paymentsFile, "record", record, new string[] { "CheckNumber", checknumber, "dueDate8", dueDate, "payDate8", datePaid, "trust85P", trust85P.ToString(), "trust100P", trust100P.ToString(), "location", location, "agentNumber", agent, "userId", user, "depositNumber", depositNumber, "edited", "TrustAdj" });

            //string cmd = "Select * from `" + contractsFile + "` where `contractNumber` = '" + workContract + "';";
            //dx = G1.get_db_data(cmd);
            //if (dx.Rows.Count > 0)
            //{
            //    record = dx.Rows[0]["record"].ObjToString();
            //    G1.update_db_table(contractsFile, "record", record, new string[] { "balanceDue", oldBalance.ToString(), "dueDate8", oldDueDate.ToString("yyyy-MM-dd"), "lastDatePaid8", oldDOLP.ToString("yyyy-MM-dd") });
            //}

            //ReverseACH(workContract, depositNumber, originalPayment);

            string audit = "Paid Date: " + datePaid + " Pmt/Credit/Debit: " + payment.ToString() + "/" + credit.ToString() + "/" + debit.ToString();
            G1.AddToAudit(LoginForm.username, "ManualPayment", "Reverse TCA", audit, workContract);

            DailyHistory_Load(null, null);

        }
        /****************************************************************************************/
    }
}