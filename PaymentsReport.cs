using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraEditors.Filtering.Templates;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using DevExpress.XtraRichEdit.API.Native;
using DocumentFormat.OpenXml.Office2016.Excel;
using GeneralLib;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class PaymentsReport : DevExpress.XtraEditors.XtraForm
    {
        /****************************************************************************************/
        public static DataTable cashRemittedDt = null;
        public static DataTable cashRemittedDt6 = null;
        public static DateTime paymentsReportDate = DateTime.Now;
        ToolStripMenuItem paidOutMenu = null;
        private DataTable workPayOffDt = null;
        private bool autoRun = false;
        private bool autoForce = false;
        private string sendTo = "";
        private string sendWhere = "";
        private DataTable cemDt = null;
        /****************************************************************************************/
        private bool doPaidOff = false;
        private string workContractNumber = "";
        private bool loading = false;
        private bool first = true;
        private bool batch = false;
        private DateTime batchStart = DateTime.Now;
        private DateTime batchStop = DateTime.Now;
        private bool continuousPrint = false;
        DataTable originalDt = null;
        public static DataTable paymentsReportDt = null;
        private string workReport = "";
        private string workTitle = "";
        private DevExpress.XtraRichEdit.RichEditControl rtb2 = new DevExpress.XtraRichEdit.RichEditControl();
        private string paymentsFile = "payments";
        private string customersFile = "customers";
        private string contractsFile = "contracts";
        private bool insurance = false;
        private DataTable funDt = null;
        private bool expandedRun = false;
        private string majorRunOn = "";
        private bool previousDateRead = false;
        /****************************************************************************************/
        public PaymentsReport()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        public PaymentsReport(DataTable payOffDt )
        {
            InitializeComponent();
            workReport = "PAID UP CONTRACTS REPORT";
            workTitle = "PAID UP CONTRACTS REPORT";
            workPayOffDt = payOffDt;
        }
        /****************************************************************************************/
        public PaymentsReport(string PaidOffContract)
        {
            doPaidOff = true;
            workContractNumber = PaidOffContract;
            //InitializeComponent();
            //txtContract.Text = workContractNumber;

            string cmd = "SELECT * FROM `contracts` p ";
            cmd += " JOIN `customers` c ON p.`contractNumber` = c.`contractNumber` ";
            cmd += " WHERE p.`contractNumber` = '" + workContractNumber + "';";

            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            dt.Columns.Add("select");
            dt.Rows[0]["select"] = "1";
            GenerateNotices(dt);
        }
        /****************************************************************************************/
        public PaymentsReport(string whatReport, string title = "")
        {
            InitializeComponent();
            workReport = whatReport;
            workTitle = title;
            if (String.IsNullOrWhiteSpace(workTitle))
                workTitle = workReport;
        }
        /****************************************************************************************/
        public PaymentsReport(bool auto, bool force, string whatReport, string title = "")
        {
            InitializeComponent();
            workReport = whatReport;
            workTitle = title;
            if (String.IsNullOrWhiteSpace(workTitle))
                workTitle = workReport;
            autoRun = auto;
            autoForce = force;
            //if ( autoRun && whatReport.ToUpper() == "PAID UP CONTRACTS REPORT")
            //{
            //    PaymentsReport_Load(null, null);
            //}
            //else
                RunAutoReports();
        }
        /****************************************************************************************/
        private void RunAutoReports()
        {
            string cmd = "Select * from `remote_processing`;";
            DataTable dt = G1.get_db_data(cmd);
            string report = "";
            DateTime date = DateTime.Now;
            int presentDay = date.Day;
            int dayToRun = 0;
            string status = "";
            string frequency = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                status = dt.Rows[i]["status"].ObjToString();
                if (status.ToUpper() == "INACTIVE")
                    continue;
                if (!autoForce)
                {
                    dayToRun = dt.Rows[i]["day_to_run"].ObjToInt32();
                    frequency = dt.Rows[i]["dateIncrement"].ObjToString();
                    if (!AutoRunSetup.CheckOkToRun(dayToRun, frequency))
                        return;
                }
                report = dt.Rows[i]["report"].ObjToString();
                if (autoForce)
                    report = workReport;
                sendTo = dt.Rows[i]["sendTo"].ObjToString();
                sendWhere = dt.Rows[i]["sendWhere"].ObjToString();
                if (report.ToUpper() == "REINSTATEMENT REPORT")
                    PaymentsReport_Load(null, null);
                else if (report.ToUpper() == "LAPSE REPORT")
                    PaymentsReport_Load(null, null);
                else if (report.ToUpper() == "PAID UP CONTRACTS REPORT")
                    PaymentsReport_Load(null, null);
            }
        }
        /****************************************************************************************/
        public PaymentsReport(string whatReport, DateTime start, DateTime stop, DateTime achStart, DateTime achStop )
        {
            InitializeComponent();
            workReport = whatReport;
            batchStart = start;
            batchStop = stop;
            batch = true;
            this.dateTimePicker1.Value = start;
            this.dateTimePicker2.Value = stop;
            this.dateTimePicker3.Value = achStart;
            this.dateTimePicker4.Value = achStop;

            gridMain.OptionsView.ShowFooter = false;
            gridMain.OptionsCustomization.AllowColumnMoving = true;
            SetupTotalsSummary();
            gridMain.OptionsView.ShowFooter = true;
            LoadData();

            SetupReport();
            btnRun_Click(null, null);
        }
        /****************************************************************************************/
        private void SetupReport()
        {
            chkSelectAll.Hide();
            btnCombine.Hide();
            cmbDateType.Hide();
            chkShowAllDeceased.Hide();
            gridMain.Columns["lapseDate8"].Visible = false;
            gridMain.Columns["reinstateDate8"].Visible = false;
            if (String.IsNullOrWhiteSpace(workReport))
                return;
            chkDeaths.Visible = false;
            chkPaidUp.Visible = false;
            chkPayments.Visible = false;
            chkShowAll.Visible = false;
            chkActiveOnly.Visible = false;
            chkDownPayments.Visible = false;
            chkBalanceLessPayment.Visible = false;
            chkShowDueNext.Visible = false;
            chkBalances.Visible = false;
            txtBalance.Visible = false;
            lblBalances.Visible = false;

            chkSortLastName.Hide();
            chkPmtLessMoPmt.Hide();
            this.Text = workTitle;
            if (workReport.ToUpper() == "PAYMENTS AFTER DEATH REPORT")
                chkDeaths.Checked = true;
            else if (workReport.ToUpper() == "PAID UP CONTRACTS REPORT")
            {
                paidOutMenu = new ToolStripMenuItem();
                paidOutMenu.Text = "Generate Paid Out Letters";
                paidOutMenu.Click += PaidOutMenu_Click;
                toolStripMenuItem1.DropDownItems.Add(paidOutMenu);
                chkPaidUp.Checked = true;
                chkSelectAll.Show();
                lblRunOn.Visible = false;
                lblRunOn.Refresh();
                cmbRunOn.Visible = false;
                cmbRunOn.Refresh();
            }
            else if (workReport.ToUpper() == "SHOW PAYMENTS DUE NEXT MONTH")
            {
                chkShowDueNext.Checked = true;
                //                chkPayments.Checked = true;
                cmbDateType.Show();
            }
            else if (workReport.ToUpper() == "PAYMENTS REPORT" || workReport.ToUpper() == "ODD PAYMENTS REPORT")
            {
                chkPayments.Checked = true;
                chkPmtLessMoPmt.Show();
                cmbDateType.Show();
            }
            else if (workReport.ToUpper() == "CASH REMITTED REPORT")
            {
                chkSortLastName.Show();
                chkShowAll.Checked = true;
            }
            else if (workReport.ToUpper() == "REMOVAL REPORT")
            {
                chkSortLastName.Show();
                chkShowAll.Checked = true;
            }
            else if (workReport.ToUpper() == "DECEASED REPORT")
            {
                chkSortLastName.Show();
                chkShowAll.Checked = true;
            }
            else if (workReport.ToUpper() == "ALL ACTIVITY REPORT")
                chkShowAll.Checked = true;
            else if (workReport.ToUpper() == "INSURANCE WEEK TOTALS")
                chkShowAll.Checked = true;
            else if (workReport.ToUpper() == "NEW BUSINESS REPORT")
                chkDownPayments.Checked = true;
            else if (workReport.ToUpper() == "ACTIVE ONLY REPORT")
                chkActiveOnly.Checked = true;
            else if (workReport.ToUpper() == "DOWN PAYMENTS REPORT")
                chkDownPayments.Checked = true;
            else if (workReport.ToUpper() == "BALANCES LESS THAN PAYMENTS REPORT")
                chkBalanceLessPayment.Checked = true;
            else if (workReport.ToUpper() == "BALANCES LESS THAN X REPORT")
            {
                chkBalances.Checked = true;
                txtBalance.Visible = true;
                lblBalances.Visible = true;
            }
            else if (workReport.ToUpper() == "TRUST EOM REPORT")
            {
                chkPayments.Checked = true;
                ClearAllPositions();
                SetTrustEomReport();
                cmbDateType.Show();
                gridMain.Columns["contractNumber"].Visible = true;
            }
            if (!chkDeaths.Checked && !chkPayments.Checked)
            {
                dateTimePicker3.Visible = false;
                dateTimePicker4.Visible = false;
                lblAllOther.Visible = false;
                lblAllOtherTo.Visible = false;
                if (!chkPaidUp.Checked)
                    dateTimePicker1.Value = dateTimePicker3.Value;
                dateTimePicker2.Value = dateTimePicker4.Value;
            }
            if (workReport.ToUpper() == "NEW BUSINESS REPORT")
            {
                ClearAllPositions();
                SetNewBusiness();
            }
            else if (workReport.ToUpper() == "REINSTATEMENT REPORT" || workReport.ToUpper() == "LAPSE REPORT" )
            {
                chkPayments.Checked = true;
                ClearAllPositions();
                SetReinstatementReport();
            }
            else if (workReport.ToUpper() == "DBR REPORT")
            {
                ClearAllPositions();
                SetDbrBusiness();
            }
            else if (workTitle.IndexOf("Trust Down Payment Master List Report") >= 0)
            {
                SetDownPayments();
            }
            else if (workReport.ToUpper() == "CASH REMITTED REPORT")
            {
                SetTrustEomReport();
                SetRemitDPs();
                SetRemitPayments();
                SetOVPPayments();
            }
            else if (workReport.ToUpper() == "REMOVAL REPORT")
            {
                SetRemovals();
            }
            else if (workReport.ToUpper() == "DECEASED REPORT")
            {
                SetDeceased();
            }
            else if (workReport.ToUpper() == "INSURANCE WEEK TOTALS")
                SetInsuranceWeeklyReport();
            else if (workReport.ToUpper() == "SHOW PAYMENTS DUE NEXT MONTH")
                SetShowNextPayments();

            ToolTip tt = new ToolTip();
            tt.SetToolTip(pictureDelete, "Delete Current Row");
            tt = new ToolTip();
            tt.SetToolTip(pictureAdd, "Append New Row");
            tt = new ToolTip();
            tt.SetToolTip(btnInsert, "Insert New Row");
        }
        /****************************************************************************************/
        private void SetupCemeterySummary ()
        {
            dgv.DataSource = null;
            dgv6.DataSource = null;
            dgv7.DataSource = null;
            dgv8.DataSource = null;

            dgv.Refresh();
            dgv6.Refresh();
            dgv7.Refresh();
            dgv8.Refresh();

            string runOn = cmbRunOn.Text.Trim().ToUpper();
            try
            {
                if (runOn == "TRUSTS")
                {
                    gridMain.Columns["lastName"].SummaryItem.DisplayFormat = "T85-DBR={0:0,0.00}";
                    gridMain7.Columns["lastName"].SummaryItem.DisplayFormat = "T85-DBR={0:0,0.00}";
                    gridMain8.Columns["lastName"].SummaryItem.DisplayFormat = "T85-DBR={0:0,0.00}";

                    string str = "";

                    for (int i = 0; i < gridMain8.GroupSummary.Count; i++)
                    {
                        str = gridMain8.GroupSummary[i].FieldName.Trim().ToUpper();
                        if (str == "LASTNAME")
                        {
                            gridMain8.GroupSummary[i].DisplayFormat = "T85-DBR=${0:0,0.00}";
                        }
                    }

                    this.bandedGridColumn133.Summary.Clear();
                    this.bandedGridColumn133.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
                    new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Custom, "lastName", "T85-DBR=${0:0,0.00}", "3")});

                    gridMain.Columns["trust85P"].Caption = "85% Trust";
                    gridMain.Columns["trust100P"].Caption = "100% Trust";

                    gridMain8.Columns["trust85P"].Caption = "85% Trust";
                    gridMain8.Columns["trust100P"].Caption = "100% Trust";

                    gridMain.Columns["dbr"].Visible = true;
                    gridMain6.Columns["dbr"].Visible = true;
                    gridMain7.Columns["dbr"].Visible = true;
                    gridMain8.Columns["dbr"].Visible = true;
                    gridMain8.Columns["trust85P"].Visible = true;
                    gridMain8.Columns["trust100P"].Visible = true;

                    this.Text = workTitle;

                }
                else if (runOn == "CEMETERIES")
                {
                    this.Text = workTitle;
                    if (workTitle.IndexOf("1.2") > 0)
                        this.Text = this.Text.Replace("1.2", "1.3");
                    else if (workTitle.IndexOf("8.0") > 0)
                        this.Text = this.Text.Replace("8.0", "8.1");

                    gridMain.Columns["lastName"].SummaryItem.DisplayFormat = "T15-DBR={0:0,0.00}";
                    gridMain7.Columns["lastName"].SummaryItem.DisplayFormat = "T15-DBR={0:0,0.00}";
                    gridMain8.Columns["lastName"].SummaryItem.DisplayFormat = "T15-DBR={0:0,0.00}";

                    string str = "";

                    for (int i = 0; i < gridMain8.GroupSummary.Count; i++)
                    {
                        str = gridMain8.GroupSummary[i].FieldName.Trim().ToUpper();
                        if (str == "LASTNAME")
                        {
                            gridMain8.GroupSummary[i].DisplayFormat = "T15-DBR=${0:0,0.00}";
                        }
                    }
                    this.bandedGridColumn133.Summary.Clear();
                    this.bandedGridColumn350.Summary.Clear();
                    this.bandedGridColumn84.Summary.Clear();
                    this.bandedGridColumn3.Summary.Clear();

                    this.bandedGridColumn3.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
                        new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Custom, "lastName", "T15-DBR={0:0,0.00}", "1")});

                    this.bandedGridColumn84.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
                        new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Custom, "lastName", "T15-DBR=${0:0,0.00}", "2")});

                    this.bandedGridColumn350.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
                        new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Custom, "lastName", "T15-DBR=${0:0,0.00}", "3")});


                    this.bandedGridColumn133.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
                        new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Custom, "lastName", "T15-DBR=${0:0,0.00}", "3")});


                    gridMain.Columns["trust85P"].Caption = "15% Trust";
                    gridMain.Columns["trust100P"].Caption = "100% Trust";
                    gridMain8.Columns["trust85P"].Caption = "15% Trust";
                    gridMain8.Columns["trust100P"].Caption = "100% Trust";

                    gridMain.Columns["dbr"].Visible = false;
                    gridMain6.Columns["dbr"].Visible = false;
                    gridMain7.Columns["dbr"].Visible = false;
                    gridMain8.Columns["dbr"].Visible = false;
                    //gridMain8.Columns["trust85P"].Visible = false;

                    gridMain.Columns["num"].Visible = false;
                    gridMain.Columns["Location Name"].GroupIndex = 0;

                    gridMain8.RefreshEditor(true);
                }
                else if (runOn == "RILES")
                {
                    this.Text = workTitle;
                    if (workTitle.IndexOf("1.2") > 0)
                        this.Text = this.Text.Replace("1.2", "1.4");
                    else if (workTitle.IndexOf("8.0") > 0)
                        this.Text = this.Text.Replace("8.0", "8.2");

                    //this.Text = workTitle;
                    //this.Text = this.Text.Replace("8.0", "8.2");

                    gridMain.Columns["lastName"].SummaryItem.DisplayFormat = "T85-DBR={0:0,0.00}";
                    gridMain7.Columns["lastName"].SummaryItem.DisplayFormat = "T85-DBR={0:0,0.00}";
                    gridMain8.Columns["lastName"].SummaryItem.DisplayFormat = "T85-DBR={0:0,0.00}";

                    string str = "";

                    for (int i = 0; i < gridMain8.GroupSummary.Count; i++)
                    {
                        str = gridMain8.GroupSummary[i].FieldName.Trim().ToUpper();
                        if (str == "LASTNAME")
                        {
                            gridMain8.GroupSummary[i].DisplayFormat = "T85-DBR=${0:0,0.00}";
                        }
                    }

                    this.bandedGridColumn133.Summary.Clear();
                    this.bandedGridColumn133.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
                    new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Custom, "lastName", "T85-DBR=${0:0,0.00}", "3")});

                    gridMain.Columns["trust85P"].Caption = "85% Trust";
                    gridMain.Columns["trust100P"].Caption = "100% Trust";

                    gridMain8.Columns["trust85P"].Caption = "85% Trust";
                    gridMain8.Columns["trust100P"].Caption = "100% Trust";

                    gridMain.Columns["dbr"].Visible = false;
                    gridMain6.Columns["dbr"].Visible = false;
                    gridMain7.Columns["dbr"].Visible = false;
                    gridMain8.Columns["dbr"].Visible = false;
                    gridMain8.Columns["trust85P"].Visible = true;
                    gridMain8.Columns["trust100P"].Visible = true;
                }
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void PaymentsReport_Load(object sender, EventArgs e)
        {
            btnGenerateLetters.Hide();
            btnSave.Hide();
            insurance = false;
            paymentsFile = "payments";
            chkUseSDI.Hide();
            if (workReport.ToUpper() == "INSURANCE WEEK TOTALS")
            {
                chkSortByTrust.Text = "Sort By UserID";
                insurance = true;
                paymentsFile = "ipayments";
                contractsFile = "icontracts";
                customersFile = "icustomers";
                chkUseSDI.Show();
            }

            if (workTitle != "New Business Report (1.2)" && workReport.ToUpper() != "CASH REMITTED REPORT" )
            {
                massReportsToolStripMenuItem.Dispose();
            }


            RemoveExtraTabs();

            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker3.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker4.Value = new DateTime(now.Year, now.Month, days);
            DateTime start = now.AddDays(-1);
            DateTime stop = new DateTime(now.Year, now.Month, days - 1);
            this.dateTimePicker1.Value = start;
            this.dateTimePicker2.Value = stop;
            if ( this.dateTimePicker3.Value.Year >= 2019 && this.dateTimePicker3.Value.Month >= 6 )
            {
                this.dateTimePicker1.Value = this.dateTimePicker3.Value;
                this.dateTimePicker2.Value = this.dateTimePicker4.Value;
                chkACH.Checked = false;
            }
            if (now >= DailyHistory.majorDate)
            {
                now = DateTime.Now;
                now = now.AddMonths(-1);
                now = new DateTime(now.Year, now.Month, 1);
                this.dateTimePicker1.Value = now;
                this.dateTimePicker3.Value = now;
                days = DateTime.DaysInMonth(now.Year, now.Month);
                now = new DateTime(now.Year, now.Month, days);
                this.dateTimePicker2.Value = now;
                this.dateTimePicker4.Value = now;
            }

            gridMain.OptionsView.ShowFooter = false;
            gridMain.OptionsCustomization.AllowColumnMoving = true;
            SetupTotalsSummary();
            gridMain.OptionsView.ShowFooter = true;
            LoadData();
            if (String.IsNullOrWhiteSpace(workReport))
                chkDownPayments_CheckedChanged(null, null);
            else
                SetupReport();
            if ( workPayOffDt != null)
            {
                btnRun.Hide();
                btnGenerateLetters.Show();
                btnRun_Click(null, null);
            }
            else
            {
                if (autoRun)
                {
                    btnRun_Click(null, null);
                    DataTable dt = (DataTable)dgv.DataSource;
                    DataView tempview = dt.DefaultView;
                    tempview.Sort = "agentNumber, contractNumber, lastName, firstName";
                    dt = tempview.ToTable();
                    dgv.DataSource = dt;

                    gridMain.Columns["agentNumber"].GroupIndex = 0;
                    gridMain.Columns["Location Name"].GroupIndex = -1;
                    gridMain.Columns["trust"].GroupIndex = -1;
                    gridMain.OptionsView.ShowFooter = true;
                    this.gridMain.ExpandAllGroups();

                    printPreviewToolStripMenuItem_Click(null, null);
                    this.Close();
                }
                else if ( doPaidOff )
                {
                    btnRun_Click(null, null);
                    //DataTable dt = (DataTable)dgv.DataSource;
                    //GenerateNotices(dt);
                    this.Close();
                }
            }
            lblAllOther.Hide();
            lblAllOtherTo.Hide();
            this.dateTimePicker3.Hide();
            this.dateTimePicker4.Hide();
            chkACH.Hide();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("amtOfMonthlyPayt", null );
            AddSummaryColumn("balanceDue", null);
            AddSummaryColumn("totalContract", null);
            AddSummaryColumn("contractValue", null);
            AddSummaryColumn("trust100P", null);
            AddSummaryColumn("trust85P", null);
            AddSummaryColumn("downPayment", null);
            AddSummaryColumn("downPayment1", null);
            AddSummaryColumn("paymentAmount", null);
            AddSummaryColumn("ccFee", null);
            AddSummaryColumn("ap", null);
            AddSummaryColumn("dpp", null);
            AddSummaryColumn("newBusiness", null);
            AddSummaryColumn("interestPaid1", null);
            AddSummaryColumn("retained", null);
            AddSummaryColumn("LiInterest", null);
            AddSummaryColumn("debitAdjustment", null);
            AddSummaryColumn("creditAdjustment", null);
            AddSummaryColumn("dbr", null);
            AddSummaryColumn("S", null, "{0}");
            AddSummaryColumn("numMonths", null, "{0}");

            AddSummaryColumn("fdlicDownPayments", gridMain6);
            AddSummaryColumn("fdlicMonthly", gridMain6);
            AddSummaryColumn("unityMonthly", gridMain6);
            AddSummaryColumn("bancorpsouthMonthly", gridMain6);
            AddSummaryColumn("pbMonthly", gridMain6);
            AddSummaryColumn("perpetualCare", gridMain6);
            AddSummaryColumn("total", gridMain6);
//            AddSummaryColumn("retained", gridMain6);

            gridMain.Columns["totalContract"].Visible = false;
            gridMain.Columns["dueDate8"].Visible = false;
            gridMain.Columns["balanceDue"].Visible = false;
            gridMain.Columns["amtOfMonthlyPayt"].Visible = false;
            gridMain.Columns["agentNumber"].Visible = true;

            AddSummaryColumn("dbr", gridMain6);

            //AddSummaryColumn("trust100P", gridMain7);
            //AddSummaryColumn("trust85P", gridMain7);
            AddSummaryColumn("downPayment", gridMain7);
            AddSummaryColumn("newBusiness", gridMain7);
            AddSummaryColumn("dbr", gridMain7);

            //gridMain7.Columns["lastName"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            //gridMain7.Columns["lastName"].SummaryItem.DisplayFormat = "Trust85 - DBR =${ 0:0,0.00}";

            AddSummaryColumn("trust100P", gridMain8);
            AddSummaryColumn("trust85P", gridMain8);
            AddSummaryColumn("retained", gridMain8);
            AddSummaryColumn("dbr", gridMain8);

            AddSummaryColumn("trust100P", gridMain13);
            AddSummaryColumn("trust85P", gridMain13);
            AddSummaryColumn("retained", gridMain13);
            AddSummaryColumn("ovp", gridMain13);
            //CreateSummaries();
        }
        /****************************************************************************************/
        private void CreateSummaries()
        {
            //GridColumnSummaryItem item1 = new GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Custom, "trut85P", "{0}");
            //gridMain7.Columns["trust85P"].Summary.Add(item1);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "")
        {
            if (gMain == null)
                gMain = gridMain;
            //if (String.IsNullOrWhiteSpace(format))
            //    format = "${0:0,0.00}";
            if (String.IsNullOrWhiteSpace(format))
                format = "{0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, string format = "")
        {
            //if (String.IsNullOrWhiteSpace(format))
            //    format = "${0:0,0.00}";
            if (String.IsNullOrWhiteSpace(format))
                format = "{0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /***********************************************************************************************/
        private void loadLocatons()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);

            cmd = "Select * from `cemeteries`;";
            DataTable cemDt = G1.get_db_data(cmd);
            string loc = "";
            string desc = "";
            DataRow dRow = null;
            
            for ( int i=0; i<cemDt.Rows.Count; i++)
            {
                loc = cemDt.Rows[i]["loc"].ObjToString();
                desc = cemDt.Rows[i]["description"].ObjToString();
                dRow = locDt.NewRow();
                dRow["keycode"] = loc;
                dRow["name"] = desc;
                dRow["LocationCode"] = desc;
                dRow["cashRemitHeading"] = loc + " " + desc + " Cemetery";
                locDt.Rows.Add(dRow);
            }

            funDt = locDt;

            chkComboLocNames.Properties.DataSource = locDt;
            chkComboLocation.Properties.DataSource = locDt;
        }
        /****************************************************************************************/
        private void LoadData()
        {
            loadLocatons();
            G1.SetupVisibleColumns(gridMain, this.columnsToolStripMenuItem, nmenu_Click);
            string cmd = "Select * from `" + paymentsFile + "` where `contractNumber` = 'XYZZYAAA';";
            DataTable dt = G1.get_db_data(cmd);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            ScaleCells();
        }
        /***********************************************************************************************/
        private void ResetColumns()
        {
            for (int i = (columnsToolStripMenuItem.DropDownItems.Count - 1); i >= 0; i--)
            {
                try
                {
                    ToolStripMenuItem nmenu = (ToolStripMenuItem)columnsToolStripMenuItem.DropDownItems[i];
                    nmenu.Click -= new EventHandler(nmenu_Click);
                    columnsToolStripMenuItem.DropDownItems.RemoveAt(i);
                }
                catch (Exception ex)
                {

                }
            }
            G1.SetupVisibleColumns(gridMain, this.columnsToolStripMenuItem, nmenu_Click);
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
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printFirst = true;
            //isPrinting = true;
            footerCount = 0;
            printRow = 1;

            footerCount = 0;
            if (this.components == null)
                this.components = new System.ComponentModel.Container();
            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            cashRemittedDt6 = null;
            DataTable tempDt = null;

            printableComponentLink1.Component = dgv;
            if (!doPaidOff)
            {
                if (dgv2.Visible)
                    printableComponentLink1.Component = dgv2;
                else if (dgv3.Visible)
                    printableComponentLink1.Component = dgv3;
                else if (dgv4.Visible)
                    printableComponentLink1.Component = dgv4;
                else if (dgv5.Visible)
                    printableComponentLink1.Component = dgv5;
                else if (dgv6.Visible)
                {
                    printableComponentLink1.Component = dgv6;
                    DataTable dx = (DataTable)dgv6.DataSource;
                    if (dx.Rows.Count > 0)
                        majorLastDetail = dx.Rows[0]["location"].ObjToString();
                    tempDt = dx.Copy();
                }
                else if (dgv7.Visible)
                    printableComponentLink1.Component = dgv7;
                else if (dgv8.Visible)
                    printableComponentLink1.Component = dgv8;
                else if (dgv9.Visible)
                    printableComponentLink1.Component = dgv9;
                else if (dgv10.Visible)
                    printableComponentLink1.Component = dgv10;
                else if (dgv11.Visible)
                    printableComponentLink1.Component = dgv11;
                else if (dgv12.Visible)
                    printableComponentLink1.Component = dgv12;
                else if (dgv13.Visible)
                    printableComponentLink1.Component = dgv13;
            }

            if (continuousPrint)
            {
                SectionMargins margins = rtb2.Document.Sections[0].Margins;
                margins.Left = 0;
                margins.Right = 0;
                margins.Top = 0;
                margins.Bottom = 0;
                printableComponentLink1.Component = rtb2;
            }
            if ( doPaidOff )
            {
                printableComponentLink1.ShowPreviewDialog();
                return;
            }
            //            printableComponentLink1.ExportToRtf ()

            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            this.gridMain.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
            this.gridMain.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);
            if ( dgv7.Visible )
            {
                chkSort.Checked = true;
                this.gridMain7.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
                this.gridMain7.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);
            }
            else if ( dgv8.Visible )
            {
                chkSort.Checked = true;
                this.gridMain8.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
                this.gridMain8.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);
            }
            else if (dgv13.Visible)
            {
                chkSort.Checked = true;
                this.gridMain13.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
                this.gridMain13.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);
            }

            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(30, 30, 90, 50);
            if (continuousPrint)
                Printer.setupPrinterMargins(0, 0, 0, 0);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;


            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;
            if (continuousPrint)
            {
                printableComponentLink1.MinMargins.Left = pageMarginLeft;
                printableComponentLink1.MinMargins.Right = pageMarginRight;
                printableComponentLink1.MinMargins.Top = pageMarginTop;
                printableComponentLink1.MinMargins.Bottom = pageMarginBottom;
            }

            //if (cashRemittedDt6 != null)
            //{
            //    printingSystem1.AddCommandHandler(new CsvExportOptionsEx());
            //    //printingSystem1.AddCommandHandler(new ExportToXlsCommandHandler());
            //    //printingSystem1.AddCommandHandler(new PrintDocumentCommandHandler());
            //}


            //printingSystem1.Document.AutoFitToPagesWidth = 1;

            if (dgv.Visible)
            {
                G1.AdjustColumnWidths(gridMain, 0.65D, true);
                //printingSystem1.Document.AutoFitToPagesWidth = 1;
            }
            if (dgv2.Visible)
                G1.AdjustColumnWidths(gridMain2, 0.65D, true);
            if (dgv3.Visible)
                G1.AdjustColumnWidths(gridMain3, 0.65D, true);
            if (dgv4.Visible)
                G1.AdjustColumnWidths(gridMain4, 0.65D, true);
            if (dgv5.Visible)
                G1.AdjustColumnWidths(gridMain5, 0.65D, true);
            if (dgv6.Visible)
                G1.AdjustColumnWidths(gridMain6, 0.65D, true);
            if (dgv7.Visible)
                G1.AdjustColumnWidths(gridMain7, 0.65D, true);
            if (dgv8.Visible)
                G1.AdjustColumnWidths(gridMain8, 0.65D, true);
            if (dgv9.Visible)
                G1.AdjustColumnWidths(gridMain9, 0.65D, true);
            if (dgv10.Visible)
                G1.AdjustColumnWidths(gridMain10, 0.65D, true);
            if (dgv11.Visible)
                G1.AdjustColumnWidths(gridMain11, 0.65D, true);


            printableComponentLink1.CreateDocument();
            if (autoRun)
            {
                DataTable dt = null;
                try
                {
                    DevExpress.XtraGrid.GridControl xDGV = (DevExpress.XtraGrid.GridControl)printableComponentLink1.Component;
                    dt = (DataTable)xDGV.DataSource;
                }
                catch (Exception ex)
                {
                    G1.AddToAudit("System", workReport, "AutoRun", "FAILED", "");
                    return;
                }
                string emailLocations = DailyHistory.ParseOutLocations(dt);

                string path = G1.GetReportPath();
                DateTime today = DateTime.Now;

                string filename = path + @"\" + workReport + "_" + today.Year.ToString("D4") + today.Month.ToString("D2") + today.Day.ToString("D2") + ".pdf";
                if (File.Exists(filename))
                    File.Delete(filename);
                printableComponentLink1.ExportToPdf(filename);
                RemoteProcessing.AutoRunSendTo(workReport, filename, sendTo, sendWhere, emailLocations);
            }
            else if ( continuousPrint )
            {
            }
            else
            {
                if (tempDt != null)
                    cashRemittedDt6 = tempDt.Copy();
                if (cashRemittedDt6 != null)
                {
                    //printingSystem1.AddCommandHandler(new CsvExportOptionsEx());
                    printingSystem1.AddCommandHandler(new ExportOptions());
                    //printingSystem1.End();
                }
                printableComponentLink1.ShowPreviewDialog();
            }

            if (dgv.Visible)
                G1.AdjustColumnWidths(gridMain, 0.65D, false );
            if (dgv2.Visible)
                G1.AdjustColumnWidths(gridMain2, 0.65D, false);
            if (dgv3.Visible)
                G1.AdjustColumnWidths(gridMain3, 0.65D, false);
            if (dgv4.Visible)
                G1.AdjustColumnWidths(gridMain4, 0.65D, false);
            if (dgv5.Visible)
                G1.AdjustColumnWidths(gridMain5, 0.65D, false);
            if (dgv6.Visible)
                G1.AdjustColumnWidths(gridMain6, 0.65D, false);
            if (dgv7.Visible)
                G1.AdjustColumnWidths(gridMain7, 0.65D, false);
            if (dgv8.Visible)
                G1.AdjustColumnWidths(gridMain8, 0.65D, false);
            if (dgv9.Visible)
                G1.AdjustColumnWidths(gridMain9, 0.65D, false);
            if (dgv10.Visible)
                G1.AdjustColumnWidths(gridMain10, 0.65D, false);
            if (dgv11.Visible)
                G1.AdjustColumnWidths(gridMain11, 0.65D, false);
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printFirst = true;
            //isPrinting = true;
            footerCount = 0;
            printRow = 1;

            footerCount = 0;
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
            {
                printableComponentLink1.Component = dgv6;
                DataTable dx = (DataTable)dgv6.DataSource;
                if (dx.Rows.Count > 0)
                    majorLastDetail = dx.Rows[0]["location"].ObjToString();
            }
            else if (dgv7.Visible)
                printableComponentLink1.Component = dgv7;
            else if (dgv8.Visible)
                printableComponentLink1.Component = dgv8;
            else if (dgv9.Visible)
                printableComponentLink1.Component = dgv9;
            else if (dgv10.Visible)
                printableComponentLink1.Component = dgv10;
            else if (dgv11.Visible)
                printableComponentLink1.Component = dgv11;
            else if (dgv12.Visible)
                printableComponentLink1.Component = dgv12;
            else if (dgv13.Visible)
                printableComponentLink1.Component = dgv13;

            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            this.gridMain.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
            this.gridMain.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);
            if (dgv7.Visible)
            {
                chkSort.Checked = true;
                this.gridMain7.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
                this.gridMain7.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);
            }
            else if (dgv8.Visible)
            {
                chkSort.Checked = true;
                this.gridMain8.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
                this.gridMain8.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);
            }
            else if (dgv13.Visible)
            {
                chkSort.Checked = true;
                this.gridMain13.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
                this.gridMain13.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);
            }

            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(30, 30, 80, 50);

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
            if (LoginForm.doLapseReport)
                printableComponentLink1.Print();
            else
                printableComponentLink1.PrintDlg();
        }
        /***********************************************************************************************/
        private void printPreviewNewBusiness_Click(object sender, EventArgs e)
        {
            if (!dgv.Visible)
                return;

            printFirst = true;
            footerCount = 0;
            printRow = 1;

            footerCount = 0;
            if (this.components == null)
                this.components = new System.ComponentModel.Container();
            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            cashRemittedDt6 = null;
            DataTable tempDt = null;

            printableComponentLink1.Component = dgv;

            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            this.gridMain.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
            this.gridMain.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);

            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(30, 30, 90, 50);
            //Printer.setupPrinterMargins(0, 0, 0, 0);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;


            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;
            if (continuousPrint)
            {
                printableComponentLink1.MinMargins.Left = pageMarginLeft;
                printableComponentLink1.MinMargins.Right = pageMarginRight;
                printableComponentLink1.MinMargins.Top = pageMarginTop;
                printableComponentLink1.MinMargins.Bottom = pageMarginBottom;
            }

            G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printableComponentLink1.CreateDocument();

            if (File.Exists(fullPath))
                File.Delete(fullPath);
            if (fullPath.ToUpper().IndexOf(".PDF") > 0)
                printableComponentLink1.ExportToPdf(fullPath);
            else
                printableComponentLink1.ExportToCsv(fullPath);

            G1.AdjustColumnWidths(gridMain, 0.65D, false);
        }
        /***********************************************************************************************/
        private void printPreviewMyCashRemit_Click(object sender, EventArgs e)
        {
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});


            printableComponentLink1.Component = dgv6;
            if ( dgv7.Visible )
                printableComponentLink1.Component = dgv7;
            else if (dgv8.Visible)
                printableComponentLink1.Component = dgv8;
            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);

            if (dgv7.Visible)
            {
                chkSort.Checked = true;
                this.gridMain7.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
                this.gridMain7.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);
            }
            else if (dgv8.Visible)
            {
                chkSort.Checked = true;
                this.gridMain8.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
                this.gridMain8.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);
            }

            printableComponentLink1.Landscape = true;

            //Printer.setupPrinterMargins(10, 10, 130, 50);
            Printer.setupPrinterMargins(10, 10, 100, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            if ( dgv6.Visible )
                G1.AdjustColumnWidths(gridMain6, 0.65D, true);
            else if (dgv7.Visible)
                G1.AdjustColumnWidths(gridMain7, 0.65D, true);
            else if (dgv8.Visible)
                G1.AdjustColumnWidths(gridMain8, 0.65D, true);

            printableComponentLink1.CreateDocument();
            //printableComponentLink1.ShowPreview();

            if (continuousPrint)
            {
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
                if (fullPath.ToUpper().IndexOf(".PDF") > 0)
                    printableComponentLink1.ExportToPdf(fullPath);
                else
                    printableComponentLink1.ExportToCsv(fullPath);
            }
            else
                printableComponentLink1.ShowPreviewDialog();

            if (dgv6.Visible)
                G1.AdjustColumnWidths(gridMain6, 0.65D, false);
            else if (dgv7.Visible)
                G1.AdjustColumnWidths(gridMain7, 0.65D, false);
            else if (dgv7.Visible)
                G1.AdjustColumnWidths(gridMain7, 0.65D, false);
        }
        /***********************************************************************************************/
        private void printPreviewCashRemit_Click(object sender, EventArgs e)
        {

            printFirst = true;
            footerCount = 0;
            printRow = 1;

            footerCount = 0;
            if (this.components == null)
                this.components = new System.ComponentModel.Container();
            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            DataTable tempDt = null;

            printableComponentLink1.Component = dgv;
            if (dgv6.Visible)
            {
                printableComponentLink1.Component = dgv6;
                DataTable dx = (DataTable)dgv6.DataSource;
                if (dx.Rows.Count > 0)
                    majorLastDetail = dx.Rows[0]["location"].ObjToString();
                tempDt = dx.Copy();
            }
            else if (dgv7.Visible)
            {
                //chkSort.Checked = true;
                printableComponentLink1.Component = dgv7;
            }
            else if (dgv8.Visible)
            {
                //chkSort.Checked = true;
                printableComponentLink1.Component = dgv8;
            }

            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            this.gridMain.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
            this.gridMain.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);

            if (dgv6.Visible)
            {
                //chkSort.Checked = true;
                this.gridMain6.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
                this.gridMain6.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);
            }
            if (dgv7.Visible)
            {
                chkSort.Checked = true;
                this.gridMain7.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
                this.gridMain7.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);
            }
            else if (dgv8.Visible)
            {
                chkSort.Checked = true;
                this.gridMain8.BeforePrintRow += new DevExpress.XtraGrid.Views.Base.BeforePrintRowEventHandler(this.gridMain_BeforePrintRow);
                this.gridMain8.AfterPrintRow += new DevExpress.XtraGrid.Views.Base.AfterPrintRowEventHandler(this.gridMain_AfterPrintRow);
            }
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(10, 10, 130, 50);
            //Printer.setupPrinterMargins(0, 0, 0, 0);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;


            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;
            if (continuousPrint)
            {
                printableComponentLink1.MinMargins.Left = pageMarginLeft;
                printableComponentLink1.MinMargins.Right = pageMarginRight;
                printableComponentLink1.MinMargins.Top = pageMarginTop;
                printableComponentLink1.MinMargins.Bottom = pageMarginBottom;
            }

            G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printableComponentLink1.CreateDocument();

            if (File.Exists(fullPath))
                File.Delete(fullPath);
            if (fullPath.ToUpper().IndexOf(".PDF") > 0)
                printableComponentLink1.ExportToPdf(fullPath);
            else
                printableComponentLink1.ExportToCsv(fullPath);

            G1.AdjustColumnWidths(gridMain, 0.65D, false);
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
        private CreateAreaEventArgs publicE = null;
        private bool printFirst = true;
        private bool printFirstToo = true;
        private void printableComponentLink1_CreateMarginalHeaderArea(object sender, CreateAreaEventArgs e)
        {
            if (printFirst)
                publicE = e;
            printFirst = false;

            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);
            //if (1 == 1)
            //    return;

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 6);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            //            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Regular);

            if (workReport.ToUpper() == "TRUST EOM REPORT")
            {
                string addTitle = this.dateTimePicker2.Value.ToString("MMMMMMMMMMMMM");
                addTitle += "-" + this.dateTimePicker2.Value.Year.ToString("D4");

                if (dgv.Visible)
                    Printer.DrawQuad(5, 8, 5, 4, "Trust EOM Payments Report " + addTitle, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                else if (dgv2.Visible)
                    Printer.DrawQuad(5, 8, 5, 4, "Trust EOM Report " + addTitle, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                else if (dgv3.Visible)
                    Printer.DrawQuad(5, 8, 5, 4, "Trust DRAFTS EOM Report " + addTitle, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                else if (dgv4.Visible)
                    Printer.DrawQuad(5, 8, 5, 4, "Trust LOCKBOX EOM Report " + addTitle, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                else if (dgv5.Visible)
                    Printer.DrawQuad(5, 8, 5, 4, "Trust MANUAL EOM Report " + addTitle, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                else if (dgv11.Visible)
                    Printer.DrawQuad(5, 8, 5, 4, "Trust EOM Report " + addTitle, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

                Printer.SetQuadSize(24, 12);
                font = new Font("Ariel", 7, FontStyle.Regular);
                string lock1 = this.dateTimePicker1.Value.ToString("MM/dd/yyyy");
                string lock2 = this.dateTimePicker2.Value.ToString("MM/dd/yyyy");
                string ach1 = this.dateTimePicker3.Value.ToString("MM/dd/yyyy");
                string ach2 = this.dateTimePicker4.Value.ToString("MM/dd/yyyy");
                if (chkACH.Checked)
                {
                    Printer.DrawQuad(22, 8, 5, 4, "ACH  Stop  " + ach2, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                    Printer.DrawQuad(22, 6, 5, 4, "ACH  Start  " + ach1, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                    Printer.DrawQuad(22, 3, 5, 4, "LKBX Stop " + lock2, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                    Printer.DrawQuad(22, 0, 5, 4, "LKBX Start " + lock1, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                }
                else
                {
                    Printer.DrawQuad(22, 3, 5, 4, "Stop " + lock2, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                    Printer.DrawQuad(22, 0, 5, 4, "Start " + lock1, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                }
            }
            else if (workReport.ToUpper() == "INSURANCE WEEK TOTALS")
            {
                Printer.DrawQuad(6, 8, 5, 4, workTitle, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                string lock1 = this.dateTimePicker1.Value.ToString("MM/dd/yyyy");
                string lock2 = this.dateTimePicker2.Value.ToString("MM/dd/yyyy");
                Printer.DrawQuad(11, 4, 5, 4, "Stop " + lock2, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                Printer.DrawQuad(11, 1, 5, 4, "Start " + lock1, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            }
            else if (chkDownPayments.Checked)
            {
                if (workTitle == "New Business Report (1.2)")
                {
                    if (majorRunOn.ToUpper() == "TRUSTS")
                        Printer.DrawQuad(6, 8, 5, 4, "Trust - " + workTitle, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                    else if (majorRunOn.ToUpper() == "CEMETERIES")
                    {
                        Printer.SetQuadSize(24, 24);
                        string title = workTitle;
                        title = title.Replace("1.2", "1.3");
                        Printer.DrawQuad(10, 15, 10, 8, "Perpetual Care - " + title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                        Printer.SetQuadSize(12, 12);
                    }
                    else if (majorRunOn.ToUpper() == "RILES")
                        Printer.DrawQuad(6, 8, 5, 4, "Riles - " + workTitle, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                }
                else
                    Printer.DrawQuad(6, 7, 5, 4, workTitle, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            }
            else if (chkPayments.Checked)
                Printer.DrawQuad(5, 8, 5, 4, workTitle, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (chkPaidUp.Checked)
            {
                Printer.DrawQuad(5, 8, 5, 4, workTitle, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                string lock1 = this.dateTimePicker1.Value.ToString("MM/dd/yyyy");
                string lock2 = this.dateTimePicker2.Value.ToString("MM/dd/yyyy");
                Printer.DrawQuad(11, 4, 5, 4, "Stop Date " + lock2, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                Printer.DrawQuad(11, 1, 5, 4, "Start Date " + lock1, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            }
            else if (chkDeaths.Checked)
                Printer.DrawQuad(5, 8, 5, 4, workTitle, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (chkBalanceLessPayment.Checked)
                Printer.DrawQuad(5, 8, 5, 4, "Trust Monthly Balance < Payment Master Listing", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (chkBalances.Checked)
                Printer.DrawQuad(5, 8, 5, 4, "Trust Monthly Balance < $" + txtBalance.Text.Trim() + " Master Listing", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            if (workReport.ToUpper() == "CASH REMITTED REPORT")
            {
                string addTitle = this.dateTimePicker2.Value.ToString("MMMMMMMMMMMMM");
                addTitle += " " + this.dateTimePicker2.Value.Year.ToString("D4");
                if (dgv6.Visible)
                {
                    if (majorRunOn.Trim().ToUpper() == "CEMETERIES")
                    {
                        Printer.SetQuadSize(24, 24);
                        Printer.DrawQuad(11, 10, 10, 6, "Perpetual Care", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

                        Printer.SetQuadSize(12, 12);
                        Printer.DrawQuad(5, 8, 5, 4, "Remit Payments 8.1 " + addTitle + " " + this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                    }
                    else if (majorRunOn.Trim().ToUpper() == "TRUSTS")
                    {
                        Printer.SetQuadSize(24, 24);
                        //Printer.DrawQuad(12, 10, 10, 6, "Post 2002", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

                        //Printer.SetQuadSize(12, 12);
                        Printer.DrawQuad(9, 10, 12, 8, "Remit Payments 8.0 " + addTitle + " Cover Sheet", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                        //Printer.DrawQuad(5, 8, 5, 4, "Remit Payments 8.0 " + addTitle + " " + this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                        //Printer.DrawQuad(5, 8, 5, 4, workTitle + " " + addTitle, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                        Printer.SetQuadSize(12, 12);
                    }
                    else if (majorRunOn.Trim().ToUpper() == "RILES")
                    {
                        Printer.SetQuadSize(24, 24);
                        Printer.DrawQuad(11, 10, 10, 6, "RILES - MFDA", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

                        Printer.SetQuadSize(12, 12);
                        Printer.DrawQuad(5, 8, 5, 4, "Remit Payments 8.2 " + addTitle + " " + this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                        //Printer.DrawQuad(5, 8, 5, 4, workTitle + " " + addTitle, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                    }
                }
                if (dgv7.Visible)
                    Printer.DrawQuad(5, 8, 5, 4, "Remit DP's 8.0 " + addTitle, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                if (dgv8.Visible)
                    Printer.DrawQuad(5, 8, 5, 4, "Remit Payments 8.0 " + addTitle, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                if (dgv13.Visible)
                    Printer.DrawQuad(5, 8, 5, 4, "OVP Payments 8.0 " + addTitle, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            }
            if ( workReport.ToUpper() == "REMOVAL REPORT")
            {
                string addTitle = this.dateTimePicker2.Value.ToString("MMMMMMMMMMMMM");
                addTitle += "-" + this.dateTimePicker2.Value.Year.ToString("D4");
                Printer.DrawQuad(5, 8, 5, 4, "Removal Report 5.1 " + addTitle, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            }
            if (workReport.ToUpper() == "DECEASED REPORT")
            {
                string addTitle = this.dateTimePicker2.Value.ToString("MMMMMMMMMMMMM");
                addTitle += "-" + this.dateTimePicker2.Value.Year.ToString("D4");
                Printer.DrawQuad(5, 8, 5, 4, "Deceased Report " + addTitle, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            }


            //            Printer.DrawQuadTicks();
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 7, FontStyle.Regular);
            //Printer.DrawQuad(16, 7, 5, 2, lblBalance.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            //Printer.DrawQuad(16, 10, 5, 2, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            font = new Font("Ariel", 8);
            string search = "Agents : All";
            if (!String.IsNullOrWhiteSpace(chkComboAgent.Text))
                search = "Agents : " + chkComboAgent.Text;
            if (dgv.Visible)
                Printer.DrawQuad(1, 6, 6, 3, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            Printer.DrawQuad(1, 9, 6, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            //DateTime date = this.dateTimePicker2.Value;
            //date = date.AddDays(1);
            //string workDate = date.ToString("MM/dd/yyyy");
            //Printer.SetQuadSize(24, 12);
            //font = new Font("Ariel", 9, FontStyle.Bold);
            //Printer.DrawQuad(20, 8, 5, 4, "Report Ending: " + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private int footerCount = 0;
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
            }
        }
        /****************************************************************************************/
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
                if (footerCount >= 1)
                {
                    footerCount = 0;
                    if (chkSort.Checked || autoRun)
                        e.PS.InsertPageBreak(e.Y);
                }
            }
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
            else if (e.Column.FieldName.ToUpper() == "DOWNPAYMENT1")
            {
                if (e.RowHandle >= 0)
                {
                    double downPayment1 = e.DisplayText.Trim().ObjToDouble();
                    int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                    DataTable dt = (DataTable)dgv.DataSource;
                    double downPayment = dt.Rows[row]["downPayment"].ObjToDouble();
                    if (downPayment != downPayment1)
                    {
                        //e.Appearance.BackColor = Color.Red;
                        //e.Appearance.ForeColor = Color.Yellow;
                    }
                }
            }
            //else if (e.Column.FieldName.ToUpper() == "DUEDATE8")
            //{
            //    if (e.RowHandle >= 0)
            //    {
            //        DateTime date = e.DisplayText.ObjToDateTime();
            //        e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
            //    }
            //}
            //else if (e.Column.FieldName.ToUpper() == "PAYDATE8")
            //{
            //    if (e.RowHandle >= 0)
            //    {
            //        DateTime date = e.DisplayText.Trim().ObjToDateTime();
            //        string str = date.ToString("MM/dd/yyyy");
            //        if (str.Trim() == "01/01/0001")
            //        {

            //        }
            //        //                    e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
            //        e.DisplayText = str;
            //    }
            //}
            //else if (e.Column.FieldName.ToUpper() == "LASTDATEPAID8")
            //{
            //    if (e.RowHandle >= 0)
            //    {
            //        DateTime date = e.DisplayText.ObjToDateTime();
            //        e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
            //    }
            //}
            //else if ( e.Column.FieldName.ToUpper().IndexOf( "DATE") >= 0 )
            //{
            //    if (e.RowHandle >= 0)
            //    {
            //        DateTime date = e.DisplayText.ObjToDateTime();
            //        if (date.Year < 100)
            //            e.DisplayText = "";
            //        else
            //            e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
            //    }
            //}
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            dgv.DataSource = null;
            dgv.Refresh();
            dgv7.DataSource = null;
            dgv7.Refresh();

            expandedRun = false;

            menuStrip1.BackColor = panelTop.BackColor;
            menuStrip1.Refresh();

            DateTime date = dateTimePicker1.Value;
            DateTime saveDate1 = date;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            DateTime saveDate2 = date;
            string date2 = G1.DateTimeToSQLDateTime(date);

            date = dateTimePicker3.Value;
            string date3 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker4.Value;
            string date4 = G1.DateTimeToSQLDateTime(date);

            bool allowSave = false;
            if (workReport.ToUpper() == "CASH REMITTED REPORT")
            {
                if (saveDate1.Day == 1)
                {
                    int dim = DateTime.DaysInMonth(saveDate2.Year, saveDate2.Month);
                    if (saveDate2.Day == dim)
                        allowSave = true;
                }
            }
            else if (workTitle == "New Business Report (1.2)")
            {
                if (saveDate1.Day == 1)
                {
                    int dim = DateTime.DaysInMonth(saveDate2.Year, saveDate2.Month);
                    if (saveDate2.Day == dim)
                        allowSave = true;
                }
            }

            string newContractsDate3 = date3;
            string newContractsDate4 = date4;

            paymentsReportDate = dateTimePicker1.Value;

            date = dateTimePicker1.Value.AddMonths(-1);
            string lastMonthStart = G1.DateTimeToSQLDateTime(date);
            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, daysInMonth);
            string lastMonthEnd = G1.DateTimeToSQLDateTime(date);

            int months = G1.GetMonthsBetween(dateTimePicker2.Value, dateTimePicker1.Value);
            if ((months + 1 ) > 1 )
                expandedRun = true;

            DateTime now = DateTime.Now;

            DateTime paidout = new DateTime(2039, 12, 31);
            //            string date3 = G1.DateTimeToSQLDateTime(paidout);

            //            string cmd = "Select * from `payments` where `lastDatePaid8` <= '" + date2 + "';";

            loading = true;
            string contractNumber = "";
            string saveDate = "";
            int passes = 1;
            if (chkShowAll.Checked)
            {
                //passes = 2;
                chkShowAll.Checked = false;
                chkDownPayments.Checked = false;
            }
            int pass = 1;

            //PleaseWait waitForm = G1.StartWait();

            DataTable dt = null;
            DataTable allDt = null;
            bool doSecondaySearch = false;
            string saveCmd = "";
            string cmd = "";
            double downPayment = 0D;
            string record = "";
            string fill1 = "";
            DataRow[] ddRx = null;
            double newInterest = 0D;
            double newTrust85 = 0D;
            double newTrust100 = 0D;
            string findRecord = "";
            double ccFee = 0D;

            cemDt = G1.get_db_data("Select * from `cemeteries`;");

            string testContract = this.txtContract.Text;

            if (insurance)
                gridMain.Columns["lastName"].SummaryItem.SummaryType = SummaryItemType.None;

            string runOn = cmbRunOn.Text.Trim().ToUpper();
            majorRunOn = runOn;

            for (;;)
            {
                cmd = "Select * from `" + contractsFile + "` p ";
                cmd += " JOIN `" + customersFile + "` c ON p.`contractNumber` = c.`contractNumber` ";
                cmd += " JOIN `" + paymentsFile + "` d ON p.`contractNumber` = d.`contractNumber` ";
                //if ( chkPaidUp.Checked )
                //    cmd += " JOIN `lapse_list` q ON p.`contractNumber` = q.`contractNumber` ";
                if (workReport.ToUpper() == "NEW BUSINESS REPORT")
                {
                    if (this.dateTimePicker3.Visible)
                    {
                        cmd += " where p.`issueDate8` >= '" + date3 + "' ";
                        cmd += " and   p.`issueDate8` <= '" + date4 + "' ";
                    }
                    else
                    {
                        cmd += " where p.`issueDate8` >= '" + date1 + "' ";
                        cmd += " and   p.`issueDate8` <= '" + date2 + "' ";
                    }
                }
                else if (workReport.ToUpper() == "SHOW PAYMENTS DUE NEXT MONTH")
                {
                    DateTime nextDue = this.dateTimePicker2.Value;
                    nextDue = nextDue.AddMonths(1);
                    nextDue = new DateTime(nextDue.Year, nextDue.Month, 1);
                    string nDate = G1.DateTimeToSQLDateTime(nextDue);
                    cmd += " where p.`dueDate8` <= '" + nDate + "' AND p.`lapsed` <> 'Y' AND p.`deleteFlag` <> 'L' and `lastDatePaid8` <> '0000-00-00' and p.`dueDate8` <> '0000-00-00'";
                    //                    cmd += " where p.`dueDate8` = '" + nDate + "'" ;
                }
                else if (workReport.ToUpper() == "DBR REPORT")
                {
                    //cmd += " where p.`deceasedDate` >= '" + date3 + "' ";
                    //cmd += " where p.`deceasedDate` <= '" + date4 + "' ";
                    cmd += " and d.`payDate8` >= '" + date1 + "' ";
                    cmd += " and d.`payDate8` <= '" + date2 + "' ";
                }
                else if (workReport.ToUpper() == "REINSTATEMENT REPORT")
                {
                    cmd += " where p.`reinstateDate8` >= '" + date1 + "' ";
                    cmd += " and   p.`reinstateDate8` <= '" + date2 + "' ";
                }
                else if (workReport.ToUpper() == "LAPSE REPORT")
                {
                    cmd += " where p.`lapseDate8` >= '" + date1 + "' ";
                    cmd += " and   p.`lapseDate8` <= '" + date2 + "' ";
                }
                else if (chkPayments.Checked || chkDeaths.Checked || chkPaidUp.Checked)
                {
                    cmd += " where d.`payDate8` >= 'XYZZY1' ";
                    cmd += " and   d.`payDate8` <= 'XYZZY2' ";
                    saveDate = cmd;

                    if (!chkPaidUp.Checked)
                    {
                        if (!chkACH.Checked)
                        {
                            //if (this.dateTimePicker3.Value >= DailyHistory.majorDate)
                            //    cmd += "AND (`depositNumber` LIKE 'T%' OR `depositNumber` LIKE 'A%') ";
                        }
                        else
                        {
//                            if (this.dateTimePicker3.Value >= DailyHistory.majorDate)
                                cmd += "AND (`depositNumber` LIKE 'T%' ) ";
                        }
                    }
                    doSecondaySearch = true;
                }
                else
                {
                    if (!this.dateTimePicker3.Visible)
                    {
                        cmd += " where p.`issueDate8` >= '" + date1 + "' ";
                        cmd += " and   p.`issueDate8` <= '" + date2 + "' ";
                    }
                    else
                    {
                        cmd += " where p.`issueDate8` >= '" + date3 + "' ";
                        cmd += " and   p.`issueDate8` <= '" + date4 + "' ";
                    }
                }
                //if ( insurance )
                //    cmd += " AND c.`coverageType` = 'ZZ' ";
                //else
                //    cmd += " AND c.`coverageType` <> 'ZZ' ";

                if (!String.IsNullOrWhiteSpace(testContract))
                    cmd += " and p.`contractNumber` = '" + testContract + "' ";
                cmd += " and d.`fill` <> 'D' ";
                if (workReport.ToUpper() == "DBR REPORT")
                    cmd += " AND p.`deceasedDate` > '1850-01-01' ";
                if (chkDownPayments.Checked)
                {
                    if (workReport.ToUpper() != "DBR REPORT")
                    {
                        if ( runOn.ToUpper() == "CEMETERIES")
                            cmd += " and d.`paymentAmount` <> '0.00' ";
                        else
                            cmd += " and p.`downPayment` <> '0.00' ";
                    }
                }
                if (chkBalances.Checked)
                    cmd += " and ( `balanceDue` <= '" + txtBalance.Text + "' AND `balanceDue` <> '0 ) ";
                if (chkBalanceLessPayment.Checked)
                    cmd += " and ( `balanceDue` <= `amtOfMonthlyPayt` AND `balanceDue` <> '0' and p.`deceasedDate` < '1200-01-01' ) ";
                if (chkActiveOnly.Checked)
                {
                    cmd += " and p.`lapsed` <> 'Y' AND c.`lapsed` <> 'Y' AND p.`deleteFlag` <> 'L' and `lastDatePaid8` <> '0000-00-00' ";
                    cmd += " and p.`deceasedDate` = '0000-00-00' ";
                }
                if (chkDeaths.Checked)
                {
                    cmd += " and p.`deceasedDate` > '1850-01-01' and d.`payDate8` > p.`deceasedDate` ";
                }
                //if (chkPaidUp.Checked) // Do this a different way
                //{
                //    cmd += " and ( p.`balanceDue` < p.`amtOfMonthlyPayt` OR p.`dueDate8` >= '2039-12-31' ) ";
                //}
                //if (chkPayments.Checked)
                //{
                //    //                cmd += " and d.paymentAmount > '0.00' ";
                //}
                //            cmd += " and d.`contractNumber` = 'B18013LI' ";

                if (first)
                {
                    first = false;
                    loadAgents(cmd, date1, date2);
                }

                string agents = getAgentQuery();
                if (!String.IsNullOrWhiteSpace(agents))
                    cmd += " and " + agents;

                if (runOn.ToUpper() == "CEMETERIES")
                    cmd += " AND (d.`contractNumber` LIKE 'nnm%' OR d.`contractNumber` LIKE 'HC%' ) ";
                else if ( runOn.ToUpper() == "RILES")
                    cmd += " AND d.`contractNumber` LIKE 'RF%' ";

                if (!chkPayments.Checked)
                {
                    if (chkDownPayments.Checked)
                        cmd += " GROUP BY p.`contractNumber` ORDER BY d.`payDate8` ASC";
                    else
                    {
                        if (chkPaidUp.Checked)
                        {
                            //                            cmd += " GROUP BY p.`contractNumber` ORDER BY p.`contractNumber`, d.`payDate8` DESC";
                            //cmd += " AND p.`deceasedDate` < '1900-01-01' ";
                            cmd += " ORDER BY p.`contractNumber`, d.`payDate8` DESC";
                        }
                        else
                            cmd += " GROUP BY p.`contractNumber` ORDER BY p.`dueDate8` DESC";
                    }
                }
                else
                {
                    if (workReport.ToUpper() == "REINSTATEMENT REPORT")
                        cmd += " GROUP BY p.`contractNumber` ORDER BY p.`reinstateDate8` ASC";
                    else if (workReport.ToUpper() == "LAPSE REPORT")
                        cmd += " GROUP BY p.`contractNumber` ORDER BY p.`lapseDate8` ASC";
                    else
                        cmd += " ORDER BY d.`payDate8` ";
                }
                cmd += ";";

                if (doSecondaySearch)
                {
                    saveCmd = cmd;
                    cmd = cmd.Replace("XYZZY1", date1);
                    cmd = cmd.Replace("XYZZY2", date2);
                }

                if ( workReport.ToUpper() == "REMOVAL REPORT")
                {
                    cmd = "Select * from `" + contractsFile + "` p ";
                    cmd += " JOIN `" + customersFile + "` c ON p.`contractNumber` = c.`contractNumber` ";
                    cmd += " WHERE p.`dateDPPaid` >= '" + date1 + "' AND p.`dateDPPaid` <= '" + date2 + "' AND p.`trustRemoved` = 'Yes' ";
                    cmd += ";";
                }
                if (workReport.ToUpper() == "DECEASED REPORT")
                {
                    cmd = "Select * from `" + contractsFile + "` p ";
                    cmd += " JOIN `" + customersFile + "` c ON p.`contractNumber` = c.`contractNumber` ";
                    cmd += " WHERE p.`deceasedDate` >= '" + date1 + "' AND p.`deceasedDate` <= '" + date2 + "' ORDER BY p.`deceasedDate` ";
                    cmd += ";";
                }
                /*****************************************************************/
                if ( workPayOffDt != null)
                {
                    cmd = "Select * from `" + contractsFile + "` p ";
                    cmd += " JOIN `" + customersFile + "` c ON p.`contractNumber` = c.`contractNumber` ";
                    cmd += " JOIN `" + paymentsFile + "` d ON p.`contractNumber` = d.`contractNumber` ";
                    cmd += " WHERE p.`contractNumber` IN (";
                    string contractList = "";
                    for ( int kk=0; kk<workPayOffDt.Rows.Count; kk++)
                    {
                        contractNumber = workPayOffDt.Rows[kk]["contractNumber"].ObjToString();
                        if ( !String.IsNullOrWhiteSpace ( contractNumber))
                            contractList += "'" + contractNumber + "',";
                    }
                    contractList = contractList.TrimEnd(',');
                    contractList += ") ";
                    cmd += contractList;
                    cmd += " GROUP BY p.`contractNumber` ORDER BY p.`contractNumber`, d.`payDate8` DESC;";
                }
                /*****************************************************************/
                if (doPaidOff && !String.IsNullOrWhiteSpace ( testContract ) )
                {
                    doSecondaySearch = false;
                    cmd = "SELECT * FROM `contracts` p ";
                    cmd += " JOIN `customers` c ON p.`contractNumber` = c.`contractNumber` ";
                    cmd += " WHERE p.`contractNumber` = '" + testContract + "';";
                }
                /*****************************************************************/
                //if (chkPaidUp.Checked) // Other way is just as fast.
                //{
                //    doSecondaySearch = false;
                //    cmd = "SELECT* FROM `contracts` p ";
                //    cmd += " JOIN `customers` c ON p.`contractNumber` = c.`contractNumber` ";
                //    cmd += " JOIN(SELECT * FROM `payments` d ";

                //    cmd += " WHERE d.`payDate8` >= '" + date1 + "' AND d.`payDate8` <= '" + date2 + "' ORDER BY d.`paidDate` DESC ";
                //    cmd += " ) AS payments ON(p.`contractNumber` = payments.`contractNumber`) ";

                //    cmd += " GROUP BY p.`contractNumber`;";
                //}

                /*****************************************************************/

                DateTime dtNow = DateTime.Now;
                dt = G1.get_db_data(cmd); // RAMMA ZAMMA

                if (runOn.ToUpper() == "CEMETERIES" || runOn.ToUpper() == "RILES")
                    doSecondaySearch = false;

                //DailyHistory.AddAP(dt);
                //DailyHistory.CleanupVisibility(gridMain);

                //double paymentAmount = 0D;
                //double ccFee = 0D;

                //for ( int i=0; i<dt.Rows.Count; i++)
                //{
                //    paymentAmount = dt.Rows[i]["paymentamount"].ObjToDouble();
                //    ccFee = dt.Rows[i]["ccFee"].ObjToDouble();
                //    paymentAmount -= ccFee;
                //    dt.Rows[i]["paymentAmount"] = paymentAmount;
                //}


                Trust85.FindContract(dt, "ZZ0003043");

                if ( expandedRun && !chkPaidUp.Checked )
                {
                    ProcessDBR(dt);
                    dgv.DataSource = dt;
                    this.Cursor = Cursors.Default;
                    return;
                }

                if ( doPaidOff )
                {
                    SetupSelectColumn(dt);
                    GenerateNotices(dt);
                    return;
                }
                DateTime dtNow2 = DateTime.Now;
                TimeSpan tts = dtNow2 - dtNow;

                if (workReport == "Paid Up Contracts Report")
                    dt = GetGroupByDate(dt);


                Trust85.FindContract(dt, "M16033UI");

                DailyHistory.RemoveDeletedPayments(dt);
                /*****************************************************************/
                Trust85.FindContract(dt, "nnm20001");

                //                dt.Columns.Add("select");
                SetupSelectColumn(dt);
                Trust85.FindContract(dt, "L24053LI");
                if (workPayOffDt != null)
                {
                    SetupSelectColumn(dt);
                    for ( int i=0; i<dt.Rows.Count; i++)
                    {
                        contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                        DataRow [] dR = workPayOffDt.Select("contractNumber='" + contractNumber + "'");
                        if ( dR.Length > 0 )
                        {
                            dt.Rows[i]["balanceDue"] = dR[0]["balanceDue"];
                            dt.Rows[i]["payDate8"] = G1.DTtoMySQLDT(dR[0]["date"].ObjToDateTime());
                        }
                    }
                }
/*****************************************************************/

                Trust85.FindContract(dt, "FF23054LI");
                DateTime downPaymentDate = DateTime.Now;
                bool rtn = false;

                double trust85_1 = 0D;
                double trust100_1 = 0D;
                string record2 = "";
                string depositNum = "";

                if (workReport.ToUpper() == "NEW BUSINESS REPORT")
                {
                    int row = 0;
                    DataRow[] dRs = null;
                    cmd = "Select * from `" + contractsFile + "` p ";
                    cmd += " JOIN `" + customersFile + "` c ON p.`contractNumber` = c.`contractNumber` ";
                    cmd += " WHERE p.`issueDate8` >= '" + date1 + "' AND p.`issueDate8` <= '" + date2 + "' AND p.`downPayment` = '0.00' ";
                    cmd += ";";
                    DataTable ddd = G1.get_db_data(cmd);
                    for (int k = 0; k < ddd.Rows.Count; k++)
                    {
                        record2 = "";
                        contractNumber = ddd.Rows[k]["contractNumber"].ObjToString();
                        if ( contractNumber == "NNM24002")
                        {
                        }
                        dRs = dt.Select("contractNumber='" + contractNumber + "'");
                        if (dRs.Length <= 0)
                        {
                            DataTable ddx = G1.get_db_data("Select * from `payments` where `contractNumber` = '" + contractNumber + "' AND `downPayment` > '0';");
                            if (ddx.Rows.Count > 0 && workTitle == "New Business Report (1.2)")
                            {
                                findRecord = ddx.Rows[0]["record"].ObjToString();
                                record2 = findRecord;
                                DailyHistory.CalcPaymentData(contractNumber, findRecord, ref newInterest, ref newTrust85, ref newTrust100);
                                trust85_1 = newTrust85;
                                trust100_1 = newTrust100;
                                downPaymentDate = ddx.Rows[0]["payDate8"].ObjToDateTime();
                                downPayment = ddx.Rows[0]["downPayment"].ObjToDouble();
                                ccFee = ddx.Rows[0]["ccFee"].ObjToDouble();
                                if (ccFee > 0D)
                                    downPayment += ccFee;
                            }
                            else
                            {
                                rtn = DailyHistory.GetDownPaymentFromPayments(contractNumber, ref downPayment, ref downPaymentDate, ref trust85_1, ref trust100_1, ref ccFee, ref record2, ref depositNum);
                                if (!rtn)
                                    continue;
                            }
                            dt.ImportRow(ddd.Rows[k]);
                            row = dt.Rows.Count - 1;
                            dt.Rows[row]["trust85P"] = trust85_1;
                            dt.Rows[row]["trust100P"] = trust100_1;
                            dt.Rows[row]["downpayment1"] = downPayment;
                            dt.Rows[row]["ccFee"] = ccFee;
                            dt.Rows[row]["debitAdjustment"] = 0D;
                            dt.Rows[row]["creditAdjustment"] = 0D;
                            if ( !String.IsNullOrWhiteSpace ( record2 ))
                            dt.Rows[row]["record2"] = record2;
                            try
                            {
                                if (rtn)
                                    dt.Rows[row]["payDate8"] = G1.DTtoMySQLDT(downPaymentDate);
                                else
                                {
                                    downPaymentDate = dt.Rows[row]["issueDate8"].ObjToDateTime();
                                    dt.Rows[row]["payDate8"] = G1.DTtoMySQLDT(downPaymentDate);
                                }
                            }
                            catch ( Exception ex)
                            {

                            }
                        }
                    }
                }

                if (chkDownPayments.Checked && workReport.ToUpper() != "DBR REPORT" )
                {
                    DataRow[] dRs = null;
                    cmd = "Select * from `" + contractsFile + "` p ";
                    cmd += " JOIN `" + customersFile + "` c ON p.`contractNumber` = c.`contractNumber` ";
                    cmd += " JOIN `" + paymentsFile + "` d ON p.`contractNumber` = d.`contractNumber` ";

                    cmd += " WHERE d.`payDate8` >= '" + date1 + "' AND d.`payDate8` <= '" + date2 + "' AND d.`downPayment` > '0.00'";
                    DataTable ddd = G1.get_db_data(cmd);
                    for ( int k=0; k<ddd.Rows.Count; k++)
                    {
                        contractNumber = ddd.Rows[k]["contractNumber"].ObjToString();
                        if (contractNumber == "P20101LI")
                        {
                        }
                        dRs = dt.Select("contractNumber='" + contractNumber + "'");
                        if ( dRs.Length <= 0 )
                        {
                            dt.ImportRow(ddd.Rows[k]);
                        }
                    }
                }

                if (this.dateTimePicker3.Visible && chkACH.Checked )
                {
                    if (doSecondaySearch)
                    {
                        cmd = saveCmd;
                        cmd = cmd.Replace("XYZZY1", date3);
                        cmd = cmd.Replace("XYZZY2", date4);
                        if (!chkACH.Checked)
                            cmd = cmd.Replace("(`depositNumber` LIKE 'T%' OR `depositNumber` LIKE 'A%')", "(`depositNumber` NOT LIKE 'T%' AND `depositNumber` NOT LIKE 'A%')");
                        else
                        {
                            cmd = cmd.Replace("(`depositNumber` LIKE 'T%' )", "(`depositNumber` NOT LIKE 'T%' ) ");
//                            cmd = cmd.Replace ("(`depositNumber` LIKE 'T%' )", " (`depositNumber` NOT LIKE 'T%' AND `depositNumber` NOT LIKE 'A%') ");
                        }
                        if (!chkPaidUp.Checked)
                        {
                            DataTable ddt = G1.get_db_data(cmd);
                            Trust85.FindContract(dt, "L17027UI");
                            for (int i = 0; i < ddt.Rows.Count; i++)
                                dt.ImportRow(ddt.Rows[i]);
                            Trust85.FindContract(dt, "L17027UI");
                            pass = 2;
                        }
                    }
                }

                dt.Columns.Add("num");
                dt.Columns.Add("customer");
                dt.Columns.Add("daysLate", Type.GetType("System.Int32"));
                dt.Columns.Add("phone");
                dt.Columns.Add("totalContract", Type.GetType("System.Double"));
                dt.Columns.Add("contractValue", Type.GetType("System.Double"));
                dt.Columns.Add("newBusiness", Type.GetType("System.Double"));

                if ( G1.get_column_number ( dt, "retained") < 0 )
                    dt.Columns.Add("retained", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "LiInterest") < 0)
                    dt.Columns.Add("LiInterest", Type.GetType("System.Double"));

                dt.Columns.Add("financedAmount", Type.GetType("System.Double"));
                dt.Columns.Add("businessDays", Type.GetType("System.Double"));
                dt.Columns.Add("dbr", Type.GetType("System.Double"));
                dt.Columns.Add("ovp", Type.GetType("System.Double"));
                dt.Columns.Add("age");
                dt.Columns.Add("loc");
                dt.Columns.Add("Location Name");
                dt.Columns.Add("trust");
                dt.Columns.Add("Trust Name");
                dt.Columns.Add("S", Type.GetType("System.Double"));
                dt.Columns.Add("numMonths", Type.GetType("System.Double"));
                dt.Columns.Add("transactionCode");

                int totalCount = dt.Rows.Count;

                if (chkDownPayments.Checked && workReport.ToUpper() != "DBR REPORT" )
                {
                    //                    FindNewContracts(dt, date1, date2); // Appeared to be wrong
                    date = this.dateTimePicker3.Value;
                    if ( this.dateTimePicker3.Visible)
                        FindNewContracts(dt, date3, date4);
                    else
                        FindNewContracts(dt, date1, date2);
                }

                if (pass >= passes)
                {
                    if (allDt != null)
                    {
                        SetSinglePremiums(dt);
                        for (int i = 0; i < allDt.Rows.Count; i++)
                            dt.ImportRow(allDt.Rows[i]);
                        allDt.Dispose();
                        allDt = null;
                        chkPayments.Checked = true;
                    }
                    break;
                }
                else
                {
                    pass++;
                    allDt = dt.Copy();
                    chkPayments.Checked = false;
                    chkShowAll.Checked = true;
                    chkDownPayments.Checked = true;
                }
                if (runOn.ToUpper() == "CEMETERIES" || runOn.ToUpper() == "RILES")
                    break;
            }
            if (workReport.ToUpper() != "NEW BUSINESS REPORT" && runOn.ToUpper() != "CEMETERIES" && runOn.ToUpper() != "RILES")
            {
                if (workTitle != "Trust Down Payment Master List Report (6.1)" && workTitle != "Balances Less Than Payments Report" && workTitle != "BALANCES LESS THAN X REPORT" )
                {
                    if ( !insurance && workReport != "Paid Up Contracts Report")
                        Trust85.LoadTrustAdjustments(dt, this.dateTimePicker1.Value, this.dateTimePicker2.Value);
                }
            }
            Trust85.FindContract(dt, "B23029L");

            if (workReport.ToUpper() == "CASH REMITTED REPORT")
            {
                if (!String.IsNullOrWhiteSpace(saveDate))
                {
                    cmd = saveDate;
                    if (chkACH.Checked)
                    {
                        cmd = cmd.Replace("XYZZY1", date3);
                        cmd = cmd.Replace("XYZZY2", date4);
                    }
                    else
                    {
                        cmd = cmd.Replace("XYZZY1", date1);
                        cmd = cmd.Replace("XYZZY2", date2);
                    }
                    cmd += " AND (`depositNumber` NOT LIKE 'T%' AND `depositNumber` NOT LIKE 'A%')";
                    cmd += " AND ( `edited` = 'Manual' OR `edited` = 'TrustAdj' OR `edited` = 'Cemetery' ) ";
                    if (!String.IsNullOrWhiteSpace(testContract))
                        cmd += " AND p.`contractNumber` = '" + testContract + "' ";
                    DataTable ddt = G1.get_db_data(cmd);
                    Trust85.FindContract(ddt, "B23029L");
                    for (int i = 0; i < ddt.Rows.Count; i++)
                    {
                        record = ddt.Rows[i]["record2"].ObjToString();
                        ddRx = dt.Select("record2='" + record + "'");
                        if ( ddRx.Length <= 0)
                            dt.ImportRow(ddt.Rows[i]);
                    }

                    //cmd = saveDate;
                    //cmd = cmd.Replace("XYZZY1", lastMonthStart);
                    //cmd = cmd.Replace("XYZZY2", lastMonthEnd );
                    //cmd += " AND d.`reportFollowing` = 'Y' ";
                    //if (!String.IsNullOrWhiteSpace(testContract))
                    //    cmd += " AND p.`contractNumber` = '" + testContract + "' ";
                    //ddt = G1.get_db_data(cmd);
                    //for (int i = 0; i < ddt.Rows.Count; i++)
                    //{
                    //    record = ddt.Rows[i]["record2"].ObjToString();
                    //    ddRx = dt.Select("record2='" + record + "'");
                    //    if (ddRx.Length <= 0)
                    //        dt.ImportRow(ddt.Rows[i]);
                    //}

                    DailyHistory.RecalcRetained(dt, "interestPaid1");

                    //Trust85.FindContract(dt, "M20077L");
                    DailyHistory.RemoveDeletedPayments(dt);
                    //Trust85.FindContract(dt, "B17054UI");
                }
            }


            Trust85.FindContract(dt, "B23029L");
            if ( workReport.ToUpper() != "REMOVAL REPORT")
                G1.ClarifyDownPayments(dt, this.dateTimePicker1.Value, this.dateTimePicker4.Value);

            Trust85.FindContract(dt, "B23029L");

            loading = false;
            contractNumber = "";

            runOn = cmbRunOn.Text.Trim().ToUpper();

            if ( runOn.ToUpper() != "RILES" )
                dt = SMFS.FilterForRiles(dt);

            if (workReport.ToUpper() == "DBR REPORT")
            {
                if ( G1.get_column_number ( dt, "ap") < 0 )
                    dt.Columns.Add("ap", Type.GetType("System.Double"));
                string deceasedDate = "";
                string issueDate = "";
                string dateDPPaid = "";
                DateTime testDate = DateTime.Now;
                int days = 0;
                gridMain.Columns["downPayment"].Visible = true;
                gridMain.Columns["dpp"].Visible = true;
                double paymentAmount = 0D;
                double interestAmount = 0D;
                for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                {
                    deceasedDate = dt.Rows[i]["deceasedDate"].ObjToString();
                    deceasedDate = G1.GetSQLDate(deceasedDate);
                    if ( deceasedDate.ObjToDateTime().Year < 1000 )
                    {
                        dt.Rows.RemoveAt(i);
                        continue;
                    }
                    issueDate = dt.Rows[i]["issueDate8"].ObjToString();
                    issueDate = G1.GetSQLDate(issueDate);
                    dateDPPaid = dt.Rows[i]["dateDPPaid"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(dateDPPaid))
                    {
                        testDate = dateDPPaid.ObjToDateTime();
                        if (testDate.Year > 1850) // Good Date, use this instead of issue date
                            issueDate = G1.GetSQLDate(dateDPPaid);
                        else
                        {
                            dateDPPaid = issueDate;
                            dt.Rows[i]["dateDPPaid"] = G1.DTtoMySQLDT(issueDate);
                        }

                    }
                    days = Commission.CalcBusinessDays(deceasedDate, issueDate);
                    dt.Rows[i]["businessDays"] = days;
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                    cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' AND `payDate8` >= '" + date1 + "' AND `payDate8` <= '" + date2 + "';";
                    DataTable ddx = G1.get_db_data(cmd);
                    if ( ddx.Rows.Count > 0 )
                    {
                        if (contractNumber == "NNM2005")
                        {
                            Trust85.FindContract(ddx, "WF15012UI");
                        }
                        DailyHistory.RemoveDeletedPayments(ddx);
                        GetPaymentData(ddx, ref paymentAmount, ref interestAmount);
                        if ( paymentAmount > 0D)
                        {

                        }
                        //dt.Rows[i]["paymentAmount"] = paymentAmount;
                        dt.Rows[i]["ap"] = paymentAmount;
                        dt.Rows[i]["interestPaid1"] = interestAmount;
                    }
                }
            }

            Trust85.FindContract(dt, "B21001L");
            originalDt = dt;

            previousDateRead = false;

            dgv.ContextMenuStrip = null;
            if (chkBalanceLessPayment.Checked)
            {
                gridMain.Columns["balanceDue"].Visible = true;
                gridMain.Columns["amtOfMonthlyPayt"].Visible = true;
                gridMain.Columns["dueDate8"].Visible = true;
                gridMain.Columns["lapseDate8"].Visible = true;
                dgv.ContextMenuStrip = contextMenuStrip3;
            }
            else if (chkBalances.Checked)
            {
                gridMain.Columns["balanceDue"].Visible = true;
                gridMain.Columns["dueDate8"].Visible = true;
                gridMain.Columns["lapseDate8"].Visible = true;
                dgv.ContextMenuStrip = contextMenuStrip3;
            }
            //dt.Columns.Add("num");
            //dt.Columns.Add("customer");
            //dt.Columns.Add("daysLate", Type.GetType("System.Int32"));
            //dt.Columns.Add("phone");
            //dt.Columns.Add("totalContract", Type.GetType("System.Double"));
            //dt.Columns.Add("retained", Type.GetType("System.Double"));
            //dt.Columns.Add("financedAmount", Type.GetType("System.Double"));
            //dt.Columns.Add("age");
            //dt.Columns.Add("loc");
            //dt.Columns.Add("Location Name");
            //dt.Columns.Add("S", Type.GetType("System.Double"));
            string fname = "";
            string lname = "";
            string name = "";
            string area = "";
            string phone = "";
            string address = "";
            string address2 = "";
            string zip = "";
            string zip2 = "";
            string miniContract = "";
            string loc = "";
            string trust = "";
            string locationName = "";
            string trustName = "";
            double serviceTotal = 0D;
            double merchandiseTotal = 0D;
            double allowMerchandise = 0D;
            double allowInsurance = 0D;
            double downpayment = 0D;
            double totalContract = 0D;
            double contractValue = 0D;
            double financedAmount = 0D;

            double totalPayments = 0D;
            double totalBalance = 0D;

            double payment = 0D;
            double balance = 0D;
            double debit = 0D;
            double credit = 0D;
            double retained = 0D;
            double interest = 0D;
            double amtPaid = 0D;
            double downPayment1 = 0D;
            double financeMonths = 0D;
            double rate = 0D;
            downPayment = 0D;
            double originalDownPayment = 0D;
            double principal = 0D;
            double amtOfMonthlyPayt = 0D;
            double trust85 = 0D;
            double trust100 = 0D;
            double Trust85Calc = 0D;
            double Trust85Paid = 0D;
            double Trust85Real = 0D;
            double actualDownPayment = 0D;
            double newBusiness = 0D;
            bool manual = false;
            string depositLocation = "";
            bool calculateTrust100 = false;
            string force = "";
            string depositNumber = "";
            DateTime tmStamp = DateTime.Now;
            DateTime payDate8 = DateTime.Now;
            string loct = "";

            if (workReport.ToUpper() == "ODD PAYMENTS REPORT")
                gridMain.Columns["amtOfMonthlyPayt"].Visible = true;

            int method = 0;
            string lockTrust85 = "";


            DateTime oldIssueDate;

            //funDt = G1.get_db_data("Select * from `funeralhomes`;");

            G1.NumberDataTable(dt);

            string edited = "";
            double prince = 0D;
            double saveRetained = 0D;
            double cashAdvance = 0D;
            double trustPercent = 0D;
            findRecord = "";
            string finale = "";
            bool honorFinale = false;
            int finaleCount = 0;
            string oldContract = "";
            newInterest = 0D;
            newTrust85 = 0D;
            newTrust100 = 0D;
            bool foundLI = false;
            string oldLoc = "";
            string agentCode = "";
            string SDICode = "";

            Trust85.FindContract(dt, "nnm2005");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (workReport.ToUpper() == "REMOVAL REPORT")
                    continue;
                if (workReport.ToUpper() == "DECEASED REPORT")
                    continue;
                try
                {
                    foundLI = false;
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                    if (contractNumber.ToUpper().EndsWith ( "LI"))
                    {
                        if ( contractNumber == "CT19037LI")
                        {
                        }
                        findRecord = dt.Rows[i]["record2"].ObjToString();
                        if (String.IsNullOrWhiteSpace(findRecord))
                        {
                            continue;
                        }
                        DailyHistory.CalcPaymentData(contractNumber, findRecord, ref newInterest, ref newTrust85, ref newTrust100);
                        foundLI = true;
                        //Trust85.FindContract(dt, "B18035LI");
                    }
                    lockTrust85 = dt.Rows[i]["lockTrust85"].ObjToString().ToUpper();
                    payDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();
                    honorFinale = false;
                    if (oldContract != contractNumber)
                        finaleCount = 0;
                    oldContract = contractNumber;
                    finale = dt.Rows[i]["new"].ObjToString().ToUpper();
                    if ( finale == "FINALE")
                    {
                        finaleCount++;
                        if (finaleCount == 1)
                            honorFinale = true;
                    }
                    //if (finale.ToUpper() == "FINALE")
                    //    continue;
                    edited = dt.Rows[i]["edited"].ObjToString();
                    if (workTitle == "Trust Down Payment Master List Report (6.1)")
                    {
                        if (edited.ToUpper() == "TRUSTADJ" )
                            continue;
                    }

                    fname = dt.Rows[i]["firstName"].ObjToString();
                    lname = dt.Rows[i]["lastName"].ObjToString();
                    name = fname + " " + lname;
                    dt.Rows[i]["customer"] = name;
                    date = dt.Rows[i]["dueDate8"].ObjToDateTime();
                    payment = dt.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                    if (payment > 500D)
                        payment = Policies.CalcMonthlyPremium(contractNumber, "", payment);
                    if (DailyHistory.isInsurance ( contractNumber))
                        payment = Policies.CalcMonthlyPremium(contractNumber, date );
                    amtOfMonthlyPayt = payment;

                    totalPayments += payment;
                    balance = dt.Rows[i]["balanceDue"].ObjToDouble();
                    totalBalance += balance;
                    date = dt.Rows[i]["dueDate8"].ObjToDateTime();
                    TimeSpan ts = now - date;
                    dt.Rows[i]["daysLate"] = (int)ts.Days;
                    area = dt.Rows[i]["areaCode"].ObjToString();
                    phone = dt.Rows[i]["phoneNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(phone))
                        phone = dt.Rows[i]["phoneNumber1"].ObjToString();
                    else
                        phone = "(" + area + ") " + phone;
                    dt.Rows[i]["phone"] = phone;
                    date = dt.Rows[i]["issueDate8"].ObjToDateTime();
                    now = dt.Rows[i]["birthDate"].ObjToDateTime();
                    int age = G1.GetAge(now, date);
                    dt.Rows[i]["age"] = age.ToString();
                    downpayment = dt.Rows[i]["downPayment1"].ObjToDouble();
                    originalDownPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                    if (originalDownPayment == 0D)
                    {
                        actualDownPayment = DailyHistory.GetDownPayment(contractNumber);
                        if (actualDownPayment > 0D)
                            originalDownPayment = actualDownPayment;
                    }

                    //if (!chkPayments.Checked)
                    //{
                    //    dt.Rows[i]["trust100P"] = downpayment;
                    //    downpayment = downpayment * 0.85D;
                    //    downpayment = G1.RoundDown(downpayment);
                    //    dt.Rows[i]["trust85P"] = downpayment;
                    //}

                    //amtPaid = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    amtPaid = DailyHistory.getPayment(dt, i);

                    newBusiness = dt.Rows[i]["newBusiness"].ObjToDouble();
                    //if ( chkPayments.Checked )
                    //{
                    //    downPayment1 = dt.Rows[i]["downPayment1"].ObjToDouble();
                    //    if (downPayment1 != 0D && amtPaid == 0D)
                    //        dt.Rows[i]["paymentAmount"] = downPayment1;
                    //}

                    if ((chkPayments.Checked || chkDownPayments.Checked) && !insurance)
                    {
                        financedAmount = DailyHistory.GetFinanceValue(dt.Rows[i]);
                        dt.Rows[i]["financedAmount"] = financedAmount;
                    }
                    interest = dt.Rows[i]["interestPaid1"].ObjToDouble();
                    debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    trust85 = dt.Rows[i]["trust85P"].ObjToDouble();
                    trust100 = dt.Rows[i]["trust100P"].ObjToDouble();
                    saveRetained = dt.Rows[i]["retained"].ObjToDouble();
                    if ( saveRetained < 0 )
                    {
                        if (Math.Abs(saveRetained) == credit)
                            saveRetained = 0D;
                    }
                    //                    retained = interest + debit - credit;
                    if (edited.ToUpper() == "TRUSTADJ")
                        retained = saveRetained;
                    else
                    {
                        retained = (amtPaid + credit - debit) - trust100;
                        retained = ImportDailyDeposits.CalculateRetained(amtPaid, credit, debit, interest, trust100);
                    }
                    //retained = amtPaid - trust100;
                    //if (amtPaid <= 0D)
                    //{
                    //    if (downpayment > 0D)
                    //        retained = downpayment - trust100;
                    //    else if (credit > 0D)
                    //        retained = credit - trust100;
                    //    else if (debit > 0D)
                    //    {
                    //        retained = debit - Math.Abs( trust100);
                    //        debit = retained * -1D;
                    //    }
                    //}
                    if (saveRetained != 0D) // This is so it matches the Weekly/Monthly Payments Report
                        retained = saveRetained;
                    dt.Rows[i]["retained"] = retained;
                    if (contractNumber.ToUpper().EndsWith("LI"))
                        dt.Rows[i]["retained"] = 0D;

                    //serviceTotal = dt.Rows[i]["serviceTotal"].ObjToDouble();
                    //merchandiseTotal = dt.Rows[i]["merchandiseTotal"].ObjToDouble();
                    //allowMerchandise = dt.Rows[i]["allowMerchandise"].ObjToDouble();
                    //allowInsurance = dt.Rows[i]["allowInsurance"].ObjToDouble();
                    //downpayment = dt.Rows[i]["downPayment"].ObjToDouble();
                    //totalContract = serviceTotal + merchandiseTotal + allowMerchandise + allowInsurance - downpayment;
                    totalContract = DailyHistory.GetFinanceValue(dt.Rows[i]);
                    dt.Rows[i]["totalContract"] = totalContract;
                    //************************************************************************
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                    //if (contractNumber != "M18040LI")
                    //    continue;

                    contractValue = DailyHistory.GetContractValuePlus(dt.Rows[i]);
                    cashAdvance = dt.Rows[i]["cashAdvance"].ObjToDouble();
                    if (workTitle.IndexOf("1.2") > 0)
                    {
                        contractValue = contractValue - cashAdvance;
                    }
                    dt.Rows[i]["contractValue"] = contractValue;

                    oldIssueDate = dt.Rows[i]["issueDate8"].ObjToDateTime();
                    oldIssueDate = DailyHistory.GetIssueDate(oldIssueDate, contractNumber, null);

                    financeMonths = dt.Rows[i]["numberOfPayments"].ObjToDouble();
                    rate = dt.Rows[i]["apr"].ObjToDouble() / 100.0D;

                    //downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                    downPayment = DailyHistory.getDownPayment(dt, i);

                    //payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    payment = DailyHistory.getPayment(dt, i);

                    debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    interest = dt.Rows[i]["interestPaid1"].ObjToDouble();
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    tmStamp = dt.Rows[i]["tmstamp2"].ObjToDateTime();
                    record = dt.Rows[i]["record2"].ObjToString();
                    if (contractNumber == "B19002LI")
                    {
                    }
                    prince = payment - interest + credit - debit;
                    if (payDate8 >= DailyHistory.majorDate)
                    {
                        if (!contractNumber.ToUpper().EndsWith("LI") || interest == 0D)
                        {
                            DailyHistory.CalculateNewInterest(contractNumber, record, ref interest, ref prince);
                            if (contractNumber.ToUpper().EndsWith("LI"))
                                dt.Rows[i]["retained"] = interest;
                        }
                        if (contractNumber.ToUpper().EndsWith("LI"))
                            dt.Rows[i]["retained"] = 0D;
                    }
                    if (contractNumber == "P20101LI")
                    {
                        //Trust85.FindContract(dt, "B18035LI");
                    }
                    downpayment = dt.Rows[i]["downPayment1"].ObjToDouble();

                    trust85 = dt.Rows[i]["trust85P"].ObjToDouble();
                    trust100 = dt.Rows[i]["trust100P"].ObjToDouble();
                    calculateTrust100 = true;
                    if (payDate8 < DailyHistory.majorDate)
                        calculateTrust100 = false;
                    else if (lockTrust85 == "Y")
                        calculateTrust100 = false;
                    //force = dt.Rows[i]["force"].ObjToString();
                    //if (force.ToUpper() == "Y")
                    //    calculateTrust100 = false;

                    if (edited.ToUpper() == "MANUAL" && trust85 < 0D)
                        calculateTrust100 = false;
                    //if (payment == 0D && downpayment == 0D && (credit != 0D || debit != 0D))
                    //{
                    //    calculateTrust100 = false;
                    //}
                    //else
                    //{
                    //                    }
                    if (insurance)
                        calculateTrust100 = false;
                    else
                    {
                        //if (edited.ToUpper() == "TRUSTADJ" || edited.ToUpper() == "CEMETERY" || honorFinale)
                        if (edited.ToUpper() == "TRUSTADJ" || edited.ToUpper() == "CEMETERY" || finale == "FINALE")
                            calculateTrust100 = false;
                    }

                    payment = G1.RoundValue(payment);
                    debit = G1.RoundValue(debit);
                    if ((edited.ToUpper() != "TRUSTADJ" && edited.ToUpper() != "CEMETERY" ) && debit == 0D && credit == 0D)
                    {
                        if (calculateTrust100)
                        {
                            dt.Rows[i]["trust85P"] = 0D;
                            dt.Rows[i]["trust100P"] = 0D;
                        }
                    }
                    credit = G1.RoundValue(credit);
                    downpayment = G1.RoundValue(downpayment);
                    contractValue = DailyHistory.GetContractValue(dt.Rows[i]);
                    contractValue = G1.RoundValue(contractValue);

                    contractValue = DailyHistory.GetContractValuePlus(dt.Rows[i]);
                    financeMonths = dt.Rows[i]["numberOfPayments"].ObjToDouble();
                    rate = dt.Rows[i]["apr"].ObjToDouble() / 100.0D;
                    downPayment1 = dt.Rows[i]["downPayment1"].ObjToDouble();
                    if (downpayment > 0D)
                    {
                        if (downPayment1 > downpayment)
                            downpayment = downPayment1;
                    }

                    principal = payment + credit - debit - interest + downpayment;

                    if (contractNumber == "L17035UI" )
                    {
                        Trust85.FindContract(dt, "B16003UI");
                    }
                    principal = G1.RoundDown(principal);
                    if (payment == 0D)
                    {

                    }

                    if (!insurance)
                    {
                        if (calculateTrust100 && debit == 0D && credit == 0D)
                        {
                            retained = dt.Rows[i]["retained"].ObjToDouble();
                            method = ImportDailyDeposits.CalcTrust85P(payDate8, amtOfMonthlyPayt, oldIssueDate.ToString("MM/dd/yyyy"), contractValue, originalDownPayment, financeMonths, amtPaid, principal, debit, credit, rate, ref trust85, ref trust100, ref retained);
                            if (principal <= 0D)
                            {
                                interest = payment;
                                dt.Rows[i]["interestPaid"] = interest;
                                dt.Rows[i]["interestPaid1"] = interest;
                                trust85 = 0D;
                                trust100 = 0D;
                                if (contractNumber.ToUpper().EndsWith("LI"))
                                    retained = interest;
                            }
                            if (saveRetained != 0D)
                                retained = saveRetained;
                            dt.Rows[i]["trust85P"] = trust85;
                            dt.Rows[i]["trust100P"] = trust100;
                            dt.Rows[i]["retained"] = retained;
                            if ( DailyHistory.isRiles ( contractNumber ))
                            {
                                if ( payDate8 >= DailyHistory.rilesDate )
                                {
                                    trustPercent = dt.Rows[i]["trustPercent"].ObjToDouble();
                                    dt.Rows[i]["trust100P"] = amtPaid;
                                    dt.Rows[i]["trust85P"] = amtPaid * trustPercent / 100D;
                                    dt.Rows[i]["retained"] = 0D;
                                }
                            }
                        }
                        else
                        {
                            if (saveRetained != 0D)
                                dt.Rows[i]["retained"] = saveRetained;
                        }
                        if (contractNumber.ToUpper().EndsWith("LI"))
                            dt.Rows[i]["retained"] = 0D;
                    }

                    if ( debit > 0D)
                    {
                        //if (payDate8 < DailyHistory.majorDate)
                        //{
                        //    trust85 = dt.Rows[i]["trust85P"].ObjToDouble();
                        //    trust100 = dt.Rows[i]["trust100P"].ObjToDouble();
                        //    trust85 = trust85 * -1D;
                        //    trust100 = trust100 * -1D;
                        //    retained = interest;
                        //    dt.Rows[i]["trust85P"] = trust85;
                        //    dt.Rows[i]["trust100P"] = trust100;
                        //    dt.Rows[i]["retained"] = retained;
                        //}
                    }

                    if (foundLI)
                    {
                        dt.Rows[i]["retained"] = newInterest;
                        dt.Rows[i]["interestPaid1"] = newInterest;
                        dt.Rows[i]["trust100P"] = newTrust100;
                        dt.Rows[i]["trust85P"] = newTrust85;
                    }
                    if (contractNumber.ToUpper().EndsWith("LI"))
                        dt.Rows[i]["retained"] = 0D;

                    //**********************************************************************************

                    //address2 = dt.Rows[i]["address2"].ObjToString();
                    //if (!String.IsNullOrWhiteSpace(address2))
                    //{
                    //    address = dt.Rows[i]["address1"].ObjToString();
                    //    address += " " + address2;
                    //    dt.Rows[i]["address1"] = address;
                    //}
                    zip2 = dt.Rows[i]["zip2"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(zip2))
                    {
                        if (zip2 != "0")
                        {
                            zip = dt.Rows[i]["zip1"].ObjToString();
                            zip += "-" + zip2;
                            dt.Rows[i]["zip1"] = zip;
                        }
                    }
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                    if ( contractNumber == "ZZ0003043")
                    {
                    }
                    miniContract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                    dt.Rows[i]["loc"] = loc;
                    if (insurance)
                    {
                        miniContract = dt.Rows[i]["location"].ObjToString();
                        dt.Rows[i]["Location Name"] = dt.Rows[i]["location"].ObjToString();
                        if ( chkUseSDI.Checked )
                        {
                            agentCode = dt.Rows[i]["agentCode"].ObjToString();
                            oldLoc = dt.Rows[i]["oldLoc"].ObjToString();
                            SDICode = InsuranceCoupons.getSDICode(agentCode, oldLoc);
                            if (String.IsNullOrWhiteSpace(SDICode))
                                SDICode = "XX";
                            dt.Rows[i]["Location"] = SDICode;
                        }
                    }
                    else
                    {
                        if (funDt.Rows.Count > 0 && !String.IsNullOrWhiteSpace(loc))
                        {
                            if (loc == "FO")
                                loc = "B";
                            else if (loc == "WC")
                                loc = "WF";
                            DataRow[] dr = funDt.Select("keycode='" + loc + "'");
                            if (dr.Length > 0)
                                dt.Rows[i]["Location Name"] = dr[0]["name"].ObjToString();
                        }
                    }
                    trust = trust.ToUpper();
                    dt.Rows[i]["trust"] = trust;
                    trustName = "";
                    if (trust == "L" || trust == "LI")
                        trustName = "FDLIC";
                    else if (trust == "U" || trust == "UI")
                        trustName = "UNITY";
                    else if (trust == "D" || trust == "DI")
                        trustName = "BANCORPSOUTH";
                    else
                    {
                        if (edited.ToUpper() == "CEMETERY")
                        {
                            if (cemDt == null)
                                cemDt = G1.get_db_data ( "Select * from `cemeteries`;" );

                            loct = dt.Rows[i]["location"].ObjToString(); // RAMMA ZAMMA
                            if (!String.IsNullOrWhiteSpace(loct))
                            {
                                loct += " ";
                                ddRx = cemDt.Select("loc='" + loct + "'");
                                if (ddRx.Length > 0)
                                    loct += ddRx[0]["description"].ObjToString().Trim() + " Cemetery";
                                else
                                    loct += "Cemetery";
                            }
                            else
                                loct += "Cemetery";
                            dt.Rows[i]["Location Name"] = loct;
                            trustName = "PC";
                        }
                        else
                            trustName = "PB";
                    }
                    dt.Rows[i]["Trust Name"] = trustName;
                    if ( insurance )
                    {
                        dt.Rows[i]["trust85P"] = 0D;
                        dt.Rows[i]["trust100P"] = 0D;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Loading Contract " + contractNumber + " Data! " + ex.Message.ToString());
                }
            }
            //lblPayment.Text = "Monthly Pmt Totals $" + G1.ReformatMoney(totalPayments);
            //lblBalance.Text = "Monthly Bal Totals $" + G1.ReformatMoney(totalBalance);

            //lblPayment.Show();
            //lblBalance.Show();

            //Trust85.FindContract(dt, "M20077L");

            if (workReport.ToUpper() != "REMOVAL REPORT" && workReport.ToUpper() != "DECEASED REPORT")
            {
                if (!chkShowAll.Checked && !insurance)
                {
                    if (chkPayments.Checked && !chkPaidUp.Checked )
                        RemoveSinglePremiumContracts(dt);
                }
            }
            //Trust85.FindContract(dt, "M23001LI");
            LoadTrusts(dt);

            //Trust85.FindContract(dt, "M23001LI");

            if (chkDownPayments.Checked)
                SortForDownPayments(dt);

            Trust85.FindContract(dt, "zz0003043");

            G1.NumberDataTable(dt);
            dt.AcceptChanges();

            //Trust85.FindContract(dt, "M23001LI");
            ProcessDBR(dt);
            //Trust85.FindContract(dt, "M23001LI");

            if (workReport.ToUpper() == "DECEASED REPORT")
                dgv10.DataSource = dt;
            if (batch)
            {
                paymentsReportDt = dt;
                this.Close();
                return;
            }
            if (workReport == "Paid Up Contracts Report")
            {
                if (G1.get_column_number(dt, "flag") < 0)
                    dt.Columns.Add("flag");
            }
            if (workReport.ToUpper() == "DBR REPORT")
            {
                gridMain.Columns["select"].Visible = false;
                //for ( int i=0; i<dt.Rows.Count; i++)
                //{
                //    if (dt.Rows[i]["SetAsDBR"].ObjToString().ToUpper() == "Y")
                //        dt.Rows[i]["select"] = "1";
                //    else
                //        dt.Rows[i]["select"] = "0";
                //}
            }

            runOn = cmbRunOn.Text.Trim().ToUpper();
            bool doNewBusiness = false;
            if (workReport.ToUpper() == "TRUST EOM REPORT")
                doNewBusiness = true;
            if (workReport.ToUpper() == "CASH REMITTED REPORT")
                doNewBusiness = true;
            if (runOn.ToUpper() == "CEMETERIES" || runOn.ToUpper() == "RILES")
                doNewBusiness = false;

            if (doNewBusiness && !batch)
            {
                Trust85.FindContract(dt, "L24053LI");
                paymentsReportDt = null;
                this.Cursor = Cursors.WaitCursor;
                double trust100P = 0D;
                double trust85P = 0D;
                string str = "";
                DataRow[] xRows = null;
                using (PaymentsReport pForm = new PaymentsReport("NEW BUSINESS REPORT", saveDate1, saveDate2, saveDate1, saveDate2 ))
                {
                    if (paymentsReportDt != null)
                    {
                        paymentsReportDt = CleanupFutureReporting(paymentsReportDt, saveDate1.ToString("yyyy-MM-dd"), saveDate2.ToString("yyyy-MM-dd") );
                        double contractDownPayment = 0D;
                        Trust85.FindContract(paymentsReportDt, "B23029L");
                        for (int i = 0; i < paymentsReportDt.Rows.Count; i++)
                        {
                            contractNumber = paymentsReportDt.Rows[i]["contractNumber"].ObjToString().Trim();
                            payDate8 = paymentsReportDt.Rows[i]["payDate8"].ObjToDateTime();
                            str = payDate8.ToString("MM/dd/yyyy");
                            if (contractNumber == "CT19050LI")
                            {
                                Trust85.FindContract(paymentsReportDt, "CT19050LI");
                                Trust85.FindContract(dt, "CT19050LI");
                            }
                            try
                            {
                                downPayment = paymentsReportDt.Rows[i]["downPayment1"].ObjToDouble();
                                if (downPayment > 0D)
                                    xRows = dt.Select("contractNumber='" + contractNumber + "' and downPayment1 = '" + downPayment.ToString() + "'");
                                else
                                {
                                    downPayment = paymentsReportDt.Rows[i]["downPayment"].ObjToDouble();
                                    xRows = dt.Select("contractNumber='" + contractNumber + "' and downPayment = '" + downPayment.ToString() + "'");
                                }
                                if (xRows.Length <= 0)
                                {
                                    downpayment = paymentsReportDt.Rows[i]["downPayment"].ObjToDouble();
                                    //if ( downPayment == 0D)
                                    paymentsReportDt.Rows[i]["downPayment"] = 0D;
                                    paymentsReportDt.Rows[i]["newBusiness"] = downpayment;
                                    paymentsReportDt.Rows[i]["paymentAmount"] = 0D;
                                    trust100P = paymentsReportDt.Rows[i]["trust100P"].ObjToDouble();
                                    if (trust100P == 0D)
                                        paymentsReportDt.Rows[i]["trust100P"] = downpayment;
                                    trust85P = paymentsReportDt.Rows[i]["trust85P"].ObjToDouble();
                                    if (trust85P == 0D)
                                        paymentsReportDt.Rows[i]["trust85P"] = downpayment * 0.85D;
                                    dt.ImportRow(paymentsReportDt.Rows[i]);
                                }
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        //Trust85.FindContract(dt, "B17054UI");
                        CleanupPayments(dt);
                        //Trust85.FindContract(dt, "B17054UI");
                    }
                }
                Trust85.FindContract(dt, "E23041LI");
                dt = CleanupFutureReporting(dt, saveDate1.ToString("yyyy-MM-dd"), saveDate2.ToString("yyyy-MM-dd"), "downPayment1" );
                Trust85.FindContract(dt, "E23041LI");


                double TotalPayments = 0D;
                double TotalDebits = 0D;
                double TotalCredits = 0D;
                double TotalInterest = 0D;
                double TotalRetained = 0D;

                dt = Trust85.FilterForCemetery(dt, runOn);

                LoadTrustEOM(dt, ref TotalPayments, ref TotalDebits, ref TotalCredits, ref TotalInterest, ref TotalRetained);
                LoadDraftsEOM(dt, TotalPayments, TotalDebits, TotalCredits, TotalInterest, TotalRetained);
                LoadLockBoxEOM(dt);
                LoadManualEOM(dt);
                CombineEOM();
                //Trust85.FindContract(dt, "B17054UI");
                //                this.Cursor = Cursors.Default;
            }
            else if (workTitle == "Trust Monthly Payment 85% Master Listing (6.2)")
            {
                CleanupPayments(dt);
            }


            if ( gridMain.Columns["balanceDue"].Visible && workTitle != "Balances Less Than Payments Report" && workTitle != "BALANCES LESS THAN X REPORT")
                LoadCurrentBalances(dt);
            //Trust85.FindContract(dt,"HT16090UI");
            if (workReport == "Paid Up Contracts Report")
                ScrubPaidUpContracts(dt);
            Trust85.FindContract(dt, "L24053LI");
            DailyHistory.RemoveTrustAdjustments(dt, this.dateTimePicker1.Value, this.dateTimePicker2.Value);
            //Trust85.FindContract(dt, "B17054UI");
            if (workReport.ToUpper() == "CASH REMITTED REPORT" && !batch)
            {
                FixSpecialLocations(dt);
                //Trust85.FindContract(dt, "nnm20001");

                ProcessDBR(dt);

                //Trust85.FindContract(dt, "C19025LI");

                DataRow[] ddR = dt.Select("loc='FO'");
                DailyHistory.RecalcRetained(dt, "interestPaid1");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Application.DoEvents();

                    foundLI = false;
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                    if (contractNumber.ToUpper().EndsWith("LI"))
                    {
                        if (contractNumber == "L24053LI")
                        {
                        }

                        findRecord = dt.Rows[i]["record2"].ObjToString();
                        DailyHistory.CalcPaymentData(contractNumber, findRecord, ref newInterest, ref newTrust85, ref newTrust100);
                        foundLI = true;
                        dt.Rows[i]["retained"] = newInterest;
                        dt.Rows[i]["interestPaid1"] = newInterest;
                        dt.Rows[i]["trust100P"] = newTrust100;
                        dt.Rows[i]["trust85P"] = newTrust85;
                        dt.Rows[i]["retained"] = 0D;
                    }
                }
                dt.Columns.Add("dpp", Type.GetType("System.Double"));
                LoadCashRemitted(dt);
                //Trust85.FindContract(dt, "C19025LI");

            }
            if (workReport.ToUpper() == "REMOVAL REPORT" && !batch)
            {
                LoadRemovals(dt);
            }
            //Trust85.FindContract(dt, "L17035UI");

            if (workReport.ToUpper() == "INSURANCE WEEK TOTALS")
            {
                string transactionCode = "";
                int numMonths = 0;
                double monthlyPayment = 0D;
                payment = 0D;
                debit = 0D;
                credit = 0D;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    numMonths = 0;
                    monthlyPayment = dt.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                    if ( contractNumber == "ZZ0004047")
                    {
                    }
                    if (monthlyPayment > 500D)
                    {
                        monthlyPayment = Policies.CalcMonthlyPremium(contractNumber, "", monthlyPayment);
                    }
                    payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    if (payment != 0D)
                    {
                        transactionCode = "01";
                        numMonths = (int)(payment / monthlyPayment);
                        if (numMonths < 0)
                            numMonths = 0;
                    }
                    else if (debit != 0D)
                        transactionCode = "98";
                    else if (credit != 0D)
                        transactionCode = "99";
                    dt.Rows[i]["transactionCode"] = transactionCode;
                    dt.Rows[i]["numMonths"] = (double)(numMonths);
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    manual = false;
                    edited = dt.Rows[i]["edited"].ObjToString();
                    if (edited.Trim().ToUpper() == "MANUAL" || edited.Trim().ToUpper() == "TRUSTADJ" || edited.Trim().ToUpper() == "CEMETERY"  )
                        manual = true;
                    if (debit != 0D || credit != 0D)
                        manual = true;
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(depositNumber) && !manual)
                    {
                        depositLocation = depositNumber.Substring(0, 1).ToUpper();
                        //if (depositLocation == "T")
                        if ( isLockBox ( depositNumber))
                        {
                            fill1 = dt.Rows[i]["fill1"].ObjToString();
                            if (fill1.ToUpper() == "TFBX")
                                dt.Rows[i]["Location Name"] = "TF";
                            else
                                dt.Rows[i]["Location Name"] = "LK";
                        }
                        else if (depositLocation == "A")
                            dt.Rows[i]["Location Name"] = "ACH";
                        else if (depositLocation == "C")
                            dt.Rows[i]["Location Name"] = "CC";
                        //if (chkCombineHO.Checked)
                        //{
                        //    loc = dt.Rows[i]["location"].ObjToString();
                        //    if (loc.Trim().ToUpper() == "HOCC")
                        //        dt.Rows[i]["location"] = "HO";
                        //}
                    }
                    else if ( manual)
                    {
                        depositLocation = depositNumber.Substring(0, 1).ToUpper();
                        //if (depositLocation == "T")
                        if ( isLockBox ( depositNumber))
                        {
                            fill1 = dt.Rows[i]["fill1"].ObjToString();
                            if (fill1.ToUpper() == "TFBX")
                                dt.Rows[i]["Location Name"] = "TF";
                            else
                                dt.Rows[i]["Location Name"] = "LK";
                        }
                    }
                }
            }
//            workTitle == "Cash Remitted Report (8.0)")
            if (workTitle == "Trust Monthly Payment 85% Master Listing (6.2)" )
            {
                for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                {
                    downPayment = dt.Rows[i]["newBusiness"].ObjToDouble();
                    if (downPayment > 0D)
                    {
                        dt.Rows.RemoveAt(i);
                    }
                }
            }
            gridMain.Columns["downPayment1"].Visible = false;
            gridMain.Columns["downPayment"].Visible = false;
            if ( workReport.ToUpper() == "DBR REPORT")
                gridMain.Columns["downPayment1"].Visible = true;
            else if (workTitle == "New Business Report (1.2)")
                gridMain.Columns["downPayment"].Visible = true;

            //if (chkDownPayments.Checked)
            //    gridMain.Columns["downPayment"].Visible = true;
            Trust85.FindContract(dt, "C19025LI");

            //dt = SMFS.FilterForRiles(dt);
            FixSpecialLocations(dt);
            SetupSelectColumn(dt);

            if (workReport == "Paid Up Contracts Report")
                dt = GetGroupByDate(dt);

            if ( workReport.ToUpper() == "CASH REMITTED REPORT")
            {
                for ( int i=0; i<dt.Rows.Count; i++)
                {
                    downPayment = dt.Rows[i]["newBusiness"].ObjToDouble();
                    if ( downPayment != 0D)
                        dt.Rows[i]["newBusiness"] = dt.Rows[i]["trust100P"];
                }
            }

            DailyHistory.RecalcRetained(dt, "interestPaid1");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    foundLI = false;
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                    if (contractNumber.ToUpper().EndsWith("LI"))
                    {
                        if (contractNumber == "FF20052LI")
                        {
                        }

                        findRecord = dt.Rows[i]["record2"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(findRecord))
                        {
                            DailyHistory.CalcPaymentData(contractNumber, findRecord, ref newInterest, ref newTrust85, ref newTrust100);
                            foundLI = true;
                            dt.Rows[i]["retained"] = newInterest;
                            dt.Rows[i]["interestPaid1"] = newInterest;
                            dt.Rows[i]["trust100P"] = newTrust100;
                            dt.Rows[i]["trust85P"] = newTrust85;
                            dt.Rows[i]["retained"] = 0D;
                        }
                    }
                }
                catch ( Exception ex )
                {
                }
            }

            LoadLiInterest(dt);

            if (chkPaidUp.Checked)
                LoadPaidOffLetters(dt);

            runOn = cmbRunOn.Text.Trim().ToUpper();
            dt = Trust85.FilterForCemetery(dt, runOn);

            if (workTitle.IndexOf("1.2") > 0 && runOn.ToUpper() == "CEMETERIES" )
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["trust100P"] = dt.Rows[i]["paymentAmount"].ObjToDouble();
            }

            if (workTitle == "New Business Report (1.2)")
                dt = CleanupFutureReporting(dt, date1, date2, "downPayment1");


            originalDt = dt;
            dgv.DataSource = dt;
            cashRemittedDt = null;
            if (workReport.ToUpper() == "CASH REMITTED REPORT" && !batch)
            {
                cashRemittedDt = dt;
                if (!addedMismatchTab)
                {
                    tabControl1.TabPages.Add(tabMismatched);
                    addedMismatchTab = true;
                }
            }

            //G1.StopWait(ref waitForm);
            //if (waitForm != null)
            //    waitForm.Close();
            //if ( waitForm != null )
            //{
            //    waitForm.Dispose();
            //    waitForm = null;
            //}

            if (workTitle.IndexOf("1.2") > 0 && runOn.ToUpper() == "CEMETERIES")
                gridMain.ExpandAllGroups();

            if ( allowSave )
            {
                btnSave.SetBounds(btnRun.Right + 15, btnRun.Top, btnSave.Width, btnSave.Height);
                btnSave.Show();
                btnSave.Refresh();
            }
            this.Cursor = Cursors.Default;
        }
        /*******************************************************************************************/
        private void LoadLiInterest(DataTable dt)
        {
            if (G1.get_column_number(dt, "LiInterest") < 0)
                dt.Columns.Add("LiInterest", Type.GetType("System.Double"));

            double interest = 0D;
            string contractNumber = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                if ( contractNumber == "CT19049LI")
                {
                }
                if (contractNumber.ToUpper().EndsWith("LI"))
                {
                    interest = dt.Rows[i]["interestPaid1"].ObjToDouble();
                    dt.Rows[i]["LiInterest"] = interest;
                }
                else
                    dt.Rows[i]["LiInterest"] = 0D;
            }
        }
        /*******************************************************************************************/
        private void LoadPaidOffLetters(DataTable dt)
        {
            if (G1.get_column_number(dt, "letter") < 0)
                dt.Columns.Add("letter");

            string contractNumber = "";
            string cmd = "";
            DataTable dx = null;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                cmd = "Select * from `lapse_list` where `contractNumber` = '" + contractNumber + "' AND `detail` = 'Paid Off Notice';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                    dt.Rows[i]["letter"] = "1";
                //else
                //    dt.Rows[i]["letter"] = "0";
            }
        }
        /***********************************************************************************************/
        public static bool isLockBox ( string depositNumber )
        {
            bool lockBox = false;
            string depositLocation = depositNumber.Substring(0, 1).ToUpper();
            if (depositLocation == "T")
            {
                string str = depositNumber.Substring(1);
                DateTime date = str.ObjToDateTime();
                if (date.Year > 2015)
                    lockBox = true;
            }
            return lockBox;
        }
        /***********************************************************************************************/
        public static DataTable GetGroupByDate(DataTable dt)
        {
            if (G1.get_column_number(dt, "Int32_date") < 0)
                dt.Columns.Add("Int32_date", typeof(int), "payDate8");

            DataView tempview = dt.DefaultView;
            //            tempview.Sort = "loc asc, agentName asc";
            tempview.Sort = "contractNumber asc, Int32_date desc";
            dt = tempview.ToTable();


            DataTable groupDt = dt.Clone();
            try
            {
                groupDt = dt.AsEnumerable().GroupBy(r => new { Col1 = r["contractNumber"] }).Select(g => g.OrderBy(r => r["Int32_date"]).First()).CopyToDataTable();
            }
            catch ( Exception ex )
            {
            }

            groupDt.Columns.Remove("Int32_date");
            return groupDt;
        }
        /****************************************************************************************/
        private void FixSpecialLocations(DataTable dt)
        {
            string loc = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                loc = dt.Rows[i]["loc"].ObjToString();
                if (loc == "FO")
                    dt.Rows[i]["loc"] = "B";
                else if (loc == "WC")
                    dt.Rows[i]["loc"] = "WF";
            }
        }
        /****************************************************************************************/
        private void GetPaymentData ( DataTable dt, ref double payment, ref double interest )
        {
            payment = 0D;
            double debit = 0D;
            double credit = 0D;
            interest = 0D;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                //payment += dt.Rows[i]["paymentAmount"].ObjToDouble();
                payment += DailyHistory.getPayment(dt, i);
                credit += dt.Rows[i]["creditAdjustment"].ObjToDouble();
                debit += dt.Rows[i]["debitAdjustment"].ObjToDouble();
                interest += dt.Rows[i]["interestPaid"].ObjToDouble();
            }
            payment = payment + credit - debit;
        }
        /****************************************************************************************/
        private void ProcessDBR(DataTable dt)
        {
            double trust85 = 0D;
            DateTime deceasedDate = DateTime.Now;
            bool setDBR = false;
            double debit = 0D;
            double credit = 0D;
            string contractNumber = "";
            if (G1.get_column_number(dt, "SetAsDBR") < 0)
                dt.Columns.Add("SetAsDBR");
            if (G1.get_column_number(dt, "dbr") < 0)
                dt.Columns.Add("dbr", Type.GetType("System.Double"));

            DateTime payDate8 = DateTime.Now;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                if ( contractNumber == "CT23030LI")
                {
                }
                if (contractNumber == "CT24044LI")
                {
                }
                setDBR = false;
                if (dt.Rows[i]["SetAsDBR"].ObjToString() == "Y")
                    setDBR = true;
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 1850)
                {
                    if ( expandedRun )
                    {
                        payDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();
                        if (deceasedDate.Year != payDate8.Year || deceasedDate.Month != payDate8.Month)
                            continue;
                    }
                    double dbr = isDBR(contractNumber, this.dateTimePicker2.Value );
                    if (dbr > 0)
                        setDBR = true;
                    //if (dbr <= 0D)
                    //{
                    //    debit = dt.Rows[i]["debitAdjustment"].ObjToDouble(); // RAMMA ZAMMA
                    //    credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    //    if (debit == 0D && credit == 0D)
                    //        setDBR = true;
                    //    else if (debit > 0D) // Ramma Zamma
                    //        setDBR = true;
                    //    else if (credit > 0D) // Ramma Zamma
                    //        setDBR = true;
                    //}
                    //else
                    //    setDBR = true;
                }
                if ( setDBR )
                {
                    dt.Rows[i]["SetAsDBR"] = "Y";
                    trust85 = dt.Rows[i]["trust85P"].ObjToDouble();
                    dt.Rows[i]["dbr"] = trust85;
                    //dt.Rows[i]["trust85P"] = 0D;
                    //dt.Rows[i]["trust100P"] = 0D;

                }
            }
        }
        /****************************************************************************************/
        private void LoadCurrentBalances(DataTable dt)
        {
            string contractNumber = "";
            double balanceDue = 0D;
            DateTime date = DateTime.Now;
            string depositNumber = "";
            string depositLocation = "";
            string edited = "";
            string fill1 = "";
            if (G1.get_column_number(dt, "where") < 0)
                dt.Columns.Add("where");

            int lastRow = dt.Rows.Count;
            //lastRow = 5; // For Testing
        
            for ( int i=0; i<lastRow; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                if (contractNumber == "L19029LI")
                {
                }
                date = dt.Rows[i]["payDate8"].ObjToDateTime();
                Trust85.CalcTrustBalance(contractNumber, date, ref balanceDue);
                dt.Rows[i]["balanceDue"] = balanceDue;
                edited = dt.Rows[i]["edited"].ObjToString();
                if ( edited.Trim().ToUpper() == "MANUAL")
                {
                    dt.Rows[i]["where"] = "MANUAL";
                    continue;
                }
                depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                if (!String.IsNullOrWhiteSpace(depositNumber))
                {
                    depositLocation = depositNumber.Substring(0, 1).ToUpper();
                    if (depositLocation == "T")
                    {
                        dt.Rows[i]["where"] = "LK";
                        fill1 = dt.Rows[i]["fill1"].ObjToString().ToUpper();
                        if (fill1 == "TFBX")
                            dt.Rows[i]["where"] = "TF";
                    }
                    else if (depositLocation == "A")
                        dt.Rows[i]["where"] = "ACH";
                    else if (depositLocation == "C")
                        dt.Rows[i]["where"] = "CC";
                }
                else
                {
                    date = dt.Rows[i]["dueDate8"].ObjToDateTime();
                    //if ( date.Year >= 2039)
                    //    dt.Rows[i]["where"] = "SINGLE PREM";
                }
            }
        }
        /****************************************************************************************/
        private void ScrubPaidUpContracts(DataTable dt)
        {
            Trust85.FindContract(dt, "HT16038UI");
            double balanceDue = 0D;
            DateTime dueDate = DateTime.Now;
            double monthlyPayment = 0D;
            string contractNumber = "";
            for ( int i=dt.Rows.Count-1; i>=0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                if ( contractNumber == "L19029LI")
                {
                }
                dueDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                if (dueDate.Year >= 2039)
                    continue;
                if (dueDate.Year == 2039)
                    continue;
                monthlyPayment = dt.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                balanceDue = dt.Rows[i]["balanceDue"].ObjToDouble();
                if (balanceDue == 0D)
                    continue;
                if (balanceDue < monthlyPayment)
                    continue;
                dt.Rows.RemoveAt(i);
            }
            LabelSinglePremiumContracts(dt);
            //Trust85.FindContract(dt, "HT16038UI");

            DataTable dx = null;
            string fill = "";
            double interest = 0D;
            double credit = 0D;
            double dMonths = 0D;
            decimal totalMonths = 0;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                monthlyPayment = dt.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                dx = FindMismatches.LoadMainData2(contractNumber, "", monthlyPayment);
                if (dx.Rows.Count <= 0)
                    continue;
                totalMonths = 0;
                dMonths = 0D;
                for ( int j=0; j<dx.Rows.Count; j++)
                {
                        fill = dx.Rows[j]["fill"].ObjToString().ToUpper();
                        if (fill == "D")
                            continue;
                        interest = dx.Rows[j]["interestPaid"].ObjToDouble();
                        credit = dx.Rows[j]["creditAdjustment"].ObjToDouble();
                        //if (credit > 0D && interest == 0D) // Fix for Credit/Interest issue (DOLP) // Removed for M23002LI on 10/15/2024
                        //    continue;
                        totalMonths += dx.Rows[j]["NumPayments"].ObjToDecimal();
                        dMonths += dx.Rows[j]["NumPayments"].ObjToDouble();
                        //totalMonths = (decimal) G1.RoundValue((double) totalMonths);
                }
                dt.Rows[i]["numMonths"] = dMonths;
            }
            gridMain.Columns["numMonths"].DisplayFormat.FormatString = "N2";
            int rows = dt.Rows.Count;
            G1.NumberDataTable(dt);
        }
        /****************************************************************************************/
        private void PrepareNewBusiness ( DataTable dt )
        {
            double newBusiness = 0D;
            string contractNumber = "";
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                newBusiness = dt.Rows[i]["newBusiness"].ObjToDouble();
                if (newBusiness == 0D)
                    dt.Rows.RemoveAt(i);
            }
        }
        /****************************************************************************************/
        private void PrepareDownPayments(DataTable dt)
        {
            double newBusiness = 0D;
            string contractNumber = "";
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                newBusiness = dt.Rows[i]["newBusiness"].ObjToDouble();
                if (newBusiness > 0D)
                {
                    dt.Rows[i]["downPayment"] = newBusiness;
                }
            }
        }
        /****************************************************************************************/
        private void CleanupPayments ( DataTable dt )
        {
            double downpayment = 0D;
            double payment = 0D;
            double debit = 0D;
            double credit = 0D;
            double newBusiness = 0D;
            double trust85P = 0D;
            string contractNumber = "";
            string edited = "";
            DateTime date = DateTime.Now;
            Trust85.FindContract(dt, "L17035UI");
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                edited = dt.Rows[i]["edited"].ObjToString();
                if (edited.ToUpper() == "TRUSTADJ" || edited.ToUpper() == "CEMETERY" )
                    continue;
                date = dt.Rows[i]["payDate8"].ObjToDateTime();
                if ( date.Year < 100)
                {
                    dt.Rows.RemoveAt(i);
                    continue;
                }
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                if ( contractNumber == "E19040LI")
                {
                    Trust85.FindContract(dt, "E19040LI");
                }
                newBusiness = dt.Rows[i]["newBusiness"].ObjToDouble();
                downpayment = dt.Rows[i]["downPayment1"].ObjToDouble();
                if ( downpayment > 0D && newBusiness == 0D)
                {
                    newBusiness = downpayment;
                    dt.Rows[i]["newBusiness"] = newBusiness;
                    dt.Rows[i]["downPayment1"] = 0D;
                    dt.Rows[i]["paymentAmount"] = 0D;
                }
                payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                trust85P = dt.Rows[i]["trust85P"].ObjToDouble();
                if (contractNumber == "CT18052LI")
                {
//                    Trust85.FindContract(dt, "CT18052LI");
                }
                Trust85.FindContract(dt, "E19040LI");
                if (newBusiness == 0D && payment == 0D && debit == 0D && credit == 0D && trust85P == 0D)
                    dt.Rows.RemoveAt(i);
                else
                {
                    if ( previousDateRead )
                    {
                        if (newBusiness > 0D)
                            dt.Rows.RemoveAt(i);
                    }
                }
            }
            Trust85.FindContract(dt, "E19040LI");
        }
        /****************************************************************************************/
        private void LoadTrustEOM(DataTable dx, ref double totalPayments, ref double debit, ref double credit, ref double interest, ref double retained)
        {
            DataTable dt = dx.Copy();
            DataView tempview = dt.DefaultView;
            tempview.Sort = "Location Name";
            dt = tempview.ToTable();

            DataTable locationDt = new DataTable();
            locationDt.Columns.Add("num");
            locationDt.Columns.Add("location");
            locationDt.Columns.Add("notes");
            locationDt.Columns.Add("nbdp", Type.GetType("System.Double"));
            locationDt.Columns.Add("debitAdjustment", Type.GetType("System.Double"));
            locationDt.Columns.Add("creditAdjustment", Type.GetType("System.Double"));
            locationDt.Columns.Add("adjustedPayments", Type.GetType("System.Double"));
            locationDt.Columns.Add("group");

            string saveLocation = "";
            string location = "";
            double newBusiness = 0D;
            double totalNewBusiness = 0D;
            double locationNewBusiness = 0D;

            string contractNumber = "";

            totalPayments = 0D;
            debit = 0D;
            credit = 0D;
            interest = 0D;
            retained = 0D;

            try
            {

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                    if ( contractNumber == "NNM21004")
                    {
                    }
                    location = dt.Rows[i]["Location Name"].ObjToString();
                    if (String.IsNullOrWhiteSpace(location))
                        location = "No Location";
                    if (String.IsNullOrWhiteSpace(saveLocation))
                        saveLocation = location;
                    if (saveLocation != location)
                    {
                        DataRow dRow = locationDt.NewRow();
                        dRow["location"] = saveLocation.ToUpper();
                        dRow["nbdp"] = locationNewBusiness;
                        dRow["group"] = "1";
                        if (saveLocation.ToUpper().IndexOf("CEMETERY") >= 0)
                            dRow["group"] = "2";
                        totalNewBusiness += locationNewBusiness;
                        locationDt.Rows.Add(dRow);
                        locationNewBusiness = 0D;
                        saveLocation = location;
                    }

                    if (dt.Rows[i]["SetAsDBR"].ObjToString() == "Y")
                        continue;

                    newBusiness = dt.Rows[i]["newBusiness"].ObjToDouble();
                    locationNewBusiness += newBusiness;
                    totalPayments += dt.Rows[i]["paymentAmount"].ObjToDouble();
                    interest += dt.Rows[i]["interestPaid1"].ObjToDouble();
                    credit += dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    debit += dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    retained += dt.Rows[i]["retained"].ObjToDouble();
                }

                DataRow dR = locationDt.NewRow();
                dR["location"] = saveLocation.ToUpper();
                dR["nbdp"] = locationNewBusiness;
                locationDt.Rows.Add(dR);
                totalNewBusiness += locationNewBusiness;

                dR = locationDt.NewRow();
                locationDt.Rows.Add(dR);

                dR = locationDt.NewRow();
                dR["location"] = "NB/DP TOTAL";
                dR["nbdp"] = totalNewBusiness;
                locationDt.Rows.Add(dR);

                double adjustedPayments = totalPayments - debit + credit;

                dR = locationDt.NewRow();
                dR["location"] = "PAYMENT TOTAL";
                dR["nbdp"] = totalPayments;
                dR["debitAdjustment"] = debit * -1D;
                dR["creditAdjustment"] = credit;
                dR["adjustedPayments"] = adjustedPayments;
                locationDt.Rows.Add(dR);

                dR = locationDt.NewRow();
                dR["location"] = "RETAINED INTEREST";
                dR["nbdp"] = interest * -1D;
                dR["adjustedPayments"] = interest * -1D;
                dR["nbdp"] = retained;
                dR["adjustedPayments"] = retained;
                locationDt.Rows.Add(dR);
                dR = locationDt.NewRow();

                double paymentAfterInterest = adjustedPayments - interest;

                dR = locationDt.NewRow();
                dR["location"] = "PAYMENT TOTALS AFTER RETAINED INT";
                dR["nbdp"] = paymentAfterInterest;
                dR["adjustedPayments"] = paymentAfterInterest;
                locationDt.Rows.Add(dR);

                dR = locationDt.NewRow();
                string category = cmbDateType.Text;
                dR["location"] = category + " TOTALS";
                dR["nbdp"] = totalNewBusiness + paymentAfterInterest;
                dR["adjustedPayments"] = totalNewBusiness + paymentAfterInterest;
                locationDt.Rows.Add(dR);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }
            G1.NumberDataTable(locationDt);
            dgv2.DataSource = locationDt;
        }
        /****************************************************************************************/
        private void LoadLockBoxEOM(DataTable dx)
        {
            DataTable dt = dx.Copy();
            DataView tempview = dt.DefaultView;
            tempview.Sort = "Location Name";
            dt = tempview.ToTable();

            DataTable locationDt = new DataTable();
            locationDt.Columns.Add("num");
            locationDt.Columns.Add("location");
            locationDt.Columns.Add("notes");
            locationDt.Columns.Add("nbdp", Type.GetType("System.Double"));
            locationDt.Columns.Add("debitAdjustment", Type.GetType("System.Double"));
            locationDt.Columns.Add("creditAdjustment", Type.GetType("System.Double"));
            locationDt.Columns.Add("adjustedPayments", Type.GetType("System.Double"));
            locationDt.Columns.Add("edit");

            string saveLocation = "";
            string location = "";
            string loc = "";
            double locationPayments = 0D;
            double totalPayments = 0D;
            double payment = 0D;
            double debit = 0D;
            double credit = 0D;
            double interest = 0D;
            double adjustedPayments = 0D;

            double totalLockBoxPayments = 0D;
            double totalLockBoxDebit = 0D;
            double totalLockBoxCredit = 0D;
            double totalLockBoxInterest = 0D;

            double totalHOPayments = 0D;
            double totalHODebit = 0D;
            double totalHOCredit = 0D;
            double totalHOInterest = 0D;

            string depositNumber = "";

            try
            {

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    location = dt.Rows[i]["Location Name"].ObjToString();
                    if (String.IsNullOrWhiteSpace(location))
                        location = "No Location";
                    if (String.IsNullOrWhiteSpace(saveLocation))
                        saveLocation = location;
                    if (saveLocation != location)
                    {
                        DataRow dRow = locationDt.NewRow();
                        dRow["location"] = saveLocation.ToUpper() + " LKBX";
                        dRow["nbdp"] = locationPayments;
                        dRow["debitAdjustment"] = debit;
                        dRow["creditAdjustMent"] = credit;
                        adjustedPayments = locationPayments - debit + credit;
                        dRow["adjustedPayments"] = adjustedPayments;
                        locationDt.Rows.Add(dRow);

                        totalLockBoxPayments += locationPayments;
                        totalLockBoxCredit += credit;
                        totalLockBoxDebit += debit;
                        totalLockBoxInterest += interest;

                        locationPayments = 0D;
                        debit = 0D;
                        credit = 0D;
                        interest = 0D;
                        saveLocation = location;
                    }

                    loc = dt.Rows[i]["location"].ObjToString();
                    if (loc.ToUpper().IndexOf("HO") >= 0)
                        continue;
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(depositNumber))
                    {
                        depositNumber = depositNumber.Substring(0, 1);
                        if (depositNumber.ToUpper() == "T")
                        {
                            payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                            locationPayments += payment;
                            totalPayments += dt.Rows[i]["paymentAmount"].ObjToDouble();
                        }
                    }
                    interest += dt.Rows[i]["interestPaid1"].ObjToDouble();
                    credit += dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    debit += dt.Rows[i]["debitAdjustment"].ObjToDouble();
                }

                DataRow dR = locationDt.NewRow();
                dR["location"] = saveLocation.ToUpper() + "LKBX";
                dR["nbdp"] = locationPayments;
                dR["debitAdjustment"] = debit;
                dR["creditAdjustMent"] = credit;
                adjustedPayments = locationPayments - debit + credit;
                dR["adjustedPayments"] = adjustedPayments;
                locationDt.Rows.Add(dR);

                totalLockBoxPayments += locationPayments;
                totalLockBoxCredit += credit;
                totalLockBoxDebit += debit;
                totalLockBoxInterest += interest;

                dR = locationDt.NewRow();
                dR["location"] = "LOCKBOX TOTAL";
                dR["nbdp"] = totalLockBoxPayments;
                dR["debitAdjustment"] = totalLockBoxDebit;
                dR["creditAdjustMent"] = totalLockBoxCredit;
                adjustedPayments = totalLockBoxPayments - totalLockBoxDebit + totalLockBoxCredit;
                dR["adjustedPayments"] = adjustedPayments;
                locationDt.Rows.Add(dR);

                StateRow = locationDt.Rows.Count;
                dR = locationDt.NewRow();
                dR["location"] = "STATE BANK REMOTE";
                locationDt.Rows.Add(dR);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    location = dt.Rows[i]["Location Name"].ObjToString();
                    if (String.IsNullOrWhiteSpace(location))
                        location = "No Location";
                    if (String.IsNullOrWhiteSpace(saveLocation))
                        saveLocation = location;
                    if (saveLocation != location)
                    {
                        DataRow dRow = locationDt.NewRow();
                        dRow["location"] = saveLocation.ToUpper();
                        dRow["edit"] = "Y";
                        locationDt.Rows.Add(dRow);
                        saveLocation = location;
                    }
                }

                dR = locationDt.NewRow();
                dR["location"] = saveLocation.ToUpper();
                dR["edit"] = "Y";
                locationDt.Rows.Add(dR);

                dR = locationDt.NewRow();
                dR["location"] = "TOTALS";
                locationDt.Rows.Add(dR);

/**************************************/
                CashRow = locationDt.Rows.Count;
                dR = locationDt.NewRow();
                dR["location"] = "CASH-LOCAL BANK";
                locationDt.Rows.Add(dR);

                locationPayments = 0D;
                debit = 0D;
                credit = 0D;
                interest = 0D;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    location = dt.Rows[i]["Location Name"].ObjToString();
                    if (String.IsNullOrWhiteSpace(location))
                        location = "No Location";
                    if (String.IsNullOrWhiteSpace(saveLocation))
                        saveLocation = location;
                    if (saveLocation != location)
                    {
                        DataRow dRow = locationDt.NewRow();
                        dRow["location"] = saveLocation.ToUpper();
                        dRow["edit"] = "Y";
                        dRow["nbdp"] = locationPayments;
                        dRow["debitAdjustment"] = debit;
                        dRow["creditAdjustMent"] = credit;
                        adjustedPayments = locationPayments - debit + credit;
                        dRow["adjustedPayments"] = adjustedPayments;
                        locationDt.Rows.Add(dRow);

                        totalHOPayments += locationPayments;
                        totalHOCredit += credit;
                        totalHODebit += debit;
                        totalHOInterest += interest;

                        locationPayments = 0D;
                        debit = 0D;
                        credit = 0D;
                        interest = 0D;
                        saveLocation = location;
                    }
                    loc = dt.Rows[i]["location"].ObjToString();
                    if (loc.ToUpper().IndexOf("HO") < 0)
                        continue;
                    payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    locationPayments += payment;
                    totalPayments += dt.Rows[i]["paymentAmount"].ObjToDouble();
                    interest += dt.Rows[i]["interestPaid1"].ObjToDouble();
                    credit += dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    debit += dt.Rows[i]["debitAdjustment"].ObjToDouble();
                }

                dR = locationDt.NewRow();
                dR["location"] = saveLocation.ToUpper();
                dR["edit"] = "Y";
                dR["nbdp"] = locationPayments;
                dR["debitAdjustment"] = debit;
                dR["creditAdjustMent"] = credit;
                adjustedPayments = locationPayments - debit + credit;
                dR["adjustedPayments"] = adjustedPayments;
                locationDt.Rows.Add(dR);

                dR = locationDt.NewRow();
                dR["location"] = "TOTALS";
                dR["nbdp"] = totalHOPayments;
                dR["debitAdjustment"] = totalHODebit;
                dR["creditAdjustMent"] = totalHOCredit;
                adjustedPayments = totalHOPayments - totalHODebit + totalHOCredit;
                dR["adjustedPayments"] = adjustedPayments;
                locationDt.Rows.Add(dR);

                MagRow = locationDt.Rows.Count;
                dR = locationDt.NewRow();
                dR["location"] = "TOTALS MAGNOLIA STATE BANK REMOTE AND CASH DEPOSITS";
                locationDt.Rows.Add(dR);

                dR = locationDt.NewRow();
                dR["location"] = "HOCC - ";
                dR["edit"] = "Y";
                locationDt.Rows.Add(dR);

                dR = locationDt.NewRow();
                dR["location"] = "HOBH - ";
                dR["edit"] = "Y";
                locationDt.Rows.Add(dR);

                dR = locationDt.NewRow();
                dR["location"] = "HOSM - ";
                dR["edit"] = "Y";
                locationDt.Rows.Add(dR);

                dR = locationDt.NewRow();
                dR["location"] = "HOJCC - ";
                dR["edit"] = "Y";
                locationDt.Rows.Add(dR);

                dR = locationDt.NewRow();
                dR["location"] = "HOBS - ";
                dR["edit"] = "Y";
                locationDt.Rows.Add(dR);

                dR = locationDt.NewRow();
                dR["location"] = "TOTALS";
                locationDt.Rows.Add(dR);
                TotalRow = locationDt.Rows.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }
            G1.NumberDataTable(locationDt);
            dgv4.DataSource = locationDt;
        }
        /****************************************************************************************/
        private void LoadDraftsEOM(DataTable dx, double TotalPayments, double TotalDebits, double TotalCredits, double TotalInterest, double TotalRetained)
        {
            DataTable dt = dx.Copy();
            DataView tempview = dt.DefaultView;
            tempview.Sort = "Location Name";
            dt = tempview.ToTable();

            DataTable locationDt = new DataTable();
            locationDt.Columns.Add("num");
            locationDt.Columns.Add("location");
            locationDt.Columns.Add("notes");
            locationDt.Columns.Add("nbdp", Type.GetType("System.Double"));
            locationDt.Columns.Add("debitAdjustment", Type.GetType("System.Double"));
            locationDt.Columns.Add("creditAdjustment", Type.GetType("System.Double"));
            locationDt.Columns.Add("adjustedPayments", Type.GetType("System.Double"));

            string saveLocation = "";
            string location = "";
            double locationPayments = 0D;
            double totalPayments = 0D;
            double payment = 0D;
            double debit = 0D;
            double credit = 0D;
            double interest = 0D;
            double adjustedPayments = 0D;

            double totalLockBoxPayments = 0D;
            double totalLockBoxDebit = 0D;
            double totalLockBoxCredit = 0D;
            double totalLockBoxInterest = 0D;

            string depositNumber = "";

            try
            {

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    location = dt.Rows[i]["Location Name"].ObjToString();
                    if (String.IsNullOrWhiteSpace(location))
                        location = "No Location";
                    if (String.IsNullOrWhiteSpace(saveLocation))
                        saveLocation = location;
                    if (saveLocation != location)
                    {
                        DataRow dRow = locationDt.NewRow();
                        dRow["location"] = saveLocation.ToUpper();
                        dRow["nbdp"] = locationPayments;
                        dRow["debitAdjustment"] = debit;
                        dRow["creditAdjustMent"] = credit;
                        adjustedPayments = locationPayments - debit + credit;
                        dRow["adjustedPayments"] = adjustedPayments;
                        locationDt.Rows.Add(dRow);

                        totalLockBoxPayments += locationPayments;
                        totalLockBoxCredit += credit;
                        totalLockBoxDebit += debit;
                        totalLockBoxInterest += interest;

                        locationPayments = 0D;
                        debit = 0D;
                        credit = 0D;
                        interest = 0D;
                        saveLocation = location;
                    }

                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(depositNumber))
                    {
                        depositNumber = depositNumber.Substring(0, 1);
                        if (depositNumber.ToUpper() == "A")
                        {
                            payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                            locationPayments += payment;
                            totalPayments += dt.Rows[i]["paymentAmount"].ObjToDouble();
                        }
                    }
                    interest += dt.Rows[i]["interestPaid1"].ObjToDouble();
                    credit += dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    debit += dt.Rows[i]["debitAdjustment"].ObjToDouble();
                }

                DataRow dR = locationDt.NewRow();
                dR["location"] = saveLocation.ToUpper();
                dR["nbdp"] = locationPayments;
                dR["debitAdjustment"] = debit;
                dR["creditAdjustMent"] = credit;
                adjustedPayments = locationPayments - debit + credit;
                dR["adjustedPayments"] = adjustedPayments;
                locationDt.Rows.Add(dR);

                totalLockBoxPayments += locationPayments;
                totalLockBoxCredit += credit;
                totalLockBoxDebit += debit;
                totalLockBoxInterest += interest;


                LoadDraftsHO(dx, locationDt, ref totalLockBoxPayments, ref totalLockBoxDebit, ref totalLockBoxCredit, ref totalLockBoxInterest );

                dR = locationDt.NewRow();
                dR["location"] = "TOTAL DRAFTS";
                dR["nbdp"] = totalLockBoxPayments;
                dR["debitAdjustment"] = totalLockBoxDebit;
                dR["creditAdjustMent"] = totalLockBoxCredit;
                adjustedPayments = totalLockBoxPayments - totalLockBoxDebit + totalLockBoxCredit;
                dR["adjustedPayments"] = adjustedPayments;
                locationDt.Rows.Add(dR);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }

            DataRow xx = locationDt.NewRow();
            locationDt.Rows.Add(xx);

            LoadCCEOM(dx, locationDt);

            xx = locationDt.NewRow();
            locationDt.Rows.Add(xx);

            adjustedPayments = TotalPayments - TotalDebits + TotalCredits;

            string category = cmbDateType.Text.ToUpper();
            DataRow ddR = locationDt.NewRow();
            ddR["location"] = category + " PAYMENT TOTALS";
            ddR["nbdp"] = TotalPayments;
            ddR["debitAdjustment"] = TotalDebits * -1D;
            ddR["creditAdjustment"] = TotalCredits;
            ddR["adjustedPayments"] = adjustedPayments;
            locationDt.Rows.Add(ddR);

            ddR = locationDt.NewRow();
            ddR["location"] = "RETAINED INTEREST";
            //            ddR["adjustedPayments"] = TotalInterest * -1D;
            ddR["nbdp"] = TotalRetained;
            ddR["adjustedPayments"] = TotalRetained;
            locationDt.Rows.Add(ddR);
            ddR = locationDt.NewRow();

            double paymentAfterInterest = adjustedPayments - TotalInterest;
            paymentAfterInterest = adjustedPayments - TotalRetained;

            ddR = locationDt.NewRow();
            ddR["location"] = category + " PAYMENT TOTAL";
            ddR["nbdp"] = paymentAfterInterest;
            ddR["adjustedPayments"] = paymentAfterInterest;
            locationDt.Rows.Add(ddR);

            G1.NumberDataTable(locationDt);
            dgv3.DataSource = locationDt;
        }
        /****************************************************************************************/
        private void LoadDraftsHO ( DataTable dx, DataTable locationDt, ref double totalBoxPayments, ref double totalBoxDebit, ref double totalBoxCredit, ref double totalBoxInterest )
        {
            DataRow[] dRows = dx.Select("location='ACH' AND edited='Manual'");

            DataTable dt = dx.Clone();
            G1.ConvertToTable(dRows, dt);

            DataView tempview = dt.DefaultView;
            tempview.Sort = "Location Name";
            dt = tempview.ToTable();

            DataRow dR = locationDt.NewRow();
            dR["location"] = "";
            locationDt.Rows.Add(dR);

            dR = locationDt.NewRow();
            dR["location"] = " DRAFTS KEYED AT HOMEOFFICE";
            locationDt.Rows.Add(dR);

            string saveLocation = "";
            string location = "";
            double locationPayments = 0D;
            double totalPayments = 0D;
            double payment = 0D;
            double debit = 0D;
            double credit = 0D;
            double interest = 0D;
            double adjustedPayments = 0D;

            double totalLockBoxPayments = 0D;
            double totalLockBoxDebit = 0D;
            double totalLockBoxCredit = 0D;
            double totalLockBoxInterest = 0D;

            try
            {

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    location = dt.Rows[i]["Location Name"].ObjToString();
                    if (String.IsNullOrWhiteSpace(location))
                        location = "No Location";
                    if (String.IsNullOrWhiteSpace(saveLocation))
                        saveLocation = location;
                    if (saveLocation != location)
                    {
                        DataRow dRow = locationDt.NewRow();
                        dRow["location"] = "HO " + saveLocation.ToUpper();
                        dRow["nbdp"] = locationPayments;
                        dRow["debitAdjustment"] = debit;
                        dRow["creditAdjustMent"] = credit;
                        adjustedPayments = locationPayments - debit + credit;
                        dRow["adjustedPayments"] = adjustedPayments;
                        locationDt.Rows.Add(dRow);

                        totalLockBoxPayments += locationPayments;
                        totalLockBoxCredit += credit;
                        totalLockBoxDebit += debit;
                        totalLockBoxInterest += interest;

                        locationPayments = 0D;
                        debit = 0D;
                        credit = 0D;
                        interest = 0D;
                        saveLocation = location;
                    }

                    payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    locationPayments += payment;
                    totalPayments += dt.Rows[i]["paymentAmount"].ObjToDouble();
                    interest += dt.Rows[i]["interestPaid1"].ObjToDouble();
                    credit += dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    debit += dt.Rows[i]["debitAdjustment"].ObjToDouble();
                }

                dR = locationDt.NewRow();
                dR["location"] = "HO " + saveLocation.ToUpper();
                dR["nbdp"] = locationPayments;
                dR["debitAdjustment"] = debit;
                dR["creditAdjustMent"] = credit;
                adjustedPayments = locationPayments - debit + credit;
                dR["adjustedPayments"] = adjustedPayments;
                locationDt.Rows.Add(dR);

                totalLockBoxPayments += locationPayments;
                totalLockBoxCredit += credit;
                totalLockBoxDebit += debit;
                totalLockBoxInterest += interest;

                dR = locationDt.NewRow();
                dR["location"] = "TOTAL DRAFTS KEYED AT HOMEOFFICE";
                dR["nbdp"] = totalLockBoxPayments;
                dR["debitAdjustment"] = totalLockBoxDebit;
                dR["creditAdjustMent"] = totalLockBoxCredit;
                adjustedPayments = totalLockBoxPayments - totalLockBoxDebit + totalLockBoxCredit;
                dR["adjustedPayments"] = adjustedPayments;
                locationDt.Rows.Add(dR);

                totalBoxPayments += totalLockBoxPayments;
                totalBoxDebit += totalLockBoxDebit;
                totalBoxCredit += totalLockBoxCredit;
                totalBoxInterest += totalLockBoxInterest;
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }
        }
        /****************************************************************************************/
        private void LoadCCEOM(DataTable dx, DataTable locationDt)
        {
            DataTable dt = dx.Copy();
            DataView tempview = dt.DefaultView;
            tempview.Sort = "Location Name";
            dt = tempview.ToTable();

            //DataTable locationDt = new DataTable();
            //locationDt.Columns.Add("num");
            //locationDt.Columns.Add("location");
            //locationDt.Columns.Add("notes");
            //locationDt.Columns.Add("nbdp", Type.GetType("System.Double"));
            //locationDt.Columns.Add("debitAdjustment", Type.GetType("System.Double"));
            //locationDt.Columns.Add("creditAdjustment", Type.GetType("System.Double"));
            //locationDt.Columns.Add("adjustedPayments", Type.GetType("System.Double"));

            string saveLocation = "";
            string location = "";
            double locationPayments = 0D;
            double totalPayments = 0D;
            double payment = 0D;
            double debit = 0D;
            double credit = 0D;
            double interest = 0D;
            double adjustedPayments = 0D;

            double totalLockBoxPayments = 0D;
            double totalLockBoxDebit = 0D;
            double totalLockBoxCredit = 0D;
            double totalLockBoxInterest = 0D;

            string depositNumber = "";
            string contractNumber = "";
            string str = "";

            try
            {

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    location = dt.Rows[i]["Location Name"].ObjToString();
                    if (String.IsNullOrWhiteSpace(location))
                        location = "No Location";
                    if (String.IsNullOrWhiteSpace(saveLocation))
                        saveLocation = location;
                    if (saveLocation != location)
                    {
                        //DataRow dRow = locationDt.NewRow();
                        //dRow["location"] = saveLocation.ToUpper();
                        //dRow["nbdp"] = locationPayments;
                        //dRow["debitAdjustment"] = debit;
                        //dRow["creditAdjustMent"] = credit;
                        //adjustedPayments = locationPayments - debit + credit;
                        //dRow["adjustedPayments"] = adjustedPayments;
                        //locationDt.Rows.Add(dRow);

                        totalLockBoxPayments += locationPayments;
                        totalLockBoxCredit += credit;
                        totalLockBoxDebit += debit;
                        totalLockBoxInterest += interest;

                        locationPayments = 0D;
                        debit = 0D;
                        credit = 0D;
                        interest = 0D;
                        saveLocation = location;
                    }

                    depositNumber = dt.Rows[i]["location"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(depositNumber))
                    {
                        if ( depositNumber.Length > 2)
                            depositNumber = depositNumber.Substring(0, 2);
                        if (depositNumber.ToUpper() == "CC")
                        {
                            payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                            locationPayments += payment;
                            totalPayments += dt.Rows[i]["paymentAmount"].ObjToDouble();
                            interest += dt.Rows[i]["interestPaid1"].ObjToDouble();
                            credit += dt.Rows[i]["creditAdjustment"].ObjToDouble();
                            debit += dt.Rows[i]["debitAdjustment"].ObjToDouble();
                            contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                            DataRow dRow = locationDt.NewRow();
                            payment = G1.RoundValue(payment);
                            str = G1.ReformatMoney(payment);

                            dRow["notes"] = contractNumber + " " + str;
                            locationDt.Rows.Add(dRow);
                        }
                    }
                }

                //DataRow dR = locationDt.NewRow();
                //dR["location"] = saveLocation.ToUpper();
                //dR["nbdp"] = locationPayments;
                //dR["debitAdjustment"] = debit;
                //dR["creditAdjustMent"] = credit;
                //adjustedPayments = locationPayments - debit + credit;
                //dR["adjustedPayments"] = adjustedPayments;
                //locationDt.Rows.Add(dR);

                DataRow dR = locationDt.NewRow();
                dR["location"] = "CREDIT/DEBIT CARDS";
                dR["nbdp"] = totalPayments;
                dR["debitAdjustment"] = debit;
                dR["creditAdjustMent"] = credit;
                adjustedPayments = totalPayments - debit + credit;
                dR["adjustedPayments"] = adjustedPayments;
                locationDt.Rows.Add(dR);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }
            //G1.NumberDataTable(locationDt);
            //dgv3.DataSource = locationDt;
        }
        /****************************************************************************************/
        private void LoadManualEOM(DataTable dx)
        {
            DataTable dt = dx.Copy();
            DataView tempview = dt.DefaultView;
            tempview.Sort = "Location Name";
            dt = tempview.ToTable();

            DataTable locationDt = new DataTable();
            locationDt.Columns.Add("num");
            locationDt.Columns.Add("location");
            locationDt.Columns.Add("notes");
            locationDt.Columns.Add("nbdp", Type.GetType("System.Double"));
            locationDt.Columns.Add("debitAdjustment", Type.GetType("System.Double"));
            locationDt.Columns.Add("creditAdjustment", Type.GetType("System.Double"));
            locationDt.Columns.Add("adjustedPayments", Type.GetType("System.Double"));

            string saveLocation = "";
            string location = "";
            double locationPayments = 0D;
            double totalPayments = 0D;
            double payment = 0D;
            double debit = 0D;
            double credit = 0D;
            double interest = 0D;
            double adjustedPayments = 0D;

            double totalLockBoxPayments = 0D;
            double totalLockBoxDebit = 0D;
            double totalLockBoxCredit = 0D;
            double totalLockBoxInterest = 0D;

            double dd = 0D;
            double cc = 0D;

            string depositNumber = "";
            string contractNumber = "";
            string str = "";
            string userId = "";
            string debitReason = "";
            string creditReason = "";
            string reason = "";

            try
            {

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    location = dt.Rows[i]["Location Name"].ObjToString();
                    if (String.IsNullOrWhiteSpace(location))
                        location = "No Location";
                    if (String.IsNullOrWhiteSpace(saveLocation))
                        saveLocation = location;
                    if (saveLocation != location)
                    {
                        //DataRow dRow = locationDt.NewRow();
                        //dRow["location"] = saveLocation.ToUpper();
                        //dRow["nbdp"] = locationPayments;
                        //dRow["debitAdjustment"] = debit;
                        //dRow["creditAdjustMent"] = credit;
                        //adjustedPayments = locationPayments - debit + credit;
                        //dRow["adjustedPayments"] = adjustedPayments;
                        //locationDt.Rows.Add(dRow);

                        totalLockBoxPayments += locationPayments;
                        totalLockBoxCredit += credit;
                        totalLockBoxDebit += debit;
                        totalLockBoxInterest += interest;

                        locationPayments = 0D;
                        debit = 0D;
                        credit = 0D;
                        interest = 0D;
                        saveLocation = location;
                    }

                    depositNumber = dt.Rows[i]["edited"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(depositNumber))
                    {
                        //                        depositNumber = depositNumber.Substring(0, 1);
                        if (depositNumber.ToUpper() == "MANUAL")
                        {
                            userId = dt.Rows[i]["userId"].ObjToString();
                            payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                            locationPayments += payment;
                            totalPayments += dt.Rows[i]["paymentAmount"].ObjToDouble();
                            interest += dt.Rows[i]["interestPaid1"].ObjToDouble();
                            credit += dt.Rows[i]["creditAdjustment"].ObjToDouble();
                            debit += dt.Rows[i]["debitAdjustment"].ObjToDouble();
                            contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                            payment = G1.RoundValue(payment);
                            str = G1.ReformatMoney(payment);

                            dd = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                            cc = dt.Rows[i]["creditAdjustment"].ObjToDouble();

                            debitReason = dt.Rows[i]["debitReason"].ObjToString();
                            creditReason = dt.Rows[i]["creditReason"].ObjToString();

                            reason = debitReason.Trim();
                            if (!String.IsNullOrWhiteSpace(creditReason))
                            {
                                if (!String.IsNullOrWhiteSpace(reason))
                                    reason += " / ";
                                reason += creditReason;
                            }

                            DataRow dRow = locationDt.NewRow();
                            dRow["location"] = "    " + contractNumber + "  (" + userId + ")";
                            dRow["notes"] = reason;
                            dRow["nbdp"] = payment;
                            dRow["debitAdjustment"] = dd;
                            dRow["creditAdjustMent"] = cc;
                            adjustedPayments = payment - dd + cc;
                            dRow["adjustedPayments"] = adjustedPayments;
                            locationDt.Rows.Add(dRow);
                        }
                    }
                }

                //DataRow dR = locationDt.NewRow();
                //dR["location"] = saveLocation.ToUpper();
                //dR["nbdp"] = locationPayments;
                //dR["debitAdjustment"] = debit;
                //dR["creditAdjustMent"] = credit;
                //adjustedPayments = locationPayments - debit + credit;
                //dR["adjustedPayments"] = adjustedPayments;
                //locationDt.Rows.Add(dR);

                totalLockBoxPayments += locationPayments;
                totalLockBoxCredit += credit;
                totalLockBoxDebit += debit;
                totalLockBoxInterest += interest;


                DataRow dR = locationDt.NewRow();
                dR["location"] = "MANUAL PAYMENTS";
                dR["nbdp"] = totalLockBoxPayments;
                dR["debitAdjustment"] = totalLockBoxDebit;
                dR["creditAdjustMent"] = totalLockBoxCredit;
                adjustedPayments = totalLockBoxPayments - totalLockBoxDebit + totalLockBoxCredit;
                dR["adjustedPayments"] = adjustedPayments;
                locationDt.Rows.Add(dR);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }
            G1.NumberDataTable(locationDt);
            dgv5.DataSource = locationDt;
        }
        /****************************************************************************************/
        private void CombineEOM ()
        {
            DataTable lockBoxDt = (DataTable)dgv4.DataSource;
            DataTable draftsDt = (DataTable)dgv3.DataSource;

            DataTable dt = lockBoxDt.Clone();

            DataRow dR = dt.NewRow();
            dR["location"] = "LOCKBOX";
            dt.Rows.Add(dR);

            for ( int i=0; i<lockBoxDt.Rows.Count; i++)
            {
                G1.copy_dt_row(lockBoxDt, i, dt, dt.Rows.Count);
            }

            dR = dt.NewRow();
            dR["location"] = "DRAFTS";
            dt.Rows.Add(dR);

            for (int i = 0; i < draftsDt.Rows.Count; i++)
            {
                G1.copy_dt_row(draftsDt, i, dt, dt.Rows.Count);
            }
            dgv11.DataSource = dt;
        }
        /****************************************************************************************/
        private void RemoveExtraTabs()
        {
            if ( workReport.ToUpper() == "DECEASED REPORT")
            {
                RemoveTabPage("TRUST EOM");
                RemoveTabPage("DRAFTS EOM");
                RemoveTabPage("LOCKBOX EOM");
                RemoveTabPage("MANUAL EOM");
                RemoveTabPage("COMBINED EOM");
                RemoveTabPage("CASH REMITTED");
                RemoveTabPage("REMIT DP'S");
                RemoveTabPage("REMIT PAYMENTS");
                RemoveTabPage("REMOVALS");
                RemoveTabPage("MISMATCHED DATA");
                return;
            }
            if (workReport.ToUpper() != "REMOVAL REPORT")
                RemoveTabPage("REMOVALS");
            if (workReport.ToUpper() == "TRUST EOM REPORT")
            {
                RemoveTabPage("CASH REMITTED");
                RemoveTabPage("REMIT DP'S");
                RemoveTabPage("REMIT PAYMENTS");
                RemoveTabPage("DECEASED");
                RemoveTabPage("MISMATCHED DATA");
                return;
            }
            if (workReport.ToUpper() == "CASH REMITTED REPORT")
            {
                RemoveTabPage("TRUST EOM");
                RemoveTabPage("DRAFTS EOM");
                RemoveTabPage("LOCKBOX EOM");
                RemoveTabPage("MANUAL EOM");
                RemoveTabPage("COMBINED EOM");
                RemoveTabPage("DECEASED");
                RemoveTabPage("MISMATCHED DATA");
                return;
            }
            if (workReport.ToUpper() == "REMOVAL REPORT")
            {
                RemoveTabPage("TRUST EOM");
                RemoveTabPage("DRAFTS EOM");
                RemoveTabPage("LOCKBOX EOM");
                RemoveTabPage("MANUAL EOM");
                RemoveTabPage("COMBINED EOM");
                RemoveTabPage("DECEASED");
                RemoveTabPage("CASH REMITTED");
                RemoveTabPage("REMIT DP'S");
                RemoveTabPage("REMIT PAYMENTS");
                RemoveTabPage("MISMATCHED DATA");
                return;
            }
            for (int i = (tabControl1.TabPages.Count - 1); i >= 0; i--)
            {
                TabPage tp = tabControl1.TabPages[i];
                if (tp.Name.ToUpper() != "TABPAYMENTS")
                {
                    tabControl1.TabPages.RemoveAt(i);
                    //                    tabControl1.TabPages[i].Hide();
                }
            }
        }
        /****************************************************************************************/
        private void RemoveTabPage ( string tabName )
        {
            for (int i = (tabControl1.TabPages.Count - 1); i >= 0; i--)
            {
                TabPage tp = tabControl1.TabPages[i];
                if (tp.Text.ToUpper() == tabName.ToUpper() )
                    tabControl1.TabPages.RemoveAt(i);
            }
        }
        /****************************************************************************************/
        private void AddTabPage(string tabName)
        {
            for (int i = (tabControl1.TabPages.Count - 1); i >= 0; i--)
            {
                TabPage tp = tabControl1.TabPages[i];
                if (tp.Text.ToUpper() == tabName.ToUpper())
                    tabControl1.TabPages.Add(tabName);
            }
        }
        /****************************************************************************************/
        //private void AddNewBusiness(DataTable dt, string date3, string date4)
        //{
        //    if (insurance)
        //        return;
        //    string cmd = "Select * from `contracts` p ";
        //    cmd += " JOIN `customers` c ON p.`contractNumber` = c.`contractNumber` ";
        //    cmd += " JOIN `payments` d ON p.`contractNumber` = d.`contractNumber` ";
        //    cmd += " where p.`issueDate8` >= '" + date3 + "' ";
        //    cmd += " and   p.`issueDate8` <= '" + date4 + "' ";
        //    cmd += " and d.`fill` <> 'D' ";
        //    if (chkDownPayments.Checked)
        //        cmd += " and p.`downPayment` <> '0.00' ";
        //    if (!chkPayments.Checked)
        //    {
        //        if (chkDownPayments.Checked)
        //            cmd += " GROUP BY p.`contractNumber` ORDER BY d.`payDate8` ASC";
        //        else
        //            cmd += " GROUP BY p.`contractNumber` ORDER BY p.`dueDate8` DESC";
        //    }
        //    else
        //        cmd += " ORDER BY d.`payDate8` ";
        //    cmd += ";";

        //    double downPayment = 0D;
        //    string contractNumber = "";

        //    DataTable dx = G1.get_db_data(cmd);

        //    dx.Columns.Add("num");
        //    dx.Columns.Add("customer");
        //    dx.Columns.Add("daysLate", Type.GetType("System.Int32"));
        //    dx.Columns.Add("phone");
        //    dx.Columns.Add("totalContract", Type.GetType("System.Double"));
        //    dx.Columns.Add("contractValue", Type.GetType("System.Double"));
        //    dx.Columns.Add("newBusiness", Type.GetType("System.Double"));
        //    dx.Columns.Add("retained", Type.GetType("System.Double"));
        //    dx.Columns.Add("financedAmount", Type.GetType("System.Double"));
        //    dx.Columns.Add("age");
        //    dx.Columns.Add("loc");
        //    dx.Columns.Add("Location Name");
        //    dx.Columns.Add("S", Type.GetType("System.Double"));

        //    string record = "";
        //    DataRow[] ddrX = null;

        //    for (int i = 0; i < dx.Rows.Count; i++)
        //    {
        //        contractNumber = dx.Rows[i]["contractNumber"].ObjToString().Trim();
        //        cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' AND `downPayment` > '0';";
        //        DataTable ddx = G1.get_db_data(cmd);
        //        if (ddx.Rows.Count > 0)
        //        {
        //            downPayment = ddx.Rows[0]["downPayment"].ObjToDouble();
        //            dx.Rows[i]["newBusiness"] = downPayment;
        //            record = ddx.Rows[0]["record"].ObjToString();
        //            ddrX = dt.Select("record2='" + record + "'");
        //            if ( ddrX.Length == 0 )
        //                dt.ImportRow(dx.Rows[i]);
        //        }
        //    }
        //}
        /****************************************************************************************/
        private void CleanupAllPayments ( DataTable dt)
        {
            //string record = "";
            //DataTable[] ddr = null;
            //for ( int i=0; i<dt.Rows.Count; i++)
            //{
            //    record = dt.Rows[i]["record2"].ObjToString();
            //    ddr = dt.Select("record2='" + record + "'");
            //    if ( ddr.Length > 1 )
            //    {
            //        for ( int j=1; j<ddr.Length; j++)
            //        {
            //            ddr[j]["record2"] = "";
            //        }
            //    }
            //}
        }
        /****************************************************************************************/
        private void SortForDownPayments(DataTable dt)
        {
            double financed = 0D;
            //for ( int i=(dt.Rows.Count-1); i>=0; i--)
            //{
            //    financed = dt.Rows[i]["financedAmount"].ObjToDouble();
            //    if (financed == 0D)
            //    {
            //        dt.Rows.RemoveAt(i);
            //    }
            //}
            DataView tempview = dt.DefaultView;
            tempview.Sort = "payDate8";
            dt = tempview.ToTable();
            G1.NumberDataTable(dt);
        }
        /****************************************************************************************/
        private void LabelSinglePremiumContracts( DataTable dt)
        {
            double financedAmount = 0D;
            double balanceDue = 0D;
            double monthlyPayment = 0D;
            string contractNumber = "";
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                monthlyPayment = dt.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                balanceDue = dt.Rows[i]["balanceDue"].ObjToDouble();
                financedAmount = dt.Rows[i]["financedAmount"].ObjToDouble();
                financedAmount = DailyHistory.GetFinanceValue(dt.Rows[i]);
                if (financedAmount == 0D)
                    dt.Rows[i]["where"] = "SINGLE PREM";
                else if ( balanceDue <= 0D)
                    dt.Rows[i]["where"] = "PAID-OFF";
                if (balanceDue > 0D && balanceDue < monthlyPayment)
                    dt.Rows[i]["flag"] = "*";
            }
        }
        /****************************************************************************************/
        private void RemoveSinglePremiumContracts(DataTable dt)
        {
            double financedAmount = 0D;
            string contractNumber = "";
            double debit = 0D;
            double credit = 0D;
            double payment = 0D;
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                payment = dt.Rows[i]["paymentAmount"].ObjToDouble();

                financedAmount = dt.Rows[i]["financedAmount"].ObjToDouble();
                if (financedAmount == 0D)
                {
                    if ( payment == 0D && debit == 0D && credit == 0D)
                        dt.Rows.RemoveAt(i);
                }
            }
        }
        /****************************************************************************************/
        public static DataTable CleanupFutureReporting ( DataTable dt, string date1, string date2, string newBusCol = "" )
        {
            DateTime lDate1 = date1.ObjToDateTime();
            DateTime lDate2 = date2.ObjToDateTime();

            int nextMonth = lDate2.Month;
            int issueMonth = 0;

            DateTime payDate = DateTime.Now;
            DateTime issueDate = DateTime.Now;

            string contractNumber = "";
            double newBusiness = 0D;
            double downPayment = 0D;
            bool gotNewBus = false;
            bool gotDP = false;
            if (G1.get_column_number(dt, "newBusiness") >= 0)
                gotNewBus = true;
            if (G1.get_column_number(dt, "downPayment1") >= 0)
                gotDP = true;

            for ( int i=dt.Rows.Count-1; i>=0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                if ( contractNumber == "E23041LI")
                {
                }
                if (contractNumber == "B23029L")
                {
                }
                newBusiness = 0D;
                downPayment = 0D;
                if (gotNewBus)
                    newBusiness = dt.Rows[i]["newBusiness"].ObjToDouble();
                if ( gotDP )
                    downPayment = dt.Rows[i]["downPayment1"].ObjToDouble();
                if (newBusiness <= 0D && downPayment <= 0D)
                    continue;
                payDate = dt.Rows[i]["payDate8"].ObjToDateTime();
                issueDate = dt.Rows[i]["issueDate8"].ObjToDateTime();
                issueMonth = issueDate.Month;
                if (issueDate.Year > lDate2.Year)
                    issueMonth += 12;
                if ( issueMonth > nextMonth )
                {
                    dt.Rows.RemoveAt(i);
                }
            }
            return dt;
        }
        /****************************************************************************************/
        private void FindNewContracts(DataTable dt, string date1, string date2)
        {
            if (insurance)
                return;
            int lastRow = 0;
            double downPayment = 0D;
            string cmd = "Select * from `contracts` p ";
            cmd += " JOIN `customers` c ON p.`contractNumber` = c.`contractNumber` ";
            cmd += " where p.`issueDate8` >= '" + date1 + "' ";
            cmd += " and   p.`issueDate8` <= '" + date2 + "' ";
            cmd += " and p.`downPayment` > '0.00' ";
            cmd += " ORDER by p.`issueDate8` ";
            cmd += ";";
            DataTable dx = G1.get_db_data(cmd);
            string contract = "";
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                contract = dx.Rows[i]["contractNumber"].ObjToString().Trim();
                if (contract == "CT19050LI")
                {
                }
                DataRow[] dRows = dt.Select("contractNumber='" + contract + "'");
                if (dRows.Length <= 0)
                {
                    dt.ImportRow(dx.Rows[i]);
                    lastRow = dt.Rows.Count - 1;
                    dt.Rows[lastRow]["payDate8"] = dt.Rows[lastRow]["issueDate8"];
                    downPayment = dt.Rows[lastRow]["downPayment"].ObjToDouble();
                    if (!chkShowAll.Checked)
                        dt.Rows[lastRow]["paymentAmount"] = downPayment;
                    dt.Rows[lastRow]["interestPaid1"] = 0D;
                    dt.Rows[lastRow]["trust100P"] = 0D;
                    dt.Rows[lastRow]["trust85P"] = 0D;
                    dt.Rows[lastRow]["debitAdjustment"] = 0D;
                    dt.Rows[lastRow]["creditAdjustment"] = 0D;
                }
            }
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                SetSpyGlass(gridMain);
            else if (dgv2.Visible)
                SetSpyGlass(gridMain2);
            else if (dgv3.Visible)
                SetSpyGlass(gridMain3);
            else if (dgv4.Visible)
                SetSpyGlass(gridMain4);
            else if (dgv5.Visible)
                SetSpyGlass(gridMain5);
            else if (dgv6.Visible)
                SetSpyGlass(gridMain6);
            else if (dgv7.Visible)
                SetSpyGlass(gridMain7);
            else if (dgv8.Visible)
                SetSpyGlass(gridMain8);
            else if (dgv12.Visible)
                SetSpyGlass(gridMain12);
            else if (dgv13.Visible)
                SetSpyGlass(gridMain13);
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
        private void loadAgents(string cmd, string date1, string date2)
        {
            string agent = "";

            cmd = cmd.Replace("XYZZY1", date1);
            cmd = cmd.Replace("XYZZY2", date2);
            //            cmd += " GROUP by `agentNumber` order by `agentNumber`;";
            cmd += " GROUP by `agentCode` order by `agentCode`;";
            DataTable _agentList = G1.get_db_data(cmd);
            chkComboAgent.Properties.DataSource = _agentList;
        }
        /*******************************************************************************************/
        private string getAgentQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboAgent.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `agentNumber` IN (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void chkComboAgent_EditValueChanged(object sender, EventArgs e)
        {
            string names = getAgentQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            //            btnRun_Click(null, null);
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            if (cmbDateType.Text.ToUpper() == "WEEKLY")
            {
                GetWeeklyDate(this.dateTimePicker2.Value, "BACK");
                return;
            }
            else if (cmbDateType.Text.ToUpper() == "DAILY")
            {
                now = now.AddDays(-1);
                this.dateTimePicker1.Value = now;
                this.dateTimePicker2.Value = now;
                this.dateTimePicker3.Value = now;
                this.dateTimePicker4.Value = now;
                return;
            }
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker3.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker4.Value = new DateTime(now.Year, now.Month, days);
            DateTime start = now.AddDays(-1);
            if (start >= DailyHistory.majorDate)
                start = now;
            DateTime stop = new DateTime(now.Year, now.Month, days - 1);
            if ( stop >= DailyHistory.majorDate )
                stop = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker1.Value = start;
            this.dateTimePicker2.Value = stop;
            if (!dateTimePicker3.Visible)
            {
                if (!chkPaidUp.Checked)
                    dateTimePicker1.Value = dateTimePicker3.Value;
                dateTimePicker2.Value = dateTimePicker4.Value;
            }
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            if (cmbDateType.Text.ToUpper() == "WEEKLY")
            {
                GetWeeklyDate(this.dateTimePicker2.Value, "FORWARD");
                return;
            }
            else if (cmbDateType.Text.ToUpper() == "DAILY")
            {
                now = now.AddDays(1);
                this.dateTimePicker1.Value = now;
                this.dateTimePicker2.Value = now;
                this.dateTimePicker3.Value = now;
                this.dateTimePicker4.Value = now;
                return;
            }
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker3.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker4.Value = new DateTime(now.Year, now.Month, days);
            if (now >= DailyHistory.majorDate)
            {
                this.dateTimePicker1.Value = this.dateTimePicker3.Value;
                this.dateTimePicker2.Value = this.dateTimePicker4.Value;
            }
            else
            {
                DateTime start = now.AddDays(-1);
                DateTime stop = new DateTime(now.Year, now.Month, days - 1);
                this.dateTimePicker1.Value = start;
                this.dateTimePicker2.Value = stop;
                if (!dateTimePicker3.Visible)
                {
                    if (!chkPaidUp.Checked)
                        dateTimePicker1.Value = dateTimePicker3.Value;
                    dateTimePicker2.Value = dateTimePicker4.Value;
                }
            }
        }
        /***********************************************************************************************/
        private void GetWeeklyDate(DateTime date, string direction)
        {
            loading = true;
            DateTime idate = date;
            if (direction == "BACK")
            {
                date = date.AddDays(-7);
                this.dateTimePicker2.Value = date;
                date = date.AddDays(-4);
                this.dateTimePicker1.Value = date;
            }
            else
            {
                date = date.AddDays(7);
                this.dateTimePicker2.Value = date;
                date = date.AddDays(-4);
                this.dateTimePicker1.Value = date;

            }
            if (workReport.ToUpper() == "TRUST EOM REPORT")
            {
                this.dateTimePicker3.Value = this.dateTimePicker1.Value;
                this.dateTimePicker4.Value = this.dateTimePicker2.Value;
            }

            loading = false;
        }
        /****************************************************************************************/
        private void cmbDateType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string dateType = cmbDateType.Text;
            if (dateType.ToUpper() == "MONTHLY")
            {
                DateTime now = DateTime.Now;
                now = now.AddMonths(-1);
                now = new DateTime(now.Year, now.Month, 1);
                this.dateTimePicker3.Value = now;
                int days = DateTime.DaysInMonth(now.Year, now.Month);
                this.dateTimePicker4.Value = new DateTime(now.Year, now.Month, days);
                DateTime start = now.AddDays(-1);
                DateTime stop = new DateTime(now.Year, now.Month, days - 1);
                this.dateTimePicker1.Value = start;
                this.dateTimePicker2.Value = stop;
                if (this.Text.ToUpper().IndexOf("PAID OFF ") > 0)
                {
                }
                else
                {
                    //if (!insurance)
                    //{
                        this.dateTimePicker3.Visible = true;
                        this.dateTimePicker4.Visible = true;
                    //}
                    lblAllOther.Show();
                    lblAllOtherTo.Show();
                }
                this.Text = this.Text.Replace("Weekly", "Monthly");
            }
            else
            {
                if (workReport.ToUpper() == "INSURANCE WEEK TOTALS")
                {
                    this.dateTimePicker3.Visible = true;
                    this.dateTimePicker4.Visible = true;
                    lblAllOther.Show();
                    lblAllOtherTo.Show();
                }
                else if (workReport.ToUpper() != "TRUST EOM REPORT")
                {
                    this.dateTimePicker3.Visible = false;
                    this.dateTimePicker4.Visible = false;
                    lblAllOther.Hide();
                    lblAllOtherTo.Hide();
                }
                this.Text = this.Text.Replace("Monthly", "Weekly");
                DateTime now = this.dateTimePicker2.Value;
                for (;;)
                {
                    if (now.DayOfWeek == DayOfWeek.Friday)
                    {
                        this.dateTimePicker2.Value = now;
                        this.dateTimePicker1.Value = now.AddDays(-4);
                        break;
                    }
                    now = now.AddDays(-1);
                }
                if (workReport.ToUpper() == "TRUST EOM REPORT")
                {
                    this.dateTimePicker3.Value = this.dateTimePicker1.Value;
                    this.dateTimePicker4.Value = this.dateTimePicker2.Value;
                }
            }
            this.Refresh();
        }
        /****************************************************************************************/
        //private void commissionReportToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    DataTable dt = (DataTable)dgv.DataSource;
        //}
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                //DataTable dt = (DataTable)dgv.DataSource;
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.ShowDialog();

                string cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
                DataTable dt = G1.get_db_data(cmd);
                if ( dt.Rows.Count > 0 )
                {
                    string address1 = dt.Rows[0]["address1"].ObjToString();
                    string address2 = dt.Rows[0]["address2"].ObjToString();
                    dr["address1"] = address1;
                    dr["address2"] = address2;
                }
                //DailyHistory dailyForm = new DailyHistory(contract);
                //dailyForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private double originalSize = 0D;
        private Font mainFont = null;
        private void ScaleCells()
        {
            if (originalSize == 0D)
            {
                //                originalSize = gridMain.Columns["address1"].AppearanceCell.FontSizeDelta.ObjToDouble();
                originalSize = gridMain.Columns["address1"].AppearanceCell.Font.Size;
                mainFont = gridMain.Columns["address1"].AppearanceCell.Font;
            }
            double scale = txtScale.Text.ObjToDouble();
            double size = scale / 100D * originalSize;
            Font font = new Font(mainFont.Name, (float)size);
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                gridMain.Columns[i].AppearanceCell.Font = font;
            }
            gridMain.RefreshData();
            dgv.Refresh();
            this.Refresh();
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
                MessageBox.Show("***ERROR*** Key entered must be a number!");
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private void txtBalance_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string balance = txtBalance.Text.Trim();
                if (!G1.validate_numeric(balance))
                {
                    MessageBox.Show("***ERROR*** Minimum Balance must be numeric!");
                    return;
                }
                double money = balance.ObjToDouble();
                balance = G1.ReformatMoney(money);
                txtBalance.Text = balance;
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
                MessageBox.Show("***ERROR*** Key entered must be a number!");
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private void LocateFuneralHome(string loc, ref string name, ref string address, ref string city, ref string state, ref string zip)
        {
            name = "";
            address = "";
            city = "";
            state = "";
            zip = "";
            DataTable dx = G1.get_db_data("Select * from `funeralhomes` where `keycode` = '" + loc + "';");
            if (dx.Rows.Count > 0)
            {
                name = dx.Rows[0]["name"].ObjToString();
                address = dx.Rows[0]["address"].ObjToString();
                city = dx.Rows[0]["city"].ObjToString();
                state = dx.Rows[0]["state"].ObjToString();
                zip = dx.Rows[0]["zip"].ObjToString();
            }
        }
        /****************************************************************************************/
        private void InsuranceSortByUser ()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (chkSortByTrust.Checked)
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "userId, lastName, firstName";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

                gridMain.Columns["userId"].GroupIndex = 0;
                gridMain.Columns["customer"].Visible = false;
                gridMain.Columns["lastName"].Visible = true;
                gridMain.Columns["firstName"].Visible = true;
                gridMain.OptionsView.ShowFooter = true;
                this.gridMain.ExpandAllGroups();
            }
            else
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "userId, lastName";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

                gridMain.Columns["userId"].GroupIndex = -1;
                gridMain.Columns["customer"].Visible = true;
                gridMain.Columns["lastName"].Visible = false;
                gridMain.Columns["firstName"].Visible = false;
                gridMain.OptionsView.ShowFooter = true;
                gridMain.CollapseAllGroups();
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void chkSort_CheckedChanged(object sender, EventArgs e)
        {
            if (dgv.DataSource == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            //if ( workReport.ToUpper() == "INSURANCE WEEK TOTALS")
            //{
            //    DataView tempview = dt.DefaultView;
            //    tempview.Sort = "Location, lastName, firstName";
            //    dt = tempview.ToTable();
            //    G1.NumberDataTable(dt);
            //    dgv.DataSource = dt;

            //    gridMain.Columns["Location Name"].GroupIndex = 0;
            //    gridMain.Columns["customer"].Visible = false;
            //    gridMain.Columns["lastName"].Visible = true;
            //    gridMain.Columns["firstName"].Visible = true;
            //    gridMain.OptionsView.ShowFooter = true;
            //    this.gridMain.ExpandAllGroups();
            //    return;
            //}
            if (chkSort.Checked)
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "Location Name, lastName, firstName";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

                gridMain.Columns["Location Name"].GroupIndex = 0;
                gridMain.Columns["customer"].Visible = false;
                gridMain.Columns["lastName"].Visible = true;
                gridMain.Columns["firstName"].Visible = true;
                gridMain.OptionsView.ShowFooter = true;
                this.gridMain.ExpandAllGroups();
            }
            else
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "Location Name, lastName";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

                gridMain.Columns["Location Name"].GroupIndex = -1;
                gridMain.Columns["customer"].Visible = true;
                gridMain.Columns["lastName"].Visible = false;
                gridMain.Columns["firstName"].Visible = false;
                gridMain.OptionsView.ShowFooter = true;
                gridMain.CollapseAllGroups();
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.dateTimePicker1.Value = new DateTime(1980, 01, 01);
            this.Refresh();
        }
        /****************************************************************************************/
        private void chkDeaths_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            chkActiveOnly.Checked = false;
            chkDownPayments.Checked = false;
            chkBalanceLessPayment.Checked = false;
            chkBalances.Checked = false;
            chkPaidUp.Checked = false;
            chkPayments.Checked = false;
            chkShowAll.Checked = false;

            ClearAllPositions();
            SetDefaultPositions();
            SetPaymentsAfterDeaths();

            loading = false;
        }
        /****************************************************************************************/
        private void chkActiveOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            chkDeaths.Checked = false;
            chkDownPayments.Checked = false;
            chkBalances.Checked = false;
            chkPaidUp.Checked = false;
            chkPayments.Checked = false;
            chkDeaths.Checked = false;
            chkShowAll.Checked = false;

            ClearAllPositions();
            SetDefaultPositions();
            SetDownPayments();

            ResetColumns();

            loading = false;
        }
        /****************************************************************************************/
        private void chkBalanceLessPayment_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            chkDeaths.Checked = false;
            chkDownPayments.Checked = false;
            chkBalances.Checked = false;
            chkPaidUp.Checked = false;
            chkPayments.Checked = false;
            chkShowAll.Checked = false;

            ClearAllPositions();
            SetDefaultPositions();
            SetBalanceLessPayments();

            ResetColumns();

            loading = false;
        }
        /****************************************************************************************/
        private void chkBalances_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            chkDeaths.Checked = false;
            chkBalanceLessPayment.Checked = false;
            chkDownPayments.Checked = false;
            chkPaidUp.Checked = false;
            chkPayments.Checked = false;
            chkShowAll.Checked = false;

            ClearAllPositions();
            SetDefaultPositions();
            SetBalanceLessThanAmount();

            ResetColumns();

            loading = false;
        }
        /****************************************************************************************/
        private void chkPaidUp_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            chkActiveOnly.Checked = false;
            chkDownPayments.Checked = false;
            chkBalanceLessPayment.Checked = false;
            chkBalances.Checked = false;
            chkDeaths.Checked = false;
            chkShowAll.Checked = false;
            cmbDateType.Show();

            ClearAllPositions();
            SetPaidUp();

            gridMain.Columns["contractValue"].Caption = "Net";
            gridMain.Columns["payDate8"].Caption = "Last Date Paid";

            ResetColumns();


            loading = false;
        }
        /****************************************************************************************/
        private void chkPayments_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            chkActiveOnly.Checked = false;
            chkDownPayments.Checked = false;
            chkBalanceLessPayment.Checked = false;
            chkBalances.Checked = false;
            chkDeaths.Checked = false;
            chkShowAll.Checked = false;

            ClearAllPositions();
            SetDefaultPositions();
            SetPayments();

            ResetColumns();

            loading = false;
        }
        /****************************************************************************************/
        private void chkDownPayments_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            chkActiveOnly.Checked = false;
            chkBalanceLessPayment.Checked = false;
            chkBalances.Checked = false;
            chkDeaths.Checked = false;
            chkPayments.Checked = false;
            chkShowAll.Checked = false;

            ClearAllPositions();
            SetDownPayments();

            ResetColumns();

            loading = false;
        }
        /****************************************************************************************/
        private void ClearAllPositions(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            for (int i = 0; i < gMain.Columns.Count; i++)
            {
                gMain.Columns[i].Visible = false;
            }
        }
        /****************************************************************************************/
        private void SetDefaultPositions()
        {
            G1.SetColumnPosition(gridMain, "num", 1);
            G1.SetColumnPosition(gridMain, "agentNumber", 2);
            G1.SetColumnPosition(gridMain, "contractNumber", 3);
            G1.SetColumnPosition(gridMain, "customer", 4);
        }
        /****************************************************************************************/
        private void SetDownPayments()
        {
            //            G1.SetColumnPosition(gridMain, "lastDatePaid8", 4);
            G1.SetColumnPosition(gridMain, "num", 1);
            G1.SetColumnPosition(gridMain, "agentNumber", 2);
            G1.SetColumnPosition(gridMain, "contractNumber", 3);
            G1.SetColumnPosition(gridMain, "lastName", 4);
            G1.SetColumnPosition(gridMain, "firstName", 5);

            G1.SetColumnPosition(gridMain, "payDate8", 6);
            G1.SetColumnPosition(gridMain, "downPayment", 7);
            G1.SetColumnPosition(gridMain, "ccFee", 8);
            G1.SetColumnPosition(gridMain, "trust100P", 9);
            G1.SetColumnPosition(gridMain, "trust85P", 10);
            G1.SetColumnPosition(gridMain, "address1", 11);
            G1.SetColumnPosition(gridMain, "city", 12);
            G1.SetColumnPosition(gridMain, "state", 13);
            G1.SetColumnPosition(gridMain, "zip1", 14);
            G1.SetColumnPosition(gridMain, "sex", 15);
            G1.SetColumnPosition(gridMain, "birthDate", 16);
            G1.SetColumnPosition(gridMain, "age", 17);
            G1.SetColumnPosition(gridMain, "ssn", 18);

            gridMain.Columns["downPayment"].Visible = true;
            gridMain.Columns["downPayment1"].Visible = false;
            gridMain.Columns["customer"].Visible = false;
        }
        /****************************************************************************************/
        private void SetRemitDPs()
        {
            //            G1.SetColumnPosition(gridMain, "lastDatePaid8", 4);
            //            G1.SetColumnPosition(gridMain, "num", 1);
            ClearAllPositions( gridMain7 );
            G1.SetColumnPosition(gridMain7, "contractNumber", 1);
            G1.SetColumnPosition(gridMain7, "lastName", 2);
            G1.SetColumnPosition(gridMain7, "firstName", 3);

            G1.SetColumnPosition(gridMain7, "payDate8", 4);
            G1.SetColumnPosition(gridMain7, "dbr", 5);
            G1.SetColumnPosition(gridMain7, "newBusiness", 6);
            G1.SetColumnPosition(gridMain7, "trust100P", 7);
            G1.SetColumnPosition(gridMain7, "trust85P", 8);
            G1.SetColumnPosition(gridMain7, "address1", 9);
            G1.SetColumnPosition(gridMain7, "city", 10);
            G1.SetColumnPosition(gridMain7, "state", 11);
            G1.SetColumnPosition(gridMain7, "zip1", 12);
            G1.SetColumnPosition(gridMain7, "sex", 13);
            G1.SetColumnPosition(gridMain7, "birthDate", 14);
            G1.SetColumnPosition(gridMain7, "age", 15);
            G1.SetColumnPosition(gridMain7, "ssn", 16);

            gridMain7.Columns["downPayment1"].Visible = false;
            gridMain7.Columns["customer"].Visible = false;
        }
        /****************************************************************************************/
        private void SetRemitPayments()
        {
            //            G1.SetColumnPosition(gridMain, "lastDatePaid8", 4);
            //            G1.SetColumnPosition(gridMain, "num", 1);
            ClearAllPositions( gridMain8 );
            G1.SetColumnPosition(gridMain8, "contractNumber", 2);
            G1.SetColumnPosition(gridMain8, "lastName", 3);
            G1.SetColumnPosition(gridMain8, "firstName", 4);

            G1.SetColumnPosition(gridMain8, "trust100P", 5);
            G1.SetColumnPosition(gridMain8, "trust85P", 6);
            G1.SetColumnPosition(gridMain8, "dbr", 7);

            gridMain8.Columns["downPayment1"].Visible = false;
            gridMain8.Columns["customer"].Visible = false;

            ClearAllPositions(gridMain12);
            G1.SetColumnPosition(gridMain12, "contractNumber", 2);
            G1.SetColumnPosition(gridMain12, "lastName", 3);
            G1.SetColumnPosition(gridMain12, "firstName", 4);

            G1.SetColumnPosition(gridMain12, "trust100P", 5);
            G1.SetColumnPosition(gridMain12, "trust85P", 6);

            gridMain12.Columns["downPayment1"].Visible = false;
            gridMain12.Columns["customer"].Visible = false;
        }
        /****************************************************************************************/
        private void SetOVPPayments()
        {
            //            G1.SetColumnPosition(gridMain, "lastDatePaid8", 4);
            //            G1.SetColumnPosition(gridMain, "num", 1);
            ClearAllPositions(gridMain13);
            G1.SetColumnPosition(gridMain13, "contractNumber", 2);
            G1.SetColumnPosition(gridMain13, "lastName", 3);
            G1.SetColumnPosition(gridMain13, "firstName", 4);

            G1.SetColumnPosition(gridMain13, "trust100P", 5);
            G1.SetColumnPosition(gridMain13, "trust85P", 6);
            G1.SetColumnPosition(gridMain13, "ovp", 7);

            gridMain13.Columns["downPayment1"].Visible = false;
            gridMain13.Columns["customer"].Visible = false;
        }
        /****************************************************************************************/
        private void SetRemovals()
        {
            ClearAllPositions(gridMain9);
            G1.SetColumnPosition(gridMain9, "num", 1);
            G1.SetColumnPosition(gridMain9, "lastName", 2);
            G1.SetColumnPosition(gridMain9, "firstName", 3);
            G1.SetColumnPosition(gridMain9, "contractNumber", 4);
            G1.SetColumnPosition(gridMain9, "ServiceId", 5);
            G1.SetColumnPosition(gridMain9, "dateDPPaid", 6);
            G1.SetColumnPosition(gridMain9, "Trust Paid", 7);
            G1.SetColumnPosition(gridMain9, "Trust Paid Date", 8);

            G1.SetColumnPosition(gridMain9, "deceasedDate", 9);
            G1.SetColumnPosition(gridMain9, "birthDate", 10);
            G1.SetColumnPosition(gridMain, "ssn", 11);

            gridMain9.Columns["downPayment1"].Visible = false;
            gridMain9.Columns["customer"].Visible = false;

            this.dateTimePicker3.Visible = false;
            this.dateTimePicker4.Visible = false;
            lblAllOther.Hide();
            lblAllOtherTo.Hide();
        }
        /****************************************************************************************/
        private void SetDeceased()
        {
            ClearAllPositions(gridMain9);
            G1.SetColumnPosition(gridMain9, "num", 1);
            G1.SetColumnPosition(gridMain9, "lastName", 2);
            G1.SetColumnPosition(gridMain9, "firstName", 3);
            G1.SetColumnPosition(gridMain9, "contractNumber", 4);
            G1.SetColumnPosition(gridMain9, "ServiceId", 5);

            G1.SetColumnPosition(gridMain9, "deceasedDate", 6);
            G1.SetColumnPosition(gridMain9, "birthDate", 7);
            G1.SetColumnPosition(gridMain, "ssn", 8);

            gridMain9.Columns["downPayment1"].Visible = false;
            gridMain9.Columns["customer"].Visible = false;

            this.dateTimePicker3.Visible = false;
            this.dateTimePicker4.Visible = false;
            lblAllOther.Hide();
            lblAllOtherTo.Hide();
        }
        /****************************************************************************************/
        private void SetNewBusiness()
        {
            G1.SetColumnPosition(gridMain, "num", 1);
            G1.SetColumnPosition(gridMain, "payDate8", 2);
            G1.SetColumnPosition(gridMain, "agentNumber", 3);
            G1.SetColumnPosition(gridMain, "contractNumber", 4);
            G1.SetColumnPosition(gridMain, "lastName", 5);
            G1.SetColumnPosition(gridMain, "firstName", 6);
            G1.SetColumnPosition(gridMain, "downPayment", 7);
            G1.SetColumnPosition(gridMain, "ccFee", 8);
            G1.SetColumnPosition(gridMain, "issueDate8", 9);
            G1.SetColumnPosition(gridMain, "trust100P", 10);
            G1.SetColumnPosition(gridMain, "trust85P", 11);

            gridMain.Columns["downPayment1"].Visible = false;
            gridMain.Columns["customer"].Visible = false;
        }
        /****************************************************************************************/
        private void SetDbrBusiness()
        {
            SetupDBRColumn();
            chkShowAllDeceased.Show();
            G1.SetColumnPosition(gridMain, "num", 1);
//            G1.SetColumnPosition(gridMain, "select", 1);
            G1.SetColumnPosition(gridMain, "agentNumber", 2);
            G1.SetColumnPosition(gridMain, "contractNumber", 3);
            G1.SetColumnPosition(gridMain, "lastName", 4);
            G1.SetColumnPosition(gridMain, "firstName", 5);
            G1.SetColumnPosition(gridMain, "downPayment", 6);
            G1.SetColumnPosition(gridMain, "issueDate8", 7);
            G1.SetColumnPosition(gridMain, "deceasedDate", 8);
            G1.SetColumnPosition(gridMain, "dateDPPaid", 9);
            G1.SetColumnPosition(gridMain, "lastDatePaid8", 10);
            G1.SetColumnPosition(gridMain, "businessDays", 11);
            G1.SetColumnPosition(gridMain, "downPayment1", 12);
            G1.SetColumnPosition(gridMain, "paymentAmount", 13);
            G1.SetColumnPosition(gridMain, "ccFee", 14);
            G1.SetColumnPosition(gridMain, "interestPaid1", 15);

            gridMain.Columns["downPayment"].Visible = false;
            gridMain.Columns["customer"].Visible = false;
        }
        /****************************************************************************************/
        private void SetBalanceLessPayments()
        {
            G1.SetColumnPosition(gridMain, "lastDatePaid8", 4);
            G1.SetColumnPosition(gridMain, "balanceDue", 5);
            G1.SetColumnPosition(gridMain, "amtOfMonthlyPayt", 6);
        }
        /****************************************************************************************/
        private void SetBalanceLessThanAmount()
        {
            G1.SetColumnPosition(gridMain, "lastDatePaid8", 4);
            G1.SetColumnPosition(gridMain, "balanceDue", 5);
            G1.SetColumnPosition(gridMain, "amtOfMonthlyPayt", 6);
        }
        /****************************************************************************************/
        private void SetPaymentsAfterDeaths()
        {
            G1.SetColumnPosition(gridMain, "payDate8", 4);
            G1.SetColumnPosition(gridMain, "deceasedDate", 5);
//            G1.SetColumnPosition(gridMain, "downPayment", 6);
            G1.SetColumnPosition(gridMain, "downPayment1", 6);
            G1.SetColumnPosition(gridMain, "paymentAmount", 7);
            G1.SetColumnPosition(gridMain, "ccFee", 8);
            G1.SetColumnPosition(gridMain, "amtOfMonthlyPayt", 9);
            G1.SetColumnPosition(gridMain, "balanceDue", 10);
        }
        /****************************************************************************************/
        private void SetPaidUp()
        {

            SetupSelectColumn();
            int i = 1;
            G1.SetColumnPosition(gridMain, "num", i++);
            G1.SetColumnPosition(gridMain, "select", i++);
            G1.SetColumnPosition(gridMain, "contractNumber", i++);
            G1.SetColumnPosition(gridMain, "letter", i++);
            G1.SetColumnPosition(gridMain, "pulled", i++);
            G1.SetColumnPosition(gridMain, "lastName", i++);
            G1.SetColumnPosition(gridMain, "firstName", i++);
            G1.SetColumnPosition(gridMain, "where", i++);
            G1.SetColumnPosition(gridMain, "contractValue", i++);
            G1.SetColumnPosition(gridMain, "amtOfMonthlyPayt", i++);
            G1.SetColumnPosition(gridMain, "numMonths", i++);
            G1.SetColumnPosition(gridMain, "balanceDue", i++);
            G1.SetColumnPosition(gridMain, "payDate8", i++);
            G1.SetColumnPosition(gridMain, "dueDate8", i++);
            G1.SetColumnPosition(gridMain, "issueDate8", i++);
            G1.SetColumnPosition(gridMain, "flag", i++);
            G1.SetColumnPosition(gridMain, "deceasedDate", i++);
            //G1.SetColumnPosition(gridMain, "address1", 12);
            //G1.SetColumnPosition(gridMain, "address2", 13);
        }
        /****************************************************************************************/
        private void SetReinstatementReport()
        {
            gridMain.Columns["customer"].Visible = false;
            gridMain.Columns["lapseDate8"].Visible = true;
            gridMain.Columns["reinstateDate8"].Visible = true;
            G1.SetColumnPosition(gridMain, "num", 1);
            G1.SetColumnPosition(gridMain, "agentNumber", 2);
            G1.SetColumnPosition(gridMain, "contractNumber", 3);
            G1.SetColumnPosition(gridMain, "lastName", 4);
            G1.SetColumnPosition(gridMain, "firstName", 5);
            G1.SetColumnPosition(gridMain, "issueDate8", 6);
            G1.SetColumnPosition(gridMain, "lapseDate8", 7);
            G1.SetColumnPosition(gridMain, "reinstateDate8", 8);
            G1.SetColumnPosition(gridMain, "contractValue", 9);
            G1.SetColumnPosition(gridMain, "dueDate8", 10);
        }
        /****************************************************************************************/
        private void SetShowNextPayments ()
        {
            ClearAllPositions();
            SetDefaultPositions();
            gridMain.Columns["customer"].Visible = false;
            G1.SetColumnPosition(gridMain, "lastName", 4);
            G1.SetColumnPosition(gridMain, "firstName", 5);
            G1.SetColumnPosition(gridMain, "dueDate8", 6);
            G1.SetColumnPosition(gridMain, "paymentAmount", 7);
            G1.SetColumnPosition(gridMain, "ccFee", 8);

            ResetColumns();
        }
        /****************************************************************************************/
        private void SetPayments()
        {
            gridMain.Columns["customer"].Visible = false;
            G1.SetColumnPosition(gridMain, "lastName", 4);
            G1.SetColumnPosition(gridMain, "firstName", 5);
            G1.SetColumnPosition(gridMain, "payDate8", 6);
            G1.SetColumnPosition(gridMain, "paymentAmount", 7);
            G1.SetColumnPosition(gridMain, "ccFee", 8);
            G1.SetColumnPosition(gridMain, "interestPaid1", 9);
            G1.SetColumnPosition(gridMain, "trust100P", 10);
            G1.SetColumnPosition(gridMain, "trust85P", 11);
            G1.SetColumnPosition(gridMain, "retained", 12);
            G1.SetColumnPosition(gridMain, "debitAdjustment", 13);
            G1.SetColumnPosition(gridMain, "creditAdjustment", 14);
            G1.SetColumnPosition(gridMain, "dbr", 14);
        }
        /****************************************************************************************/
        private void SetPayments1()
        {
            gridMain.Columns["customer"].Visible = false;
            G1.SetColumnPosition(gridMain, "lastName", 4);
            G1.SetColumnPosition(gridMain, "firstName", 5);
            G1.SetColumnPosition(gridMain, "payDate8", 6);
            G1.SetColumnPosition(gridMain, "paymentAmount", 7);
            G1.SetColumnPosition(gridMain, "ccFee", 8);
            G1.SetColumnPosition(gridMain, "interestPaid1", 9);
            G1.SetColumnPosition(gridMain, "trust100P", 10);
            G1.SetColumnPosition(gridMain, "trust85P", 11);
            G1.SetColumnPosition(gridMain, "retained", 12);
            G1.SetColumnPosition(gridMain, "debitAdjustment", 13);
            G1.SetColumnPosition(gridMain, "creditAdjustment", 14);
        }
        /****************************************************************************************/
        private void SetTrustEomReport()
        {
            gridMain.Columns["customer"].Visible = false;
            G1.SetColumnPosition(gridMain, "lastName", 4);
            G1.SetColumnPosition(gridMain, "firstName", 5);
            G1.SetColumnPosition(gridMain, "payDate8", 6);
            G1.SetColumnPosition(gridMain, "dbr", 7);
            G1.SetColumnPosition(gridMain, "newBusiness", 8);
            G1.SetColumnPosition(gridMain, "paymentAmount", 9);
            G1.SetColumnPosition(gridMain, "ccFee", 10);
            G1.SetColumnPosition(gridMain, "interestPaid1", 11);
            G1.SetColumnPosition(gridMain, "trust100P", 12);
            G1.SetColumnPosition(gridMain, "trust85P", 13);
            G1.SetColumnPosition(gridMain, "retained", 14);
            G1.SetColumnPosition(gridMain, "LiInterest", 15);
            G1.SetColumnPosition(gridMain, "debitAdjustment", 16);
            G1.SetColumnPosition(gridMain, "creditAdjustment", 17);
            gridMain.Columns["downPayment1"].Visible = false;
        }
        /****************************************************************************************/
        private void SetAll()
        {
            gridMain.Columns["customer"].Visible = false;
            G1.SetColumnPosition(gridMain, "lastName", 4);
            G1.SetColumnPosition(gridMain, "firstName", 5);
            G1.SetColumnPosition(gridMain, "payDate8", 6);
            G1.SetColumnPosition(gridMain, "S", 7);
            G1.SetColumnPosition(gridMain, "downPayment", 8);
            G1.SetColumnPosition(gridMain, "downPayment1", 8);
            G1.SetColumnPosition(gridMain, "paymentAmount", 9);
            G1.SetColumnPosition(gridMain, "ccFee", 10);
            G1.SetColumnPosition(gridMain, "interestPaid1", 11);
            G1.SetColumnPosition(gridMain, "trust100P", 12);
            G1.SetColumnPosition(gridMain, "trust85P", 13);
            G1.SetColumnPosition(gridMain, "retained", 14);
            G1.SetColumnPosition(gridMain, "LiInterest", 15);
            G1.SetColumnPosition(gridMain, "debitAdjustment", 16);
            G1.SetColumnPosition(gridMain, "creditAdjustment", 17);

            gridMain.Columns["downPayment1"].Visible = false;
        }
        /****************************************************************************************/
        private void SetInsuranceWeeklyReport()
        {
            label6.Text = "Users :";
            cmbDateType.Show();
            ClearAllPositions(gridMain);
            gridMain.Columns["customer"].Visible = false;
            G1.SetColumnPosition(gridMain, "num", 1);
            G1.SetColumnPosition(gridMain, "contractNumber", 2);
            G1.SetColumnPosition(gridMain, "payer", 3);
            G1.SetColumnPosition(gridMain, "lastName", 4);
            G1.SetColumnPosition(gridMain, "firstName", 5);
            G1.SetColumnPosition(gridMain, "numMonths", 6);
            G1.SetColumnPosition(gridMain, "payDate8", 7);
            G1.SetColumnPosition(gridMain, "paymentAmount", 8);
            G1.SetColumnPosition(gridMain, "debitAdjustment", 9);
            G1.SetColumnPosition(gridMain, "creditAdjustment", 10);
            G1.SetColumnPosition(gridMain, "ccFee", 11 );
            G1.SetColumnPosition(gridMain, "transactionCode", 12);
            G1.SetColumnPosition(gridMain, "location", 13);
            G1.SetColumnPosition(gridMain, "depositNumber", 14);
            //this.dateTimePicker3.Visible = false;
            //this.dateTimePicker4.Visible = false;

            HideSecondSet();
        }
        /*******************************************************************************************/
        private void HideSecondSet ()
        {
            cmbDateType.Hide();
            dateTimePicker3.Hide();
            dateTimePicker4.Hide();
            lblAllOther.Hide();
            lblAllOtherTo.Hide();
            chkACH.Hide();
        }
        /*******************************************************************************************/
        private string getTrustQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboTrust.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `trust` IN (" + procLoc + ") " : "";
        }
        /*******************************************************************************************/
        private string getLocationQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboLocation.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `loc` IN (" + procLoc + ") " : "";
        }
        /*******************************************************************************************/
        private string getLocationNameQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboLocNames.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    string cmd = "Select * from `funeralhomes` where `name` = '" + locIDs[i].Trim() + "';";
                    DataTable dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        string id = dt.Rows[0]["keycode"].ObjToString();
                        procLoc += "'" + id.Trim() + "'";
                    }
                    else
                    {
                        cmd = "Select * from `cemeteries` where `description` = '" + locIDs[i].Trim() + "';";
                        dt = G1.get_db_data(cmd);
                        if (dt.Rows.Count > 0)
                        {
                            string id = dt.Rows[0]["loc"].ObjToString();
                            procLoc += "'" + id.Trim() + "'";
                        }
                    }
                }
            }
            return procLoc.Length > 0 ? " `loc` IN (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void chkComboTrust_EditValueChanged(object sender, EventArgs e)
        {
            string locationFilter = this.chkComboLocation.Text.Trim();
            string trustFilter = this.chkComboTrust.Text.Trim();
            string locNamesFilter = this.chkComboLocNames.Text.Trim();

            string names = getTrustQuery();

            if (!String.IsNullOrWhiteSpace(locationFilter))
            {
                string names2 = getLocationQuery();
                if (!String.IsNullOrWhiteSpace(names))
                    names += " AND ";
                names += names2;
            }

            if (!String.IsNullOrWhiteSpace(locNamesFilter))
            {
                string names2 = getLocationNameQuery();
                if (!String.IsNullOrWhiteSpace(names))
                    names += " AND ";
                names += names2;
            }

            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /****************************************************************************************/
        private void chkComboLocation_EditValueChanged(object sender, EventArgs e)
        {
            string locationFilter = this.chkComboLocation.Text.Trim();
            string trustFilter = this.chkComboTrust.Text.Trim();
            string locNamesFilter = this.chkComboLocNames.Text.Trim();

            if (!String.IsNullOrWhiteSpace(trustFilter))
            {
            }
            string names = getLocationQuery();

            if (!String.IsNullOrWhiteSpace(trustFilter))
            {
                string names2 = getTrustQuery();
                if (!String.IsNullOrWhiteSpace(names))
                    names += " AND ";
                names += names2;
            }

            if (!String.IsNullOrWhiteSpace(locNamesFilter))
            {
                string names2 = getLocationNameQuery();
                if (!String.IsNullOrWhiteSpace(names))
                    names += " AND ";
                names += names2;
            }

            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.gridMain.ExpandAllGroups();
        }
        /****************************************************************************************/
        private void chkComboLocNames_EditValueChanged(object sender, EventArgs e)
        {
            string locationFilter = this.chkComboLocation.Text.Trim();
            string trustFilter = this.chkComboTrust.Text.Trim();
            string locNamesFilter = this.chkComboLocNames.Text.Trim();

            string names = getLocationNameQuery();

            if (!String.IsNullOrWhiteSpace(trustFilter))
            {
                string names2 = getTrustQuery();
                if (!String.IsNullOrWhiteSpace(names))
                    names += " AND ";
                names += names2;
            }

            if (!String.IsNullOrWhiteSpace(locationFilter))
            {
                string names2 = getLocationQuery();
                if (!String.IsNullOrWhiteSpace(names))
                    names += " AND ";
                names += names2;
            }

            if (!String.IsNullOrWhiteSpace(names))
            {
                DataRow[] dRows = originalDt.Select(names);
                DataTable dt = originalDt.Clone();
                for (int i = 0; i < dRows.Length; i++)
                    dt.ImportRow(dRows[i]);
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;
                this.gridMain.ExpandAllGroups();
            }
        }
        /****************************************************************************************/
        private void chkSortByTrust_CheckedChanged(object sender, EventArgs e)
        {
            if (dgv.DataSource == null)
                return;
            if ( insurance )
            {
                InsuranceSortByUser();
                return;
            }
            DataTable dt = (DataTable)dgv.DataSource;
            if (chkSortByTrust.Checked)
            {
                DataView tempview = originalDt.DefaultView;
                tempview.Sort = "Location Name, trust, lastName, firstName";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

                gridMain.Columns["Location Name"].GroupIndex = 0;
                gridMain.Columns["trust"].GroupIndex = 1;
                gridMain.Columns["customer"].Visible = false;
                gridMain.Columns["paymentAmount"].Visible = false;
                gridMain.Columns["ccFee"].Visible = false;
                gridMain.Columns["interestPaid1"].Visible = false;
                gridMain.Columns["retained"].Visible = false;
                gridMain.Columns["debitAdjustment"].Visible = false;
                gridMain.Columns["creditAdjustment"].Visible = false;
                gridMain.Columns["payDate8"].Visible = false;
                gridMain.Columns["lastName"].Visible = true;
                gridMain.Columns["firstName"].Visible = true;
                gridMain.OptionsView.ShowFooter = true;
                this.gridMain.ExpandAllGroups();
            }
            else
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "Location Name, lastName";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

                gridMain.Columns["trust"].GroupIndex = -1;
                gridMain.Columns["Location Name"].GroupIndex = -1;
                gridMain.Columns["customer"].Visible = true;
                gridMain.Columns["paymentAmount"].Visible = true;
                gridMain.Columns["ccFee"].Visible = true;
                gridMain.Columns["interestPaid1"].Visible = true;
                gridMain.Columns["retained"].Visible = true;
                gridMain.Columns["debitAdjustment"].Visible = true;
                gridMain.Columns["creditAdjustment"].Visible = true;
                gridMain.Columns["payDate8"].Visible = true;
                gridMain.Columns["lastName"].Visible = false;
                gridMain.Columns["firstName"].Visible = false;
                gridMain.OptionsView.ShowFooter = true;
                gridMain.CollapseAllGroups();
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /*******************************************************************************************/
        private void SetSinglePremiums(DataTable dt)
        {
            if (chkPayments.Checked)
                return;
            double financed = 0D;
            double allowInsurance = 0D;
            double allowMerchandise = 0D;
            string contract = "";
            string trust = "";
            string loc = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    financed = DailyHistory.GetFinanceValue(dt.Rows[i]);
                    contract = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                    contract = Trust85.decodeContractNumber(contract, ref trust, ref loc);
                    //                    dt.Rows[i]["trust"] = trust;

                    if (String.IsNullOrWhiteSpace(trust))
                        continue;
                    trust = trust.ToUpper();
                    allowInsurance = dt.Rows[i]["allowInsurance"].ObjToDouble();
                    allowMerchandise = dt.Rows[i]["allowMerchandise"].ObjToDouble();
                    if (financed == 0D)
                    {
                        dt.Rows[i]["S"] = 1D;
                        dt.Rows[i]["trust100P"] = 0D;
                        dt.Rows[i]["trust85P"] = 0D;
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
        /*******************************************************************************************/
        public static string getTrusts(DataTable dt, bool insurance = false )
        {
            if (G1.get_column_number(dt, "trust") < 0)
                dt.Columns.Add("trust");

            string trusts = "";
            string trust = "";
            string loc = "";
            string contract = "";
            if (G1.get_column_number(dt, "trust") < 0)
                return "";

            DataTable dx = new DataTable();
            dx.Columns.Add("trust");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contract = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                contract = Trust85.decodeContractNumber(contract, ref trust, ref loc);
                if (insurance)
                    trust = dt.Rows[i]["userId"].ObjToString();
                dt.Rows[i]["trust"] = trust;

                if (String.IsNullOrWhiteSpace(trust))
                    continue;
                trust = trust.ToUpper();
                DataRow[] dRows = dx.Select("trust='" + trust + "'");
                if (dRows.Length <= 0)
                {
                    DataRow dR = dx.NewRow();
                    dR["trust"] = trust;
                    dx.Rows.Add(dR);
                }
            }
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                trust = dx.Rows[i]["trust"].ObjToString();
                trusts += trust + ",";
            }
            trusts = trusts.TrimEnd(',');
            return trusts;
        }
        /****************************************************************************************/
        private void LoadTrusts(DataTable dt)
        {
            DataTable trustDt = new DataTable();
            trustDt.Columns.Add("trusts");
            string c = "";

            string trusts = getTrusts(dt, insurance );
            string[] Lines = trusts.Split(',');
            for (int i = 0; i < Lines.Length; i++)
            {
                c = Lines[i].Trim();
                if (String.IsNullOrWhiteSpace(c))
                    continue;
                DataRow dRow = trustDt.NewRow();
                dRow["trusts"] = c;
                trustDt.Rows.Add(dRow);
            }
            chkComboTrust.Properties.DataSource = trustDt;
        }
        /****************************************************************************************/
        private void chkShowAll_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            loading = true;
            chkActiveOnly.Checked = false;
            chkDownPayments.Checked = false;
            chkBalanceLessPayment.Checked = false;
            chkBalances.Checked = false;
            chkDeaths.Checked = false;

            ClearAllPositions();
            SetDefaultPositions();
            SetAll();

            chkPayments.Checked = true;
            chkDownPayments.Checked = true;

            ResetColumns();

            loading = false;
        }
        /****************************************************************************************/
        private void chkAgent_CheckedChanged(object sender, EventArgs e)
        {
            if (dgv.DataSource == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (chkAgent.Checked)
            {
                DataView tempview = originalDt.DefaultView;
                tempview.Sort = "agentNumber, contractNumber, lastName, firstName";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

                gridMain.Columns["agentNumber"].GroupIndex = 0;
                gridMain.Columns["Location Name"].GroupIndex = -1;
                gridMain.Columns["trust"].GroupIndex = -1;
                //gridMain.Columns["customer"].Visible = false;
                //gridMain.Columns["paymentAmount"].Visible = false;
                //gridMain.Columns["interestPaid1"].Visible = false;
                //gridMain.Columns["retained"].Visible = false;
                //gridMain.Columns["debitAdjustment"].Visible = false;
                //gridMain.Columns["creditAdjustment"].Visible = false;
                //gridMain.Columns["payDate8"].Visible = false;
                //gridMain.Columns["lastName"].Visible = true;
                //gridMain.Columns["firstName"].Visible = true;
                gridMain.OptionsView.ShowFooter = true;
                this.gridMain.ExpandAllGroups();
            }
            else
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "Location Name, lastName";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

                gridMain.Columns["agentNumber"].GroupIndex = -1;
                gridMain.Columns["trust"].GroupIndex = -1;
                gridMain.Columns["Location Name"].GroupIndex = -1;
                //gridMain.Columns["customer"].Visible = true;
                //gridMain.Columns["paymentAmount"].Visible = true;
                //gridMain.Columns["interestPaid1"].Visible = true;
                //gridMain.Columns["retained"].Visible = true;
                //gridMain.Columns["debitAdjustment"].Visible = true;
                //gridMain.Columns["creditAdjustment"].Visible = true;
                //gridMain.Columns["payDate8"].Visible = true;
                //gridMain.Columns["lastName"].Visible = false;
                //gridMain.Columns["firstName"].Visible = false;
                gridMain.OptionsView.ShowFooter = true;
                gridMain.CollapseAllGroups();
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void btnSelectColumns_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;
            SelectColumns sform = new SelectColumns(dgv, "Payments Report", "Primary", actualName);
            sform.Done += new SelectColumns.d_void_eventdone(sform_Done);
            sform.Show();
        }
        /***********************************************************************************************/
        void sform_Done()
        {
            dgv.Refresh();
            this.Refresh();
        }
        /****************************************************************************************/
        private void cmbSelectColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetupSelectedColumns();
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns()
        {
            string group = cmbSelectColumns.Text.Trim().ToUpper();
            if (group.Trim().Length == 0)
                return;
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = 'Payments Report' order by seq";
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
        /****************************************************************************************/
        private void chkShowAllDeceased_CheckedChanged(object sender, EventArgs e)
        {
            btnRun_Click(null, null);
        }
        /****************************************************************************************/
        private void chkCollapse_CheckedChanged(object sender, EventArgs e)
        {
            if (dgv.Visible)
                CollapeReport(gridMain);
            else if (dgv7.Visible)
                CollapeReport(gridMain7);
            else if (dgv8.Visible)
                CollapeReport(gridMain8);
            else if (dgv13.Visible)
                CollapeReport(gridMain13);
        }
        /****************************************************************************************/
        private void CollapeReport (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain )
        {
            if (!chkCollapse.Checked)
            {
                gridMain.OptionsBehavior.AutoExpandAllGroups = true;
                gridMain.ExpandAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = true;
                gridMain.OptionsPrint.PrintGroupFooter = true;
            }
            else
            {
                gridMain.OptionsBehavior.AutoExpandAllGroups = false;
                gridMain.CollapseAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = false;
                gridMain.OptionsPrint.PrintGroupFooter = true;
            }
        }
        /****************************************************************************************/
        private int activeColumnIndex = -1;
        private string cellValue = "";
        private int MagRow = -1;
        private int CashRow = -1;
        private int StateRow = -1;
        private int TotalRow = -1;
        private void gridMain4_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            activeColumnIndex = e.Column.VisibleIndex;
            string columnName = e.Column.FieldName.ObjToString();
            cellValue = e.Value.ObjToString();
            int row = gridMain4.FocusedRowHandle;
            DataTable dt = (DataTable)dgv4.DataSource;
            string edit = dt.Rows[row]["edit"].ObjToString();
            if (edit != "Y")
                return;
        }
        /****************************************************************************************/
        private void gridMain4_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            activeColumnIndex = e.Column.VisibleIndex;
            string columnName = e.Column.FieldName.ObjToString();
            cellValue = e.Value.ObjToString();
            int row = gridMain4.FocusedRowHandle;
            DataTable dt = (DataTable)dgv4.DataSource;
            string edit = dt.Rows[row]["edit"].ObjToString();
            if (edit != "Y")
                return;
            dt.Rows[row][columnName] = cellValue;
            if (e.RowHandle < CashRow)
            {

            }
            else if (e.RowHandle < MagRow)
            {

            }
        }
        /****************************************************************************************/
        private void gridMain4_ValidateRow(object sender, DevExpress.XtraGrid.Views.Base.ValidateRowEventArgs e)
        {
            GridView view = sender as GridView;
            int row = gridMain4.FocusedRowHandle;
            DataTable dt = (DataTable)dgv4.DataSource;
            string edit = dt.Rows[row]["edit"].ObjToString();
            //if (edit != "Y")
            //{
            //    view.SetColumnError(null, "Invalid data");
            //    e.Valid = false;
            //    return;
            //}
        }
        /****************************************************************************************/
        private void gridMain4_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            int row = gridMain4.FocusedRowHandle;
            row = gridMain4.GetDataSourceRowIndex(row);
            DataTable dt = (DataTable)dgv4.DataSource;
            string edit = dt.Rows[row]["edit"].ObjToString();
            if (edit != "Y")
            {
                e.Valid = false;
                e.ErrorText = "This Row May Not Be Edited!";
                return;
            }
            string field = view.FocusedColumn.FieldName.ToUpper();
            if ( field == "ADJUSTEDPAYMENTS")
            {
                e.Valid = false;
                e.ErrorText = "This Column May Not Be Edited!";
                return;
            }
            if (field == "NBDP" || field == "DEBITADJUSTMENT" || field == "CREDITADJUSTMENT")
            {
                double price = 0;
                if (!Double.TryParse(e.Value as String, out price))
                {
                    e.Valid = false;
                    e.ErrorText = "Only numeric values are accepted.";
                }
                dt.Rows[row][field] = price;

                string location = "";
                double totalPayments = 0D;
                double totalDebits = 0D;
                double totalCredits = 0D;
                double totalAdjusted = 0D;
                double payment = dt.Rows[row]["NBDP"].ObjToDouble();
                double debit = dt.Rows[row]["debitAdjustment"].ObjToDouble();
                double credit = dt.Rows[row]["creditAdjustment"].ObjToDouble();
                double adjustedPayment = payment - debit + credit;
                dt.Rows[row]["adjustedPayments"] = adjustedPayment;
                if (row < CashRow)
                {
                    for ( int i=StateRow; i<CashRow; i++)
                    {
                        location = dt.Rows[i]["location"].ObjToString();
                        if (location.Trim().ToUpper() == "TOTALS")
                        {
                            dt.Rows[i]["NBDP"] = totalPayments;
                            dt.Rows[i]["debitAdjustment"] = totalDebits;
                            dt.Rows[i]["creditAdjustment"] = totalCredits;
                            dt.Rows[i]["adjustedPayments"] = totalAdjusted;

                            break;
                        }
                        payment = dt.Rows[i]["NBDP"].ObjToDouble();
                        debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                        credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                        adjustedPayment = payment - debit + credit;
                        dt.Rows[i]["adjustedPayments"] = adjustedPayment;
                        totalPayments += payment;
                        totalDebits += debit;
                        totalCredits += credit;
                        totalAdjusted += adjustedPayment;
                    }
                }
                else if (row < MagRow)
                {
                    for (int i = CashRow; i < MagRow; i++)
                    {
                        location = dt.Rows[i]["location"].ObjToString();
                        if (location.Trim().ToUpper() == "TOTALS")
                        {
                            dt.Rows[i]["NBDP"] = totalPayments;
                            dt.Rows[i]["debitAdjustment"] = totalDebits;
                            dt.Rows[i]["creditAdjustment"] = totalCredits;
                            dt.Rows[i]["adjustedPayments"] = totalAdjusted;

                            break;
                        }
                        payment = dt.Rows[i]["NBDP"].ObjToDouble();
                        debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                        credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                        adjustedPayment = payment - debit + credit;
                        dt.Rows[i]["adjustedPayments"] = adjustedPayment;
                        totalPayments += payment;
                        totalDebits += debit;
                        totalCredits += credit;
                        totalAdjusted += adjustedPayment;
                    }
                }
                else if (row < TotalRow)
                {
                    for (int i = MagRow; i < TotalRow; i++)
                    {
                        location = dt.Rows[i]["location"].ObjToString();
                        if (location.Trim().ToUpper() == "TOTALS")
                        {
                            dt.Rows[i]["NBDP"] = totalPayments;
                            dt.Rows[i]["debitAdjustment"] = totalDebits;
                            dt.Rows[i]["creditAdjustment"] = totalCredits;
                            dt.Rows[i]["adjustedPayments"] = totalAdjusted;

                            break;
                        }
                        payment = dt.Rows[i]["NBDP"].ObjToDouble();
                        debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                        credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                        adjustedPayment = payment - debit + credit;
                        dt.Rows[i]["adjustedPayments"] = adjustedPayment;
                        totalPayments += payment;
                        totalDebits += debit;
                        totalCredits += credit;
                        totalAdjusted += adjustedPayment;
                    }
                }

                //else if (price <= 0)
                //{
                //    e.Valid = false;
                //    e.ErrorText = "The unit price must be positive.";
                //}
            }
        }
        /****************************************************************************************/
        private void pictureAdd_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv4.DataSource;
            int lines = 1;
            for (int i = 0; i < lines; i++)
            {
                DataRow dRow = dt.NewRow();
                dRow["num"] = dt.Rows.Count.ObjToInt32();
                dRow["edit"] = "Y";
                dt.Rows.Add(dRow);
            }
            dgv4.DataSource = dt;

            int row = gridMain.FocusedRowHandle;
            gridMain.UnselectRow(row);

            row = dt.Rows.Count - 1;
            gridMain4.SelectRow(row);
            gridMain4.FocusedRowHandle = row;
            dgv4.RefreshDataSource();
            dgv4.Refresh();
        }
        /****************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain4.GetFocusedDataRow();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to delete this Row ?", "Delete Row Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            DataTable dt = (DataTable)dgv4.DataSource;
            int[] rows = gridMain4.GetSelectedRows();
            int dtRow = 0;
            int firstRow = 0;
            if (rows.Length > 0)
                firstRow = rows[0];
            int row = 0;
            string edit = "";
            string location = "";
            try
            {
                loading = true;
                for (int i = 0; i < rows.Length; i++)
                {
                    row = rows[i];
                    dtRow = gridMain4.GetDataSourceRowIndex(row);
                    if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
                    {
                        continue;
                    }
                    location = dt.Rows[dtRow]["location"].ObjToString().Trim().ToUpper();
                    if (location == "TOTALS" || location == "CASH-LOCAL BANK")
                        continue;
                    if (location == "LOCKBOX TOTAL" || location == "STATE BANK REMOTE")
                        continue;
                    if (location == "TOTALS MAGNOLIA STATE BANK REMOTE AND CASH DEPOSITS")
                        continue;
                    edit = dt.Rows[dtRow]["edit"].ObjToString();
                    if (edit.ToUpper() != "Y")
                        continue;
                    if (dtRow < StateRow)
                    {
                        StateRow--;
                        CashRow--;
                        MagRow--;
                        TotalRow--;
                    }
                    else if (dtRow < CashRow)
                    {
                        CashRow--;
                        MagRow--;
                        TotalRow--;
                    }
                    else if (dtRow < MagRow)
                    {
                        MagRow--;
                        TotalRow--;
                    }
                    else if (dtRow < TotalRow)
                        TotalRow--;
                    dt.Rows.RemoveAt(dtRow);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("*ERROR*** " + ex.Message.ToString());
            }

            //            gridMain.FocusedRowHandle = firstRow;
            loading = false;
            if (firstRow > (dt.Rows.Count - 1))
                firstRow = (dt.Rows.Count - 1);
            dgv4.DataSource = dt;
            gridMain4.RefreshData();
            dgv4.Refresh();

            gridMain4.FocusedRowHandle = firstRow;
            gridMain4.SelectRow(firstRow);
        }
        /****************************************************************************************/
        private void btnInsert_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv4.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain4.GetFocusedDataRow();
            int rowHandle = gridMain4.FocusedRowHandle;
            int dtRow = gridMain4.GetDataSourceRowIndex(rowHandle);
            if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
                return;
            //if (rowHandle == (dt.Rows.Count - 1))
            //    return; // Already at the last row
            if ( dtRow <= StateRow )
            {
                MessageBox.Show("***ERROR*** Cannot Insert Prior to State Bank Row!");
                return;
            }
            if (dtRow < TotalRow)
                TotalRow++;
            if (dtRow < MagRow)
                MagRow++;
            if (dtRow < CashRow)
                CashRow++;
            DataRow dRow = dt.NewRow();
            dRow["edit"] = "Y";
            dt.Rows.InsertAt(dRow, dtRow);
            G1.NumberDataTable(dt);
            dt.AcceptChanges();
            dgv4.DataSource = dt;
            gridMain4.ClearSelection();
            gridMain4.RefreshData();
            gridMain4.FocusedRowHandle = rowHandle + 1;
            gridMain4.SelectRow(rowHandle + 1);
            dgv4.Refresh();
        }
        /****************************************************************************************/
        private void setAsDBRToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
        private void resetDBRToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void SetupDBRColumn()
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            //string filename = "";
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    filename = dt.Rows[i]["agreementFile"].ObjToString();
            //    if (!String.IsNullOrWhiteSpace(filename))
            //        dt.Rows[i]["agreement"] = "1";
            //    else
            //        dt.Rows[i]["agreement"] = "0";
            //    dt.Rows[i]["select"] = "0";
            //}
        }
        /***********************************************************************************************/
        private void SetupSelectColumn( DataTable dt = null )
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";

            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew8 = this.repositoryItemCheckEdit8;
            selectnew8.NullText = "";
            selectnew8.ValueChecked = "1";
            selectnew8.ValueUnchecked = "0";
            selectnew8.ValueGrayed = "";

            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew9 = this.repositoryItemCheckEdit9;
            selectnew9.NullText = "";
            selectnew9.ValueChecked = "1";
            selectnew9.ValueUnchecked = "0";
            selectnew9.ValueGrayed = "";

            if ( dt != null)
            {
                string set = "";
                if (G1.get_column_number(dt, "select") < 0)
                    dt.Columns.Add("select");
                if (G1.get_column_number(dt, "letter") < 0)
                    dt.Columns.Add("letter");
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["select"] = "0";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    set = dt.Rows[i]["letter"].ObjToString();
                    if (set != "1")
                        set = "0";
                    dt.Rows[i]["letter"] = set;
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    set = dt.Rows[i]["pulled"].ObjToString();
                    if (set != "1")
                        set = "0";
                    dt.Rows[i]["pulled"] = set;
                }
            }
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if ( !String.IsNullOrWhiteSpace ( record ))
            {
                string set = dr["select"].ObjToString();
                if (set == "0")
                    dr["select"] = "1";
                else
                    dr["select"] = "0";
            }
        }
        /****************************************************************************************/
        private void gridMain4_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (1 == 1)
                return;
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender;
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                string location = View.GetRowCellDisplayText(e.RowHandle, View.Columns["location"]);
                if (gridMain4_CheckForBold(location))
                {
                    Font f = e.BrickGraphics.DefaultBrickStyle.Font;
                    string name = f.Name.ObjToString();
                    Font font = new Font(name, f.Size, FontStyle.Bold & FontStyle.Italic);
                    //                    e.BrickGraphics.DefaultBrickStyle.Font = font;

                    float x = e.X;
                    float y = e.Y;
                    float height = f.Height;

                    RectangleF rect = new RectangleF(x + 35, y - height - 4, 232, height + 5);
                    Brush brush = new SolidBrush(Color.Black);

                    TextBrick textBrick = Printer.localE.Graph.DrawString(location, rect);
                    textBrick.Font = new Font(name, font.Size, FontStyle.Bold & FontStyle.Italic);
                    textBrick.HorzAlignment = HorzAlignment.Near;
                    textBrick.VertAlignment = VertAlignment.Center;
                    //                    textBrick.Style = new BrickStyle(BorderSide.All, 5F, Color.Red, Color.White, Color.Black, font, null);
                    textBrick.BorderWidth = 1;
                    e.BrickGraphics.DefaultBrickStyle.Font = font;
                    e.BrickGraphics.DrawBrick(textBrick, rect);
                }
            }
        }
        /****************************************************************************************/
        private bool gridMain4_CheckForBold(string text)
        {
            if (text.ToUpper().IndexOf("TOTAL") >= 0)
                return true;
            if (text.ToUpper().IndexOf("STATE BANK REMOTE") >= 0)
                return true;
            if (text.ToUpper().IndexOf("CASH-LOCAL BANK") >= 0)
                return true;
            if (text.ToUpper().IndexOf("MAGNOLIA STATE BANK") >= 0)
                return true;
            return false;
        }
        /****************************************************************************************/
        private void gridMain4_RowStyle(object sender, RowStyleEventArgs e)
        {
            //GridView View = sender as GridView;
            //if (e.RowHandle >= 0)
            //{
            //    string location = View.GetRowCellDisplayText(e.RowHandle, View.Columns["location"]);
            //    if (gridMain4_CheckForBold(location))
            //    {
            //        e.Appearance.BackColor = Color.Salmon;
            //        e.Appearance.BackColor2 = Color.SeaShell;
            //        Font f = e.Appearance.Font;
            //        string name = f.Name.ObjToString();
            //        Font font = new Font(name, e.Appearance.Font.Size, FontStyle.Bold);
            //        e.Appearance.Font = font;
            //        e.HighPriority = true;
            //    }
            //}
        }
        /****************************************************************************************/
        private string GetNewLocation ( string location )
        {
            string newLocation = location;
            DataTable dt = (DataTable)chkComboLocNames.Properties.DataSource;

            DataRow[] dRows = dt.Select("name='" + location + "'");
            if (dRows.Length > 0)
            {
                newLocation = dRows[0]["cashRemitHeading"].ObjToString();
                if (String.IsNullOrWhiteSpace(newLocation))
                    newLocation = location;
            }
            return newLocation;
        }
        /****************************************************************************************/
        private void ReLoadLocations(DataTable dt)
        {
            string location = "";
            string newLocation = "";
            DataTable dx = (DataTable)chkComboLocNames.Properties.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                location = dt.Rows[i]["Location Name"].ObjToString();
                DataRow[] dRows = dx.Select("Name='" + location + "'");
                if (dRows.Length > 0)
                {
                    newLocation = dRows[0]["cashRemitHeading"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(newLocation))
                        dt.Rows[i]["Location Name"] = newLocation;
                }
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                location = dt.Rows[i]["Location Name"].ObjToString();
                if ( String.IsNullOrWhiteSpace ( location))
                {

                }
            }
        }
        /****************************************************************************************/
        private DataTable CalculateDiscards ( DataTable dt )
        {
            DataTable trickDt = new DataTable();
            trickDt.Columns.Add("contractNumber");
            trickDt.Columns.Add("discard");
            trickDt.Columns.Add("row", Type.GetType("System.Double"));
            trickDt.Columns.Add("newBusiness", Type.GetType("System.Double"));
            trickDt.Columns.Add("paymentAmount", Type.GetType("System.Double"));
            trickDt.Columns.Add("debitAdjustment", Type.GetType("System.Double"));
            trickDt.Columns.Add("creditAdjustment", Type.GetType("System.Double"));
            trickDt.Columns.Add("trust85P", Type.GetType("System.Double"));

            string contractNumber = "";
            string discard = "";
            double row = 0D;
            double newBusiness = 0D;
            double paymentAmount = 0D;
            double debitAdjustment = 0D;
            double creditAdjustment = 0D;
            double totalPayment = 0D;
            bool add = false;
            DateTime deceasedDate = DateTime.Now;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                if (contractNumber == "B18017LI")
                {

                }

                add = false;
                if (dt.Rows[i]["SetAsDBR"].ObjToString() == "Y")
                    add = true;
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 1850)
                    add = true;
                if ( add )
                {
                    DataRow dRow = trickDt.NewRow();
                    dRow["contractNumber"] = contractNumber;
                    dRow["row"] = i.ObjToDouble();
                    dRow["newBusiness"] = dt.Rows[i]["newBusiness"].ObjToDouble();
                    dRow["paymentAmount"] = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    dRow["debitAdjustment"] = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    dRow["creditAdjustment"] = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    dRow["trust85P"] = dt.Rows[i]["trust85P"].ObjToDouble();
                    trickDt.Rows.Add(dRow);
                }
            }
            Trust85.FindContract(dt, "B18017LI");

            for ( int i=0; i<trickDt.Rows.Count; i++)
            {
                discard = trickDt.Rows[i]["discard"].ObjToString();
                if (discard == "Y")
                    continue;
                newBusiness = 0D;
                paymentAmount = 0D;
                debitAdjustment = 0D;
                creditAdjustment = 0D;
                contractNumber = trickDt.Rows[i]["contractNumber"].ObjToString().Trim();
                if ( contractNumber == "B18017LI")
                {

                }
                DataRow [] dRows = trickDt.Select("contractNumber='" + contractNumber +  "'");
                DataTable ddx = trickDt.Clone();
                G1.ConvertToTable(dRows, ddx);
                totalPayment = 0D;
                for ( int j=0; j<dRows.Length; j++)
                {
                    newBusiness = dRows[j]["newBusiness"].ObjToDouble();
                    paymentAmount = dRows[j]["paymentAmount"].ObjToDouble();
                    debitAdjustment = dRows[j]["debitAdjustment"].ObjToDouble();
                    creditAdjustment = dRows[j]["creditAdjustment"].ObjToDouble();
                    totalPayment += newBusiness + paymentAmount - debitAdjustment + creditAdjustment;
                }
                //if (totalPayment == 0D)
                //    trickDt.Rows[i]["discard"] = "Y";
            }
            Trust85.FindContract(dt, "B18017LI");
            CalcCancelDebits(dt, trickDt);
            return trickDt;
        }
        /****************************************************************************************/
        private void CalcCancelDebits(DataTable dt, DataTable trickDt)
        {
            string contractNumber = "";
            string discard = "";
            double row = 0D;
            double newBusiness = 0D;
            double paymentAmount = 0D;
            double debitAdjustment = 0D;
            double creditAdjustment = 0D;
            double totalPayment = 0D;
            bool add = false;
            DateTime deceasedDate = DateTime.Now;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                newBusiness = 0D;
                paymentAmount = 0D;
                debitAdjustment = 0D;
                creditAdjustment = 0D;
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                if (contractNumber == "B18017LI")
                {

                }
                DataRow[] dRows = dt.Select("contractNumber='" + contractNumber + "'");
                DataTable ddx = dt.Clone();
                G1.ConvertToTable(dRows, ddx);
                totalPayment = 0D;
                for (int j = 0; j < dRows.Length; j++)
                {
                    newBusiness = dRows[j]["newBusiness"].ObjToDouble();
                    paymentAmount = dRows[j]["paymentAmount"].ObjToDouble();
                    debitAdjustment = dRows[j]["debitAdjustment"].ObjToDouble();
                    creditAdjustment = dRows[j]["creditAdjustment"].ObjToDouble();
                    totalPayment += newBusiness + paymentAmount - debitAdjustment + creditAdjustment;
                }
                //if (totalPayment == 0D)
                //{
                //    DataRow dRow = trickDt.NewRow();
                //    dRow["contractNumber"] = contractNumber;
                //    dRow["row"] = i.ObjToDouble();
                //    dRow["newBusiness"] = dt.Rows[i]["newBusiness"].ObjToDouble();
                //    dRow["paymentAmount"] = dt.Rows[i]["paymentAmount"].ObjToDouble();
                //    dRow["debitAdjustment"] = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                //    dRow["creditAdjustment"] = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                //    dRow["trust85P"] = dt.Rows[i]["trust85P"].ObjToDouble();
                //    dRow["discard"] = "Y";
                //    trickDt.Rows.Add(dRow);
                //}
            }
            return;
        }
        /****************************************************************************************/
        private void CleanUpDebits ( DataTable dt, DataTable trickDt)
        {
            double paymentAmount = 0D;
            double debit = 0D;
            double credit = 0D;
            double total = 0D;
            string contractNumber = "";
            string discard = "";

            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                if (contractNumber == "P17903DI")
                {
                }

                DataRow[] dRows = trickDt.Select("contractNumber='" + contractNumber + "'");
                if (dRows.Length > 0)
                {
                    discard = dRows[0]["discard"].ObjToString().ToUpper();
                    if (discard == "Y")
                    {
                        dt.Rows.RemoveAt(i);
                        continue;
                    }
                }

                paymentAmount = dt.Rows[i]["paymentAmount"].ObjToDouble();
                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                total = paymentAmount - debit + credit;
                //if (total <= 0D)
                //    dt.Rows.RemoveAt(i);
            }
        }
        /****************************************************************************************/
        private void LoadCashRemitted(DataTable dx )
        {
            //double downPayment = 0D;
            //double ccFee = 0D;
            //for (int i = 0; i < dx.Rows.Count; i++)
            //{
            //    downPayment = dx.Rows[i]["newBusiness"].ObjToDouble();
            //    ccFee = dx.Rows[i]["ccFee"].ObjToDouble();
            //    if (downPayment != 0D)
            //        dx.Rows[i]["newBusiness"] = dx.Rows[i]["trust100P"].ObjToDouble() + ccFee;
            //}

            DataTable trickDt = CalculateDiscards(dx);
            //Trust85.FindContract(dx, "NNM2005");

            CleanUpDebits(dx, trickDt);
            //Trust85.FindContract(dx, "P17903DI");

            DataTable dt = dx.Copy();
            ReLoadLocations(dt);
            DataView tempview = dt.DefaultView;
            tempview.Sort = "Location Name";
            dt = tempview.ToTable();

            DataTable locationDt = new DataTable();
            locationDt.Columns.Add("num");
            locationDt.Columns.Add("loc");
            locationDt.Columns.Add("location");
            locationDt.Columns.Add("trust");
            locationDt.Columns.Add("Trust Name");
            locationDt.Columns.Add("comments");
            locationDt.Columns.Add("cancelled");
            locationDt.Columns.Add("dbr", Type.GetType("System.Double"));
            locationDt.Columns.Add("fdlicDownPayments", Type.GetType("System.Double"));
            locationDt.Columns.Add("fdlicMonthly", Type.GetType("System.Double"));
            locationDt.Columns.Add("unityMonthly", Type.GetType("System.Double"));
            locationDt.Columns.Add("bancorpsouthMonthly", Type.GetType("System.Double"));
            locationDt.Columns.Add("pbMonthly", Type.GetType("System.Double"));
            locationDt.Columns.Add("perpetualCare", Type.GetType("System.Double"));
            locationDt.Columns.Add("perpetualCareHC", Type.GetType("System.Double"));
            locationDt.Columns.Add("total", Type.GetType("System.Double"));
            locationDt.Columns.Add("group");
            locationDt.Columns.Add("depositNumber");
            locationDt.Columns.Add("payDate8");
            locationDt.Columns.Add("bankName");
            locationDt.Columns.Add("purchaserName");

            string runOn = cmbRunOn.Text.Trim().ToUpper();
            if ( runOn == "TRUSTS" )
            {
                gridMain6.Columns["fdlicDownPayments"].Visible = true;
                gridMain6.Columns["fdlicMonthly"].Visible = true;
                gridMain6.Columns["unityMonthly"].Visible = true;
                gridMain6.Columns["bancorpsouthMonthly"].Visible = true;
                gridMain6.Columns["pbMonthly"].Visible = true;
                gridMain6.Columns["perpetualCare"].Visible = false;
                gridMain6.Columns["perpetualCareHC"].Visible = false;
                gridMain6.Columns["depositNumber"].Visible = false;
                gridMain6.Columns["payDate8"].Visible = false;
                gridMain6.Columns["bankName"].Visible = false;
                gridMain6.Columns["purchaserName"].Visible = false;
            }
            else
            {
                gridMain6.Columns["fdlicDownPayments"].Visible = false;
                gridMain6.Columns["fdlicMonthly"].Visible = false;
                gridMain6.Columns["unityMonthly"].Visible = false;
                gridMain6.Columns["bancorpsouthMonthly"].Visible = false;
                gridMain6.Columns["pbMonthly"].Visible = false;
                gridMain6.Columns["perpetualCare"].Visible = true;
                gridMain6.Columns["perpetualCareHC"].Visible = true;
                gridMain6.Columns["depositNumber"].Visible = true;
                gridMain6.Columns["payDate8"].Visible = true;
                gridMain6.Columns["bankName"].Visible = true;
                gridMain6.Columns["purchaserName"].Visible = true;

                AddSummaryColumn("perpetualCareHC", gridMain6);
            }

            string saveLocation = "";
            string location = "";
            string trust = "";
            string trustName = "";
            string saveTrustName = "";
            double newBusiness = 0D;
            double totalNewBusiness = 0D;
            double locationNewBusiness = 0D;
            double totalPayments = 0D;
            double debit = 0D;
            double credit = 0D;
            double interest = 0D;
            double dValue = 0D;
            double downPayment1 = 0D;
            double trust85 = 0D;

            double fdlicDownPayments = 0D;
            double fdlicMonthly = 0D;
            double unityMonthly = 0D;
            double bancorpsouthMonthly = 0D;
            double pbMonthly = 0D;
            double perpetualCare = 0D;
            double total = 0D;

            DateTime deceasedDate;
            DateTime payDate8;

            double TotalfdlicDownPayments = 0D;
            double TotalfdlicMonthly = 0D;
            double TotalunityMonthly = 0D;
            double TotalbancorpsouthMonthly = 0D;
            double TotalpbMonthly = 0D;
            double TotalperpetualCare = 0D;
            double Totaltotal = 0D;

            string dbrContracts = "";
            double totalDBR = 0D;
            string lastName = "";
            string firstName = "";
            string str = "";
            string contractNumber = "";
            string discard = "";
            double totalTrust85 = 0D;
            double totalEliminated = 0D;
            string creditReason = "";
            bool tca = false;

            DataTable dddx = null;

            DataRow[] dRowsx = dt.Select("contractNumber LIKE 'NNM%'");
            if (dRowsx.Length > 0)
            {
                dddx = dRowsx.CopyToDataTable();
            }

//            Trust85.FindContract(dt, "E15025UI", true);

            try
            {
                if ( runOn.ToUpper() == "CEMETERIES")
                {
                    dRowsx = dt.Select("edited='Cemetery'");
                    if (dRowsx.Length > 0)
                        dt = dRowsx.CopyToDataTable();
                    else
                        dt.Rows.Clear();
                }
                else if (runOn.ToUpper() == "RILES")
                {
                    dRowsx = dt.Select("contractNumber LIKE 'RF%'");
                    if (dRowsx.Length > 0)
                        dt = dRowsx.CopyToDataTable();
                    else
                        dt.Rows.Clear();
                }
                string edited = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                    if ( contractNumber.ToUpper().IndexOf ( "NNM") >= 0 )
                    {
                    }
                    if ( contractNumber == "L24064L")
                    {
                    }

                    if (contractNumber == "WM23029LI")
                    {
                    }
                    tca = false;
                    edited = dt.Rows[i]["edited"].ObjToString();
                    trust85 = dt.Rows[i]["trust85P"].ObjToDouble();
                    totalTrust85 += trust85;

                    creditReason = dt.Rows[i]["creditReason"].ObjToString().Trim();
                    if (creditReason.ToUpper() == "TCA")
                        tca = true;

                    location = dt.Rows[i]["Location Name"].ObjToString();
                    if (String.IsNullOrWhiteSpace(location))
                        location = "No Location";
                    trustName = dt.Rows[i]["Trust Name"].ObjToString();
                    if (String.IsNullOrWhiteSpace(trustName))
                        trustName = "No Trust";
                    if (String.IsNullOrWhiteSpace(saveLocation))
                        saveLocation = location;
                    if (String.IsNullOrWhiteSpace(saveTrustName))
                        saveTrustName = trustName;
                    if (saveLocation != location || runOn.ToUpper() == "CEMETERIES" || runOn.ToUpper() == "RILES" )
                    {
                        dbrContracts = dbrContracts.TrimEnd('\n');

                        DataRow dRow = locationDt.NewRow();
                        dRow["location"] = saveLocation.ToUpper();
                        dRow["Trust Name"] = saveTrustName;

                        dRow["cancelled"] = dbrContracts;
                        if (runOn.ToUpper() == "CEMETERIES" || runOn.ToUpper() == "RILES" )
                        {
                            dRow["cancelled"] = contractNumber;
                            dRow["depositNumber"] = dt.Rows[i]["depositNumber"].ObjToString();
                            dRow["payDate8"] = dt.Rows[i]["payDate8"].ObjToDateTime().ToString("MM/dd/yyyy");
                            dRow["perpetualCare"] = dt.Rows[i]["paymentAmount"].ObjToDouble();
                            dRow["perpetualCareHC"] = dt.Rows[i]["trust85P"].ObjToDouble();
                            dRow["location"] = location.ToUpper();
                            lastName = dt.Rows[i]["lastName"].ObjToString();
                            firstName = dt.Rows[i]["firstName"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(firstName))
                            {
                                firstName = firstName.Substring(0, 1) + ". ";
                                firstName = firstName.ToUpper();
                            }
                            dRow["bankName"] = GetBankName(dt.Rows[i]["bank_account"].ObjToString());
                            dRow["purchaserName"] = firstName + lastName;
                        }

                        dRow["dbr"] = totalDBR;
                        dRow["fdlicDownPayments"] = fdlicDownPayments;
                        dRow["fdlicMonthly"] = fdlicMonthly;
                        dRow["unityMonthly"] = unityMonthly;
                        dRow["bancorpsouthMonthly"] = bancorpsouthMonthly;
                        dRow["pbMonthly"] = pbMonthly;
                        if (runOn.ToUpper() != "CEMETERIES" && runOn.ToUpper() != "RILES" )
                        {
                            if (runOn == "HC")
                                dRow["perpetualCareHC"] = perpetualCare;
                            else
                                dRow["perpetualCare"] = perpetualCare;
                        }
                        dRow["total"] = fdlicDownPayments + fdlicMonthly + unityMonthly + bancorpsouthMonthly + pbMonthly + perpetualCare;
                        TotalfdlicDownPayments += fdlicDownPayments;
                        TotalfdlicMonthly += fdlicMonthly;
                        TotalunityMonthly += unityMonthly;
                        TotalbancorpsouthMonthly += bancorpsouthMonthly;
                        TotalpbMonthly += pbMonthly;
                        TotalperpetualCare += perpetualCare;
                        locationDt.Rows.Add(dRow);
                        fdlicDownPayments = 0D;
                        fdlicMonthly = 0D;
                        unityMonthly = 0D;
                        bancorpsouthMonthly = 0D;
                        pbMonthly = 0D;
                        perpetualCare = 0D;
                        saveLocation = location;
                        saveTrustName = trustName;
                        dbrContracts = "";
                        totalDBR = 0D;
                    }

                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                    if ( contractNumber == "nnm2005")
                    {
                    }

                    DataRow[] dRows = trickDt.Select("contractNumber='" + contractNumber + "'");
                    if ( dRows.Length > 0 )
                    {
                        discard = dRows[0]["discard"].ObjToString().ToUpper();
                        if (discard == "Y")
                            continue;
                    }
                    if ( dt.Rows[i]["SetAsDBR"].ObjToString() == "Y" )
                    {
                        newBusiness = dt.Rows[i]["newBusiness"].ObjToDouble();
                        dValue = dt.Rows[i]["paymentAmount"].ObjToDouble();
                        debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                        trust85 = dt.Rows[i]["trust85P"].ObjToDouble();
                        lastName = dt.Rows[i]["lastName"].ObjToString();
                        str = "DP-";
                        if (dValue > 0D || tca )
                        {
                            str = "MO-";
                            newBusiness = dValue;
                            newBusiness = trust85;
                        }
                        if ( debit > 0D)
                        {
                            str = "DB-";
                            newBusiness = dValue;
                            newBusiness = trust85;
                        }
                        dbrContracts +=  str + lastName + "-";
                        str = G1.ReformatMoney(newBusiness);
                        str = G1.ReformatMoney(trust85);
                        str = str.Replace("$", "");
                        if (debit > 0D)
                            str = "(" + str + ")";
                        dbrContracts += str + "\n";
                        //if ( trust85 > 0D ) // ramma zamma
                        //if ( debit == 0D ) // ramma zamma
                            totalDBR += trust85;
                        //if (debit > 0D)
                        //{
                        //    if (trustName == "FDLIC")
                        //        fdlicMonthly += trust85;
                        //    else if (trustName == "UNITY")
                        //        unityMonthly += trust85;
                        //    else if (trustName == "BANCORPSOUTH")
                        //        bancorpsouthMonthly += trust85;
                        //    else if (trustName == "PB")
                        //        pbMonthly += trust85;
                        //    else
                        //        perpetualCare += dValue;
                        //}
                        continue;
                    }
                    deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                    if (deceasedDate.Year > 1850 && dt.Rows[i]["SetAsDBR"].ObjToString() != "Y" )
                    {
                        payDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();
                        trust85 = dt.Rows[i]["trust85P"].ObjToDouble();
                        downPayment1 = dt.Rows[i]["downPayment1"].ObjToDouble();
                        dValue = dt.Rows[i]["paymentAmount"].ObjToDouble();
                        if (dValue > 0D || tca)
                        {
                            lastName = dt.Rows[i]["lastName"].ObjToString();
                            str = "MO-";
                            dbrContracts += str + lastName + "-";
                            //str = G1.ReformatMoney(dValue);
                            str = G1.ReformatMoney(trust85);
                            str = str.Replace("$", "");
                            dbrContracts += str + "\n";
                            dt.Rows[i]["SetAsDBR"] = "Y";
                            dt.Rows[i]["dbr"] = trust85;
                            dRowsx = dx.Select("contractNumber = '" + contractNumber + "'");
                            if (dRowsx.Length > 0)
                            {
                                for (int m = 0; m < dRowsx.Length; m++)
                                {
                                    dRowsx[m]["SetAsDBR"] = "Y";
                                    dRowsx[m]["dbr"] = dRowsx[m]["trust85P"].ObjToDouble();
                                }
                            }
                        }
                        else if (downPayment1 > 0D)
                        {
                            lastName = dt.Rows[i]["lastName"].ObjToString();
                            str = "DP-";
                            dbrContracts += str + lastName + "-";
                            //str = G1.ReformatMoney(downPayment1);
                            str = G1.ReformatMoney(trust85);
                            str = str.Replace("$", "");
                            dbrContracts += str + "\n";
                            dt.Rows[i]["SetAsDBR"] = "Y";
                            dt.Rows[i]["dbr"] = trust85;
                            dRowsx = dx.Select("contractNumber = '" + contractNumber + "'");
                            if (dRowsx.Length > 0)
                            {
                                for (int m = 0; m < dRowsx.Length; m++)
                                {
                                    dRowsx[m]["SetAsDBR"] = "Y";
                                    dRowsx[m]["dbr"] = dRowsx[m]["trust85P"].ObjToDouble();
                                }
                            }
                        }
                        totalDBR += trust85;
                        continue;
                    }
                    dValue = dt.Rows[i]["trust85P"].ObjToDouble();
                    newBusiness = dt.Rows[i]["newBusiness"].ObjToDouble();
                    if (newBusiness > 0D)
                        fdlicDownPayments += dValue;
                    else
                    {
                        //                    dValue = dt.Rows[i]["paymentAmount"].ObjToDouble();
                        //if ( edited.ToUpper() == "CEMETERY")
                        //    perpetualCare += dValue;
                        if (trustName == "FDLIC")
                            fdlicMonthly += dValue;
                        else if (trustName == "UNITY")
                            unityMonthly += dValue;
                        else if (trustName == "BANCORPSOUTH")
                            bancorpsouthMonthly += dValue;
                        else if (trustName == "PB")
                            pbMonthly += dValue;
                        else
                            perpetualCare += dValue;
                    }
                }

                dbrContracts = dbrContracts.TrimEnd('\n');

                if (runOn.ToString() != "CEMETERIES" && runOn.ToUpper() != "RILES" )
                {
                    DataRow dR = locationDt.NewRow();
                    dR["location"] = saveLocation.ToUpper();
                    dR["Trust Name"] = saveTrustName;
                    dR["cancelled"] = dbrContracts;
                    dR["dbr"] = totalDBR;
                    dR["fdlicDownPayments"] = fdlicDownPayments;
                    dR["fdlicMonthly"] = fdlicMonthly;
                    dR["unityMonthly"] = unityMonthly;
                    dR["bancorpsouthMonthly"] = bancorpsouthMonthly;
                    dR["pbMonthly"] = pbMonthly;
                    if (runOn.ToUpper() != "CEMETERIES" && runOn.ToUpper() != "RILES" )
                    {
                        if (runOn == "HC")
                            dR["perpetualCareHC"] = perpetualCare;
                        else
                            dR["perpetualCare"] = perpetualCare;
                    }
                    dR["total"] = fdlicDownPayments + fdlicMonthly + unityMonthly + bancorpsouthMonthly + pbMonthly + perpetualCare;
                    locationDt.Rows.Add(dR);

                    TotalfdlicDownPayments += fdlicDownPayments;
                    TotalfdlicMonthly += fdlicMonthly;
                    TotalunityMonthly += unityMonthly;
                    TotalbancorpsouthMonthly += bancorpsouthMonthly;
                    TotalpbMonthly += pbMonthly;
                    TotalfdlicDownPayments += fdlicDownPayments;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }

            totalTrust85 = G1.RoundValue(totalTrust85);
            DataRow [] dd = trickDt.Select("discard='Y'");
            for (int i = 0; i < dd.Length; i++)
                totalEliminated += dd[i]["trust85P"].ObjToDouble();
            totalEliminated = G1.RoundValue(totalEliminated);
            str = G1.ReformatMoney(totalTrust85) + " Eliminated " + G1.ReformatMoney(totalEliminated);
            this.gridBand6.Caption = "Cash Remitted " + str;

            if ( runOn.ToUpper() != "RILES" )
                locationDt = VerifyLocations(locationDt);

            locationDt = SetupCemeteries(locationDt, gridMain6);


            G1.NumberDataTable(locationDt);
            dgv6.DataSource = locationDt;
            gridMain6.ExpandAllGroups();

            Trust85.FindContract(dx, "CT24044LI");

            LoadCashDPs(dx);
            LoadCashPayments(dx, trickDt );
            LoadOVPPayments(dx, trickDt);

            locationDt = (DataTable) dgv6.DataSource;
        }
        /***********************************************************************************************/
        public static string GetBankName ( string bank_account )
        {
            if (String.IsNullOrWhiteSpace(bank_account))
                return "";
            string[] Lines = bank_account.Split('~');
            if (Lines.Length < 3)
                return bank_account;
            string account = Lines[2].Trim();
            string cmd = "Select * from `bank_accounts` where `account_no` = '" + account + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return bank_account;
            bank_account = dx.Rows[0]["localDescription"].ObjToString();
            return bank_account;
        }
        /***********************************************************************************************/
        private DataTable SetupCemeteries(DataTable dt, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            string location = "";
            if (G1.get_column_number(dt, "group") < 0)
                dt.Columns.Add("group");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                location= dt.Rows[i]["location"].ObjToString();
                if (location.ToUpper().IndexOf("CEMETERY") >= 0)
                {
                    if (location.ToUpper().IndexOf("NMM") == 0)
                        dt.Rows[i]["group"] = 2;
                    else
                        dt.Rows[i]["group"] = 3;
                }
                else if (location.ToUpper().IndexOf("RILES") >= 0)
                    dt.Rows[i]["group"] = 5;
                else
                    dt.Rows[i]["group"] = 1;

            }
            string runOn = cmbRunOn.Text.Trim().ToUpper();
            try
            {
                DataRow[] dRows = null;
                if ( runOn == "TRUSTS")
                {
                    dRows = dt.Select("group='1'");
                    if (dRows.Length > 0)
                        dt = dRows.CopyToDataTable();
                }
                else if ( runOn.ToUpper() == "CEMETERIES")
                {
                    this.gridBand6.OptionsBand.ShowCaption = false;
                    dRows = dt.Select("group = '2' OR group = '3'");
                    if (dRows.Length > 0)
                    {
                        dt = dRows.CopyToDataTable();
                        if ( runOn == "NNM")
                        {
                            dRows = dt.Select("location LIKE 'NMM%'");
                            if (dRows.Length > 0)
                                dt = dRows.CopyToDataTable();
                        }
                        else if ( runOn == "HC")
                        {
                            dRows = dt.Select("location LIKE 'HC%'");
                            if (dRows.Length > 0)
                                dt = dRows.CopyToDataTable();
                        }
                        DataView tempview = dt.DefaultView;
                        tempview.Sort = "location DESC";
                        dt = tempview.ToTable();

                        string group = "";
                        for ( int i=0; i<dt.Rows.Count; i++)
                        {
                            group = dt.Rows[i]["group"].ObjToString();
                            if (group == "2")
                                group += " " + dt.Rows[i]["location"].ObjToString();
                            else if (group == "3")
                                group += " " + dt.Rows[i]["location"].ObjToString();
                            dt.Rows[i]["group"] = group;
                        }
                    }
                }
                else if ( runOn.ToUpper() == "RILES")
                {
                    this.gridBand6.OptionsBand.ShowCaption = false;
                    dRows = dt.Select("location LIKE 'RF%'");
                    if (dRows.Length > 0)
                        dt = dRows.CopyToDataTable();
                }
            }
            catch ( Exception ex)
            {
            }

            grid.Columns["location"].GroupIndex = -1;

            if (runOn.ToUpper() == "CEMETERIES" )
            {
                grid.Columns["location"].GroupIndex = 0;

                grid.Columns["cancelled"].Caption = "Contract #";
                grid.Columns["location"].Visible = false;
                grid.Columns["total"].Visible = false;
                grid.Columns["perpetualCareHC"].Caption = "Trustee Remittance Amount (15%)";
            }
            else if (runOn.ToUpper() == "RILES")
            {
                grid.Columns["location"].GroupIndex = -1;

                grid.Columns["cancelled"].Caption = "Contract #";
                grid.Columns["location"].Visible = false;
                grid.Columns["total"].Visible = false;
                grid.Columns["perpetualCareHC"].Caption = "Trustee Remittance Amount (From Contract 80% or 100%)";
            }
            return dt;
        }
        /****************************************************************************************/
        private DataTable VerifyLocations( DataTable dtIn)
        {
            DataTable dt = dtIn.Copy();

            string location = "";
            string heading = "";
            bool found = false;
            DataRow dR = null;
            DataTable groupDt = funDt.AsEnumerable().GroupBy(r => new { Col1 = r["cashRemitHeading"] }).Select(g => g.OrderBy(r => r["cashRemitHeading"]).First()).CopyToDataTable();

            for ( int i=0; i<groupDt.Rows.Count; i++)
            {
                heading = groupDt.Rows[i]["cashRemitHeading"].ObjToString().Trim().ToUpper();
                if (String.IsNullOrWhiteSpace(heading))
                    continue;
                found = false;
                for ( int j=0; j<dt.Rows.Count; j++)
                {
                    location = dt.Rows[j]["location"].ObjToString().Trim().ToUpper();
                    if ( heading == location)
                    {
                        found = true;
                        break;
                    }
                }
                if ( !found)
                {
                    try
                    {
                        heading = groupDt.Rows[i]["cashRemitHeading"].ObjToString().Trim();
                        dR = dt.NewRow();
                        dR["location"] = heading;
                        dR["Trust Name"] = "";
                        dR["cancelled"] = "";
                        dR["dbr"] = 0D;
                        dR["fdlicDownPayments"] = 0D;
                        dR["fdlicMonthly"] = 0D;
                        dR["unityMonthly"] = 0D;
                        dR["bancorpsouthMonthly"] = 0D;
                        dR["pbMonthly"] = 0D;
                        dR["perpetualCare"] = 0D;
                        dR["total"] = 0D;
                        dt.Rows.Add(dR);
                    }
                    catch ( Exception ex)
                    {
                    }
                }
            }
            dt.AcceptChanges();
            return dt;
        }
        /****************************************************************************************/
        private void LoadCashDPs(DataTable dx)
        {
            DataTable dt = dx.Copy();
            DataView tempview = dt.DefaultView;
            tempview.Sort = "Location Name, Trust Name";
            dt = tempview.ToTable();

            double newBusiness = 0D;

            double downPayment = 0D;
            double downPayment1 = 0D;
            //double newBusiness = 0D;
            double ccFee = 0D;
            double dpp = 0D;
            string contractNumber = "";
            if (!previousDateRead)
            {
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    contractNumber = dx.Rows[i]["contractNumber"].ObjToString().Trim();
                    if ( contractNumber == "CT23030LI")
                    {
                    }
                    newBusiness = dx.Rows[i]["newBusiness"].ObjToDouble();
                    downPayment1 = dx.Rows[i]["downPayment1"].ObjToDouble();
                    downPayment = dx.Rows[i]["downPayment"].ObjToDouble();
                    ccFee = dx.Rows[i]["ccFee"].ObjToDouble();

                    dx.Rows[i]["dpp"] = newBusiness;
                    dpp = dx.Rows[i]["dpp"].ObjToDouble();
                    if (dpp != 0D)
                        dx.Rows[i]["newBusiness"] = dpp;
                }
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                    newBusiness = dt.Rows[i]["newBusiness"].ObjToDouble();
                    downPayment1 = dt.Rows[i]["downPayment1"].ObjToDouble();
                    downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                    ccFee = dt.Rows[i]["ccFee"].ObjToDouble();

                    dpp = dt.Rows[i]["dpp"].ObjToDouble();
                    if (dpp != 0D)
                        dt.Rows[i]["newBusiness"] = dpp;
                }
            }

            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                newBusiness = dt.Rows[i]["newBusiness"].ObjToDouble();
                if (newBusiness <= 0D)
                    dt.Rows.RemoveAt(i);
            }

            Trust85.FindContract(dt, "M23001LI");

            dgv7.DataSource = dt;
            gridMain7.ExpandAllGroups();
        }
        /****************************************************************************************/
        private void LoadCashPayments(DataTable dx, DataTable trickDt )
        {
            DataTable dt = dx.Copy();
            DataView tempview = dt.DefaultView;
            tempview.Sort = "Location Name, Trust Name";
            dt = tempview.ToTable();

            double newBusiness = 0D;
            double downPayment = 0D;
            double paymentAmount = 0D;
            double debit = 0D;
            double credit = 0D;
            double total = 0D;
            string contractNumber = "";
            string discard = "";
            string edited = "";

            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                if (contractNumber == "CT23030LI")
                {
                }


                DataRow[] dRows = trickDt.Select("contractNumber='" + contractNumber + "'");
                if (dRows.Length > 0)
                {
                    discard = dRows[0]["discard"].ObjToString().ToUpper();
                    if (discard == "Y")
                    {
                        dt.Rows.RemoveAt(i);
                        continue;
                    }
                }

                newBusiness = dt.Rows[i]["newBusiness"].ObjToDouble();
                downPayment = dt.Rows[i]["downPayment1"].ObjToDouble();
                if (newBusiness > 0D && downPayment == 0D)
                    downPayment = newBusiness;
                paymentAmount = dt.Rows[i]["paymentAmount"].ObjToDouble();
                edited = dt.Rows[i]["edited"].ObjToString();
                if ( downPayment > 0D && paymentAmount == 0D && (edited.ToUpper() != "TRUSTADJ" && edited.ToUpper() != "CEMETERY" ) )
                {
                    dt.Rows.RemoveAt(i);
                    continue;
                }
                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                total = paymentAmount - debit + credit;
                //if (total <= 0D)
                //    dt.Rows.RemoveAt(i);
            }
            Trust85.FindContract(dt, "CT23030LI");

            dgv8.DataSource = dt;
            gridMain8.ExpandAllGroups();
        }
        /****************************************************************************************/
        private void LoadOVPPayments(DataTable dx, DataTable trickDt)
        {
            DataTable dt = dx.Copy();
            DataView tempview = dt.DefaultView;
            tempview.Sort = "Location Name, Trust Name";
            dt = tempview.ToTable();

            double newBusiness = 0D;
            double downPayment = 0D;
            double paymentAmount = 0D;
            double debit = 0D;
            double credit = 0D;
            double total = 0D;
            string contractNumber = "";
            string discard = "";
            string edited = "";
            string finale = "";
            string oldContract = "";
            bool honorFinale = false;
            int finaleCount = 0;

            //for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                if (contractNumber == "HT16086UI")
                {
                }
                honorFinale = false;
                finale = dt.Rows[i]["new"].ObjToString().ToUpper();
                if (oldContract != contractNumber)
                    finaleCount = 0;
                oldContract = contractNumber;
                if (finale == "FINALE")
                    finaleCount++;

                //DataRow[] dRows = trickDt.Select("contractNumber='" + contractNumber + "'");
                //if (dRows.Length > 0)
                //{
                //    discard = dRows[0]["discard"].ObjToString().ToUpper();
                //    if (discard == "Y")
                //    {
                //        dt.Rows.RemoveAt(i);
                //        continue;
                //    }
                //}

                newBusiness = dt.Rows[i]["newBusiness"].ObjToDouble();
                downPayment = dt.Rows[i]["downPayment1"].ObjToDouble();
                if (newBusiness > 0D && downPayment == 0D)
                    downPayment = newBusiness;
                paymentAmount = dt.Rows[i]["paymentAmount"].ObjToDouble();
                edited = dt.Rows[i]["edited"].ObjToString();
                if (downPayment > 0D && paymentAmount == 0D && (edited.ToUpper() != "TRUSTADJ" && edited.ToUpper() != "CEMETERY"))
                {
                    //dt.Rows.RemoveAt(i);
                    continue;
                }

                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                total = paymentAmount - debit + credit;

                if (finaleCount <= 1)
                {
                    //dt.Rows.RemoveAt(i);
                    continue;
                }
                else if (finaleCount > 1)
                {
                    if (debit > 0D || credit > 0D)
                    {
                        //dt.Rows.RemoveAt(i);
                        continue;
                    }
                }

                dt.Rows[i]["ovp"] = dt.Rows[i]["trust85P"].ObjToDouble();
            }
            for ( int i=(dt.Rows.Count-1); i>=0; i--)
            {
                if (dt.Rows[i]["ovp"].ObjToDouble() == 0D)
                    dt.Rows.RemoveAt(i);
            }
            dgv13.DataSource = dt;
            gridMain13.ExpandAllGroups();
        }
        /****************************************************************************************/
        private void LoadRemovals(DataTable dx )
        {
            DataTable dt = dx.Copy();
            DataView tempview = dt.DefaultView;
            tempview.Sort = "Location Name, Trust Name";
            dt = tempview.ToTable();

            DataTable rDt = dt.Clone();
            rDt.Columns.Add("Trust Paid");
            rDt.Columns.Add("Trust Paid Date");

            DataTable workDt = new DataTable();
            workDt.Columns.Add("trust");
            workDt.Columns.Add("date");
            string trust = "";
            DateTime date = DateTime.Now;
            string field = "";
            int row = 0;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                workDt.Rows.Clear();
                for (int j = 1; j <= 5; j++)
                {
                    field = "trustPaid" + j.ToString();
                    trust = dt.Rows[i][field].ObjToString();
                    field = "trustPaidDate" + j.ToString();
                    date = dt.Rows[i][field].ObjToDateTime();
                    if ( !String.IsNullOrWhiteSpace ( trust))
                    {
                        DataRow dR = workDt.NewRow();
                        dR["trust"] = trust;
                        if (date.Year > 1000)
                            dR["date"] = date.ToString("MM/dd/yyyy");
                        workDt.Rows.Add(dR);
                    }
                }
                if (workDt.Rows.Count <= 0)
                {
                    G1.copy_dt_row(dt, i, rDt, rDt.Rows.Count);
                }
                else
                {
                    for (int j = 0; j < workDt.Rows.Count; j++)
                    {
                        G1.copy_dt_row(dt, i, rDt, rDt.Rows.Count);
                        row = rDt.Rows.Count - 1;
                        rDt.Rows[row]["Trust Paid"] = workDt.Rows[j]["trust"].ObjToString();
                        rDt.Rows[row]["Trust Paid Date"] = workDt.Rows[j]["date"].ObjToString();
                    }
                }
            }

            dgv9.DataSource = rDt;
            gridMain9.ExpandAllGroups();
        }
        /****************************************************************************************/
        private void gridMain3_RowStyle(object sender, RowStyleEventArgs e)
        {
            //GridView View = sender as GridView;
            //if (e.RowHandle >= 0)
            //{
            //    string location = View.GetRowCellDisplayText(e.RowHandle, View.Columns["location"]);
            //    if (CheckForBold(location))
            //    {
            //        e.Appearance.BackColor = Color.Salmon;
            //        e.Appearance.BackColor2 = Color.SeaShell;
            //        Font f = e.Appearance.Font;
            //        string name = f.Name.ObjToString();
            //        Font font = new Font(name, e.Appearance.Font.Size, FontStyle.Bold);
            //        e.Appearance.Font = font;
            //        e.HighPriority = true;
            //    }
            //}
        }
        /****************************************************************************************/
        private void gridMain3_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                string location = View.GetRowCellDisplayText(e.RowHandle, View.Columns["location"]);
                if (CheckForBold(location))
                {
                    //e.Appearance.BackColor = Color.Salmon;
                    //e.Appearance.BackColor2 = Color.SeaShell;
                    Font f = e.Appearance.Font;
                    string name = f.Name.ObjToString();
                    Font font = new Font(name, e.Appearance.Font.Size, FontStyle.Bold);
                    e.Appearance.Font = font;
                }
            }
        }
        /****************************************************************************************/
        private void gridMain3_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (1 == 1)
                return;
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)sender;
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                string location = View.GetRowCellDisplayText(e.RowHandle, View.Columns["location"]);
                if (CheckForBold(location))
                {
                    Font f = e.BrickGraphics.DefaultBrickStyle.Font;
                    string name = f.Name.ObjToString();
                    Font font = new Font(name, f.Size, FontStyle.Bold & FontStyle.Italic);
                    //                    e.BrickGraphics.DefaultBrickStyle.Font = font;

                    float x = e.X;
                    float y = e.Y;
                    float height = f.Height;

                    RectangleF rect = new RectangleF(x + 35, y - height - 4, 232, height + 5);
                    Brush brush = new SolidBrush(Color.Black);

                    TextBrick textBrick = Printer.localE.Graph.DrawString(location, rect);
                    textBrick.Font = new Font(name, font.Size, FontStyle.Bold & FontStyle.Italic);
                    textBrick.HorzAlignment = HorzAlignment.Near;
                    textBrick.VertAlignment = VertAlignment.Center;
                    //                    textBrick.Style = new BrickStyle(BorderSide.All, 5F, Color.Red, Color.White, Color.Black, font, null);
                    textBrick.BorderWidth = 1;
                    e.BrickGraphics.DefaultBrickStyle.Font = font;
                    e.BrickGraphics.DrawBrick(textBrick, rect);
                    //e.HighPriority = true;
                }
            }
        }
        /****************************************************************************************/
        private bool CheckForBold(string text)
        {
            if (text.ToUpper().IndexOf("TOTAL") >= 0)
                return true;
            if (text.ToUpper() == "LOCKBOX")
                return true;
            if (text.ToUpper().IndexOf("DRAFTS") >= 0)
                return true;
            if (text.ToUpper().IndexOf("CREDIT/DEBIT CARDS") >= 0)
                return true;
            if (text.ToUpper().IndexOf("STATE BANK REMOTE") >= 0)
                return true;
            if (text.ToUpper().IndexOf("CASH-LOCAL BANK") >= 0)
                return true;
            if (text.ToUpper().IndexOf("MAGNOLIA STATE BANK") >= 0)
                return true;
            return false;
        }
        /****************************************************************************************/
        private void gridMain4_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                string location = View.GetRowCellDisplayText(e.RowHandle, View.Columns["location"]);
                if (CheckForBold(location))
                {
                    //e.Appearance.BackColor = Color.Salmon;
                    //e.Appearance.BackColor2 = Color.SeaShell;
                    Font f = e.Appearance.Font;
                    string name = f.Name.ObjToString();
                    Font font = new Font(name, e.Appearance.Font.Size, FontStyle.Bold);
                    e.Appearance.Font = font;
                }
            }
        }
        /****************************************************************************************/
        private void gridMain6_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                string cancelled = View.GetRowCellDisplayText(e.RowHandle, View.Columns["cancelled"]);
                if ( !String.IsNullOrWhiteSpace (cancelled))
                {
                    int originalRowHeight = e.RowHeight;
                    cancelled = cancelled.TrimEnd('\n');
                    string[] Lines = cancelled.Split('\n');
                    int count = Lines.Length;
                    if (count > 1)
                        e.RowHeight = originalRowHeight * count;
                }
            }
        }
        /****************************************************************************************/
        private void gridMain6_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "CANCELLED")
            {
                if (e.RowHandle >= 0)
                {
                    string text = e.DisplayText;
                    if ( !String.IsNullOrWhiteSpace ( text ))
                        e.DisplayText = text.Replace("~", "\n");
                }
            }
        }
        /****************************************************************************************/
        private void chkSortLastName_CheckedChanged(object sender, EventArgs e)
        {
            if (dgv.DataSource == null)
                return;
            bool sort = false;
            if (chkSortLastName.Checked)
                sort = true;
            if (dgv.Visible)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataView tempview = dt.DefaultView;
                if (sort)
                    tempview.Sort = "lastName,firstName,contractNumber";
                else
                    tempview.Sort = "contractNumber";
                dt = tempview.ToTable();
                dgv.DataSource = dt;
                gridMain.ExpandAllGroups();
            }
            else if (dgv7.Visible)
            {
                DataTable dt = (DataTable)dgv7.DataSource;
                DataView tempview = dt.DefaultView;
                if ( sort )
                    tempview.Sort = "Location Name,Trust Name,lastName,firstName,contractNumber";
                else
                    tempview.Sort = "Location Name,Trust Name";
                dt = tempview.ToTable();
                dgv7.DataSource = dt;
                gridMain7.ExpandAllGroups();
            }
            else if (dgv8.Visible)
            {
                DataTable dt = (DataTable)dgv8.DataSource;
                DataView tempview = dt.DefaultView;
                if (sort)
                    tempview.Sort = "Location Name,Trust Name,lastName,firstName,contractNumber";
                else
                    tempview.Sort = "Location Name,Trust Name";
                dt = tempview.ToTable();
                dgv8.DataSource = dt;
                gridMain8.ExpandAllGroups();
            }
            else if (dgv13.Visible)
            {
                DataTable dt = (DataTable)dgv13.DataSource;
                if (dt != null)
                {
                    DataView tempview = dt.DefaultView;
                    if (sort)
                        tempview.Sort = "Location Name,Trust Name,lastName,firstName,contractNumber";
                    else
                        tempview.Sort = "Location Name,Trust Name";
                    dt = tempview.ToTable();
                    dgv13.DataSource = dt;
                    gridMain13.ExpandAllGroups();
                }
            }
        }
        /****************************************************************************************/
        private void compareResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            CompareTrustResults compareForm = new CompareTrustResults(dt);
            compareForm.Show();
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            if (workReport.ToUpper() == "ODD PAYMENTS REPORT")
            {
                int row = e.ListSourceRow;
                DataTable dt = (DataTable)dgv.DataSource;

                double monthlyPayment = dt.Rows[row]["amtOfMonthlyPayt"].ObjToDouble();
                double payment = dt.Rows[row]["paymentAmount"].ObjToDouble();
                double downPayment = dt.Rows[row]["downPayment"].ObjToDouble();
                if ( payment == 0D && downPayment > 0D)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
                if (monthlyPayment == payment)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
                if ( chkPmtLessMoPmt.Checked )
                {
                    if ( payment < monthlyPayment )
                    {
                        e.Visible = true;
                        e.Handled = true;
                    }
                    else
                    {
                        e.Visible = false;
                        e.Handled = true;
                    }
                    return;
                }
                int count = (int)(payment / monthlyPayment);
                double amount = (double)count * monthlyPayment;
                if (amount == payment)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
        }
        /****************************************************************************************/
        private void PaymentsReport_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (paidOutMenu != null)
                paidOutMenu.Dispose();
            paidOutMenu = null;
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
            G1.CleanupDataGrid(ref dgv7);
            GC.Collect();
            G1.CleanupDataGrid(ref dgv8);
            GC.Collect();
            G1.CleanupDataGrid(ref dgv9);
            GC.Collect();
            G1.CleanupDataGrid(ref dgv10);
            GC.Collect();
            G1.CleanupDataGrid(ref dgv11);
            GC.Collect();
            G1.CleanupDataGrid(ref dgv12);
            GC.Collect();
        }
        /****************************************************************************************/
        private void PaidOutMenu_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            GenerateNotices(dt);
        }
        /****************************************************************************************/
        private void GenerateNotices(DataTable dt)
        {
            this.Cursor = Cursors.WaitCursor;
            int count = 0;
            string str = "";
            string contract = "";
            string miniContract = "";
            string trust = "";
            string loc = "";
            string line = "";
            string funeralHomeName = "";
            string name = "";
            string address = "";
            string address2 = "";
            string city = "";
            string state = "";
            string zip = "";
            string zip2 = "";

            string mailAddress = "";
            string mailAddress2 = "";
            string mailCity = "";
            string mailState = "";
            string mailZip = "";
            string mailZip2 = "";

            double payment = 0D;
            double balanceDue = 0D;
            string money = "";
            string prefix = "";
            string suffix = "";
            string fname = "";
            string lname = "";
            string mname = "";
            string pronown = "";
            string gender = "";
            string select = "";

            string POBox = "";
            string POCity = "";
            string POState = "";
            string POZip = "";
            string funeralPhoneNumber = "";
            string manager = "";
            string signer = "";

            string agentCode = "";
            string agentName = "";
            DataTable agentDt = null;

            RichTextBox rtb3 = new RichTextBox();
            rtb3.Font = new Font("", 9);

            int padLeft = 10;

            //int padright = GetLapseNoticeValues("Lapse Notices Left Side Width", "30");
            //int padTop = GetLapseNoticeValues("Lapse Notices Top Border Lines");
            //int padLeft = GetLapseNoticeValues("Lapse Notices Left Border Spaces");
            //int padBottom = GetLapseNoticeValues("Lapse Notices Bottom Border Lines");
            //int padToCustomer = GetLapseNoticeValues("Lapse Notices Lines Prior to Customer");
            //int tof = GetLapseNoticeValues("TOF after X Notices");

            //DateTime lapseDate = this.dateTimePickerAsOf.Value;

            rtb2.Document.Text = "";
            int noticeCount = 0;

            double allowInsurance = 0D;

            int lastRow = dt.Rows.Count;
//            lastRow = 5; // For Testing
            string letterFont = "Lucida Sans Unicode";
            letterFont = "Lucida Console";
            letterFont = "Times New Roman";
            //letterFont = "Courier";
            //letterFont = "Roboto Mono";
            float letterSize = 14F;
            padLeft = 8;
            padLeft = 16;

            bool found = false;
            for ( int i=0; i<lastRow; i++)
            {
                select = dt.Rows[i]["select"].ObjToString();
                if (select == "1")
                {
                    found = true;
                    break;
                }
            }

            DialogResult result;

            bool doAll = false;
            if (doPaidOff)
            {
                found = true;
                doAll = true;
            }
            if ( !found)
            {
                result = MessageBox.Show("There are NO contracts selected!\nDo you want to generate letters for all contracts?", "Paid Off Letters Generated Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    this.Cursor = Cursors.Default;
                    return;
                }
                doAll = true;
            }

            string contractList = "";

            for (int i = 0; i < lastRow; i++)
            {
                if (!doAll)
                {
                    select = dt.Rows[i]["select"].ObjToString();
                    if (select != "1")
                        continue;
                }
                contract = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                contractList += contract + ",";
                balanceDue = dt.Rows[i]["balanceDue"].ObjToDouble();
                payment = dt.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                miniContract = Trust85.decodeContractNumber(contract, ref trust, ref loc);
                PastDue.LocateFuneralHome(loc, ref funeralHomeName, ref address, ref city, ref state, ref zip, ref POBox, ref POCity, ref POState, ref POZip, ref funeralPhoneNumber, ref manager, ref signer );
                if (!String.IsNullOrWhiteSpace(signer))
                    manager = signer;

                pronown = " ";
                agentCode = dt.Rows[i]["agentCode"].ObjToString();
                agentName = "";
                agentDt = G1.get_db_data("Select * from `agents` where `agentCode` = '" + agentCode + "';");
                if (agentDt.Rows.Count > 0)
                {
                    agentName = agentDt.Rows[0]["firstName"].ObjToString() + " " + agentDt.Rows[0]["lastName"].ObjToString();
                    gender = agentDt.Rows[0]["gender"].ObjToString();
                    if (gender == "F")
                        pronown = "her";
                }

                if ( noticeCount > 0 )
                    rtb3.AppendText("\f");

                rtb3.SelectionAlignment = HorizontalAlignment.Center;
                G1.Toggle_Font(rtb3, "Times New Roman", 14f);
                G1.Toggle_Bold(rtb3, true, false);
                rtb3.AppendText("\n\n\n" + funeralHomeName + "\n");

                rtb3.SelectionAlignment = HorizontalAlignment.Center;
                G1.Toggle_Font(rtb3, "Times New Roman", 14f);
                rtb3.AppendText(address + "\n");
                if ( !String.IsNullOrWhiteSpace ( POBox))
                {
                    rtb3.SelectionAlignment = HorizontalAlignment.Center;
                    G1.Toggle_Font(rtb3, "Times New Roman", 14f);
                    rtb3.AppendText("Post Office Box " + POBox + "\n");
                }
                rtb3.SelectionAlignment = HorizontalAlignment.Center;
                G1.Toggle_Font(rtb3, "Times New Roman", 14f);
                rtb3.AppendText(city + ", " + state + "  " + zip + "\n");

                rtb3.SelectionAlignment = HorizontalAlignment.Center;
                G1.Toggle_Font(rtb3, "Times New Roman", 14f);
                rtb3.AppendText(funeralPhoneNumber + "\n\n");

                DateTime now = DateTime.Now;
                string nowMonth = DateTime.Now.ToString("MMMM");

                str = nowMonth + " " + now.Day.ToString() + ", " + now.Year.ToString("D4");
                rtb3.SelectionAlignment = HorizontalAlignment.Center;
                G1.Toggle_Font(rtb3, "Times New Roman", 14f);
                rtb3.AppendText(str + "\n");


                //rtb3.SelectAll();
                //rtb3.SelectionAlignment = HorizontalAlignment.Center;

                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                rtb3.AppendText("\n\n\n\n");
                name = "";
                prefix = dt.Rows[i]["prefix"].ObjToString();
                suffix = dt.Rows[i]["suffix"].ObjToString();

                fname = dt.Rows[i]["firstName"].ObjToString();
                fname = FunCustomer.FixUpperLowerNames(fname);

                lname = dt.Rows[i]["lastName"].ObjToString();
                lname = FunCustomer.FixUpperLowerNames(lname);

                mname = dt.Rows[i]["middleName"].ObjToString();
                mname = FunCustomer.FixUpperLowerNames(mname);

                if (!String.IsNullOrWhiteSpace(prefix))
                    name = prefix + " ";
                name += fname + " ";
                if (!String.IsNullOrWhiteSpace(mname))
                    name += mname + " ";
                name += lname;
                if (!String.IsNullOrWhiteSpace(suffix))
                    name += ", " + suffix;

                //name = G1.force_lower_line(name);

                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + name + "\n");

                address = dt.Rows[i]["address1"].ObjToString(); // Home Address
                //address2 = dt.Rows[i]["address2"].ObjToString();
                //if ( address.IndexOf( address2) < 0 )
                //    address += address2;

                mailAddress = dt.Rows[i]["mailAddress1"].ObjToString(); // Mailing Address
                if ( !String.IsNullOrWhiteSpace ( mailAddress))
                {
                    //mailAddress2 = dt.Rows[i]["mailAddress2"].ObjToString();
                    //if (mailAddress.IndexOf(mailAddress2) < 0)
                    //    mailAddress += mailAddress2;
                    address = mailAddress;
                }

                address = G1.force_lower_line(address);
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + address + "\n");

                address = dt.Rows[i]["address2"].ObjToString(); // Home Address 2

                mailAddress = dt.Rows[i]["mailAddress2"].ObjToString(); // Mailing Address
                if (!String.IsNullOrWhiteSpace(mailAddress))
                    address = mailAddress2;
                //{
                //    mailAddress2 = dt.Rows[i]["mailAddress2"].ObjToString();
                //    if (mailAddress.IndexOf(mailAddress2) < 0)
                //        mailAddress += mailAddress2;
                //    address = mailAddress;
                //}
                if (!String.IsNullOrWhiteSpace(address))
                {
                    address = G1.force_lower_line(address);
                    rtb3.SelectionAlignment = HorizontalAlignment.Left;
                    G1.Toggle_Font(rtb3, letterFont, letterSize);
                    rtb3.AppendText("".PadLeft(padLeft) + address + "\n");
                }

                city = dt.Rows[i]["city"].ObjToString();
                state = dt.Rows[i]["state"].ObjToString();
                zip = dt.Rows[i]["zip1"].ObjToString();
                str = dt.Rows[i]["zip2"].ObjToString();
                if (!String.IsNullOrWhiteSpace(str))
                {
                    if ( str != "0")
                        zip += "-" + str;
                }
                str = city + ", " + state + "  " + zip;

                if ( !String.IsNullOrWhiteSpace ( mailAddress))
                {
                    mailCity = dt.Rows[i]["mailCity"].ObjToString();
                    mailState = dt.Rows[i]["mailState"].ObjToString();
                    mailZip = dt.Rows[i]["mailZip1"].ObjToString();
                    str = dt.Rows[i]["mailZip2"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(str))
                    {
                        if (str != "0")
                            zip += "-" + str;
                    }
                    str = mailCity + ", " + mailState + "  " + mailZip;
                }

                str = G1.force_lower_line(str);
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft)+str + "\n\n");

                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + "Dear " + name + ":" + "\n\n");

                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                line = "We are pleased to notify you that your contract " + contract + " is fully paid. For itemized";
                line += "";
                rtb3.AppendText("".PadLeft(padLeft+5) + line + "\n");

                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                line = "details, please refer to your contract. If your contract is fully or partially funded by your";
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                allowInsurance = dt.Rows[i]["allowInsurance"].ObjToDouble();

                line = "insurance policy, please continue to pay the insurance premiums to keep the policy in";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                line = "force.";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                line = "";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                line = "Congratulations on completing payments on your pre-arranged funeral contract. If";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft+5) + line + "\n");

                //line = "arranged funeral contract.  If you have any questions";
                //rtb3.SelectionAlignment = HorizontalAlignment.Left;
                //G1.Toggle_Font(rtb3, letterFont, letterSize);
                //rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                line = "you have any questions concerning your contract, or perhaps would like to add other";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                //line = "other merchandise or upgrade your selection, please call me";
                //rtb3.SelectionAlignment = HorizontalAlignment.Left;
                //G1.Toggle_Font(rtb3, letterFont, letterSize);
                //rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                line = "merchandise or upgrade your selection, please call me and I will be happy to help you.";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n\n");

                line = "Pre-arranging funeral services is one of the most caring acts you can do for your";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft+5) + line + "\n");

                //line = "caring acts you can do for your family. We are sure your";
                //rtb3.SelectionAlignment = HorizontalAlignment.Left;
                //G1.Toggle_Font(rtb3, letterFont, letterSize);
                //rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                line = "family. We are sure your family will be very thankful that you did this. You may know";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                line = "of someone else among your family or friends that you feel would benefit from learning";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                //line = "feel would benefit from learning more about pre-";
                //rtb3.SelectionAlignment = HorizontalAlignment.Left;
                //G1.Toggle_Font(rtb3, letterFont, letterSize);
                //rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                line = "more about pre-arrangements, if so please let me know. You can reach me at ";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                line = funeralPhoneNumber + ".";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n\n");

                //line = "at " + funeralPhoneNumber + "."; ;
                //rtb3.SelectionAlignment = HorizontalAlignment.Left;
                //G1.Toggle_Font(rtb3, letterFont, letterSize);
                //rtb3.AppendText("".PadLeft(padLeft) + line + "\n\n");

                //line = funeralPhoneNumber + ".";
                //rtb3.SelectionAlignment = HorizontalAlignment.Left;
                //G1.Toggle_Font(rtb3, letterFont, letterSize);
                //rtb3.AppendText("".PadLeft(padLeft) + line + "\n\n");

                name = funeralHomeName;
                //if ( String.IsNullOrWhiteSpace ( name ))
                //{
                //    if (contract.ToUpper().IndexOf("WF") == 0)
                //        name = "Wright and Ferguson Funeral Home";
                //}
                //bool gotColonial = false;
                //bool atEnd = false;
                //if (name.IndexOf("Colonial Chapel Funeral Home") == 0)
                //{
                //    name = name.Replace("Colonial Chapel Funeral Home", "").Trim();
                //    gotColonial = true;
                //}
                //else if (name.IndexOf("Colonial Chapel Funeral Home") > 0)
                //{
                //    name = name.Replace("Colonial Chapel Funeral Home", "").Trim();
                //    atEnd = true;
                //}
                //if (name.IndexOf("of") == 0)
                //    name = name.Replace("of", "");
                //else if (name.IndexOf("Of") == 0)
                //    name = name.Replace("Of", "");
                //name = name.Trim();
                //line = "We thank you for allowing ";
                //if (gotColonial)
                //    line += "Colonial Chapel Funeral Home of";
                //else
                //    line += name;
                line = "We thank you for allowing " + name + " the opportunity to serve you.";

                string[] splitLines = G1.WordWrap(line, 85);
                for ( int k=0; k<splitLines.Length; k++)
                {
                    rtb3.SelectionAlignment = HorizontalAlignment.Left;
                    G1.Toggle_Font(rtb3, letterFont, letterSize);
                    if ( k == 0 )
                        rtb3.AppendText("".PadLeft(padLeft + 5) + splitLines[k] + "\n");
                    else
                        rtb3.AppendText("".PadLeft(padLeft) + splitLines[k] + "\n");
                }

                //line = "";
                //if (gotColonial)
                //    line += name + " the opportunity to serve you.";
                //else if (atEnd)
                //    line += "Colonial Chapel Funeral Home the opportunity to serve you.";
                //else
                //    line += "the opportunity to serve you.";

                line = "";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n\n\n");

                line = "Sincerely,";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n\n\n\n\n");

                line = manager;
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                //line = "                                    Manager";
                //rtb3.SelectionAlignment = HorizontalAlignment.Left;
                //G1.Toggle_Font(rtb3, letterFont, letterSize);
                //rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                noticeCount++;
            }
            continuousPrint = true;
            rtb2.Document.RtfText = rtb3.Rtf;
            result = MessageBox.Show("Do you want to save these notices to the Database?", "Lapse Notices Generated Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                DateTime date = DateTime.Now;
                string sDate = date.ToString("MM/dd/yyyy");
                string record = "";
                string noticeRecord = G1.SaveToLapseDatabase(rtb3.Rtf, "PaidOff");
                if (!String.IsNullOrWhiteSpace(noticeRecord))
                {
                    contractList = contractList.TrimEnd(',');
                    string[] Lines = contractList.Split(',');
                    for (int i = 0; i < Lines.Length; i++)
                    {
                        contract = Lines[i].Trim();
                        if (!String.IsNullOrWhiteSpace(contract))
                        {
                            record = G1.create_record("lapse_list", "type", "-1");
                            if (!G1.BadRecord("lapse_list", record))
                            {
                                G1.update_db_table("lapse_list", "record", record, new string[] { "contractNumber", contract, "noticeDate", sDate, "type", "trust", "noticeRecord", noticeRecord, "detail", "Paid Off Notice" });
                            }
                        }
                    }
                }
            }

            this.Cursor = Cursors.Default;

            printPreviewToolStripMenuItem_Click(null, null);
            continuousPrint = false;
        }
        /****************************************************************************************/
        private void GenerateNoticesx(DataTable dt)
        {
            this.Cursor = Cursors.WaitCursor;
            int count = 0;
            string str = "";
            string contract = "";
            string miniContract = "";
            string trust = "";
            string loc = "";
            string line = "";
            string funeralHomeName = "";
            string name = "";
            string address = "";
            string address2 = "";
            string city = "";
            string state = "";
            string zip = "";
            string zip2 = "";

            string mailAddress = "";
            string mailAddress2 = "";
            string mailCity = "";
            string mailState = "";
            string mailZip = "";
            string mailZip2 = "";

            double payment = 0D;
            double balanceDue = 0D;
            string money = "";
            string prefix = "";
            string suffix = "";
            string fname = "";
            string lname = "";
            string mname = "";
            string pronown = "";
            string gender = "";
            string select = "";

            string POBox = "";
            string POCity = "";
            string POState = "";
            string POZip = "";
            string funeralPhoneNumber = "";
            string manager = "";
            string signer = "";

            string agentCode = "";
            string agentName = "";
            DataTable agentDt = null;

            RichTextBox rtb3 = new RichTextBox();
            rtb3.Font = new Font("", 9);

            int padLeft = 10;

            //int padright = GetLapseNoticeValues("Lapse Notices Left Side Width", "30");
            //int padTop = GetLapseNoticeValues("Lapse Notices Top Border Lines");
            //int padLeft = GetLapseNoticeValues("Lapse Notices Left Border Spaces");
            //int padBottom = GetLapseNoticeValues("Lapse Notices Bottom Border Lines");
            //int padToCustomer = GetLapseNoticeValues("Lapse Notices Lines Prior to Customer");
            //int tof = GetLapseNoticeValues("TOF after X Notices");

            //DateTime lapseDate = this.dateTimePickerAsOf.Value;

            rtb2.Document.Text = "";
            int noticeCount = 0;

            double allowInsurance = 0D;

            int lastRow = dt.Rows.Count;
            //            lastRow = 5; // For Testing
            string letterFont = "Lucida Sans Unicode";
            letterFont = "Lucida Console";
            //letterFont = "Times New Roman";
            //letterFont = "Courier";
            //letterFont = "Roboto Mono";
            float letterSize = 14F;
            padLeft = 8;

            bool found = false;
            for (int i = 0; i < lastRow; i++)
            {
                select = dt.Rows[i]["select"].ObjToString();
                if (select == "1")
                {
                    found = true;
                    break;
                }
            }

            DialogResult result;

            bool doAll = false;
            if (!found)
            {
                result = MessageBox.Show("There are NO contracts selected!\nDo you want to generate letters for all contracts?", "Paid Off Letters Generated Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    this.Cursor = Cursors.Default;
                    return;
                }
                doAll = true;
            }

            string contractList = "";

            for (int i = 0; i < lastRow; i++)
            {
                if (!doAll)
                {
                    select = dt.Rows[i]["select"].ObjToString();
                    if (select != "1")
                        continue;
                }
                contract = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                contractList += contract + ",";
                balanceDue = dt.Rows[i]["balanceDue"].ObjToDouble();
                payment = dt.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                miniContract = Trust85.decodeContractNumber(contract, ref trust, ref loc);
                PastDue.LocateFuneralHome(loc, ref funeralHomeName, ref address, ref city, ref state, ref zip, ref POBox, ref POCity, ref POState, ref POZip, ref funeralPhoneNumber, ref manager, ref signer);
                if (!String.IsNullOrWhiteSpace(signer))
                    manager = signer;

                pronown = " ";
                agentCode = dt.Rows[i]["agentCode"].ObjToString();
                agentName = "";
                agentDt = G1.get_db_data("Select * from `agents` where `agentCode` = '" + agentCode + "';");
                if (agentDt.Rows.Count > 0)
                {
                    agentName = agentDt.Rows[0]["firstName"].ObjToString() + " " + agentDt.Rows[0]["lastName"].ObjToString();
                    gender = agentDt.Rows[0]["gender"].ObjToString();
                    if (gender == "F")
                        pronown = "her";
                }

                if (noticeCount > 0)
                    rtb3.AppendText("\f");

                rtb3.SelectionAlignment = HorizontalAlignment.Center;
                G1.Toggle_Font(rtb3, "Times New Roman", 14f);
                G1.Toggle_Bold(rtb3, true, false);
                rtb3.AppendText("\n" + funeralHomeName + "\n");

                rtb3.SelectionAlignment = HorizontalAlignment.Center;
                G1.Toggle_Font(rtb3, "Times New Roman", 14f);
                rtb3.AppendText(address + "\n");
                if (!String.IsNullOrWhiteSpace(POBox))
                {
                    rtb3.SelectionAlignment = HorizontalAlignment.Center;
                    G1.Toggle_Font(rtb3, "Times New Roman", 14f);
                    rtb3.AppendText("Post Office Box " + POBox + "\n");
                }
                rtb3.SelectionAlignment = HorizontalAlignment.Center;
                G1.Toggle_Font(rtb3, "Times New Roman", 14f);
                rtb3.AppendText(city + ", " + state + "  " + zip + "\n");

                rtb3.SelectionAlignment = HorizontalAlignment.Center;
                G1.Toggle_Font(rtb3, "Times New Roman", 14f);
                rtb3.AppendText(funeralPhoneNumber + "\n\n");

                DateTime now = DateTime.Now;
                string nowMonth = DateTime.Now.ToString("MMMM");

                str = nowMonth + " " + now.Day.ToString() + ", " + now.Year.ToString("D4");
                rtb3.SelectionAlignment = HorizontalAlignment.Center;
                G1.Toggle_Font(rtb3, "Times New Roman", 14f);
                rtb3.AppendText(str + "\n");


                //rtb3.SelectAll();
                //rtb3.SelectionAlignment = HorizontalAlignment.Center;

                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                rtb3.AppendText("\n\n\n\n");
                name = "";
                prefix = dt.Rows[i]["prefix"].ObjToString();
                suffix = dt.Rows[i]["suffix"].ObjToString();
                fname = dt.Rows[i]["firstName"].ObjToString();
                lname = dt.Rows[i]["lastName"].ObjToString();
                mname = dt.Rows[i]["middleName"].ObjToString();
                if (!String.IsNullOrWhiteSpace(prefix))
                    name = prefix + " ";
                name += fname + " ";
                if (!String.IsNullOrWhiteSpace(mname))
                    name += mname + " ";
                name += lname;
                if (!String.IsNullOrWhiteSpace(suffix))
                    name += ", " + suffix;

                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + name + "\n");

                address = dt.Rows[i]["address1"].ObjToString(); // Home Address
                //address2 = dt.Rows[i]["address2"].ObjToString();
                //if ( address.IndexOf( address2) < 0 )
                //    address += address2;

                mailAddress = dt.Rows[i]["mailAddress1"].ObjToString(); // Mailing Address
                if (!String.IsNullOrWhiteSpace(mailAddress))
                {
                    //mailAddress2 = dt.Rows[i]["mailAddress2"].ObjToString();
                    //if (mailAddress.IndexOf(mailAddress2) < 0)
                    //    mailAddress += mailAddress2;
                    address = mailAddress;
                }
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + address + "\n");

                address = dt.Rows[i]["address2"].ObjToString(); // Home Address 2

                mailAddress = dt.Rows[i]["mailAddress2"].ObjToString(); // Mailing Address
                if (!String.IsNullOrWhiteSpace(mailAddress))
                    address = mailAddress2;
                //{
                //    mailAddress2 = dt.Rows[i]["mailAddress2"].ObjToString();
                //    if (mailAddress.IndexOf(mailAddress2) < 0)
                //        mailAddress += mailAddress2;
                //    address = mailAddress;
                //}
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + address + "\n");

                city = dt.Rows[i]["city"].ObjToString();
                state = dt.Rows[i]["state"].ObjToString();
                zip = dt.Rows[i]["zip1"].ObjToString();
                str = dt.Rows[i]["zip2"].ObjToString();
                if (!String.IsNullOrWhiteSpace(str))
                {
                    if (str != "0")
                        zip += "-" + str;
                }
                str = city + ", " + state + "  " + zip;

                if (!String.IsNullOrWhiteSpace(mailAddress))
                {
                    mailCity = dt.Rows[i]["mailCity"].ObjToString();
                    mailState = dt.Rows[i]["mailState"].ObjToString();
                    mailZip = dt.Rows[i]["mailZip1"].ObjToString();
                    str = dt.Rows[i]["mailZip2"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(str))
                    {
                        if (str != "0")
                            zip += "-" + str;
                    }
                    str = mailCity + ", " + mailState + "  " + mailZip;
                }

                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + str + "\n\n");

                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + "Dear " + name + ":" + "\n\n");

                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                line = "We are pleased to notify you that your contract";
                rtb3.AppendText("".PadLeft(padLeft + 5) + line + "\n");

                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                line = contract + " is fully paid. If your contract is fully or";
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                //line = contract + " is fully paid. For itimiized details, please refer to your contract." +
                //"If your contract is fully or";

                allowInsurance = dt.Rows[i]["allowInsurance"].ObjToDouble();

                line = "partially funded by your insurance policy, please continue";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                line = "to pay the insurance premiums to keep the policy in force.";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                line = "";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n\n");

                line = "Congratulations on completing payments on your pre-";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft + 5) + line + "\n");

                line = "arranged funeral contract.  If you have any questions";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                line = "concerning your contract, or perhaps would like to add";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                line = "other merchandise or upgrade your selection, please call me";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                line = "and I will be happy to help you.";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n\n");

                line = "Pre-arranging funeral services is one of the most";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft + 5) + line + "\n");

                line = "caring acts you can do for your family. We are sure your";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                line = "family will be very thankful that you did this. You may";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                line = "know of someone else among your family or friends that you";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                line = "feel would benefit from learning more about pre-";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                line = "arrangements, if so please let me know. You can reach me";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                line = "at " + funeralPhoneNumber + "."; ;
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n\n");

                //line = funeralPhoneNumber + ".";
                //rtb3.SelectionAlignment = HorizontalAlignment.Left;
                //G1.Toggle_Font(rtb3, letterFont, letterSize);
                //rtb3.AppendText("".PadLeft(padLeft) + line + "\n\n");

                name = funeralHomeName;
                //if ( String.IsNullOrWhiteSpace ( name ))
                //{
                //    if (contract.ToUpper().IndexOf("WF") == 0)
                //        name = "Wright and Ferguson Funeral Home";
                //}
                //bool gotColonial = false;
                //bool atEnd = false;
                //if (name.IndexOf("Colonial Chapel Funeral Home") == 0)
                //{
                //    name = name.Replace("Colonial Chapel Funeral Home", "").Trim();
                //    gotColonial = true;
                //}
                //else if (name.IndexOf("Colonial Chapel Funeral Home") > 0)
                //{
                //    name = name.Replace("Colonial Chapel Funeral Home", "").Trim();
                //    atEnd = true;
                //}
                //if (name.IndexOf("of") == 0)
                //    name = name.Replace("of", "");
                //else if (name.IndexOf("Of") == 0)
                //    name = name.Replace("Of", "");
                //name = name.Trim();
                //line = "We thank you for allowing ";
                //if (gotColonial)
                //    line += "Colonial Chapel Funeral Home of";
                //else
                //    line += name;
                line = "We thank you for allowing " + name + " the opportunity to serve you.";

                string[] splitLines = G1.WordWrap(line, 55);
                for (int k = 0; k < splitLines.Length; k++)
                {
                    rtb3.SelectionAlignment = HorizontalAlignment.Left;
                    G1.Toggle_Font(rtb3, letterFont, letterSize);
                    if (k == 0)
                        rtb3.AppendText("".PadLeft(padLeft + 5) + splitLines[k] + "\n");
                    else
                        rtb3.AppendText("".PadLeft(padLeft) + splitLines[k] + "\n");
                }

                //line = "";
                //if (gotColonial)
                //    line += name + " the opportunity to serve you.";
                //else if (atEnd)
                //    line += "Colonial Chapel Funeral Home the opportunity to serve you.";
                //else
                //    line += "the opportunity to serve you.";

                line = "";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                line = "                                    Sincerely,";
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n\n\n\n\n");

                line = "                                    " + manager;
                rtb3.SelectionAlignment = HorizontalAlignment.Left;
                G1.Toggle_Font(rtb3, letterFont, letterSize);
                rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                //line = "                                    Manager";
                //rtb3.SelectionAlignment = HorizontalAlignment.Left;
                //G1.Toggle_Font(rtb3, letterFont, letterSize);
                //rtb3.AppendText("".PadLeft(padLeft) + line + "\n");

                noticeCount++;
            }
            continuousPrint = true;
            rtb2.Document.RtfText = rtb3.Rtf;
            result = MessageBox.Show("Do you want to save these notices to the Database?", "Lapse Notices Generated Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                DateTime date = DateTime.Now;
                string sDate = date.ToString("MM/dd/yyyy");
                string record = "";
                string noticeRecord = G1.SaveToLapseDatabase(rtb3.Rtf, "PaidOff");
                if (!String.IsNullOrWhiteSpace(noticeRecord))
                {
                    contractList = contractList.TrimEnd(',');
                    string[] Lines = contractList.Split(',');
                    for (int i = 0; i < Lines.Length; i++)
                    {
                        contract = Lines[i].Trim();
                        if (!String.IsNullOrWhiteSpace(contract))
                        {
                            record = G1.create_record("lapse_list", "type", "-1");
                            if (!G1.BadRecord("lapse_list", record))
                            {
                                G1.update_db_table("lapse_list", "record", record, new string[] { "contractNumber", contract, "noticeDate", sDate, "type", "trust", "noticeRecord", noticeRecord, "detail", "Paid Off Notice" });
                            }
                        }
                    }
                }
            }

            this.Cursor = Cursors.Default;

            printPreviewToolStripMenuItem_Click(null, null);
            continuousPrint = false;
        }
        /****************************************************************************************/
        private void btnGenerateLetters_Click(object sender, EventArgs e)
        {
            PaidOutMenu_Click(null, null);
        }
        /****************************************************************************************/
        private void btnCombine_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataView tempview = dt.DefaultView;
            tempview.Sort = "contractNumber asc";
            dt = tempview.ToTable();
            string contractNumber = "";
            string contract = "";
            double trust85 = 0D;
            double trust100 = 0D;
            double trust85P = 0D;
            double trust100P = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    continue;
                for (int j = (i + 1); j < dt.Rows.Count; j++)
                {
                    contract = dt.Rows[j]["contractNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(contractNumber))
                        continue;
                    if (contract != contractNumber)
                        break;
                    trust85P = dt.Rows[i]["trust85P"].ObjToDouble();
                    trust100P = dt.Rows[i]["trust100P"].ObjToDouble();
                    trust85 = dt.Rows[j]["trust85P"].ObjToDouble();
                    trust100 = dt.Rows[j]["trust100P"].ObjToDouble();
                    trust85P += trust85;
                    trust100P += trust100;
                    dt.Rows[i]["trust85P"] = trust85P;
                    dt.Rows[i]["trust100P"] = trust100P;
                    dt.Rows[j]["contractNumber"] = "";
                }
            }
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    dt.Rows.RemoveAt(i);
            }
            dgv.DataSource = dt;
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void chkACH_CheckedChanged(object sender, EventArgs e)
        {
            if ( chkACH.Checked )
            {
                this.dateTimePicker3.Visible = true;
                this.dateTimePicker4.Visible = true;
            }
            else
            {
                this.dateTimePicker3.Visible = false;
                this.dateTimePicker4.Visible = false;
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >=0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf ( "0001") >= 0 )
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    if (date.Year < 10)
                        e.DisplayText = "";
                    else
                        e.DisplayText = date.ToString("yyyy-MM-dd");
                        //e.DisplayText = date.ToString("MM/dd/yyyy");
                }
            }
        }
        /****************************************************************************************/
        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            if (G1.get_column_number(dt, "select") < 0)
                dt.Columns.Add("select");
            if (chkSelectAll.Checked)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["select"] = "1";
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["select"] = "0";
            }
            dgv.DataSource = dt;
            dgv.RefreshDataSource();
            dgv.Refresh();
        }

        private void markCustomerAsLapsedToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void generateNoticesToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        /****************************************************************************************/
        private void gridMain7_CustomDrawGroupRow(object sender, RowObjectCustomDrawEventArgs e)
        {
            //var view = (GridView)sender;
            //var info = (GridGroupRowInfo)e.Info;
            //var caption = info.Column.Caption;
            //if (info.Column.Caption == string.Empty)
            //{
            //    caption = info.Column.ToString();
            //}
            //info.GroupText = $"{caption} : {info.GroupValueText} ({view.GetChildRowCount(e.RowHandle)})";
        }
        /****************************************************************************************/
        private void gridMain7_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
                if (footerCount >= 1)
                {
                    footerCount = 0;
                    AddFooter(e);
                    //if (chkSort.Checked || autoRun)
                    //    e.PS.InsertPageBreak(e.Y);
                }
            }
        }
        /***********************************************************************************************/
//        private void AddFooter(DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        private void AddFooter(DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            TextBrick tb = e.PS.CreateTextBrick() as TextBrick;
            tb.Text = "This is an Extra Footer!";
            tb.Font = new Font(tb.Font, FontStyle.Bold);
            tb.HorzAlignment = DevExpress.Utils.HorzAlignment.Near;
            tb.Padding = new PaddingInfo(5, 0, 0, 0);
            tb.BackColor = Color.LightGray;
            tb.ForeColor = Color.Black;
            // Get the client page width. 
            SizeF clientPageSize = (e.BrickGraphics as BrickGraphics).ClientPageSize;
            float textBrickHeight = e.Graphics.MeasureString(tb.Text, tb.Font).Height + 4;
            // Calculate a rectangle for the brick and draw the brick. 
            RectangleF textBrickRect = new RectangleF(0, e.Y, (int)clientPageSize.Width, textBrickHeight);
            e.BrickGraphics.DrawBrick(tb, textBrickRect);
            // Adjust the current Y position to print the following row below the brick. 
            e.Y += (int)textBrickHeight;
        }
        /****************************************************************************************/
        private double lastDBR = 0D;
        private double lastTrust85 = 0D;
        private void gridMain7_CustomDrawFooterCell(object sender, FooterCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "TRUST85P")
            {
                return;
            }
            else if (e.Column.FieldName.ToUpper() == "DBR")
            {
                return;
            }
        }
        /****************************************************************************************/
        private double totalPrice = 0D;
        private double totalTrust85 = 0D;
        private double localTrust85 = 0D;

        private double totalTrust100 = 0D;

        private double group1 = 0D;
        private double group2 = 0D;
        private double group3 = 0D;
        private void gridMain7_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            int summaryID = 0;
            if ( e.IsTotalSummary )
            {
            }
            if ( e.IsGroupSummary )
            {
            }

            if (e.SummaryProcess == CustomSummaryProcess.Start)
            {
                if ( e.IsTotalSummary )
                {
                    group1 = 0D;
                    group2 = 0D;
                    group3 = 0D;
                }
                totalPrice = 0D;
                totalTrust85 = 0D;
                GridView view = sender as GridView;
                summaryID = Convert.ToInt32((e.Item as GridSummaryItem).Tag);
                if (summaryID == 3)
                    localTrust85 = 0D;
                if (summaryID == 2)
                    totalTrust100 = 0D;
            }
            double dbr = 0D;
            double trust85 = 0D;
            double trust100 = 0D;
            string location = "";
            DataTable dt = (DataTable)dgv7.DataSource;
            if (e.SummaryProcess == CustomSummaryProcess.Calculate)
            {
                GridView view = sender as GridView;
                summaryID = Convert.ToInt32((e.Item as GridSummaryItem).Tag);
                switch (summaryID)
                {
                    case 1: // The total summary calculated against the 'UnitPrice' column.  
                        dbr = gridMain7.GetRowCellValue(e.RowHandle, "dbr").ObjToDouble();
                        trust85 = gridMain7.GetRowCellValue(e.RowHandle, "trust85P").ObjToDouble();
                        location = gridMain7.GetRowCellValue(e.RowHandle, "Location Name").ObjToString();
                        //totalPrice += trust85 - dbr;
                        //totalPrice = -999D;
                        if ( dbr <= 0D )
                            totalPrice += trust85;

                        if (e.IsTotalSummary)
                            group1 = totalPrice;
                        break;
                    case 2: // The total summary calculated against the 'UnitPrice' column.  
                        dbr = gridMain7.GetRowCellValue(e.RowHandle, "dbr").ObjToDouble();
                        trust100 = gridMain7.GetRowCellValue(e.RowHandle, "trust100P").ObjToDouble();
                        location = gridMain7.GetRowCellValue(e.RowHandle, "Location Name").ObjToString();
                        if ( location.ToUpper().IndexOf ("SHARK") >= 0 )
                        {
                        }
                        //totalTrust85 += trust85 - dbr;
                        if (dbr <= 0D)
                            totalTrust85 += trust100;
                        //totalTrust85 = -888D;
                        if ( e.IsTotalSummary )
                        {
                            if (dbr <= 0D)
                                totalTrust100 += trust100;
                        }
                        if (e.IsTotalSummary )
                            group2 = totalTrust100;
                        break;
                    case 3: // The total summary calculated against the 'UnitPrice' column.  
                        dbr = gridMain7.GetRowCellValue(e.RowHandle, "dbr").ObjToDouble();
                        trust85 = gridMain7.GetRowCellValue(e.RowHandle, "trust85P").ObjToDouble();
                        //localTrust85 += trust85 - dbr;
                        if (dbr <= 0D)
                            localTrust85 += trust85;
                        if (e.IsTotalSummary)
                            group3 = localTrust85;
                        //localTrust85 = -777D;
                        break;
                }
            }
            if (e.SummaryProcess == CustomSummaryProcess.Finalize)
            {
                GridView view = sender as GridView;
                summaryID = Convert.ToInt32((e.Item as GridSummaryItem).Tag);
                switch (summaryID)
                {
                    case 1:
                        e.TotalValue = totalPrice;
                        break;
                    case 2:
                        e.TotalValue = totalTrust85;
                        if (e.IsTotalSummary)
                            e.TotalValue = totalTrust100;
                        break;
                    case 3:
                        if ( e.IsTotalSummary )
                        {
                        }
                        e.TotalValue = localTrust85;
                        break;
                }
            }
        }
        /****************************************************************************************/
        private double totalPrice_8 = 0D;
        private double totalTrust85_8 = 0D;
        private double localTrust85_8 = 0D;
        private void gridMain8_CustomSummaryCalculate(object sender, CustomSummaryEventArgs e)
        {
            int summaryID = 0;
            if (e.SummaryProcess == CustomSummaryProcess.Start)
            {
                totalPrice_8 = 0D;
                totalTrust85_8 = 0D;
                GridView view = sender as GridView;
                summaryID = Convert.ToInt32((e.Item as GridSummaryItem).Tag);
                if (summaryID == 3)
                    localTrust85_8 = 0D;
            }
            double dbr = 0D;
            double trust85 = 0D;
            if (e.SummaryProcess == CustomSummaryProcess.Calculate)
            {
                GridView view = sender as GridView;
                summaryID = Convert.ToInt32((e.Item as GridSummaryItem).Tag);
                switch (summaryID)
                {
                    case 1: // The total summary calculated against the 'UnitPrice' column.  
                        dbr = gridMain8.GetRowCellValue(e.RowHandle, "dbr").ObjToDouble();
                        trust85 = gridMain8.GetRowCellValue(e.RowHandle, "trust85P").ObjToDouble();
                        totalPrice_8 += trust85 - dbr;
                        break;
                    case 2: // The total summary calculated against the 'UnitPrice' column.  
                        dbr = gridMain8.GetRowCellValue(e.RowHandle, "dbr").ObjToDouble();
                        trust85 = gridMain8.GetRowCellValue(e.RowHandle, "trust85P").ObjToDouble();
                        totalTrust85_8 += trust85 - dbr;
                        break;
                    case 3: // The total summary calculated against the 'UnitPrice' column.  
                        dbr = gridMain8.GetRowCellValue(e.RowHandle, "dbr").ObjToDouble();
                        trust85 = gridMain8.GetRowCellValue(e.RowHandle, "trust85P").ObjToDouble();
                        localTrust85_8 += trust85 - dbr;
                        break;
                }
            }
            if (e.SummaryProcess == CustomSummaryProcess.Finalize)
            {
                GridView view = sender as GridView;
                summaryID = Convert.ToInt32((e.Item as GridSummaryItem).Tag);
                switch (summaryID)
                {
                    case 1:
                        e.TotalValue = totalPrice_8;
                        break;
                    case 2:
                        e.TotalValue = totalTrust85_8;
                        break;
                    case 3:
                        e.TotalValue = localTrust85_8;
                        break;
                }
            }
        }
        /****************************************************************************************/
        private double paymentsDBR = 0D;
        private void gridMain_CustomSummaryCalculate(object sender, CustomSummaryEventArgs e)
        {
            int summaryID = 0;
            if (e.SummaryProcess == CustomSummaryProcess.Start)
            {
                paymentsDBR = 0D;
                GridView view = sender as GridView;
                summaryID = Convert.ToInt32((e.Item as GridSummaryItem).Tag);
            }
            double dbr = 0D;
            double trust85 = 0D;
            if (e.SummaryProcess == CustomSummaryProcess.Calculate)
            {
                GridView view = sender as GridView;
                summaryID = Convert.ToInt32((e.Item as GridSummaryItem).Tag);
                switch (summaryID)
                {
                    case 1: // The total summary calculated against the 'UnitPrice' column.  
                        dbr = gridMain.GetRowCellValue(e.RowHandle, "dbr").ObjToDouble();
                        trust85 = gridMain.GetRowCellValue(e.RowHandle, "trust85P").ObjToDouble();
                        paymentsDBR += trust85 - dbr;
                        break;
                }
            }
            if (e.SummaryProcess == CustomSummaryProcess.Finalize)
            {
                GridView view = sender as GridView;
                summaryID = Convert.ToInt32((e.Item as GridSummaryItem).Tag);
                switch (summaryID)
                {
                    case 1:
                        e.TotalValue = paymentsDBR;
                        break;
                }
            }
        }
        /****************************************************************************************/
        private void gridMain7_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
            if (e.Column.FieldName.ToUpper() == "TRUST85P" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                DataTable dt = (DataTable)dgv7.DataSource;
                double dbr = dt.Rows[e.ListSourceRowIndex]["DBR"].ObjToDouble();
                if ( dbr > 0D)
                {
                    string text = e.DisplayText;
                    e.DisplayText = "0.00";
                }
            }
            if (e.Column.FieldName.ToUpper() == "TRUST100P" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                DataTable dt = (DataTable)dgv7.DataSource;
                double dbr = dt.Rows[e.ListSourceRowIndex]["DBR"].ObjToDouble();
                if (dbr > 0D)
                {
                    string text = e.DisplayText;
                    e.DisplayText = "0.00";
                }
            }
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
        private void compareBalanceSheetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!addedMismatchTab)
            {
                tabControl1.TabPages.Add(tabMismatched);
                addedMismatchTab = true;
            }

            DataTable balanceDt = WeeklyClose.balanceDt;
            if (balanceDt == null)
            {
                MessageBox.Show("***ERROR*** You must run the Weekly/Monthly Balance Sheet First!");
                return;
            }
            DataTable dt = balanceDt;
            DataView tempview = dt.DefaultView;
            tempview.Sort = "contractNumber,trust100P";
            balanceDt = tempview.ToTable();

            DataTable dx = (DataTable)dgv8.DataSource;
            tempview = dx.DefaultView;
            tempview.Sort = "contractNumber,trust100P";
            dx = tempview.ToTable();

            string contractNumber = "";
            DataRow[] dR1 = null;
            DataRow[] dR2 = null;
            DataRow dRow = null;

            double trust100 = 0D;
            double dValue = 0D;

            DataTable badDt = dx.Clone();
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString().Trim();
                dR1 = dx.Select("contractNumber='" + contractNumber + "'");
                dR2 = dt.Select("contractNumber='" + contractNumber + "'");
                if ( dR1.Length != dR2.Length)
                {
                    dRow = badDt.NewRow();
                    dRow["contractNumber"] = contractNumber;
                    dRow["lastName"] = "Bad Row Count";
                    badDt.Rows.Add(dRow);
                }
                else
                {
                    for ( int j=0; j<dR1.Length; j++)
                    {
                        trust100 = dR1[j]["trust100P"].ObjToDouble();
                        dValue = dR2[j]["trust100P"].ObjToDouble();
                        if ( trust100 != dValue)
                        {
                            dRow = badDt.NewRow();
                            dRow["contractNumber"] = contractNumber;
                            dRow["lastName"] = "Bad Trust100";
                            badDt.Rows.Add(dRow);
                        }
                    }
                }
            }
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                dR1 = dx.Select("contractNumber='" + contractNumber + "'");
                if ( dR1.Length <= 0)
                {
                    dRow = badDt.NewRow();
                    dRow["contractNumber"] = contractNumber;
                    dRow["lastName"] = "Not in Remit";
                    badDt.Rows.Add(dRow);
                }
            }
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString().Trim();
                dR1 = dt.Select("contractNumber='" + contractNumber + "'");
                if (dR1.Length <= 0)
                {
                    dRow = badDt.NewRow();
                    dRow["contractNumber"] = contractNumber;
                    dRow["lastName"] = "Not in Payments";
                    badDt.Rows.Add(dRow);
                }
            }
            dgv12.DataSource = badDt;
        }
        private bool addedMismatchTab = false;
        /****************************************************************************************/
        private void compareTrust2013 ()
        {
            if (!addedMismatchTab)
            {
                tabControl1.TabPages.Add(tabMismatched);
                addedMismatchTab = true;
            }

            DataTable balanceDt = TrustReports.trustReportDt;
            if (balanceDt == null)
            {
                MessageBox.Show("***ERROR*** You must Pull Old Data from Trust Repoprt First!");
                return;
            }
            DataTable dt = balanceDt;
            DataView tempview = dt.DefaultView;
            tempview.Sort = "contractNumber,paymentCurrMonth";
            balanceDt = tempview.ToTable();

//            DataTable dx = (DataTable)dgv8.DataSource;
            DataTable dx = (DataTable)dgv.DataSource;
            tempview = dx.DefaultView;
            tempview.Sort = "contractNumber,trust85P";
            dx = tempview.ToTable();

            string oldContractNumber = "";
            string contractNumber = "";
            double oldTrust85 = 0D;
            double newTrust85 = 0D;
            int oldRow = -1;
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString().Trim();
                //if (contractNumber == "E21044LI")
                //{
                //    Trust85.FindContract(dx, contractNumber);
                //}
                if ( contractNumber == oldContractNumber)
                {
                    oldTrust85 = dx.Rows[oldRow]["trust85P"].ObjToDouble();
                    newTrust85 = dx.Rows[i]["trust85P"].ObjToDouble();
                    dx.Rows[oldRow]["trust85P"] = oldTrust85 + newTrust85;
                    dx.Rows[i]["contractNumber"] = "";
                    continue;
                }
                else
                    newTrust85 = dx.Rows[i]["trust85P"].ObjToDouble();
                oldRow = i;
                oldContractNumber = contractNumber;
            }

            DataRow[] dR1 = null;
            DataRow[] dR2 = null;
            DataRow dRow = null;

            double trust85 = 0D;
            double dValue = 0D;

            DataTable badDt = dx.Clone();
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString().Trim();
                //if ( contractNumber == "E21044LI")
                //{
                //}
                dR1 = dx.Select("contractNumber='" + contractNumber + "'");
                dR2 = dt.Select("contractNumber='" + contractNumber + "'");
                if ( dR1.Length > 0 && dR2.Length > 0 )
                {
                    trust85 = dR1[0]["trust85P"].ObjToDouble();
                    trust85 = G1.RoundValue(trust85);
                    dValue = dR2[0]["paymentCurrMonth"].ObjToDouble();
                    dValue = G1.RoundValue(dValue);
                    if (trust85 != dValue)
                    {
                        dRow = badDt.NewRow();
                        dRow["contractNumber"] = contractNumber;
                        dRow["trust85P"] = trust85;
                        dRow["trust100P"] = dValue;
                        //dRow["lastName"] = "Bad Trust85";
                        badDt.Rows.Add(dRow);
                    }
                }
                //if (dR1.Length != dR2.Length)
                //{
                //    dRow = badDt.NewRow();
                //    dRow["contractNumber"] = contractNumber;
                //    dRow["lastName"] = "Bad Row Count";
                //    badDt.Rows.Add(dRow);
                //}
                //else
                //{
                //    for (int j = 0; j < dR1.Length; j++)
                //    {
                //        trust85 = dR1[j]["trust85P"].ObjToDouble();
                //        dValue = dR2[j]["paymentCurrMonth"].ObjToDouble();
                //        if (trust85 != dValue)
                //        {
                //            dRow = badDt.NewRow();
                //            dRow["contractNumber"] = contractNumber;
                //            dRow["lastName"] = "Bad Trust85";
                //            badDt.Rows.Add(dRow);
                //        }
                //    }
                //}
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                dR1 = dx.Select("contractNumber='" + contractNumber + "'");
                if (dR1.Length <= 0)
                {
                    dValue = dt.Rows[i]["paymentCurrMonth"].ObjToDouble();
                    if (dValue == 0D)
                        continue;
                    dRow = badDt.NewRow();
                    dRow["contractNumber"] = contractNumber;
                    dRow["lastName"] = "Not in Remit";
                    dRow["trust100P"] = dValue;
                    badDt.Rows.Add(dRow);
                }
            }
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString().Trim();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    continue;
                dR1 = dt.Select("contractNumber='" + contractNumber + "'");
                if (dR1.Length <= 0)
                {
                    dRow = badDt.NewRow();
                    dRow["contractNumber"] = contractNumber;
                    dRow["lastName"] = "Not in Trust2013";
                    dValue = dx.Rows[i]["trust85P"].ObjToDouble();
                    dRow["trust85P"] = dValue;
                    badDt.Rows.Add(dRow);
                }
            }
            G1.NumberDataTable(badDt);
            dgv12.DataSource = badDt;
        }
        /****************************************************************************************/
        private void forceCustomerPaidOffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                DateTime payOffDate = new DateTime(2039, 12, 31);
                dr["dueDate8"] = G1.DTtoMySQLDT(payOffDate);
                G1.update_db_table("contracts", "record", record, new string[] { "dueDate8", "12/31/2039"});
                gridMain.RefreshData();
                dgv.Refresh();
            }
        }
        /****************************************************************************************/
        private void compareTrust2013ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!addedMismatchTab)
            {
                tabControl1.TabPages.Add(tabMismatched);
                addedMismatchTab = true;
            }
            compareTrust2013();
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit8_CheckedChanged(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                string set = dr["letter"].ObjToString();
                if (set == "0")
                    set = "1";
                else
                    set = "0";
                dr["letter"] = set;
                G1.update_db_table("contracts", "record", record, new string[] { "letter", set });
            }
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit9_CheckedChanged(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                string set = dr["pulled"].ObjToString();
                if (set == "0")
                    set = "1";
                else
                    set = "0";
                dr["pulled"] = set;
                G1.update_db_table("contracts", "record", record, new string[] { "pulled", set });
            }
        }
        /****************************************************************************************/
        private void gridMain_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            string str = View.GetRowCellValue(e.RowHandle, "deceasedDate").ObjToString();
            if (str != null)
            {
                DateTime date = str.ObjToDateTime();
                if (date.Year > 100)
                {
                    if (workReport.ToUpper() != "DBR REPORT")
                        e.Appearance.BackColor = Color.LimeGreen;
                }
            }
        }
        /****************************************************************************************/
        private void cmbRunOn_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetupCemeterySummary();
        }
        /***********************************************************************************************/
        private string majorLastLocation = "";
        private string majorLastDetail = "";
        private string lastLocation = "";
        /***********************************************************************************************/
        private void FindLastLocation(DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            majorLastLocation = majorLastDetail;
            lastLocation = "";

            try
            {
                DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null;
                DataTable dt = (DataTable)dgv6.DataSource;
                gMain = gridMain6;
                int rowHandle = e.RowHandle;
                if (rowHandle < 0)
                    return;
                int row = gMain.GetDataSourceRowIndex(rowHandle);
                majorLastDetail = dt.Rows[row]["location"].ObjToString();
            }
            catch ( Exception ex )
            {
            }
        }
        /****************************************************************************************/
        private int printRow = 0;
        private void gridMain6_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            if (majorRunOn.ToUpper() != "CEMETERIES")
                return;
            FindLastLocation(e);
            gridBand6.Caption = majorLastDetail;
            printRow++;
            if (printFirstToo && publicE != null)
            {
                AddHeading(e);
                printFirstToo = false;
            }
            if (e.HasFooter)
            {
                footerCount++;
                if ((footerCount + 1) >= 2)
                {
                    CustomFooter(e, "Location (" + majorLastLocation + ")");
                    printFirstToo = true;
                }
            }
        }
        /***********************************************************************************************/
        private void AddHeading(DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            if (1 == 1)
                return;
            TextBrick tb = e.PS.CreateTextBrick() as TextBrick;
            tb.Text = majorLastDetail;
            tb.Font = new Font(tb.Font, FontStyle.Bold);
            tb.HorzAlignment = DevExpress.Utils.HorzAlignment.Near;
            tb.Padding = new PaddingInfo(5, 0, 0, 0);
            tb.BackColor = Color.LightGray;
            tb.ForeColor = Color.Black;
            // Get the client page width. 
            SizeF clientPageSize = (e.BrickGraphics as BrickGraphics).ClientPageSize;
            float textBrickHeight = e.Graphics.MeasureString(tb.Text, tb.Font).Height + 4;
            // Calculate a rectangle for the brick and draw the brick. 
            RectangleF textBrickRect = new RectangleF(0, e.Y, (int)clientPageSize.Width, textBrickHeight);
            e.BrickGraphics.DrawBrick(tb, textBrickRect);
            // Adjust the current Y position to print the following row below the brick. 
            e.Y += (int)textBrickHeight;
        }
        /***********************************************************************************************/
        private void CustomFooter(DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e, string footer)
        {
            if (String.IsNullOrWhiteSpace(footer))
                return;
            if (1 == 1)
                return;

            // Create a text brick and customize its appearance settings. 
            TextBrick tb = e.PS.CreateTextBrick() as TextBrick;
            tb.Text = footer;
            tb.Font = new Font(tb.Font, FontStyle.Bold);
            tb.HorzAlignment = DevExpress.Utils.HorzAlignment.Near;
            tb.Padding = new PaddingInfo(5, 0, 0, 0);
            tb.BackColor = Color.LightGray;
            tb.ForeColor = Color.Black;
            // Get the client page width. 
            SizeF clientPageSize = (e.BrickGraphics as BrickGraphics).ClientPageSize;
            float textBrickHeight = e.Graphics.MeasureString(tb.Text, tb.Font).Height + 4;
            // Calculate a rectangle for the brick and draw the brick. 
            int y = e.Y;
            //y = y - 100;
            RectangleF textBrickRect = new RectangleF(19, y, (int)clientPageSize.Width - 19, textBrickHeight);
            e.BrickGraphics.DrawBrick(tb, textBrickRect);
            // Adjust the current Y position to print the following row below the brick. 
            e.Y += (int)textBrickHeight;
        }
        /****************************************************************************************/
        private void gridMain6_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (majorRunOn.ToUpper() != "CEMETERIES")
                return;
        }
        /****************************************************************************************/
        private void gridMain6_CustomDrawGroupRow(object sender, RowObjectCustomDrawEventArgs e)
        {
            GridView view = sender as GridView;
            GridGroupRowInfo info = e.Info as GridGroupRowInfo;
            try
            {
                string location = info.GroupText;
                location = location.Replace("Group", "");
                info.GroupText = location;
                //info.GroupText = "";
                //info.GroupValueText = "";
                //info.EditValue = "";
                //string location = view.GetGroupRowValue(e.RowHandle, "location").ObjToString();
                //string location = view.GetGroupRowValue(e.RowHandle, info.Column).ObjToString();
                //int quantity = Convert.ToInt32(view.GetGroupRowValue(e.RowHandle, info.Column));
                //string colorName = getColorName(quantity);
                //info.GroupText += "<color=LightSteelBlue>" + view.GetGroupSummaryText(e.RowHandle) + "</color> ";
            }
            catch ( Exception ex)
            {
            }
        }
        void doc_EndPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            if (e.PrintAction == System.Drawing.Printing.PrintAction.PrintToPrinter)
            {
                // Printing to the printer!
            }
            else if (e.PrintAction == System.Drawing.Printing.PrintAction.PrintToPreview)
            {
                // Printing to the preview dialog!
            }
        }
        /****************************************************************************************/
        //public class ExportToXlsCommandHandler : ICommandHandler
        //{
        //    public virtual void HandleCommand(PrintingSystemCommand command, object[] args, IPrintControl control, ref bool handled)
        //    {
        //        if (!CanHandleCommand(command, control)) 
        //            return;
        //        if (MessageBox.Show("Cash Remit Is Saved!!?\nDo you want to save the data in the database?", "Cash Remit Save Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
        //        {
        //            PaymentsCashRemitted();
        //        }
        //    }
        //    public static void PaymentsCashRemitted()
        //    {
        //        if (cashRemittedDt6 == null)
        //            return;
        //    }
        //    public virtual bool CanHandleCommand(PrintingSystemCommand command, IPrintControl control)
        //    {
        //        // This handler is used for the ExportGraphic command.
        //        return command == PrintingSystemCommand.ExportXls;
        //    }
        //}
        //public class PrintDocumentCommandHandler : ICommandHandler
        //{
        //    public virtual void HandleCommand(PrintingSystemCommand command, object[] args, IPrintControl printControl, ref bool handled)
        //    {
        //        string text = command.ToString();
        //        if (!CanHandleCommand(command, printControl))
        //            return;
        //        if (MessageBox.Show("Contract Is Being Printed!!?", "Contract Printed Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
        //        {
        //        }
        //    }
        //    public virtual bool CanHandleCommand(PrintingSystemCommand command, IPrintControl printControl)
        //    {
        //        return command == PrintingSystemCommand.ExportXls;
        //    }
        //}
        /****************************************************************************************/
        public class CsvExportOptionsEx : ICommandHandler
        {
            public virtual void HandleCommand(PrintingSystemCommand command, object[] args, IPrintControl control, ref bool handled)
            {
                if (!CanHandleCommand(command, control))
                    return;
                if (MessageBox.Show("Cash Remit Is Saved!!?\nDo you want to save the data in the database?", "Cash Remit Save Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
                {
                    PaymentsCashRemitted();
                }
            }
            public static void PaymentsCashRemitted()
            {
                if (cashRemittedDt6 == null)
                    return;
            }
            public virtual bool CanHandleCommand(PrintingSystemCommand command, IPrintControl control)
            {
                if (cashRemittedDt6 == null)
                    return false;

                bool doit = false;
                if (command == PrintingSystemCommand.ExportCsv)
                    doit = true;
                if (command == PrintingSystemCommand.ExportXls )
                    doit = true;
                if (command == PrintingSystemCommand.ExportXlsx)
                    doit = true;
                if (doit)
                {
                    if (MessageBox.Show("Cash Remit Coversheet Print Preview!!?\nDo you want to save the data in the database too?", "Cash Remit Save Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
                    {
                        PaymentsCashRemitted();
                    }
                }
                return command == PrintingSystemCommand.ExportXls;
            }
        }
        /****************************************************************************************/
        public class ExportOptions : ICommandHandler
        {
            public virtual void HandleCommand(PrintingSystemCommand command, object[] args, IPrintControl control, ref bool handled)
            {
            }
            public static void PaymentsCashRemitted()
            {
                if (cashRemittedDt6 == null)
                    return;

                DateTime date = cashRemittedDt.Rows[0]["payDate8"].ObjToDateTime();
                date = paymentsReportDate;
                int days = DateTime.DaysInMonth(date.Year, date.Month);
                date = new DateTime(date.Year, date.Month, days);

                double fdlicDownPayments = 0D;
                double fdlicPayments = 0D;
                for ( int i=0; i<cashRemittedDt6.Rows.Count; i++)
                {
                    fdlicDownPayments += cashRemittedDt6.Rows[i]["fdlicDownPayments"].ObjToDouble();
                    fdlicPayments += cashRemittedDt6.Rows[i]["fdlicMonthly"].ObjToDouble();
                }
                string date1 = date.ToString("yyyy-MM-dd");

                string cmd = "DELETE from `cashremit_coversheet` where `date` = '" + date1 + "';";
                cmd += ";";
                try
                {
                    G1.get_db_data(cmd);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Delete Previous Cash Remit for " + date1 + " " + ex.Message.ToString());
                }

                string record = G1.create_record("cashremit_coversheet", "spare", "-1");
                if (G1.BadRecord("cashremit_coversheet", record))
                    return;
                G1.update_db_table("cashremit_coversheet", "record", record, new string[] { "spare", "", "date", date1, "fdlicDownPayments", fdlicDownPayments.ToString(), "fdlicMonthlyPayments", fdlicPayments.ToString() });
            }
            public virtual bool CanHandleCommand(PrintingSystemCommand command, IPrintControl control)
            {
                if (cashRemittedDt6 == null)
                    return false;
                if (!control.IsVisible())
                    return false;

                bool doit = false;
                if (command == PrintingSystemCommand.ExportCsv)
                    doit = true;
                if (command == PrintingSystemCommand.ExportXls)
                    doit = true;
                if (command == PrintingSystemCommand.ExportXlsx)
                    doit = true;
                if (doit)
                {
                    if (MessageBox.Show("Cash Remit Coversheet is being SAVED!!?\nDo you want to save this data in the database too?", "Cash Remit Save Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
                    {
                        PaymentsCashRemitted();
                    }
                }
                //return command == PrintingSystemCommand.ExportXls;
                return false;
            }
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            
            DialogResult result = new DialogResult();
            if (workTitle == "New Business Report (1.2)")
                result = MessageBox.Show("Are you sure you want to save this data to the database?", "Save New Business Data Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            else
                result = MessageBox.Show("Are you sure you want to save this data to the database?", "Save Cash Remitted Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            try
            {
                if (workTitle == "New Business Report (1.2)")
                    SaveDataNewBusiness(dt);
                else
                {
                    SaveData(dt);
                    string runWhat = cmbRunOn.Text.Trim();
                    if ( runWhat.ToUpper() == "TRUSTS")
                        SaveDBRs(dt);
                }
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void SaveDBRs( DataTable dt )
        {
            string record = "";
            string mod = "";
            string data = "";
            string contractNumber = "";
            DateTime payDate8 = DateTime.Now;
            double payment = 0D;
            DateTime deceasedDate = DateTime.Now;

            DateTime startDate = this.dateTimePicker1.Value;
            DateTime stopDate = this.dateTimePicker2.Value;

            string date1 = startDate.ToString("yyyy-MM-dd");
            string date2 = stopDate.ToString("yyyy-MM-dd");
            string cmd = "DELETE from `dbrs` where `cashRemitStartDate` >= '" + date1 + "' AND `cashRemitStopDate` <= '" + date2 + "' ";
            cmd += ";";
            try
            {
                G1.get_db_data(cmd);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Delete Previous Data " + ex.Message.ToString());
            }

            this.Cursor = Cursors.WaitCursor;

            dt = G1.GetGridViewTable(gridMain, dt);

            double dbr = 0D;
            string paymentType = "";
            double downPayment = 0D;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = G1.create_record("dbrs", "note", "-1");
                if (G1.BadRecord("dbrs", record))
                    return;
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
                dbr = dt.Rows[i]["dbr"].ObjToDouble();
                if (dbr == 0D)
                    continue;
                payDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                downPayment = dt.Rows[i]["downPayment1"].ObjToDouble();
                payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                paymentType = "DP";
                if (payment > 0D)
                    paymentType = "payment";
                G1.update_db_table("dbrs", "record", record, new string[] { "note", "", "contractNumber", contractNumber, "dbr", dbr.ToString(), "payDate8", payDate8.ToString("yyyy-MM-dd"), "deceasedDate", deceasedDate.ToString("yyyy-MM-dd"), "cashRemitStartDate", date1, "cashRemitStopDate", date2, "paymentType", paymentType });
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private bool SaveData(DataTable dt)
        {
            this.Cursor = Cursors.WaitCursor;

            DateTime date = this.dateTimePicker2.Value;
            int days = DateTime.DaysInMonth(date.Year, date.Month);

            string runDate1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-01";
            string runDate2 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + days.ToString("D2");

            if (!DeletePreviousData())
            {
                this.Cursor = Cursors.Default;
                return false;
            }

            DataTable saveDt = dt.Copy();

            if (G1.get_column_number(saveDt, "tmstamp") >= 0)
                saveDt.Columns.Remove("tmstamp");
            if (G1.get_column_number(saveDt, "record") >= 0)
                saveDt.Columns.Remove("record");
            if (G1.get_column_number(saveDt, "runwhat") >= 0)
                saveDt.Columns.Remove("runWhat");
            if (G1.get_column_number(saveDt, "runDate2") >= 0)
                saveDt.Columns.Remove("runDate2");
            if (G1.get_column_number(saveDt, "runDate1") >= 0)
                saveDt.Columns.Remove("runDate1");


            DataColumn ColR3 = saveDt.Columns.Add("runWhat", System.Type.GetType("System.String"));
            ColR3.SetOrdinal(0);// to put the column in position 0;
            DataColumn ColR2 = saveDt.Columns.Add("runDate2", System.Type.GetType("System.String"));
            ColR2.SetOrdinal(0);// to put the column in position 0;
            DataColumn ColR1 = saveDt.Columns.Add("runDate1", System.Type.GetType("System.String"));
            ColR1.SetOrdinal(0);// to put the column in position 0;
            DataColumn Col = saveDt.Columns.Add("record", System.Type.GetType("System.String"));
            Col.SetOrdinal(0);// to put the column in position 0;
            DataColumn Col1 = saveDt.Columns.Add("tmstamp", System.Type.GetType("System.String"));
            Col1.SetOrdinal(0);// to put the column in position 0;

            double dValue = 0D;
            string str = "";
            string runWhat = cmbRunOn.Text.Trim();

            for (int i = 0; i < saveDt.Rows.Count; i++)
            {
                //saveDt.Rows[i]["tmstamp"] = "0000-00-00";
                saveDt.Rows[i]["tmstamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                saveDt.Rows[i]["record"] = "0";
                saveDt.Rows[i]["runDate1"] = runDate1;
                saveDt.Rows[i]["runDate2"] = runDate2;
                saveDt.Rows[i]["runWhat"] = runWhat;
                CleanupData(saveDt, i, "firstName", 80);
                CleanupData(saveDt, i, "lastName", 80);
                CleanupData(saveDt, i, "address1", 80);
                CleanupData(saveDt, i, "address2", 80);
                CleanupData(saveDt, i, "city", 80);
                CleanupData(saveDt, i, "state", 80);
                CleanupData(saveDt, i, "zip1", 80);
                CleanupData(saveDt, i, "notes1", 80);
            }

            MySQL.CleanupTable(saveDt);

            string strFile = "/CashRemitted/CashRemitted_P_" + date.ToString("yyyyMMdd") + ".csv";
            string Server = "C:/rag";
            //Create directory if not exist... Make sure directory has required rights..
            if (!Directory.Exists(Server + "/CashRemitted/"))
                Directory.CreateDirectory(Server + "/CashRemitted/");

            G1.GrantDirectoryAccess ( Server + "/CashRemitted/");
            if (File.Exists(Server + strFile))
                File.Delete(Server + strFile);

            //If file does not exist then create it and right data into it..
            if (!File.Exists(Server + strFile))
            {
                FileStream fs = new FileStream(Server + strFile, FileMode.Create, FileAccess.Write);
                fs.Close();
                fs.Dispose();
            }

            //Generate csv file from where data read

            try
            {
                DateTime saveDate = this.dateTimePicker2.Value;
                //int days = DateTime.DaysInMonth(saveDate.Year, saveDate.Month);
                //                string mySaveDate = saveDate.Year.ToString("D4") + "-" + saveDate.Month.ToString("D2") + "-" + days.ToString("D2") + " 00:00:00";

                var mySaveDate = G1.DTtoMySQLDT(saveDate);

                //for ( int i=0; i<saveDt.Rows.Count; i++)
                //    saveDt.Rows[i]["payDate8"] = mySaveDate;

                MySQL.CreateCSVfile(saveDt, Server + strFile );
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Creating CSV File to load into Database " + ex.Message.ToString());
                this.Cursor = Cursors.Default;
                return false;
            }
            try
            {
                Structures.TieDbTable("cashRemitted", saveDt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Tieing CashRemitted to DataTable " + ex.Message.ToString());
                this.Cursor = Cursors.Default;
                return false;
            }
            try
            {
                G1.conn1.Open();
                MySqlBulkLoader bcp1 = new MySqlBulkLoader(G1.conn1);
                bcp1.TableName = "cashRemitted"; //Create table into MYSQL database...
                bcp1.FieldTerminator = ",";

                bcp1.LineTerminator = "\r\n";
                bcp1.FileName = Server + strFile;
                bcp1.NumberOfLinesToSkip = 0;
                //bcp1.FieldTerminator = "~";
                bcp1.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Bulk Loading CashRemitted to DataTable " + ex.Message.ToString());
            }

            saveDt.Dispose();
            saveDt = null;

            File.Delete(Server + strFile);

            btnSave.Hide();
            btnSave.Refresh();

            this.Cursor = Cursors.Default;
            return true;
        }
        /***********************************************************************************************/
        private bool SaveDataNewBusiness(DataTable dt)
        {
            this.Cursor = Cursors.WaitCursor;

            DateTime date = this.dateTimePicker2.Value;
            int days = DateTime.DaysInMonth(date.Year, date.Month);

            string runDate1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-01";
            string runDate2 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + days.ToString("D2");

            if (!DeletePreviousData( "newBusiness"))
            {
                this.Cursor = Cursors.Default;
                return false;
            }

            DataTable saveDt = dt.Copy();

            if (G1.get_column_number(saveDt, "tmstamp") >= 0)
                saveDt.Columns.Remove("tmstamp");
            if (G1.get_column_number(saveDt, "record") >= 0)
                saveDt.Columns.Remove("record");
            if (G1.get_column_number(saveDt, "runwhat") >= 0)
                saveDt.Columns.Remove("runWhat");
            if (G1.get_column_number(saveDt, "runDate2") >= 0)
                saveDt.Columns.Remove("runDate2");
            if (G1.get_column_number(saveDt, "runDate1") >= 0)
                saveDt.Columns.Remove("runDate1");


            DataColumn ColR3 = saveDt.Columns.Add("runWhat", System.Type.GetType("System.String"));
            ColR3.SetOrdinal(0);// to put the column in position 0;
            DataColumn ColR2 = saveDt.Columns.Add("runDate2", System.Type.GetType("System.String"));
            ColR2.SetOrdinal(0);// to put the column in position 0;
            DataColumn ColR1 = saveDt.Columns.Add("runDate1", System.Type.GetType("System.String"));
            ColR1.SetOrdinal(0);// to put the column in position 0;
            DataColumn Col = saveDt.Columns.Add("record", System.Type.GetType("System.String"));
            Col.SetOrdinal(0);// to put the column in position 0;
            DataColumn Col1 = saveDt.Columns.Add("tmstamp", System.Type.GetType("System.String"));
            Col1.SetOrdinal(0);// to put the column in position 0;

            double dValue = 0D;
            string str = "";
            string runWhat = cmbRunOn.Text.Trim();

            for (int i = 0; i < saveDt.Rows.Count; i++)
            {
                //saveDt.Rows[i]["tmstamp"] = "0000-00-00";
                saveDt.Rows[i]["tmstamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                saveDt.Rows[i]["record"] = "0";
                saveDt.Rows[i]["runDate1"] = runDate1;
                saveDt.Rows[i]["runDate2"] = runDate2;
                saveDt.Rows[i]["runWhat"] = runWhat;
                CleanupData(saveDt, i, "firstName", 80);
                CleanupData(saveDt, i, "lastName", 80);
                CleanupData(saveDt, i, "address1", 80);
                CleanupData(saveDt, i, "address2", 80);
                CleanupData(saveDt, i, "city", 80);
                CleanupData(saveDt, i, "state", 80);
                CleanupData(saveDt, i, "zip1", 80);
            }

            MySQL.CleanupTable(saveDt);

            string strFile = "/CashRemitted/NewBusiness_P_" + date.ToString("yyyyMMdd") + ".csv";
            string Server = "C:/rag";
            //Create directory if not exist... Make sure directory has required rights..
            if (!Directory.Exists(Server + "/CashRemitted/"))
                Directory.CreateDirectory(Server + "/CashRemitted/");

            G1.GrantDirectoryAccess(Server + "/CashRemitted/");
            if (File.Exists(Server + strFile))
                File.Delete(Server + strFile);

            //If file does not exist then create it and right data into it..
            if (!File.Exists(Server + strFile))
            {
                FileStream fs = new FileStream(Server + strFile, FileMode.Create, FileAccess.Write);
                fs.Close();
                fs.Dispose();
            }

            //Generate csv file from where data read

            try
            {
                DateTime saveDate = this.dateTimePicker2.Value;
                //int days = DateTime.DaysInMonth(saveDate.Year, saveDate.Month);
                //                string mySaveDate = saveDate.Year.ToString("D4") + "-" + saveDate.Month.ToString("D2") + "-" + days.ToString("D2") + " 00:00:00";

                var mySaveDate = G1.DTtoMySQLDT(saveDate);

                //for ( int i=0; i<saveDt.Rows.Count; i++)
                //    saveDt.Rows[i]["payDate8"] = mySaveDate;

                MySQL.CreateCSVfile(saveDt, Server + strFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Creating CSV File to load into Database " + ex.Message.ToString());
                this.Cursor = Cursors.Default;
                return false;
            }
            try
            {
                Structures.TieDbTable("newBusiness", saveDt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Tieing New Business to DataTable " + ex.Message.ToString());
                this.Cursor = Cursors.Default;
                return false;
            }
            try
            {
                G1.conn1.Open();
                MySqlBulkLoader bcp1 = new MySqlBulkLoader(G1.conn1);
                bcp1.TableName = "newBusiness"; //Create table into MYSQL database...
                bcp1.FieldTerminator = ",";

                bcp1.LineTerminator = "\r\n";
                bcp1.FileName = Server + strFile;
                bcp1.NumberOfLinesToSkip = 0;
                //bcp1.FieldTerminator = "~";
                bcp1.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Bulk Loading NewBusiness to DataTable " + ex.Message.ToString());
            }

            saveDt.Dispose();
            saveDt = null;

            File.Delete(Server + strFile);

            btnSave.Hide();
            btnSave.Refresh();


            this.Cursor = Cursors.Default;
            return true;
        }
        /***********************************************************************************************/
        private void CleanupData ( DataTable dt, int i, string what, int length )
        {
            try
            {
                dt.Rows[i][what] = G1.try_protect_data(dt.Rows[i][what].ObjToString());
                dt.Rows[i][what] = G1.Truncate(dt.Rows[i][what].ObjToString(), length);
            }
            catch ( Exception ex )
            {
            }
        }
        /***********************************************************************************************/
        private bool DeletePreviousData(string table = "")
        {
            bool success = false;

            //if (!CheckForFutureData(allData))
            //    return false;

            if (String.IsNullOrWhiteSpace(table))
                table = "cashRemitted";
            DateTime date = this.dateTimePicker2.Value;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            string date1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-01";
            string date2 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + days.ToString("D2");

            string runWhat = cmbRunOn.Text.Trim();

            string cmd = "Select * from `" + table + "` WHERE `runDate1` >= '" + date1 + "' AND `runDate2` <= '" + date2 + "' AND `runWhat` = '" + runWhat + "';";
            DataTable ddd = G1.get_db_data(cmd);
            if (ddd.Rows.Count <= 0)
                return true;

            cmd = "DELETE from `" + table + "` where `runDate1` >= '" + date1 + "' AND `runDate2` <= '" + date2 + "' AND `runWhat` = '" + runWhat + "' ";

            cmd += ";";
            try
            {
                G1.get_db_data(cmd);
                success = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Delete Previous Data " + ex.Message.ToString());
            }
            return success;
        }
        /****************************************************************************************/
        private void menuReadPrevious_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            DateTime begin = this.dateTimePicker1.Value;
            DateTime end = this.dateTimePicker2.Value;

            int days = DateTime.DaysInMonth(begin.Year, begin.Month);
            DateTime last = new DateTime(begin.Year, begin.Month, days);
            if (last > end)
            {
                this.Cursor = Cursors.Default;
                return;
            }

            string runDate1 = begin.ToString("yyyy-MM-dd");
            string runDate2 = end.ToString("yyyy-MM-dd");
            string runWhat = cmbRunOn.Text.Trim();

            string cmd = "Select * from `cashRemitted` where `runDate1` = '" + runDate1 + "' AND `runDate2` = '" + runDate2 + "' AND `runWhat` = '" + runWhat + "';";
            if (workTitle == "New Business Report (1.2)")
                cmd = "Select * from `newBusiness` where `runDate1` = '" + runDate1 + "' AND `runDate2` = '" + runDate2 + "' AND `runWhat` = '" + runWhat + "';";
            DataTable dt = G1.get_db_data(cmd);
            dgv.DataSource = dt;

            originalDt = dt;
            previousDateRead = true;

            if (workReport.ToUpper() == "CASH REMITTED REPORT" && !batch)
                SetupCashRemitted(dt);
            else if (workTitle == "Trust Monthly Payment 85% Master Listing (6.2)")
                CleanupPayments(dt);
            else if (workTitle == "New Business Report (1.2)" )
            {
                //PrepareNewBusiness(dt);
                dt = CleanupFutureReporting(dt, runDate1, runDate2);
            }
            else if (workReport.ToUpper() == "DOWN PAYMENTS REPORT")
            {
                //ClearAllPositions();
                //SetDownPayments();
                SetupSelectColumn(dt);
                LoadPaidOffLetters(dt);
                //ResetColumns();
                PrepareNewBusiness(dt);
                PrepareDownPayments(dt);
                SortForDownPayments(dt);
                //dt = CleanupFutureReporting(dt, runDate1, runDate2);
            }
            else if (workReport.ToUpper() == "TRUST EOM REPORT")
            {
                double TotalPayments = 0D;
                double TotalDebits = 0D;
                double TotalCredits = 0D;
                double TotalInterest = 0D;
                double TotalRetained = 0D;

                double downPayment = 0D;
                double ccFee = 0D;
                double dpp = 0D;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    downPayment = dt.Rows[i]["newBusiness"].ObjToDouble();
                    ccFee = dt.Rows[i]["ccFee"].ObjToDouble();
                    dpp = dt.Rows[i]["dpp"].ObjToDouble();
                    if (dpp > 0D)
                        dt.Rows[i]["newBusiness"] = dpp;
                    //if (downPayment != 0D)
                    //    dt.Rows[i]["newBusiness"] = downPayment + ccFee;
                }


                LoadTrustEOM(dt, ref TotalPayments, ref TotalDebits, ref TotalCredits, ref TotalInterest, ref TotalRetained);
                LoadDraftsEOM(dt, TotalPayments, TotalDebits, TotalCredits, TotalInterest, TotalRetained);
                LoadLockBoxEOM(dt);
                LoadManualEOM(dt);
                CombineEOM();
            }

            menuStrip1.BackColor = Color.LightGreen;

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable SetupCashRemitted(DataTable dt)
        {
            //FixSpecialLocations(dt);

            //ProcessDBR(dt);

            //bool foundLI = false;
            //string contractNumber = "";
            //string findRecord = "";


            //DataRow[] ddR = dt.Select("loc='FO'");
            //DailyHistory.RecalcRetained(dt, "interestPaid1");
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    Application.DoEvents();

            //    foundLI = false;
            //    contractNumber = dt.Rows[i]["contractNumber"].ObjToString().Trim();
            //    if (contractNumber.ToUpper().EndsWith("LI"))
            //    {
            //        findRecord = dt.Rows[i]["record2"].ObjToString();
            //        DailyHistory.CalcPaymentData(contractNumber, findRecord, ref newInterest, ref newTrust85, ref newTrust100);
            //        foundLI = true;
            //        dt.Rows[i]["retained"] = newInterest;
            //        dt.Rows[i]["interestPaid1"] = newInterest;
            //        dt.Rows[i]["trust100P"] = newTrust100;
            //        dt.Rows[i]["trust85P"] = newTrust85;
            //        dt.Rows[i]["retained"] = 0D;
            //    }
            //}

            LoadCashRemitted(dt);
            return dt;
        }
        /***********************************************************************************************/
        private string fullPath = "";
        private string format = "";
        //private bool continuousPrint = false;
        /***********************************************************************************************/
        private void generateMassReportToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (workReport.ToUpper() == "CASH REMITTED REPORT")
            {
                generateMassCashRemitReport_Click(sender, e);
                return;
            }

            if (workTitle != "New Business Report (1.2)")
                return;

            string mainReport = "";
            string report = "";
            string locations = "";
            format = "";
            string outputFilname = "";
            string outputDirectory = "";
            string saveActive = SMFS.activeSystem;

            fullPath = "";

            string location = "";
            string loc = "";
            bool foundLocations = false;

            string lastReadOldData = "";
            string newReadOldData = "";

            DataTable dt = null;

            string[] Lines = null;

            DateTime date = dateTimePicker2.Value;

            string yyyy = date.Year.ToString("D4");
            string month = G1.ToMonthName(date);

            DialogResult result = MessageBox.Show("Are you sure you want to RUN the Mass Reports for New Business for " + date.ToString("MM/dd/yyyy") + "?", "Mass Report Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            string cmd = "Select * from `mass_reports` where `mainReport` = 'New Business';";
            DataTable dx = G1.get_db_data(cmd);

            int lastRow = dx.Rows.Count;
            //lastRow = 6;

            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            barImport.Show();
            barImport.Refresh();

            bool gotRiles = false;
            bool lastRiles = false;
            DataTable ddt = (DataTable)dgv.DataSource;
            DataTable tempDt = ddt.Copy();
            ddt = null;
            DataView tempview = tempDt.DefaultView;

            for (int i = 0; i < lastRow; i++)
            {
                try
                {
                    Application.DoEvents();

                    barImport.Value = i + 1;
                    barImport.Refresh();

                    mainReport = dx.Rows[i]["mainReport"].ObjToString();
                    report = dx.Rows[i]["report"].ObjToString();
                    locations = dx.Rows[i]["locations"].ObjToString();
                    format = dx.Rows[i]["format"].ObjToString();
                    outputFilname = dx.Rows[i]["outputFilename"].ObjToString();
                    outputDirectory = dx.Rows[i]["outputDirectory"].ObjToString();

                    outputDirectory = outputDirectory.Replace("2021", yyyy);
                    outputFilname = outputFilname.Replace("yyyy", yyyy);
                    outputFilname = outputFilname.Replace("month", month);

                    fullPath = outputDirectory + "/" + outputFilname;

                    G1.verify_path(outputDirectory);

                    this.Text = mainReport + " " + report + " / " + locations;

                    SMFS.activeSystem = "";
                    gotRiles = false;

                    lastRiles = gotRiles;

                    lastReadOldData = newReadOldData;

                    this.Cursor = Cursors.WaitCursor;

                    dt = tempDt.Copy();
                    if (report.ToUpper().IndexOf("ALPHA") >= 0)
                    {
                        tempview = dt.DefaultView;
                        tempview.Sort = "lastName asc, firstName asc";
                        dt = tempview.ToTable();
                    }
                    dgv.DataSource = dt;
                    dgv.Refresh();

                    //dt = (DataTable)dgv.DataSource;
                    if (dt == null)
                        break;
                    if (dt.Rows.Count <= 0)
                        continue;

                    continuousPrint = true;
                    printPreviewNewBusiness_Click(null, null);
                    continuousPrint = false;

                    this.Cursor = Cursors.Default;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** " + ex.Message.ToString(), "Mass Report Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
            }

            dgv.DataSource = tempDt;
            dgv.RefreshDataSource();
            dgv.Refresh();

            barImport.Value = lastRow;
            barImport.Refresh();

            //try
            //{
            //    PleaseWait pleaseForm = null;
            //    pleaseForm = new PleaseWait("Please Wait!\nSaving New Business Data to Database!");
            //    pleaseForm.Show();
            //    pleaseForm.Refresh();

            //    SaveDataNewBusiness(dt);

            //    pleaseForm.FireEvent1();
            //    pleaseForm.Dispose();
            //    pleaseForm = null;

            //    MessageBox.Show("Mass Reports Finished for New Business Report!", "Mass Report Finished Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("***ERROR*** Saving Data to Database " + ex.Message.ToString(), "Save Data Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //}

            barImport.Hide();
            barImport.Refresh();

            SMFS.activeSystem = saveActive;
        }
        /***********************************************************************************************/
        private void generateMassCashRemitReport_Click(object sender, EventArgs e)
        {
            string mainReport = "";
            string report = "";
            string locations = "";
            format = "";
            string outputFilname = "";
            string outputDirectory = "";

            fullPath = "";

            string location = "";
            string loc = "";
            bool foundLocations = false;

            string lastReadOldData = "";
            string newReadOldData = "";

            DataTable dt = null;

            string[] Lines = null;

            DateTime date = dateTimePicker2.Value;

            string yyyy = date.Year.ToString("D4");
            string month = G1.ToMonthName(date);

            string cmd = "Select * from `mass_reports` where `mainReport` = 'Cash Remit';";
            DataTable dx = G1.get_db_data(cmd);

            int lastRow = dx.Rows.Count;
            //lastRow = 6;

            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            barImport.Show();
            barImport.Refresh();

            bool gotRiles = false;
            bool lastRiles = false;
            DataTable ddt = (DataTable)dgv.DataSource;
            DataTable tempDt = ddt.Copy();
            ddt = null;
            DataView tempview = tempDt.DefaultView;

            for (int i = 0; i < lastRow; i++)
            {
                try
                {
                    Application.DoEvents();

                    barImport.Value = i + 1;
                    barImport.Refresh();

                    mainReport = dx.Rows[i]["mainReport"].ObjToString();
                    report = dx.Rows[i]["report"].ObjToString();
                    locations = dx.Rows[i]["locations"].ObjToString();
                    format = dx.Rows[i]["format"].ObjToString();
                    outputFilname = dx.Rows[i]["outputFilename"].ObjToString();
                    outputDirectory = dx.Rows[i]["outputDirectory"].ObjToString();

                    outputDirectory = outputDirectory.Replace("2021", yyyy);
                    outputFilname = outputFilname.Replace("yyyy", yyyy);
                    outputFilname = outputFilname.Replace("month", month);

                    fullPath = outputDirectory + "/" + outputFilname;

                    G1.verify_path(outputDirectory);

                    this.Text = mainReport + " " + report + " / " + locations;
                    if ( String.IsNullOrWhiteSpace ( locations ))
                        this.Text = mainReport + " " + report;


                    SMFS.activeSystem = "";
                    gotRiles = false;


                    lastReadOldData = newReadOldData;

                    this.Cursor = Cursors.WaitCursor;

                    majorRunOn = "Trusts";
                    if (report.ToUpper().IndexOf("RILES") >= 0)
                    {
                        gotRiles = true;
                        majorRunOn = "Riles";
                    }
                    else if (report.ToUpper().IndexOf("CEMETERIES") >= 0)
                    {
                        majorRunOn = "Cemeteries";
                    }

                    cmbRunOn.Text = majorRunOn;

                    menuReadPrevious_Click(null, null);

                    dt = tempDt.Copy();
                    if ( report.ToUpper().IndexOf ( "COVER SHEET") > 0 )
                    {
                        tabControl1.SelectedTab = tabCashRemitted;
                        dgv6.Visible = true;
                        gridMain6.RefreshEditor(true);
                        dgv6.Refresh();
                        dt = (DataTable) dgv6.DataSource;
                    }
                    else if ( report.ToUpper().IndexOf ( "DPS") > 0 )
                    {
                        tabControl1.SelectedTab = tabRemitDP;
                        gridMain7.RefreshEditor(true);
                        dgv7.Refresh();
                        dt = (DataTable)dgv7.DataSource;
                    }
                    else if (report.ToUpper().IndexOf("PAYMENTS") > 0)
                    {
                        tabControl1.SelectedTab = tabRemitPayments;
                        gridMain8.RefreshEditor(true);
                        dgv8.Refresh();
                        dt = (DataTable)dgv8.DataSource;
                    }
                    if (report.ToUpper().IndexOf("ALPHA") >= 0)
                    {
                        tempview = dt.DefaultView;
                        tempview.Sort = "lastName asc, firstName asc";
                        dt = tempview.ToTable();
                    }
                    //dgv.DataSource = dt;
                    //dgv.Refresh();

                    //dt = (DataTable)dgv.DataSource;
                    if (dt == null)
                        break;
                    if (dt.Rows.Count <= 0)
                        continue;

                    continuousPrint = true;
                    printFirst = true;
                    printFirstToo = true;

                    printPreviewMyCashRemit_Click(null, null);

                    printFirst = true;
                    printFirstToo = true;
                    continuousPrint = false;

                    lastRiles = gotRiles;

                    this.Cursor = Cursors.Default;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** " + ex.Message.ToString(), "Mass Report Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }

            chkSort.Checked = false;
            tabControl1.SelectedTab = tabPayments;
            dgv.DataSource = tempDt;
            dgv.RefreshDataSource();
            dgv.Refresh();

            barImport.Value = lastRow;
            barImport.Refresh();

            try
            {
                PleaseWait pleaseForm = null;
                pleaseForm = new PleaseWait("Please Wait!\nSaving Cash Remitted Data to Database!");
                pleaseForm.Show();
                pleaseForm.Refresh();

                dt = (DataTable)dgv.DataSource;
                //SaveData(dt);
                //SaveDBRs(dt);

                pleaseForm.FireEvent1();
                pleaseForm.Dispose();
                pleaseForm = null;

                MessageBox.Show("Mass Reports Finished for Cash Remitted Report!", "Mass Report Finished Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Saving Data to Database " + ex.Message.ToString(), "Save Data Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }

            barImport.Hide();
            barImport.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnSort(object sender, CustomColumnSortEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (e.Column.FieldName == "deceasedDate")
            {
                DataTable tempDt = dt.Copy();
                string str = "";
                tempDt.Columns.Add("tempSort");
                for (int i = 0; i < tempDt.Rows.Count; i++)
                {
                    str = tempDt.Rows[i]["deceasedDate"].ObjToDateTime().ToString("yyyyMMdd");
                    tempDt.Rows[i]["tempSort"] = str;
                }
                DataView tempview = tempDt.DefaultView;
                tempview.Sort = "tempSort asc";
                dt = tempview.ToTable();

                dt.Columns.Remove("tempSort");
                dgv.DataSource = dt;
            }

        }
        /****************************************************************************************/
        public static double isDBR ( string contract )
        {
            DateTime runDate = DateTime.MinValue;
            double dbr = isDBR(contract, runDate);
            return dbr;
        }
        /****************************************************************************************/
        public static double isDBR(string contract, DateTime runDate )
        {
            double dbr = 0D;
            double dbDBR = 0D;

            string cmd = "Select * from `customers` c JOIN `contracts` t ON c.`contractNumber` = t.`contractNumber` WHERE c.`contractNumber` = '" + contract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return dbr;
            DateTime deceasedDate = dt.Rows[0]["deceasedDate"].ObjToDateTime();
            if (deceasedDate.Year < 1000)
                return dbr;

            DateTime dateBOM = G1.GetDateBOM(deceasedDate);
            DateTime dateEOM = G1.GetDateEOM(deceasedDate);

            if (dateBOM.Year < 1000 || dateEOM.Year < 1000)
                return dbr;

            if ( runDate.Year > 1000 )
            {
                dateBOM = G1.GetDateBOM(runDate);
                dateEOM = G1.GetDateEOM(runDate);
            }

            double currentPayments = 0D;

            //string apr = dt.Rows[0]["APR"].ObjToString();
            //double dAPR = apr.ObjToDouble() / 100.0D;
            //int numPayments = dt.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
            //double startBalance = DailyHistory.GetFinanceValue(dt.Rows[0]);

            string sDate = dateBOM.ToString("yyyy-MM-dd");
            string eDate = dateEOM.ToString("yyyy-MM-dd");

            cmd = "Select * from `payments` where `contractNumber` = '" + contract + "' AND `payDate8` >= '" + sDate + "' AND `payDate8` <= '" + eDate + "' order by `paydate8` DESC, `tmstamp` DESC;";
            DataTable dp = G1.get_db_data(cmd);
            if (dp.Rows.Count > 0)
            {
                if (runDate.Year > 1000 && deceasedDate >= dateBOM && deceasedDate <= dateEOM)
                {
                    for (int i = 0; i < dp.Rows.Count; i++)
                        dbr += dp.Rows[i]["trust85P"].ObjToDouble();
                }
                else
                {
                    if (runDate.Year > 1000)
                    {
                        for (int i = 0; i < dp.Rows.Count; i++)
                            currentPayments += dp.Rows[i]["trust85P"].ObjToDouble();
                    }
                }
            }

            DateTime lastMonthDate = deceasedDate.AddMonths(-1);
            if (runDate.Year > 1000)
            {
                cmd = "SELECT * FROM `cashremitted` WHERE contractNumber = '" + contract + "' and `runDate2` = '" + dateEOM.ToString("yyyy-MM-dd") + "' ORDER BY runDate2 DESC;";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    lastMonthDate = runDate.AddMonths(-1);
                else
                    lastMonthDate = runDate.AddMonths(1);
            }

            dateBOM = G1.GetDateBOM(lastMonthDate);
            dateEOM = G1.GetDateEOM(lastMonthDate);

            cmd = "Select * from `payments` where `contractNumber` = '" + contract + "' AND `payDate8` >= '" + dateBOM.ToString("yyyy-MM-dd") + "' AND `payDate8` <= '" + dateEOM.ToString("yyyy-MM-dd") + "' order by `paydate8` DESC, `tmstamp` DESC;";
            dp = G1.get_db_data(cmd);
            if (dp.Rows.Count > 0 )
            { // Check Cash Remitted
                cmd = "SELECT * FROM `cashremitted` WHERE contractNumber = '" + contract + "' and `runDate2` = '" + dateEOM.ToString("yyyy-MM-dd") + "' ORDER BY runDate2 DESC;";
                DataTable dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count > 0 )
                {
                    DateTime tmStamp = dx.Rows[0]["tmstamp"].ObjToDateTime();
                    cmd = "SELECT * FROM `dbrs` WHERE `contractNumber` = '" + contract + "' AND `cashRemitStopDate` = '" + dateEOM.ToString("yyyy-MM-dd") + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                        dbr += dx.Rows[0]["dbr"].ObjToDouble();
                    else
                        dbr += currentPayments;
                }
                else
                {
                    for (int i = 0; i < dp.Rows.Count; i++)
                        dbr += dp.Rows[i]["trust85P"].ObjToDouble();
                }
            }
            else
            {
                if ( runDate.Year > 1000 )
                {
                    cmd = "SELECT * FROM `cashremitted` WHERE contractNumber = '" + contract + "' and `runDate2` = '" + dateEOM.ToString("yyyy-MM-dd") + "' ORDER BY runDate2 DESC;";
                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        DateTime tmStamp = dx.Rows[0]["tmstamp"].ObjToDateTime();
                        cmd = "SELECT * FROM `dbrs` WHERE `contractNumber` = '" + contract + "' AND `cashRemitStopDate` = '" + dateEOM.ToString("yyyy-MM-dd") + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                            dbr += dx.Rows[0]["dbr"].ObjToDouble();
                    }
                    else
                    {
                        for (int i = 0; i < dp.Rows.Count; i++)
                            dbr += dp.Rows[i]["trust85P"].ObjToDouble();
                    }
                }
            }
            if (dbr <= 0D && currentPayments > 0D)
                dbr = currentPayments;
            return dbr;
        }
        /****************************************************************************************/
    }
}