using DevExpress.Utils;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrintingLinks;
using DevExpress.XtraReports.ReportGeneration;
using DevExpress.XtraReports.UI;
using ExcelLibrary.SpreadSheet;
using GeneralLib;
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class Trust85 : DevExpress.XtraEditors.XtraForm
    {
        private bool autoRun = false;
        private bool autoForce = false;
        private string sendTo = "";
        private string sendWhere = "";
        private bool firstAgent = true;
        private DataTable _agentList = null;
        private DataTable originalDt = null;
        private string mainQuery = "";
        private DataTable customerDt = null;
        private bool batchProcess = false;
        private string saveAgent = "";
        private bool commissionRan = false;
        private bool allowDebits = true;
        private bool loading = false;
        /****************************************************************************************/
        public static DataTable allAgentsDt = null;
        public static DataTable trust85_dt = null;
        public static DataTable trust85_dt8 = null;
        public static DataTable trust85_dt9 = null;
        /****************************************************************************************/
        public Trust85(DataTable custDt = null)
        {
            InitializeComponent();
            SetupTotalsSummary();
            customerDt = custDt;
            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker3.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker4.Value = new DateTime(now.Year, now.Month, days);
            if (this.dateTimePicker4.Value >= DailyHistory.majorDate)
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
                if (this.dateTimePicker3.Value.Year >= 2019 && this.dateTimePicker3.Value.Month >= 6)
                {
                    this.dateTimePicker1.Value = this.dateTimePicker3.Value;
                    this.dateTimePicker2.Value = this.dateTimePicker4.Value;
                }
            }
        }
        /****************************************************************************************/
        public Trust85(DateTime start, DateTime stop)
        {
            InitializeComponent();
            batchProcess = true;
            this.dateTimePicker1.Value = start;
            this.dateTimePicker2.Value = stop;
            this.dateTimePicker3.Value = start;
            this.dateTimePicker4.Value = stop;
            btnRun_Click(null, null);
        }
        /****************************************************************************************/
        public Trust85(bool auto, bool force)
        {
            InitializeComponent();
            autoRun = auto;
            autoForce = force;
            RunAutoReports();
            this.Close();
        }
        /****************************************************************************************/
        private void RunAutoReports()
        {
            string cmd = "Select * from `remote_processing`;";
            DataTable dt = G1.get_db_data(cmd);
            string report = "";
            DateTime date = DateTime.Now;
            long currentDay = G1.date_to_days(date.ToString("MM/dd/yyyy"));
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
                sendTo = dt.Rows[i]["sendTo"].ObjToString();
                sendWhere = dt.Rows[i]["sendWhere"].ObjToString();
                if (report.ToUpper() == "LAPSE REPORT")
                {
                    DateTime start = DateTime.Now;
                    DateTime stop = DateTime.Now;
                    allAgentsDt = G1.get_db_data("Select * from `agents`;");
                    DataTable dt8 = RunLapseReport(ref start, ref stop);
                    dgv8.DataSource = dt8;
                    printPreviewToolStripMenuItem_Click(null, null);
                }
            }
        }
        /****************************************************************************************/
        public Trust85(DateTime start, DateTime stop, string agent)
        {
            InitializeComponent();
            SetupTotalsSummary();
            this.dateTimePicker1.Value = start;
            this.dateTimePicker2.Value = stop;
            this.dateTimePicker3.Value = start;
            this.dateTimePicker4.Value = stop;

            DateTime now = start;

            int days = DateTime.DaysInMonth(now.Year, now.Month);

            DateTime start1 = now.AddDays(-1);
            DateTime stop1 = new DateTime(now.Year, now.Month, days - 1);
            this.dateTimePicker1.Value = start1;
            this.dateTimePicker2.Value = stop1;

            saveAgent = agent;
            AfterLoad();
            btnRun_Click(null, null);
        }
        /****************************************************************************************/
        private void Trust85_Load(object sender, EventArgs e)
        {
            barImport.Hide();
            AfterLoad();
            trustReportsToolStripMenuItem.Enabled = true;
            trustReportsToolStripMenuItem.Enabled = false;
            historicCommissionsToolStripMenuItem.Enabled = false;
            agentsPieChartToolStripMenuItem.Enabled = false;

            btnSaveCommissions.Hide();

            chkACH.Hide();
            label4.Hide();
            this.dateTimePicker3.Hide();
            this.dateTimePicker4.Hide();
        }
        /***********************************************************************************************/
        private void AfterLoad()
        {
            btnMatch.Hide();
            btnPrintAll.Hide();
            btnChart.Hide();
            progressBar1.Hide();
            label7.Hide();

            G1.SetupVisibleColumns(gridMain, this.columnsToolStripMenuItem, nmenu_Click);
            btnCalc.Hide();
            chkMainDoSplits.Hide();
            //            reportsToolStripMenuItem.Visible = false;
            LoadData();
            if (LoginForm.doLapseReport)
            {
                DateTime d1 = DateTime.Now;
                DateTime d2 = DateTime.Now;
                DataTable dt8 = RunLapseReport(ref d1, ref d2);
                LoadAgentNames(dt8);
                DataView tempview = dt8.DefaultView;
                //            tempview.Sort = "loc asc, agentName asc";
                tempview.Sort = "agentNumber asc";
                dt8 = tempview.ToTable();
                G1.NumberDataTable(dt8);
                dgv8.DataSource = dt8;
                dgv8.RefreshDataSource();
                this.dateTimePicker1.Value = d1;
                this.dateTimePicker2.Value = d2;
                this.dateTimePicker3.Value = d1;
                this.dateTimePicker4.Value = d2;
                printToolStripMenuItem_Click(null, null);
                return;
            }
            loadGroupCombo(cmbSelectColumns, "Trust85", "Primary");
            loadGroupCombo(cmbSelectCommission, "TrustCommission", "commissions");
        }
        /***********************************************************************************************/
        private void loadGroupCombo(System.Windows.Forms.ComboBox cmb, string key, string module)
        {
            cmb.Items.Clear();
            string cmd = "Select * from procfiles where ProcType = '" + key + "' AND `module` = '" + module + "' group by name;";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Name"].ToString();
                cmb.Items.Add(name);
            }
        }
        /****************************************************************************************/
        private void LoadAgentNames(DataTable dt)
        {
            string cmd = "";
            string agentCode = "";
            string fname = "";
            string lname = "";
            string name = "";
            string contract = "";
            string loc = "";
            string trust = "";
            if (G1.get_column_number(dt, "loc") < 0)
                dt.Columns.Add("loc");
            if (G1.get_column_number(dt, "trust") < 0)
                dt.Columns.Add("trust");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contract = dt.Rows[i]["contractNumber"].ObjToString();
                contract = decodeContractNumber(contract, ref trust, ref loc);
                dt.Rows[i]["trust"] = trust;
                dt.Rows[i]["loc"] = loc;
                agentCode = dt.Rows[i]["agentNumber"].ObjToString();
                if (!String.IsNullOrWhiteSpace(agentCode))
                {
                    cmd = "Select * from `agents` where `agentCode` = '" + agentCode + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        fname = dx.Rows[0]["firstName"].ObjToString().Trim();
                        lname = dx.Rows[0]["lastName"].ObjToString().Trim();
                        name = fname + " " + lname;
                        dt.Rows[i]["agentName"] = name;
                    }
                }
            }
        }
        /****************************************************************************************/
        private void LoadData()
        {
            string cmd = "Select * from `payments` where `contractNumber` = 'XYZZYAAA';";
            DataTable dt = G1.get_db_data(cmd);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            loadLocatons();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            btnCombine.Hide();
            AddSummaryColumn("Recap", null);
            AddSummaryColumn("RecapContracts", null);
            AddSummaryColumn("Reins", null);
            AddSummaryColumn("downPayment", null);
            AddSummaryColumn("totalPayments", null);
            AddSummaryColumn("paymentAmount", null);
            AddSummaryColumn("ccFee", null);
            AddSummaryColumn("ap", null);
            AddSummaryColumn("dpp", null);
            AddSummaryColumn("trust85P", null);
            AddSummaryColumn("trust100P", null);
            AddSummaryColumn("commission", null);
            AddSummaryColumn("dbcMoney", null);
            AddSummaryColumn("contractValue", null);
            AddSummaryColumn("cashAdvance", null);
            AddSummaryColumn("ibtrust", null);
            AddSummaryColumn("sptrust", null);
            AddSummaryColumn("xxtrust", null);
            AddSummaryColumn("debit", null);
            AddSummaryColumn("credit", null);
            AddSummaryColumn("interestPaid", null);
            AddSummaryColumn("lapseContract$", null);
            AddSummaryColumn("reinstateContract$", null);
            AddSummaryColumn("fbi", null, "{0}");
            AddSummaryColumn("dbc", null);
            //            AddSummaryColumn("Recap", gridMain);

            AddSummaryColumn("downPayment", gridMain2);
            AddSummaryColumn("dpp", gridMain2);
            AddSummaryColumn("totalPayments", gridMain2);
            AddSummaryColumn("paymentAmount", gridMain2);
            AddSummaryColumn("ap", gridMain2);
            AddSummaryColumn("contractValue", gridMain2);
            AddSummaryColumn("ibtrust", gridMain2);
            AddSummaryColumn("sptrust", gridMain2);
            AddSummaryColumn("xxtrust", gridMain2);
            AddSummaryColumn("total", gridMain2);
            AddSummaryColumn("cashAdvance", gridMain2);

            AddSummaryColumn("downPayment", gridMain3);
            AddSummaryColumn("dpp", gridMain3);
            AddSummaryColumn("dpr", gridMain3);
            AddSummaryColumn("dbc", gridMain3);
            AddSummaryColumn("dbc_5", gridMain3);
            AddSummaryColumn("fbi", gridMain3);
            AddSummaryColumn("fbiCommission", gridMain3);
            AddSummaryColumn("totalPayments", gridMain3);
            AddSummaryColumn("Split Payment", gridMain3);
            AddSummaryColumn("Split DownPayment", gridMain3);
            AddSummaryColumn("interestPaid", gridMain3);
            AddSummaryColumn("paymentAmount", gridMain3);
            AddSummaryColumn("ap", gridMain3);
            AddSummaryColumn("contractValue", gridMain3);
            AddSummaryColumn("Recap", gridMain3);
            AddSummaryColumn("recapAmount", gridMain3);
            AddSummaryColumn("Reins", gridMain3);
            AddSummaryColumn("commission", gridMain3);
            AddSummaryColumn("ccFee", gridMain3);
            AddSummaryColumn("debitAdjustment", gridMain3);
            AddSummaryColumn("creditAdjustment", gridMain3);
            AddSummaryColumn("cashAdvance", gridMain3);

            AddSummaryColumn("downPayment", gridMain4);
            AddSummaryColumn("dpp", gridMain4);
            AddSummaryColumn("totalPayments", gridMain4);
            AddSummaryColumn("paymentAmount", gridMain4);
            AddSummaryColumn("ap", gridMain4);
            AddSummaryColumn("contractValue", gridMain4);
            AddSummaryColumn("ibtrust", gridMain4);
            AddSummaryColumn("sptrust", gridMain4);
            AddSummaryColumn("xxtrust", gridMain4);
            AddSummaryColumn("totalTrusts", gridMain4);

            AddSummaryColumn("downPayment", gridMain5);
            AddSummaryColumn("dpp", gridMain5);
            AddSummaryColumn("totalPayments", gridMain5);
            AddSummaryColumn("paymentAmount", gridMain5);
            AddSummaryColumn("ap", gridMain5);
            AddSummaryColumn("contractValue", gridMain5);
            AddSummaryColumn("ibtrust", gridMain5);
            AddSummaryColumn("sptrust", gridMain5);
            AddSummaryColumn("xxtrust", gridMain5);
            AddSummaryColumn("total", gridMain5);

            AddSummaryColumn("downPayment", gridMain7);
            AddSummaryColumn("dpp", gridMain7);
            AddSummaryColumn("totalPayments", gridMain7);
            AddSummaryColumn("contractValue", gridMain7);
            AddSummaryColumn("ibtrust", gridMain7);
            AddSummaryColumn("sptrust", gridMain7);
            AddSummaryColumn("xxtrust", gridMain7);
            AddSummaryColumn("total", gridMain7);

            AddSummaryColumn("Recap", gridMain8);
            AddSummaryColumn("contractValue", gridMain8);
            AddSummaryColumn("downPayment", gridMain8);
            AddSummaryColumn("commission", gridMain8);
            AddSummaryColumn("dbrSales", gridMain8);
            AddSummaryColumn("lapseRecaps", gridMain8);
            AddSummaryColumn("totalContracts", gridMain8);

            AddSummaryColumn("Recap", gridMain9);
            AddSummaryColumn("Reins", gridMain9);
            AddSummaryColumn("contractValue", gridMain9);
            AddSummaryColumn("downPayment", gridMain9);
            AddSummaryColumn("commission", gridMain9);
            AddSummaryColumn("dbrSales", gridMain9);
            AddSummaryColumn("lapseRecaps", gridMain9);
            AddSummaryColumn("totalContracts", gridMain9);

            AddSummaryColumn("totalCommission", gridMain10);
            AddSummaryColumn("totalPayments", gridMain10);
            AddSummaryColumn("commission", gridMain10);
            AddSummaryColumn("dbcMoney", gridMain10);
            AddSummaryColumn("splitCommission", gridMain10);
            AddSummaryColumn("splitBaseCommission", gridMain10);
            AddSummaryColumn("splitGoalCommission", gridMain10);
            AddSummaryColumn("goalCommission", gridMain10);
            AddSummaryColumn("mainCommission", gridMain10);
            AddSummaryColumn("contractValue", gridMain10);
            AddSummaryColumn("Formula Sales", gridMain10);
            AddSummaryColumn("Location Sales", gridMain10);
            AddSummaryColumn("dbrValue", gridMain10);
            AddSummaryColumn("Recap", gridMain10);
            AddSummaryColumn("Reins", gridMain10);
            AddSummaryColumn("pastRecap", gridMain10);
            AddSummaryColumn("pastFailures", gridMain10);
            AddSummaryColumn("totalContracts", gridMain10);
            AddSummaryColumn("contractCommission", gridMain10);
            AddSummaryColumn("fbi", gridMain10, "{0}");
            AddSummaryColumn("fbi$", gridMain10);
            AddSummaryColumn("dbc", gridMain10);
            AddSummaryColumn("MR", gridMain10);
            AddSummaryColumn("MC", gridMain10);

            AddSummaryColumn("beginningBalance", gridMain11);
            AddSummaryColumn("ytdPrevious", gridMain11);
            AddSummaryColumn("paymentCurrMonth", gridMain11);
            AddSummaryColumn("currentRemovals", gridMain11);
            AddSummaryColumn("endingBalance", gridMain11);
            AddSummaryColumn("calcTrust85", gridMain11);
            AddSummaryColumn("difference", gridMain11);

            AddSummaryColumn("Recap", gridMain12);
            AddSummaryColumn("Reins", gridMain12);
            AddSummaryColumn("contractValue", gridMain12);
            AddSummaryColumn("downPayment", gridMain12);
            AddSummaryColumn("commission", gridMain12);
            AddSummaryColumn("dbrSales", gridMain12);
            AddSummaryColumn("lapseRecaps", gridMain12);
            AddSummaryColumn("totalContracts", gridMain12);

            AddSummaryColumn("MC", gridMain13);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "")
        {
            if (gMain == null)
                gMain = gridMain;
            if (String.IsNullOrWhiteSpace(format))
                format = "${0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            //            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            //            gMain.Columns[columnName].SummaryItem.DisplayFormat = "${0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
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
            footerCount = 0;
            startPrint = false;
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
            else if (dgv13.Visible)
                printableComponentLink1.Component = dgv13;
            else
                printableComponentLink1.Component = dgv;

            if (autoRun)
                printableComponentLink1.Component = dgv8;

            if (LoginForm.doLapseReport)
                printableComponentLink1.Component = dgv2;

            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            //Printer.setupPrinterMargins(50, 100, 80, 50);
            Printer.setupPrinterMargins(10, 10, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            //printingSystem1.Document.AutoFitToPagesWidth = 1;

            if (dgv2.Visible)
                G1.AdjustColumnWidths(gridMain2, 0.65D, true);
            else if (dgv3.Visible)
                G1.AdjustColumnWidths(gridMain3, 0.65D, true);
            else if (dgv4.Visible)
                G1.AdjustColumnWidths(gridMain4, 0.65D, true);
            else if (dgv5.Visible)
                G1.AdjustColumnWidths(gridMain5, 0.65D, true);
            else if (dgv7.Visible)
                G1.AdjustColumnWidths(gridMain7, 0.65D, true);
            else if (dgv8.Visible)
                G1.AdjustColumnWidths(gridMain8, 0.65D, true);
            else if (dgv9.Visible)
                G1.AdjustColumnWidths(gridMain9, 0.65D, true);
            else if (dgv10.Visible)
                G1.AdjustColumnWidths(gridMain10, 0.65D, true);
            else if (dgv11.Visible)
                G1.AdjustColumnWidths(gridMain11, 0.65D, true);
            else if (dgv13.Visible)
                G1.AdjustColumnWidths(gridMain13, 0.65D, true);
            else
                G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printableComponentLink1.CreateDocument();

            if (autoRun)
            {
                string path = G1.GetReportPath();
                DateTime today = DateTime.Now;

                DataTable dx = (DataTable)dgv8.DataSource;
                string emailLocations = DailyHistory.ParseOutLocations(dx);

                string filename = path + @"\LapseReport_" + today.Year.ToString("D4") + today.Month.ToString("D2") + today.Day.ToString("D2") + ".pdf";
                if (File.Exists(filename))
                    File.Delete(filename);
                printableComponentLink1.ExportToPdf(filename);
                RemoteProcessing.AutoRunSendTo("Lapse Report", filename, sendTo, sendWhere, emailLocations);
            }
            else
            {
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
                    printableComponentLink1.ShowPreview();
            }

            if (dgv2.Visible)
                G1.AdjustColumnWidths(gridMain2, 0.65D, false);
            else if (dgv3.Visible)
                G1.AdjustColumnWidths(gridMain3, 0.65D, false);
            else if (dgv4.Visible)
                G1.AdjustColumnWidths(gridMain4, 0.65D, false);
            else if (dgv5.Visible)
                G1.AdjustColumnWidths(gridMain5, 0.65D, false);
            else if (dgv7.Visible)
                G1.AdjustColumnWidths(gridMain7, 0.65D, false);
            else if (dgv8.Visible)
                G1.AdjustColumnWidths(gridMain8, 0.65D, false);
            else if (dgv9.Visible)
                G1.AdjustColumnWidths(gridMain9, 0.65D, false);
            else if (dgv10.Visible)
                G1.AdjustColumnWidths(gridMain10, 0.65D, false);
            else if (dgv11.Visible)
                G1.AdjustColumnWidths(gridMain11, 0.65D, false);
            else if (dgv13.Visible)
                G1.AdjustColumnWidths(gridMain13, 0.65D, false);
            else
                G1.AdjustColumnWidths(gridMain, 0.65D, false);
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            footerCount = 0;
            startPrint = false;
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
            else if (dgv13.Visible)
                printableComponentLink1.Component = dgv13;
            else
                printableComponentLink1.Component = dgv;
            if (LoginForm.doLapseReport)
                printableComponentLink1.Component = dgv8;

            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 100, 80, 50);

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

            font = new Font("Ariel", 10, FontStyle.Regular);
            if (dgv2.Visible)
            {
                if (chkReverseAgentsAndLocations.Checked)
                    Printer.DrawQuad(6, 8, 4, 4, "Locations by Agent Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                else
                    Printer.DrawQuad(6, 8, 4, 4, "Agents by Location Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            }
            else if (dgv3.Visible)
                Printer.DrawQuad(6, 8, 4, 4, "Agent Totals Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (dgv4.Visible)
                Printer.DrawQuad(6, 8, 4, 4, "Locations by Agent Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (dgv5.Visible)
                Printer.DrawQuad(6, 8, 4, 4, "Location Totals Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (dgv7.Visible)
                Printer.DrawQuad(6, 8, 4, 4, "Contracts by Location Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (dgv8.Visible || autoRun)
                Printer.DrawQuad(6, 8, 4, 4, "Lapse Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (dgv9.Visible)
                Printer.DrawQuad(6, 8, 4, 4, "Reinstate Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if (dgv10.Visible)
            {
                string title = "Commission Report";
                if (chkConsolidate.Checked)
                    title = "Consolidated Commission Report";
                else if (cmbShow.Text.Trim().ToUpper() == "1%")
                    title = "1% Goal Commission Report";
                else if (cmbShow.Text.Trim().ToUpper() == "5%")
                    title = "5% Standard Commission Report";
                Printer.DrawQuad(6, 8, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            }
            else if (dgv13.Visible)
                Printer.DrawQuad(6, 8, 4, 4, "Meeting Commissions Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else
                Printer.DrawQuad(6, 8, 4, 4, "Trust85 Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            if (LoginForm.doLapseReport)
                Printer.DrawQuad(6, 8, 4, 4, "Lapse Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            DateTime date = this.dateTimePicker2.Value;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
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
            else if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0)
            {
                if (e.RowHandle >= 0)
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                    if (date.Year == 1)
                        e.DisplayText = "";
                }
            }
        }
        /****************************************************************************************/
        string GetContractDate(string cnum)
        {
            string result = "";
            string cmd = "Select `contractDate` from `customers` where `contractNumber` = '" + cnum + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                result = G1.GetSQLDate(dt, 0, "contractDate");
            return result;
        }
        /****************************************************************************************/
        public static void AddColumnToTable(DataTable dt, string column, string type = "")
        {
            if (G1.get_column_number(dt, column) >= 0)
                return;
            if (String.IsNullOrWhiteSpace(type))
                dt.Columns.Add(column);
            else
                dt.Columns.Add(column, Type.GetType(type));
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            barImport.Show();
            barImport.Minimum = 0;
            barImport.Maximum = 11;
            barImport.Value = 0;
            Color backColor = btnRun.BackColor;
            btnRun.BackColor = Color.Green;
            btnRun.Refresh();
            allAgentsDt = G1.get_db_data("Select * from `agents`;");

            try
            {
                DateTime date = dateTimePicker1.Value;
                string date1 = G1.DateTimeToSQLDateTime(date);
                date = dateTimePicker2.Value;
                string date2 = G1.DateTimeToSQLDateTime(date);

                //DateTime dd2 = dateTimePicker1.Value.AddDays(-1);
                //string ddd2 = G1.DateTimeToSQLDateTime(dd2);
                //DateTime dd3 = dateTimePicker2.Value.AddDays(-1);
                //string ddd3 = G1.DateTimeToSQLDateTime(dd3);

                date = dateTimePicker3.Value;
                string date3 = G1.DateTimeToSQLDateTime(date);
                date = dateTimePicker4.Value;
                string date4 = G1.DateTimeToSQLDateTime(date);

                string paidDate = "`payDate8` >= '" + date3 + "' and `payDate8` <= '" + date4 + "' ";
                string ddDate = "`payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' AND (`depositNumber` LIKE 'T%' OR `depositNumber` LIKE 'A%') ";
                if (!chkACH.Checked)
                    ddDate = "`payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' AND (`depositNumber` LIKE 'T%' OR `depositNumber` LIKE 'A%') ";
                else
                    ddDate = "`payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' AND (`depositNumber` LIKE 'T%' ) ";



                string cmd = "Select p.*,d.*,a.`firstName`,a.`lastName`,c.`meetingNumber` from `payments` p LEFT JOIN `contracts` d on p.`contractNumber` = d.`contractNumber` LEFT JOIN `agents` a ON p.`agentNumber` = a.`agentCode` LEFT JOIN `customers` c ON p.`contractNumber` = c.`contractNumber` ";
                cmd += " WHERE ";

                string original_cmd = cmd;
                cmd += ddDate;

                if (firstAgent)
                {
                    firstAgent = false;
                    loadAgents(cmd);
                }

                string agents = getAgentQuery();
                if (!String.IsNullOrWhiteSpace(agents))
                    cmd += " and " + agents;

                if (String.IsNullOrWhiteSpace(mainQuery))
                    mainQuery = cmd;

                DataTable dt = G1.get_db_data(cmd);

                DataRow[] dddRows = dt.Select("meetingNumber > '0'");
                if (dddRows.Length > 0)
                {
                    DataTable meetingDt = dddRows.CopyToDataTable();
                }
                //FindContract(dt, "M17119UI");

                cmd = original_cmd;
                cmd += paidDate;
                if (!chkACH.Checked)
                    cmd += "AND (`depositNumber` NOT LIKE 'T%' AND `depositNumber` NOT LIKE 'A%');";
                else
                    cmd += "AND (`depositNumber` NOT LIKE 'T%' );";
                DataTable ddt = G1.get_db_data(cmd);

                //FindContract(ddt, "C22028LI");
                for (int i = 0; i < ddt.Rows.Count; i++)
                {
                    dt.ImportRow(ddt.Rows[i]);
                }
                barImport.Value++;

                LoadTrustAdjustments(dt, this.dateTimePicker1.Value, this.dateTimePicker2.Value);
                //FindContract(dt, "M17119UI");
                RemoveTrustAdjustments(dt, this.dateTimePicker1.Value, this.dateTimePicker2.Value);
                //FindContract(dt, "M17119UI");

                FilterDeletedPayments(dt);
                //FindContract(dt, "M17119UI");

                CalculateNewContracts(dt, this.dateTimePicker1.Value, this.dateTimePicker2.Value);
                FindContract(dt, "C22028LI");
                //if ( 1 == 1)
                //{
                //    dgv.DataSource = dt;
                //    return;
                //}
                //FindContract(dt, "M17119UI");
                LoadAgentNames(dt);
                barImport.Value++;

                DataTable dt8 = CheckForMainLapse(dt, this.dateTimePicker1.Value, this.dateTimePicker2.Value);
                barImport.Value++;
                dt8 = CalculatePastFormulas(dt8, "LAPSEDATE8");
                barImport.Value++;
                G1.NumberDataTable(dt8);
                dgv8.DataSource = dt8;

                DataTable dt9 = CheckForMainReinstate(dt, this.dateTimePicker1.Value, this.dateTimePicker2.Value);
                barImport.Value++;
                dt9 = CalculatePastFormulas(dt9, "REINSTATEDATE8");
                barImport.Value++;
                G1.NumberDataTable(dt9);
                dgv9.DataSource = dt9;

                //FindContract(dt, "HT19041L");

                LoadLocationTrustCombo(dt);
                barImport.Value++;

                CalcLapseReinstate(dt);
                barImport.Value++;

                ReconcileAllAgents(dt, dt8, dt9);
                barImport.Value++;
                FixLapseReins(dt, dt8, dt9);

                ProcessCashAdvances(dt);
                barImport.Value++;
                FindContract(dt);

                if (commissionRan)
                {
                    //btnRun.BackColor = backColor;
                    //btnRun.Refresh();
                    //btnCalc_Click(null, null);
                    dgv10.DataSource = null;
                }
                btnCalc.Show();
                chkMainDoSplits.Show();

                //dt8 = SMFS.FilterForRiles(dt8);
                //dt9 = SMFS.FilterForRiles(dt9);


                G1.NumberDataTable(dt8);
                G1.NumberDataTable(dt9);
                dgv8.DataSource = dt8;
                dgv9.DataSource = dt9;

                G1.NumberDataTable(dt);
                //FindContract(dt, "HT19041L");
                DateTime sortedPayDate = DateTime.Now;
                if (G1.get_column_number(dt, "sortedPayDate8") < 0)
                    dt.Columns.Add("sortedPayDate8");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sortedPayDate = dt.Rows[i]["payDate8"].ObjToDateTime();
                    dt.Rows[i]["sortedPayDate8"] = sortedPayDate.Year.ToString("D4") + sortedPayDate.Month.ToString("D2") + sortedPayDate.Day.ToString("D2");
                }
                DataView tempview = dt.DefaultView;
                tempview.Sort = "sortedPayDate8 asc, depositNumber asc";
                dt = tempview.ToTable();

                double fbi = 0D;
                double contractValue = 0D;
                string agentNumber = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    fbi = dt.Rows[i]["fbi"].ObjToDouble();
                    contractValue = dt.Rows[i]["contractValue"].ObjToDouble();
                    if (contractValue < 0D && fbi == 1D)
                        dt.Rows[i]["contractValue"] = 0D;
                    //agentNumber = dt.Rows[i]["agentNumber"].ObjToString();
                    //if ( String.IsNullOrWhiteSpace ( agentNumber))
                    //{
                    //    dt.Rows[i]["agentNumber"] = "XXX";
                    //    dt.Rows[i]["agentName"] = "Agent XXX";
                    //}
                }

                string runOn = cmbRunOn.Text.Trim().ToUpper();

                if (runOn.ToUpper() != "RILES")
                    dt = SMFS.FilterForRiles(dt);

                dt = Trust85.FilterForCemetery(dt, runOn);

                DailyHistory.CleanupVisibility(gridMain);

                dgv.DataSource = dt;
                dt = (DataTable)dgv.DataSource;
                FindContract(dt);
                btnRun.BackColor = backColor;
                btnRun.Refresh();
                barImport.Value++;

                dgv.RefreshDataSource();
                gridMain.RefreshData();
                dgv.Refresh();
                this.Refresh();

                originalDt = dt;
                FindContract(dt);
                this.Cursor = Cursors.Default;
                trustReportsToolStripMenuItem.Enabled = true;
                if (batchProcess)
                {
                    trust85_dt = dt;
                    trust85_dt8 = dt8;
                    trust85_dt9 = dt9;
                    this.Close();
                }
                if (!String.IsNullOrWhiteSpace(saveAgent))
                {
                    this.chkComboAgentNames.EditValue = saveAgent;
                    //                    this.chkComboAgentNames.Text = saveAgent;
                    int left = this.Left + 10;
                    int top = this.Top + 10;
                    this.SetBounds(left, top, this.Width, this.Height);
                    dgv.Refresh();
                    this.Refresh();
                }
                dt = (DataTable)dgv.DataSource;
                FindContract(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }
            btnCalc.Show();
            chkMainDoSplits.Show();
        }
        /***********************************************************************************************/
        public static DataTable FilterForCemetery(DataTable dt, string cemetery = "")
        {
            if (G1.get_column_number(dt, "contractNumber") < 0)
                return dt;
            DataTable dx = null;
            try
            {
                dx = dt.Clone();
                DataRow[] dRows = null;
                if (cemetery.Trim().ToUpper() == "TRUSTS")
                    cemetery = "";

                string cmd = "contractNumber NOT LIKE 'NNM%' AND contractNumber NOT LIKE 'HC%'";
                if (!String.IsNullOrWhiteSpace(cemetery))
                {
                    if (cemetery.ToUpper() == "RILES")
                        cmd = "contractNumber LIKE 'RF%' ";
                    else
                        cmd = "contractNumber LIKE 'NNM%' OR contractNumber LIKE 'HC%'";
                }
                dRows = dt.Select(cmd);
                G1.ConvertToTable(dRows, dx);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Trying to Filter Cemetery Data!");
            }
            return dx;
        }
        /****************************************************************************************/
        public static void ValidatePastInterestPaid(string contractNumber)
        {
            string cmd = "";
            DataTable dt = null;
        }
        /****************************************************************************************/
        private void FixLapseReins(DataTable dt, DataTable dt8, DataTable dt9)
        {
            string contractNumber = "";
            string agent = "";
            double reins = 0D;
            double recap = 0D;
            double totals = 0D;
            int row = 0;
            G1.NumberDataTable(dt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["Recap"] = 0D;
                dt.Rows[i]["Reins"] = 0D;
            }
            string newAgent = "";
            DataRow[] dRows = null;
            for (int i = 0; i < dt8.Rows.Count; i++)
            {
                contractNumber = dt8.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "N18021LI")
                {
                }
                agent = dt8.Rows[i]["agentNumber"].ObjToString();
                recap = dt8.Rows[i]["Recap"].ObjToDouble();
                if (recap > 0D)
                {
                    dRows = dt.Select("contractNumber='" + contractNumber + "' AND agentNumber = '" + agent + "'");
                    if (dRows.Length > 0)
                    {
                        newAgent = dRows[0]["agentNumber"].ObjToString();
                        row = dRows[0]["num"].ObjToInt32() - 1;
                        totals = dt.Rows[row]["Recap"].ObjToDouble();
                        dt.Rows[row]["Recap"] = totals + recap;
                    }
                }
            }
            for (int i = 0; i < dt9.Rows.Count; i++)
            {
                contractNumber = dt9.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "N18021LI")
                {
                }
                agent = dt9.Rows[i]["agentNumber"].ObjToString();
                reins = dt9.Rows[i]["Reins"].ObjToDouble();
                if (reins > 0D)
                {
                    dRows = dt.Select("contractNumber='" + contractNumber + "' AND agentNumber = '" + agent + "'");
                    if (dRows.Length > 0)
                    {
                        row = dRows[0]["num"].ObjToInt32() - 1;
                        totals = dt.Rows[row]["Reins"].ObjToDouble();
                        dt.Rows[row]["Reins"] = totals + reins;
                    }
                }
            }
        }
        /****************************************************************************************/
        public static void FilterDeletedPayments(DataTable dt)
        {
            string status = "";
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                status = dt.Rows[i]["fill"].ObjToString();
                if (status.ToUpper() == "D")
                    dt.Rows.RemoveAt(i);
            }
        }
        /****************************************************************************************/
        private void CalcLapseReinstate(DataTable dt)
        {
            //            DateTime runDate = this.dateTimePicker1.Value;
            DateTime runDate = this.dateTimePicker3.Value;
            runDate = runDate.AddMonths(-1);
            DateTime lapseDate = DateTime.Now;
            DateTime reinstateDate = DateTime.Now;
            double contractValue = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractValue = dt.Rows[i]["contractValue"].ObjToDouble();
                lapseDate = dt.Rows[i]["lapseDate8"].ObjToDateTime();
                reinstateDate = dt.Rows[i]["reinstateDate8"].ObjToDateTime();
                if (lapseDate.Year > 1975)
                {
                    //                    if ( lapseDate < runDate )
                    dt.Rows[i]["lapseContract$"] = contractValue;
                }
                if (reinstateDate.Year > 1975)
                {
                    if (lapseDate.Year > 1975)
                    {
                        if (reinstateDate > lapseDate)
                        {
                            dt.Rows[i]["lapseContract$"] = 0D;
                            dt.Rows[i]["reinstateContract$"] = contractValue;
                        }
                    }
                    else
                        dt.Rows[i]["reinstateContract$"] = contractValue;
                }
            }
        }
        /****************************************************************************************/
        private void ReconcileAllAgents(DataTable dt, DataTable dt8, DataTable dt9)
        {
            DataTable dx = dt9.Copy();
            dx.Columns.Add("Where");
            dx.Columns.Add("Done");
            if (G1.get_column_number(dx, "LLOC") < 0)
                dx.Columns.Add("LLoc");
            for (int i = 0; i < dx.Rows.Count; i++)
                dx.Rows[i]["Where"] = "9";

            string contractNumber = "";
            string contract = "";
            double totalContracts = 0D;
            double contractValue = 0D;
            double goal = 0D;
            double recap = 0D;
            double reins = 0D;
            double dbr = 0D;
            double value = 0D;
            double lapseRecaps = 0D;
            double lapseReins = 0D;
            int count = dx.Rows.Count;
            for (int i = 0; i < dt8.Rows.Count; i++)
                dx.ImportRow(dt8.Rows[i]);
            int icount = dx.Rows.Count;

            DataView tempview = dx.DefaultView;
            tempview.Sort = "agentName asc, issueDate8 asc, contractNumber asc";
            dx = tempview.ToTable();

            string agentName = "";
            string newAgentName = "";
            DateTime issueDate8 = DateTime.Now;
            DateTime newIssueDate8 = DateTime.Now;
            bool first = true;
            double totalRecaps = 0D;
            double totalReins = 0D;
            double totalCurrentRecaps = 0D;
            double totalCurrentReins = 0D;
            double totalLapseRecaps = 0D;
            double totalLapseReins = 0D;
            double totalDbrSales = 0D;
            double percent = 0D;
            string done = "";
            string where = "";
            DateTime lapseDate = DateTime.Now;
            DateTime reinsDate = DateTime.Now;
            int row = 0;
            bool bonusWasPaid = false;
            double bonusPaid = 0D;
            bool fixedIt = false;


            for (int i = 0; i < dx.Rows.Count; i++)
            {
                done = dx.Rows[i]["done"].ObjToString();
                if (done == "DONE")
                    continue;

                totalRecaps = 0D;
                totalReins = 0D;
                totalCurrentRecaps = 0D;
                totalCurrentReins = 0D;
                totalLapseRecaps = 0D;
                totalLapseReins = 0D;
                totalDbrSales = 0D;

                agentName = dx.Rows[i]["agentName"].ObjToString();
                if (String.IsNullOrWhiteSpace(agentName))
                    continue;
                issueDate8 = dx.Rows[i]["issueDate8"].ObjToDateTime();
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "M15075UI")
                {
                }
                if (agentName.ToUpper() == "BERT BRYANT")
                {

                }


                where = dx.Rows[i]["where"].ObjToString();
                recap = dx.Rows[i]["Recap"].ObjToDouble();
                //if ( where == "9")
                //    recap = 0D;
                reins = dx.Rows[i]["Reins"].ObjToDouble();
                dbr = dx.Rows[i]["dbrSales"].ObjToDouble();
                value = dx.Rows[i]["contractValue"].ObjToDouble();
                lapseDate = dx.Rows[i]["lapseDate8"].ObjToDateTime();
                if (recap > 0D)
                {
                    totalRecaps += recap;
                    totalCurrentRecaps += value;
                }
                else if (reins > 0D)
                {
                    totalReins += reins;
                    totalCurrentReins += value;
                    if (lapseDate.Year > 1910)
                    {
                        percent = dx.Rows[i]["percent"].ObjToDouble();
                        double oldRecap = percent * value;
                        //                        totalRecaps += oldRecap;
                        totalLapseRecaps += value;
                    }
                }
                if (dbr > 0D)
                    totalDbrSales += dbr;
                if (where == "9")
                {
                    lapseReins = dx.Rows[i]["lapseRecaps"].ObjToDouble();
                    totalLapseReins = lapseReins;
                }
                else
                {
                    lapseRecaps = dx.Rows[i]["lapseRecaps"].ObjToDouble();
                    totalLapseRecaps = lapseRecaps;
                }
                row = i;

                for (int j = (i + 1); j < dx.Rows.Count; j++)
                {
                    newAgentName = dx.Rows[j]["agentName"].ObjToString();
                    if (newAgentName.ToUpper() == "WALTER DUKES")
                    {

                    }
                    newIssueDate8 = dx.Rows[j]["issueDate8"].ObjToDateTime();
                    if (newAgentName != agentName)
                        break;
                    if (newIssueDate8.Year != issueDate8.Year || newIssueDate8.Month != issueDate8.Month)
                        break;
                    contract = dx.Rows[j]["contractNumber"].ObjToString();
                    if (contract == contractNumber)
                    { // Damn! Same Contract, same Date
                    }
                    if (contract == "M15075UI")
                    {
                    }

                    row = j;
                    where = dx.Rows[j]["where"].ObjToString();
                    recap = dx.Rows[j]["Recap"].ObjToDouble();
                    reins = dx.Rows[j]["Reins"].ObjToDouble();
                    dbr = dx.Rows[j]["dbrSales"].ObjToDouble();
                    value = dx.Rows[j]["contractValue"].ObjToDouble();
                    if (recap > 0D)
                    {
                        totalRecaps += recap;
                        totalCurrentRecaps += value;
                    }
                    else if (reins > 0D)
                    {
                        totalReins += reins;
                        totalCurrentReins += value;
                    }
                    if (dbr > 0D)
                        totalDbrSales += dbr;
                    if (where == "9")
                    { // This is old Reinstates from previous processing
                        lapseReins = dx.Rows[j]["lapseRecaps"].ObjToDouble();
                        totalLapseReins = lapseReins;
                    }
                    else
                    { // This is old Lapse Recaps from previous processing
                        lapseRecaps = dx.Rows[j]["lapseRecaps"].ObjToDouble();
                        totalLapseRecaps = lapseRecaps;
                    }
                    dx.Rows[j]["done"] = "DONE";
                }

                // This gets confusing!
                if (totalLapseReins > 0D)
                {

                }
                percent = dx.Rows[i]["percent"].ObjToDouble();
                totalContracts = dx.Rows[i]["totalContracts"].ObjToDouble(); // Total Value of the Contracts
                totalContracts -= totalLapseRecaps; // Reduce Total Contracts by previously lapsed contracts.
                totalContracts += totalLapseReins; // Increase Total Contracts by previously reinstated contracts.
                totalContracts -= totalDbrSales; // Reduce Total Contracts by previously DBR's
                goal = dx.Rows[i]["goal"].ObjToDouble();
                if (goal == 0D)
                { // No Goal Setup at all! Do no harm
                    for (int j = i; j <= row; j++)
                    {
                        dx.Rows[j]["Recap"] = 0D;
                        dx.Rows[j]["Reins"] = 0D;
                        dx.Rows[j]["commission"] = 0D;
                    }
                    continue;
                }
                dx.Rows[i]["lapseRecaps"] = totalLapseRecaps;
                bonusWasPaid = false;
                if (totalContracts >= goal)
                    bonusWasPaid = true;
                double totalTotal = totalContracts - totalCurrentRecaps + totalCurrentReins;
                if ((totalContracts - totalCurrentRecaps + totalCurrentReins) < goal)
                { // Crap! Gotta take it away.
                    for (int j = i; j <= row; j++)
                    {
                        dx.Rows[j]["Recap"] = 0D;
                        dx.Rows[j]["Reins"] = 0D;
                    }
                    if (bonusWasPaid)
                    { // Gotta remove the entire bonus that was paid whenever.
                        bonusPaid = totalContracts * percent;
                        fixedIt = false;
                        for (int k = row; k >= i; k--)
                        {
                            where = dx.Rows[k]["where"].ObjToString();
                            if (String.IsNullOrWhiteSpace(where))
                            {
                                fixedIt = true;
                                dx.Rows[k]["Recap"] = bonusPaid; // Take it all back!
                                break;
                            }
                        }
                        if (!fixedIt)
                            dx.Rows[i]["Recap"] = bonusPaid; // Take it all away. This should never happen here.
                    }
                }
                else if (!bonusWasPaid)
                { // Bonus was not previously paid. Now I have to pay it.
                    for (int j = i; j <= row; j++)
                    {
                        dx.Rows[j]["Recap"] = 0D;
                        dx.Rows[j]["Reins"] = 0D;
                    }
                    bonusPaid = (totalContracts - totalCurrentRecaps + totalCurrentReins) * percent;
                    fixedIt = false;
                    for (int k = row; k >= i; k--)
                    {
                        where = dx.Rows[k]["where"].ObjToString();
                        if (where == "9") // Try to find a row from the Reinstate Tab
                        {
                            fixedIt = true;
                            dx.Rows[k]["Reins"] = bonusPaid; // Give it all back!
                            break;
                        }
                    }
                    if (!fixedIt)
                        dx.Rows[i]["Reins"] = bonusPaid; // Give it all back. This should never happen here.
                }
            }
            dgv12.DataSource = dx;
            dt8.Rows.Clear();
            dt9.Rows.Clear();
            bool found = false;
            int lastFoundRow = -1;
            string mainAgent = "";
            string foundAgent = "";
            DataTable tempDt = dt.Clone();
            DataRow[] dR = dx.Select("contractNumber='M15075UI'");
            tempDt = dx.Clone();
            G1.ConvertToTable(dR, tempDt);
            dR = dt.Select("contractNumber='M15075UI'");
            tempDt = dt.Clone();
            G1.ConvertToTable(dR, tempDt);
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                where = dx.Rows[i]["where"].ObjToString();
                if (where == "9")
                    dt9.ImportRow(dx.Rows[i]);
                else
                    dt8.ImportRow(dx.Rows[i]);
                issueDate8 = dx.Rows[i]["issueDate8"].ObjToDateTime();
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "M15075UI")
                {
                }
                mainAgent = dx.Rows[i]["agentNumber"].ObjToString();
                if (mainAgent == "N30")
                {
                }
                found = false;
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    contract = dt.Rows[j]["contractNumber"].ObjToString();
                    newIssueDate8 = dt.Rows[j]["issueDate8"].ObjToDateTime();
                    if (contract != contractNumber)
                        continue;
                    if (issueDate8 == newIssueDate8)
                    {
                        foundAgent = dt.Rows[j]["agentNumber"].ObjToString();
                        lastFoundRow = j;
                        if (foundAgent == mainAgent)
                        {
                            recap = dx.Rows[i]["Recap"].ObjToDouble();
                            dt.Rows[j]["Recap"] = dx.Rows[i]["Recap"].ObjToDouble();
                            dt.Rows[j]["Reins"] = dx.Rows[i]["Reins"].ObjToDouble();
                            found = true;
                            break;
                        }
                        //break;
                    }
                }
                //if (!found && lastFoundRow >= 0)
                //{
                //    G1.copy_dt_row(dt, lastFoundRow, dt, dt.Rows.Count);
                //    lastFoundRow = dt.Rows.Count - 1;
                //    dt.Rows[lastFoundRow]["agentNumber"] = mainAgent;
                //    dt.Rows[lastFoundRow]["agentName"] = dx.Rows[i]["agentName"].ObjToString();
                //    dt.Rows[lastFoundRow]["Recap"] = dx.Rows[i]["Recap"].ObjToDouble();
                //    dt.Rows[lastFoundRow]["Reins"] = dx.Rows[i]["Reins"].ObjToDouble();
                //    dt.Rows[lastFoundRow]["paymentAmount"] = 0D;
                //    dt.Rows[lastFoundRow]["debitAdjustment"] = 0D;
                //    dt.Rows[lastFoundRow]["creditAdjustment"] = 0D;
                //    dt.Rows[lastFoundRow]["downPayment"] = 0D;
                //    dt.Rows[lastFoundRow]["interestPaid"] = 0D;
                //    dt.Rows[lastFoundRow]["trust85P"] = 0D;
                //    dt.Rows[lastFoundRow]["trust100P"] = 0D;
                //    dt.Rows[lastFoundRow]["totalPayments"] = 0D;
                //    dt.Rows[lastFoundRow]["contractValue"] = 0D;
                //}
            }
            dR = dt.Select("contractNumber='L17045UI'");
            tempDt = dt.Clone();
            G1.ConvertToTable(dR, tempDt);
            //            dR = dt8.Select("contractNumber='L17045UI'");
            dR = dt8.Select("agentNumber='V25' AND (loc='C' OR loc='L' OR loc='E')");
            tempDt = dt8.Clone();
            G1.ConvertToTable(dR, tempDt);
        }
        /****************************************************************************************/
        private void LabelNewContracts(DataTable dt, DateTime d1, DateTime d2)
        {
            if (G1.get_column_number(dt, "NEWCONTRACT") < 0)
                dt.Columns.Add("NEWCONTRACT");
            DateTime issueDate = DateTime.Now;
        }
        /****************************************************************************************/
        public static void FindContract(DataTable dt, string contract = "", bool stop = false)
        {
            if (dt == null)
                return;
            if (String.IsNullOrWhiteSpace(contract))
                contract = "C18058L";
            DataRow[] dRows = dt.Select("contractNumber='" + contract + "'");
            if (dRows.Length > 0)
            {
                DataTable ddx = dt.Clone();
                G1.ConvertToTable(dRows, ddx);
                if (stop)
                    MessageBox.Show("***Here*** Found Contract " + contract + " with " + ddx.Rows.Count + " Rows!");
            }
        }
        /****************************************************************************************/
        private void LoadLocationTrustCombo(DataTable dt)
        {
            DataTable locDt = new DataTable();
            locDt.Columns.Add("locations");
            DataTable trustDt = new DataTable();
            trustDt.Columns.Add("trusts");
            string c = "";

            string locations = getLocations(dt);
            string[] Lines = locations.Split(',');
            for (int i = 0; i < Lines.Length; i++)
            {
                c = Lines[i].Trim();
                if (String.IsNullOrWhiteSpace(c))
                    continue;
                DataRow dRow = locDt.NewRow();
                dRow["locations"] = c;
                locDt.Rows.Add(dRow);
            }
            //            chkComboLocation.Properties.DataSource = locDt;

            string trusts = getTrusts(dt);
            Lines = trusts.Split(',');
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
        public static void CalculateNewContracts(DataTable dt, DateTime date1, DateTime date2)
        {
            FindContract(dt);

            int days = 0;

            if (date1 == date2)
            {
                DateTime lapseDate = date1;
                days = lapseDate.Day;
                if (date1 == date2)
                    days = 1;
                string start = lapseDate.Year.ToString("D4") + "-" + lapseDate.Month.ToString("D2") + "-" + days.ToString("D2");
                date1 = start.ObjToDateTime();

                lapseDate = date2;
                if (date1 == date2)
                    days = DateTime.DaysInMonth(lapseDate.Year, lapseDate.Month);
                string end = lapseDate.Year.ToString("D4") + "-" + lapseDate.Month.ToString("D2") + "-" + days.ToString("D2");
                date2 = end.ObjToDateTime();
            }

            AddColumnToTable(dt, "num");
            AddColumnToTable(dt, "customer");
            AddColumnToTable(dt, "agentName");
            AddColumnToTable(dt, "Location Name");
            AddColumnToTable(dt, "fbi", "System.Double");
            AddColumnToTable(dt, "totalPayments", "System.Double");
            AddColumnToTable(dt, "debit", "System.Double");
            AddColumnToTable(dt, "credit", "System.Double");
            AddColumnToTable(dt, "commission", "System.Double");
            AddColumnToTable(dt, "dbcMoney", "System.Double");
            AddColumnToTable(dt, "contractValue", "System.Double");
            AddColumnToTable(dt, "ibtrust", "System.Double");
            AddColumnToTable(dt, "sptrust", "System.Double");
            AddColumnToTable(dt, "xxtrust", "System.Double");
            AddColumnToTable(dt, "lapseContract$", "System.Double");
            AddColumnToTable(dt, "reinstateContract$", "System.Double");
            AddColumnToTable(dt, "dbr");
            AddColumnToTable(dt, "newcontract");
            AddColumnToTable(dt, "method", "System.Double");
            AddColumnToTable(dt, "dbc", "System.Double");

            string cmd = "";

            string fname = "";
            string lname = "";
            string name = "";

            double totalPay = 0D;

            double payment = 0D;
            double ccFee = 0D;
            double debit = 0D;
            double credit = 0D;
            double trust85 = 0D;
            double trust100 = 0D;
            double downpayment = 0D;
            double downPayment = 0D;
            double downPayment1 = 0D;
            double contractValue = 0D;
            double cashAdvance = 0D;
            double interest = 0D;
            string date7 = "";
            string contract = "";
            DateTime issueDate = DateTime.Now;
            DateTime deceasedDate;
            double financeMonths = 0D;
            double rate = 0D;
            double principal = 0D;
            double amtOfMonthlyPayt = 0D;
            double retained = 0D;
            int method = 0;
            bool dbr = false;
            double dbc = 0D;
            bool calculateTrust100 = false;
            G1.NumberDataTable(dt);
            string edited = "";
            string record = "";
            double prince = 0D;
            double originalDownPayment = 0D;
            DateTime payDate8 = DateTime.Now;
            double saveRetained = 0D;
            string lockTrust85 = "";
            string finale = "";

            //if ( G1.get_column_number ( dt, "ap") < 0 )
            //    AddColumnToTable(dt, "ap", "System.Double");

            DailyHistory.AddAP(dt);


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                fname = dt.Rows[i]["firstName"].ObjToString();
                lname = dt.Rows[i]["lastName"].ObjToString();
                name = fname + " " + lname;
                dt.Rows[i]["customer"] = name;
                payDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();

                //payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                //dt.Rows[i]["ap"] = payment;
                //ccFee = dt.Rows[i]["ccFee"].ObjToDouble();
                //payment -= ccFee;
                //dt.Rows[i]["paymentAmount"] = payment;

                //payment = DailyHistory.getPayment(dt, i);

                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                downpayment = dt.Rows[i]["downPayment"].ObjToDouble();
                lockTrust85 = dt.Rows[i]["lockTrust85"].ObjToString().ToUpper();

                contract = dt.Rows[i]["contractNumber"].ObjToString();
                if (contract == "C22028LI")
                {

                }
                issueDate = dt.Rows[i]["issueDate8"].ObjToDateTime();
                issueDate = DailyHistory.GetIssueDate(issueDate, contract, null);
                if (downpayment > 0D)
                {
                    date7 = G1.GetSQLDate(dt, i, "issueDate8");
                    if (date7.IndexOf("0000") >= 0)
                    {
                        cmd = "Select `contractDate` from `customers` where `contractNumber` = '" + contract + "';";
                        DataTable ddx = G1.get_db_data(cmd);
                        if (ddx.Rows.Count > 0)
                        {
                            try
                            {
                                dt.Rows[i]["issueDate8"] = G1.DTtoMySQLDT(ddx.Rows[0]["contractDate"]);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                }
                //saveRetained = dt.Rows[i]["retained"].ObjToDouble();
                trust85 = dt.Rows[i]["trust85P"].ObjToDouble();
                trust100 = dt.Rows[i]["trust100P"].ObjToDouble();
                finale = dt.Rows[i]["new"].ObjToString().ToUpper();
                edited = dt.Rows[i]["edited"].ObjToString();
                if (edited.ToUpper() == "TRUSTADJ" || edited.ToUpper() == "CEMETERY")
                {
                    principal = trust100;
                    continue;
                }
                payment = G1.RoundValue(payment);
                debit = G1.RoundValue(debit);
                credit = G1.RoundValue(credit);
                downpayment = G1.RoundValue(downpayment);
                contractValue = DailyHistory.GetContractValue(dt.Rows[i]);
                contractValue = G1.RoundValue(contractValue);
                if (contract.ToUpper() == "E14111UI")
                {
                }
                if (payment == 0D && downpayment == 0D && (credit != 0D || debit != 0D))
                {
                    calculateTrust100 = false;
                    trust85 = dt.Rows[i]["trust85P"].ObjToDouble();
                    trust100 = dt.Rows[i]["trust100P"].ObjToDouble();
                }
                else
                {
                    if (lockTrust85 != "Y" && payDate8 >= DailyHistory.majorDate)
                    {
                        if (finale.ToUpper() != "FINALE")
                        {
                            dt.Rows[i]["trust85P"] = 0D;
                            dt.Rows[i]["trust100P"] = 0D;
                        }
                    }
                }

                dbr = false;
                if (downpayment != 0D)
                {
                    //cashAdvance = dt.Rows[i]["cashAdvance"].ObjToDouble();
                    //cashAdvance = G1.RoundValue(cashAdvance);
                    //contractValue -= cashAdvance;
                    dbc = 0D;
                    dt.Rows[i]["contractValue"] = contractValue;
                    deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                    if (deceasedDate.Year > 1850)
                    {
                        dbr = true;
                        dt.Rows[i]["dbr"] = "DBR";
                        //if (dt.Rows[i]["SetAsDBR"].ObjToString().ToUpper() == "Y")
                        //{
                        //    dt.Rows[i]["dbr"] = "DBR";
                        //    dbr = true;
                        //}
                        if (!Commission.ShouldCommissionBePaid(dt, i))
                        {
                            if (downpayment != 0D)
                                dbc = contractValue;
                            dt.Rows[i]["dbc"] = dbc;
                        }
                        //days = Commission.CalcBusinessDays(deceasedDate.ToString("MM/dd/yyyy"), issueDate.ToString("MM/dd/yyyy"));
                        //if (days < 10)
                        //{
                        //    dt.Rows[i]["dbr"] = "DBR";
                        //    dbr = true;
                        //}
                    }
                    //date7 = G1.GetSQLDate(dt, i, "deceasedDate");
                    //if (G1.validate_date(date7))
                    //    dt.Rows[i]["dbr"] = "DBR";
                }

                //if (contract != "B18035LI")
                //{
                //    continue;
                //}

                contractValue = DailyHistory.GetContractValuePlus(dt.Rows[i]);
                issueDate = dt.Rows[i]["issueDate8"].ObjToDateTime();
                issueDate = DailyHistory.GetIssueDate(issueDate, contract, null);
                financeMonths = dt.Rows[i]["numberOfPayments"].ObjToDouble();
                amtOfMonthlyPayt = dt.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                rate = dt.Rows[i]["apr1"].ObjToDouble() / 100.0D;
                downPayment1 = dt.Rows[i]["downPayment1"].ObjToDouble();
                if (downpayment > 0D)
                {
                    if (downPayment1 > downpayment)
                        downpayment = downPayment1;
                }

                record = dt.Rows[i]["record"].ObjToString();
                if (payDate8 >= DailyHistory.majorDate)
                {
                    DailyHistory.CalculateNewInterest(contract, record, ref interest, ref prince);
                    if (payDate8 > DailyHistory.secondDate)
                    {
                        if (dt.Rows[i]["interestPaid"].ObjToDouble() != interest)
                            dt.Rows[i]["interestPaid"] = interest;
                    }
                }

                principal = payment + credit - debit - interest + downpayment;
                principal = G1.RoundDown(principal);
                trust85 = dt.Rows[i]["trust85P"].ObjToDouble();
                trust100 = dt.Rows[i]["trust100P"].ObjToDouble();
                calculateTrust100 = true;
                if (finale.ToUpper() == "FINALE")
                {
                    calculateTrust100 = false;
                    trust85 = dt.Rows[i]["trust85P"].ObjToDouble();
                    trust100 = dt.Rows[i]["trust100P"].ObjToDouble();
                }
                if (payDate8 < DailyHistory.majorDate)
                    calculateTrust100 = false;
                if (payment == 0D && downpayment == 0D && (credit != 0D || debit != 0D))
                {
                    calculateTrust100 = false;
                    trust85 = dt.Rows[i]["trust85P"].ObjToDouble();
                    trust100 = dt.Rows[i]["trust100P"].ObjToDouble();
                }
                else
                {
                    if (calculateTrust100 && lockTrust85 != "Y")
                    {
                        dt.Rows[i]["trust85P"] = 0D;
                        dt.Rows[i]["trust100P"] = 0D;
                    }
                }
                if (payment != 0D || downpayment != 0D || credit != 0D || debit != 0D)
                    calculateTrust100 = true;
                if (lockTrust85 == "Y" || payDate8 < DailyHistory.majorDate || finale.ToUpper() == "FINALE")
                    calculateTrust100 = false;

                if (downPayment1 == 0D)
                {
                    originalDownPayment = DailyHistory.GetOriginalDownPayment(dt.Rows[i]);
                    if (originalDownPayment > 0D)
                        downPayment1 = originalDownPayment;
                    if (downPayment1 == 0D)
                    {
                        originalDownPayment = DailyHistory.GetDownPaymentFromPayments(contract);
                        if (originalDownPayment > 0D)
                            downPayment1 = originalDownPayment;
                    }
                }

                if (calculateTrust100 && debit == 0D && credit == 0D)
                {
                    method = ImportDailyDeposits.CalcTrust85P(payDate8, amtOfMonthlyPayt, issueDate.ToString("MM/dd/yyyy"), contractValue, downPayment1, financeMonths, payment, principal, debit, credit, rate, ref trust85, ref trust100, ref retained);
                    if (principal < 0D && debit == 0D)
                        interest = payment;
                    //                    method = ImportDailyDeposits.CalcTrust85(issueDate.ToString("MM/dd/yyyy"), contractValue, downPayment1, financeMonths, payment, principal, rate, ref trust85, ref trust100);
                }
                else
                {
                    trust85 = dt.Rows[i]["trust85P"].ObjToDouble();
                    trust100 = dt.Rows[i]["trust100P"].ObjToDouble();
                    retained = payment - trust100;
                    if (payment == 0D && credit > 0D)
                        retained = credit - trust100;
                    else if (payment == 0D && debit > 0D)
                    {
                        retained = debit - Math.Abs(trust100);
                        retained = retained * -1D;
                    }
                    if (finale.ToUpper() == "FINALE")
                        retained = dt.Rows[i]["retained"].ObjToDouble();
                    //if (saveRetained != 0D)
                    //    retained = saveRetained;
                }
                //if (!dbr)
                //{
                dt.Rows[i]["trust85P"] = trust85;
                dt.Rows[i]["trust100P"] = trust100;
                dt.Rows[i]["method"] = method.ObjToDouble();

                if (DailyHistory.IsFundedByInsurance(dt.Rows[i]))
                {
                    dt.Rows[i]["fbi"] = 1D;
                    dt.Rows[i]["contractValue"] = 0D;
                }

                totalPay = payment + downpayment + credit - debit - interest;
                if (dt.Rows[i]["dbc"].ObjToDouble() > 0D)
                    totalPay = totalPay - downpayment;

                dt.Rows[i]["totalPayments"] = totalPay;
                dt.Rows[i]["debit"] = debit;
                dt.Rows[i]["credit"] = credit;
                //}
                //else
                //{
                //    dt.Rows[i]["totalPayments"] = 0D;
                //    dt.Rows[i]["debit"] = 0D;
                //    dt.Rows[i]["credit"] = 0D;
                //}

                fname = dt.Rows[i]["firstName1"].ObjToString().Trim();
                lname = dt.Rows[i]["lastName1"].ObjToString().Trim();
                name = fname + " " + lname;
                dt.Rows[i]["agentName"] = name;
            }

            G1.NumberDataTable(dt);

            FindContract(dt, "B18035LI");
            LoadOtherCombos(dt);
            FindContract(dt);

            LoadPaidContracts(dt, date1, date2);
            GuaranteeContractsIssued(dt, date1, date2);

            FindContract(dt);

            //string locations = getLocationQuery();
            //if (!String.IsNullOrWhiteSpace(locations))
            //{
            //    DataRow[] dRows = dt.Select(locations);
            //    if (dRows.Length > 0)
            //    {
            //        DataTable newDt = dt.Clone();
            //        for (int i = 0; i < dRows.Length; i++)
            //        {
            //            newDt.ImportRow(dRows[i]);
            //        }
            //        dt.Rows.Clear();
            //        dt = newDt.Copy();
            //    }
            //}

            //            PaymentsWithoutContracts(dt);
        }
        /****************************************************************************************/
        //private void btnRunx_Click(object sender, EventArgs e)
        //{
        //    this.Cursor = Cursors.WaitCursor;
        //    DateTime date = dateTimePicker1.Value;
        //    string date1 = G1.DateTimeToSQLDateTime(date);
        //    date = dateTimePicker2.Value;
        //    string date2 = G1.DateTimeToSQLDateTime(date);

        //    date = dateTimePicker4.Value;
        //    string date3 = G1.DateTimeToSQLDateTime(date);
        //    date = dateTimePicker3.Value;
        //    string date4 = G1.DateTimeToSQLDateTime(date);

        //    if (!chkDatePaid.Checked && !chkDueDate.Checked)
        //        chkDatePaid.Checked = true;

        //    string paidDate = "`payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' ";
        //    string dueDate = "`dueDate8` >= '" + date3 + "' and `dueDate8` <= '" + date4 + "' ";

        //    //            string cmd = "Select * from `customers` p JOIN `contracts` d ON p.`contractNumber` = d.`contractNumber` ";


        //    string cmd = "Select p.*,d.*,a.`firstName`,a.`lastName` from `payments` p LEFT JOIN `contracts` d on p.`contractNumber` = d.`contractNumber` LEFT JOIN `agents` a ON p.`agentNumber` = a.`agentCode` ";
        //    //            cmd += " LEFT JOIN `customers` c ON p.`contractNumber` = c.`contractNumber` ";
        //    cmd += " WHERE ";
        //    if (chkDatePaid.Checked)
        //        cmd += paidDate;
        //    if (chkDueDate.Checked)
        //    {
        //        if (chkDatePaid.Checked)
        //            cmd += " and ";
        //        cmd += dueDate;
        //    }

        //    if (firstAgent)
        //    {
        //        firstAgent = false;
        //        loadAgents(cmd);
        //    }

        //    string agents = getAgentQuery();
        //    if (!String.IsNullOrWhiteSpace(agents))
        //        cmd += " and " + agents;

        //    if (String.IsNullOrWhiteSpace(mainQuery))
        //        mainQuery = cmd;

        //    DataTable dt = G1.get_db_data(cmd);

        //    //            DataRow [] dR = dt.Select("contractNumber='N13007U'");

        //    dt.Columns.Add("num");
        //    dt.Columns.Add("customer");
        //    dt.Columns.Add("agentName");
        //    dt.Columns.Add("Location Name");
        //    dt.Columns.Add("totalPayments", Type.GetType("System.Double"));
        //    dt.Columns.Add("commission", Type.GetType("System.Double"));
        //    dt.Columns.Add("contractValue", Type.GetType("System.Double"));
        //    dt.Columns.Add("ibtrust", Type.GetType("System.Double"));
        //    dt.Columns.Add("sptrust", Type.GetType("System.Double"));
        //    dt.Columns.Add("xxtrust", Type.GetType("System.Double"));
        //    dt.Columns.Add("lapseContract$", Type.GetType("System.Double"));
        //    dt.Columns.Add("reinstateContract$", Type.GetType("System.Double"));
        //    dt.Columns.Add("dbr");
        //    string fname = "";
        //    string lname = "";
        //    string name = "";

        //    //double totalTrust85 = 0D;
        //    //double totalTrust100 = 0D;
        //    //double totalPayments = 0D;
        //    //double totalDown = 0D;
        //    double totalPay = 0D;

        //    double payment = 0D;
        //    double debit = 0D;
        //    double credit = 0D;
        //    double interest = 0D;
        //    double trust85 = 0D;
        //    double trust100 = 0D;
        //    double downpayment = 0D;
        //    double contractValue = 0D;
        //    double cashAdvance = 0D;
        //    string date7 = "";
        //    string contract = "";

        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        fname = dt.Rows[i]["firstName"].ObjToString();
        //        lname = dt.Rows[i]["lastName"].ObjToString();
        //        name = fname + " " + lname;
        //        dt.Rows[i]["customer"] = name;
        //        payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
        //        debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
        //        credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
        //        interest = dt.Rows[i]["interestPaid"].ObjToDouble();
        //        downpayment = dt.Rows[i]["downPayment"].ObjToDouble();
        //        contract = dt.Rows[i]["contractNumber"].ObjToString();
        //        if (downpayment > 0D)
        //        {
        //            date7 = G1.GetSQLDate(dt, i, "issueDate8");
        //            if (date7.IndexOf("0000") >= 0)
        //            {
        //                cmd = "Select `contractDate` from `customers` where `contractNumber` = '" + contract + "';";
        //                DataTable ddx = G1.get_db_data(cmd);
        //                if (ddx.Rows.Count > 0)
        //                {
        //                    try
        //                    {
        //                        dt.Rows[i]["issueDate8"] = G1.DTtoMySQLDT(ddx.Rows[0]["contractDate"]);
        //                    }
        //                    catch (Exception ex)
        //                    {

        //                    }
        //                }
        //            }
        //        }
        //        trust85 = dt.Rows[i]["trust85P"].ObjToDouble();
        //        trust100 = dt.Rows[i]["trust100P"].ObjToDouble();
        //        payment = G1.RoundValue(payment);
        //        downpayment = G1.RoundValue(downpayment);
        //        credit = G1.RoundValue(credit);
        //        debit = G1.RoundValue(debit);
        //        interest = G1.RoundValue(interest);
        //        if (downpayment != 0D)
        //        {
        //            contractValue = DailyHistory.GetContractValue(dt.Rows[i]);
        //            contractValue = G1.RoundValue(contractValue);
        //            cashAdvance = dt.Rows[i]["cashAdvance"].ObjToDouble();
        //            cashAdvance = G1.RoundValue(cashAdvance);
        //            contractValue -= cashAdvance;
        //            dt.Rows[i]["contractValue"] = contractValue;
        //            date7 = G1.GetSQLDate(dt, i, "deceasedDate");
        //            if (G1.validate_date(date7))
        //                dt.Rows[i]["dbr"] = "DBR";
        //        }

        //        totalPay = payment + downpayment + credit - debit - interest;
        //        dt.Rows[i]["totalPayments"] = totalPay;

        //        //totalDown += downpayment;
        //        //totalPayments += payment;
        //        //totalTrust85 += trust85;
        //        //totalTrust100 += trust100;

        //        fname = dt.Rows[i]["firstName1"].ObjToString();
        //        lname = dt.Rows[i]["lastName1"].ObjToString();
        //        name = fname + " " + lname;
        //        dt.Rows[i]["agentName"] = name;
        //    }


        //    if (btnCalc.Visible)
        //        CalcCommission(dt, this.dateTimePicker1.Value, this.dateTimePicker2.Value);
        //    btnCalc.Show();

        //    LoadOtherCombos(dt);

        //    LoadPaidContracts(dt, this.dateTimePicker1.Value, this.dateTimePicker2.Value);

        //    string locations = getLocationQuery();
        //    if (!String.IsNullOrWhiteSpace(locations))
        //    {
        //        DataRow[] dRows = dt.Select(locations);
        //        if (dRows.Length > 0)
        //        {
        //            DataTable newDt = dt.Clone();
        //            for (int i = 0; i < dRows.Length; i++)
        //            {
        //                newDt.ImportRow(dRows[i]);
        //            }
        //            dt.Rows.Clear();
        //            dt = newDt.Copy();
        //        }
        //    }

        //    //            PaymentsWithoutContracts(dt);
        //    //            CheckForMainLapse(dt);

        //    DataTable dt8 = CheckForMainLapse(dt, this.dateTimePicker1.Value, this.dateTimePicker2.Value);
        //    G1.NumberDataTable(dt);
        //    dgv.DataSource = dt;

        //    dgv.RefreshDataSource();
        //    gridMain.RefreshData();
        //    dgv.Refresh();
        //    this.Refresh();

        //    originalDt = dt;
        //    this.Cursor = Cursors.Default;
        //}
        /*******************************************************************************************/
        //private void PaymentsWithoutContracts(DataTable dt)
        //{
        //    DateTime date = dateTimePicker1.Value;
        //    string date1 = G1.DateTimeToSQLDateTime(date);
        //    date = dateTimePicker2.Value;
        //    string date2 = G1.DateTimeToSQLDateTime(date);

        //    string paidDate = "`payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' ";

        //    string cmd = "SELECT * FROM `payments` WHERE `downPayment` > '0' AND ";

        //    if (chkDatePaid.Checked)
        //        cmd += paidDate;

        //    string agents = getAgentQuery();
        //    if (!String.IsNullOrWhiteSpace(agents))
        //        cmd += " and " + agents;

        //    cmd += ";";
        //    DataTable dx = G1.get_db_data(cmd);

        //    DataTable dd = dt.Clone();
        //    string contract = "";

        //    try
        //    {
        //        //                DataRow[] dRow = dx.Select("price=downPayment");
        //        for (int i = 0; i < dx.Rows.Count; i++)
        //        {
        //            contract = dx.Rows[i]["contractNumber"].ObjToString();
        //            DataRow[] dR = dt.Select("contractNumber='" + contract + "'");
        //            if (dR.Length > 0)
        //                continue;
        //            DataRow d = dd.NewRow();
        //            d["contractNumber"] = dx.Rows[i]["contractNumber"].ObjToString();
        //            d["payDate8"] = dx.Rows[i]["payDate8"];
        //            d["downPayment"] = dx.Rows[i]["downPayment"].ObjToDouble();
        //            dd.Rows.Add(d);
        //        }
        //        for (int i = 0; i < dd.Rows.Count; i++)
        //            dt.ImportRow(dd.Rows[i]);
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        /*******************************************************************************************/
        public static void LoadPaidContracts(DataTable dt, DateTime start, DateTime stop)
        {
            //DateTime date = dateTimePicker1.Value;
            //string date1 = G1.DateTimeToSQLDateTime(date);
            //date = dateTimePicker2.Value;
            //string date2 = G1.DateTimeToSQLDateTime(date);

            DateTime date = start;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = stop;
            string date2 = G1.DateTimeToSQLDateTime(date);

            int totalRows = dt.Rows.Count;

            //string start = date1.Month.ToString("D2") + "/" + date1.Day.ToString("D2") + "/" + date1.Year.ToString("D4");
            //string end  = date2.Month.ToString("D2") + "/" + date2.Day.ToString("D2") + "/" + date2.Year.ToString("D4");


            string paidDate = "`issueDate8` >= '" + date1 + "' and `issueDate8` <= '" + date2 + "' ";

            string cmd = "SELECT *,(`serviceTotal` + `merchandiseTotal`) AS `price`, (`allowMerchandise` + `allowInsurance` + `downPayment`) AS `totalDown` FROM `contracts`  c LEFT JOIN `customers` u on c.`contractNumber` = u.`contractNumber` WHERE ";

            //            if (chkDatePaid.Checked)
            cmd += paidDate;

            //string agents = getAgentQuery("agentCode");
            //if (!String.IsNullOrWhiteSpace(agents))
            //    cmd += " and " + agents;

            cmd += ";";
            DataTable dx = G1.get_db_data(cmd);

            FindContract(dx);

            //DataRow[] ddr = dx.Select("contractNumber='CT17041'");
            //for (int i = 0; i < ddr.Length; i++)
            //{

            //}

            DataTable dd = dt.Clone();
            string contract = "";
            string ch = "";
            int idx = 0;

            double ibtrust = 0D;
            double sptrust = 0D;
            double xxtrust = 0D;
            double dbrSales = 0D;
            bool fbi = false;
            string xtrust = "";
            string deceasedDate = "";
            string issueDate = "";
            DateTime oldIssueDate = DateTime.Now;
            double financeMonths = 0D;
            double amtOfMonthlyPayt = 0D;
            double rate = 0D;
            double principal = 0D;
            double downPayment = 0D;
            double payment = 0D;
            double trust85 = 0D;
            double trust100 = 0D;
            double dbc = 0D;
            int method = 0;
            bool dbr = false;

            try
            {
                //                DataRow[] dRow = dx.Select("price=downPayment");
                string trust = "";
                string loc = "";
                double contractValue = 0D;
                DataTable test1Dt = dx.Clone();
                DataTable test2Dt = dx.Clone();
                DataRow[] dRow = dx.Select("price=totalDown");
                for (int i = 0; i < dRow.Length; i++)
                    test1Dt.ImportRow(dRow[i]);
                for (int i = 0; i < dRow.Length; i++)
                {
                    contract = dRow[i]["contractNumber"].ObjToString();
                    if (contract == "E16041UI")
                    {

                    }

                    DataRow[] dR = dt.Select("contractNumber='" + contract + "'");
                    if (dR.Length > 0)
                    {
                        test2Dt.Rows.Clear();
                        for (int kk = 0; kk < dR.Length; kk++)
                            test2Dt.ImportRow(dR[kk]);
                        continue;
                    }
                    ibtrust = 0D;
                    sptrust = 0D;
                    xxtrust = 0D;
                    xtrust = dRow[i]["xtrust"].ObjToString();

                    deceasedDate = dRow[i]["deceasedDate"].ObjToString();
                    deceasedDate = G1.GetSQLDate(deceasedDate);
                    issueDate = dRow[i]["issueDate8"].ObjToString();
                    issueDate = G1.GetSQLDate(issueDate);

                    contractValue = DailyHistory.GetContractValue(dRow[i]);
                    dbr = false;

                    //                    if (dRow[i]["SetAsDBR"].ObjToString().ToUpper() == "Y")
                    if (!Commission.CheckDeathDateCommission(deceasedDate, issueDate))
                    {
                        dbrSales += contractValue;
                        dbr = true;
                    }
                    dbc = 0D;
                    if (!Commission.ShouldCommissionBePaid(test1Dt, i))
                        dbc = contractValue;

                    DataRow d = dd.NewRow();
                    d["contractNumber"] = contract;
                    d["agentNumber"] = dRow[i]["agentCode"].ObjToString();
                    d["customer"] = dRow[i]["firstName"].ObjToString() + " " + dRow[i]["lastName"].ObjToString();
                    d["firstName"] = dRow[i]["firstName"].ObjToString();
                    d["lastName"] = dRow[i]["lastName"].ObjToString();
                    d["issueDate8"] = dRow[i]["issueDate8"];
                    d["downPayment"] = dRow[i]["downPayment"].ObjToDouble();
                    d["serviceTotal"] = dRow[i]["serviceTotal"].ObjToDouble();
                    d["merchandiseTotal"] = dRow[i]["merchandiseTotal"].ObjToDouble();
                    d["allowMerchandise"] = dRow[i]["allowMerchandise"].ObjToDouble();
                    d["allowInsurance"] = dRow[i]["allowInsurance"].ObjToDouble();
                    d["contractValue"] = contractValue;
                    d["dbc"] = dbc;

                    contractValue = DailyHistory.GetContractValuePlus(dRow[i]);
                    //                    d["contractValue"] = contractValue;

                    oldIssueDate = dRow[i]["issueDate8"].ObjToDateTime();
                    oldIssueDate = DailyHistory.GetIssueDate(oldIssueDate, contract, null);

                    financeMonths = dRow[i]["numberOfPayments"].ObjToDouble();
                    amtOfMonthlyPayt = dRow[i]["amtOfMonthlyPayt"].ObjToDouble();
                    rate = dRow[i]["apr"].ObjToDouble() / 100.0D;
                    downPayment = dRow[i]["downPayment"].ObjToDouble();

                    principal = downPayment;
                    payment = downPayment;

                    method = ImportDailyDeposits.CalcTrust85(amtOfMonthlyPayt, oldIssueDate.ToString("MM/dd/yyyy"), contractValue, downPayment, financeMonths, payment, principal, rate, ref trust85, ref trust100);
                    if (!dbr)
                    {
                        d["trust85P"] = trust85;
                        d["trust100P"] = trust100;
                        contract = decodeContractNumber(contract, ref trust, ref loc);
                        d["trust"] = trust;
                        d["loc"] = loc;
                        d["xtrust"] = xtrust;
                        fbi = DailyHistory.IsFundedByInsurance(dRow[i]);
                        if (fbi)
                        {
                            d["fbi"] = 1D;
                            contractValue = 0D;
                        }

                        if (trust.Length > 0)
                        {
                            idx = trust.Length - 1;
                            ch = trust.Substring(idx);
                            if (ch.ToUpper() == "I")
                                ibtrust = contractValue;
                            else
                                sptrust = contractValue;
                        }
                        else
                            sptrust = contractValue;

                        if (xtrust.ToUpper() == "Y")
                        {
                            xxtrust = ibtrust + sptrust;
                            ibtrust = 0D;
                            sptrust = 0D;
                        }

                        d["ibtrust"] = ibtrust;
                        d["sptrust"] = sptrust;
                        d["xxtrust"] = xxtrust;
                    }
                    d["newcontract"] = "1";
                    dd.Rows.Add(d);
                }
                for (int i = 0; i < dd.Rows.Count; i++)
                {
                    contract = dd.Rows[i]["contractNumber"].ObjToString();
                    DataRow[] ddRows = dt.Select("contractNumber='" + contract + "'");
                    if (ddRows.Length <= 0)
                        dt.ImportRow(dd.Rows[i]);
                }
            }
            catch (Exception ex)
            {

            }
        }
        /*******************************************************************************************/
        public static void GuaranteeContractsIssued(DataTable dt, DateTime start, DateTime stop)
        {
            DateTime date = start;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = stop;
            string date2 = G1.DateTimeToSQLDateTime(date);

            string paidDate = "`issueDate8` >= '" + date1 + "' and `issueDate8` <= '" + date2 + "' ";
            //            string cmd = "SELECT *,(`serviceTotal` + `merchandiseTotal`) AS `price`, (`allowMerchandise` + `allowInsurance` + `downPayment`) AS `totalDown` FROM `contracts`  c LEFT JOIN `customers` u on c.`contractNumber` = u.`contractNumber` WHERE ";
            string cmd = "SELECT * FROM `contracts`  c LEFT JOIN `customers` u on c.`contractNumber` = u.`contractNumber` WHERE ";
            cmd += paidDate;
            cmd += ";";
            DataTable ddx = G1.get_db_data(cmd);

            FindContract(ddx);

            int rows = ddx.Rows.Count;
            int count = 0;
            bool found = false;

            DataTable dd = dt.Clone();
            string contract = "";
            string contractNumber = "";
            string ch = "";
            int idx = 0;

            double ibtrust = 0D;
            double sptrust = 0D;
            double xxtrust = 0D;
            double dbrSales = 0D;
            double dp = 0D;
            double ccFee = 0D;
            DateTime payDate8 = DateTime.Now;
            DataTable ccDt = null;
            bool fbi = false;
            string xtrust = "";
            string deceasedDate = "";
            string issueDate = "";
            string trust = "";
            string loc = "";
            double contractValue = 0D;
            string newcontract = "";
            double downPayment1 = 0D;
            double downPayment2 = 0D;
            bool dbr = false;

            int i = 0;


            for (i = 0; i < ddx.Rows.Count; i++)
            {
                contract = ddx.Rows[i]["contractNumber"].ObjToString();
                if (contract == "B23029L")
                {
                }
                if (contract == "HU23015L")
                {
                }

                contractNumber = contract;

                downPayment1 = ddx.Rows[i]["downPayment"].ObjToDouble();
                ccFee = 0D;

                DataRow[] dRow = dt.Select("contractNumber='" + contract + "'");
                found = false;
                for (int j = 0; j < dRow.Length; j++)
                {
                    contractValue = dRow[j]["contractValue"].ObjToDouble();
                    newcontract = dRow[j]["newcontract"].ObjToString();
                    if (contractValue > 0D || newcontract == "1")
                    {
                        found = true;
                        ccFee = dRow[j]["ccFee"].ObjToDouble();
                        if (ccFee == 0D)
                        {
                            cmd = "Select * from `payments` where `contractNumber` = '" + contract + "' AND `downPayment` > '0.00' AND `ccFee` > '0.00';";
                            ccDt = G1.get_db_data(cmd);
                            if (ccDt.Rows.Count > 0)
                            {
                                ccFee = ccDt.Rows[0]["ccFee"].ObjToDouble();
                                dRow[j]["ccFee"] = ccFee;
                                dRow[j]["dpp"] = ccDt.Rows[0]["downPayment"].ObjToDouble() + ccFee;
                                payDate8 = ccDt.Rows[0]["payDate8"].ObjToDateTime();
                                dRow[j]["payDate8"] = G1.DTtoMySQLDT(payDate8);
                            }
                        }
                        break;
                    }
                    downPayment2 = dRow[j]["downPayment"].ObjToDouble();
                    if (downPayment1 == downPayment2)
                    {
                        found = true;
                        ccFee = dRow[j]["ccFee"].ObjToDouble();
                        if (ccFee == 0D)
                        {
                            cmd = "Select * from `payments` where `contractNumber` = '" + contract + "' AND `downPayment` > '0.00' AND `ccFee` > '0.00';";
                            ccDt = G1.get_db_data(cmd);
                            if (ccDt.Rows.Count > 0)
                            {
                                ccFee = ccDt.Rows[0]["ccFee"].ObjToDouble();
                                dRow[j]["ccFee"] = ccFee;
                                dRow[j]["dpp"] = dRow[j]["downPayment"].ObjToDouble() + ccFee;
                                payDate8 = ccDt.Rows[0]["payDate8"].ObjToDateTime();
                                dRow[j]["payDate8"] = G1.DTtoMySQLDT(payDate8);
                            }
                        }
                        break;
                    }
                }
                if (!found)
                {
                    try
                    {
                        ibtrust = 0D;
                        sptrust = 0D;
                        xxtrust = 0D;
                        xtrust = ddx.Rows[i]["xtrust"].ObjToString();

                        deceasedDate = ddx.Rows[i]["deceasedDate"].ObjToString();
                        deceasedDate = G1.GetSQLDate(deceasedDate);
                        issueDate = ddx.Rows[i]["issueDate8"].ObjToString();
                        issueDate = G1.GetSQLDate(issueDate);

                        contractValue = DailyHistory.GetContractValue(ddx.Rows[i]);

                        dbr = false;
                        //                        if (!Commission.CheckDeathDateCommission(deceasedDate, issueDate))
                        if (!Commission.CheckDeathDateCommission(ddx, i, start, stop))
                        {
                            dbrSales += contractValue;
                            dbr = true;
                        }

                        fbi = DailyHistory.IsFundedByInsurance(ddx.Rows[i]);
                        if (fbi)
                        {
                            if (dRow.Length > 0)
                                continue;
                        }
                        DataRow d = dd.NewRow();
                        d["contractNumber"] = contract;
                        d["agentNumber"] = ddx.Rows[i]["agentCode"].ObjToString();
                        d["customer"] = ddx.Rows[i]["firstName"].ObjToString() + " " + ddx.Rows[i]["lastName"].ObjToString();
                        d["firstName"] = ddx.Rows[i]["firstName"].ObjToString();
                        d["lastName"] = ddx.Rows[i]["lastName"].ObjToString();
                        d["issueDate8"] = ddx.Rows[i]["issueDate8"];
                        d["downPayment"] = ddx.Rows[i]["downPayment"].ObjToDouble();
                        d["serviceTotal"] = ddx.Rows[i]["serviceTotal"].ObjToDouble();
                        d["merchandiseTotal"] = ddx.Rows[i]["merchandiseTotal"].ObjToDouble();
                        d["allowMerchandise"] = ddx.Rows[i]["allowMerchandise"].ObjToDouble();
                        d["allowInsurance"] = ddx.Rows[i]["allowInsurance"].ObjToDouble();
                        d["cashAdvance"] = ddx.Rows[i]["cashAdvance"].ObjToDouble();
                        //                    d["contractValue"] = dRow[i]["price"].ObjToDouble();
                        d["contractValue"] = contractValue;
                        contract = decodeContractNumber(contract, ref trust, ref loc);
                        d["trust"] = trust;
                        d["loc"] = loc;
                        if (!dbr)
                        {
                            d["xtrust"] = xtrust;
                            if (fbi)
                            {
                                d["fbi"] = 1D;
                                contractValue = 0D;
                            }

                            if (trust.Length > 0)
                            {
                                idx = trust.Length - 1;
                                ch = trust.Substring(idx);
                                if (ch.ToUpper() == "I")
                                    ibtrust = contractValue;
                                else
                                    sptrust = contractValue;
                            }
                            else
                                sptrust = contractValue;

                            if (xtrust.ToUpper() == "Y")
                            {
                                xxtrust = ibtrust + sptrust;
                                ibtrust = 0D;
                                sptrust = 0D;
                            }

                            d["ibtrust"] = ibtrust;
                            d["sptrust"] = sptrust;
                            d["xxtrust"] = xxtrust;
                        }
                        d["newcontract"] = "1";
                        cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' AND `downPayment` > '0.00' AND `ccFee` > '0.00';";
                        ccDt = G1.get_db_data(cmd);
                        if (ccDt.Rows.Count > 0)
                        {
                            ccFee = ccDt.Rows[0]["ccFee"].ObjToDouble();
                            d["ccFee"] = ccFee;
                            d["dpp"] = ccDt.Rows[0]["downPayment"].ObjToDouble() + ccFee;
                            payDate8 = ccDt.Rows[0]["payDate8"].ObjToDateTime();
                            d["payDate8"] = G1.DTtoMySQLDT(payDate8);
                        }
                        dd.Rows.Add(d);
                        count++;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                    }
                }
            }
            for (i = 0; i < dd.Rows.Count; i++)
                dt.ImportRow(dd.Rows[i]);
        }
        /*******************************************************************************************/
        private void RecalculateTrusts(DataTable dt)
        {
            string contract = "";
            string miniContract = "";
            string trust = "";
            string loc = "";
            string ch = "";
            int idx = 0;

            double ibtrust = 0D;
            double sptrust = 0D;
            double xxtrust = 0D;
            string xtrust = "";
            bool dbr = false;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contract = dt.Rows[i]["contractNumber"].ObjToString();
                if (contract.ToUpper() == "T18007L")
                {

                }
                ibtrust = 0D;
                sptrust = 0D;
                xxtrust = 0D;
                xtrust = dt.Rows[i]["xtrust"].ObjToString();
                dbr = false;
                if (dt.Rows[i]["DBR"].ObjToString().ToUpper() == "DBR")
                    dbr = true;

                miniContract = decodeContractNumber(contract, ref trust, ref loc);

                if (trust.Length > 0)
                {
                    idx = trust.Length - 1;
                    ch = trust.Substring(idx);
                    if (ch.ToUpper() == "I")
                        ibtrust = dt.Rows[i]["contractValue"].ObjToDouble();
                    else
                        sptrust = dt.Rows[i]["contractValue"].ObjToDouble();
                }
                else
                    sptrust = dt.Rows[i]["contractValue"].ObjToDouble();

                if (xtrust.ToUpper() == "Y")
                {
                    xxtrust = ibtrust + sptrust;
                    ibtrust = 0D;
                    sptrust = 0D;
                }
                if (dbr)
                {
                    ibtrust = 0D;
                    sptrust = 0D;
                    xxtrust = 0D;
                }

                dt.Rows[i]["ibtrust"] = ibtrust;
                dt.Rows[i]["sptrust"] = sptrust;
                dt.Rows[i]["xxtrust"] = xxtrust;
            }
        }
        /*******************************************************************************************/
        public static string decodeContractNumber(string contract, ref string trust, ref string loc)
        {
            contract = decodeContractNumber(contract, false, ref trust, ref loc);
            return contract;
        }
        /*******************************************************************************************/
        public static string decodeContractNumber(string contract, bool isFuneral, ref string trust, ref string loc)
        {
            loc = "";
            trust = "";
            string c = "";
            if (isFuneral)
            {
            }
            if (contract.IndexOf('-') > 0)
            {
                int idx = contract.IndexOf('-');
                contract = contract.Substring(0, idx);
            }
            for (int j = 0; j < contract.Length; j++)
            {
                c = contract.Substring(j, 1);
                if (G1.validate_numeric(c))
                    break;
                loc += c;
                if (isFuneral && j >= 1)
                    break;
            }
            for (int j = (contract.Length - 1); j >= 0; j--)
            {
                c = contract.Substring(j, 1);
                if (G1.validate_numeric(c))
                    break;
                trust = contract.Substring(j);
            }
            if (!String.IsNullOrWhiteSpace(trust))
                contract = contract.Replace(trust, "");
            if (!String.IsNullOrWhiteSpace(loc))
                contract = contract.Replace(loc, "");
            return contract;
        }
        /*******************************************************************************************/
        public static string getLocations(DataTable dt)
        {
            string locations = "";
            string loc = "";
            if (G1.get_column_number(dt, "loc") < 0)
                return "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                loc = dt.Rows[i]["loc"].ObjToString();
                if (!locations.Contains(loc))
                    locations += loc + ",";
            }
            return locations;
        }
        /*******************************************************************************************/
        public static string getTrusts(DataTable dt)
        {
            string trusts = "";
            string trust = "";
            if (G1.get_column_number(dt, "trust") < 0)
                return "";

            DataTable dx = new DataTable();
            dx.Columns.Add("trust");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                trust = dt.Rows[i]["trust"].ObjToString();
                if (String.IsNullOrWhiteSpace(trust))
                    continue;
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
        /*******************************************************************************************/
        public static void LoadOtherCombos(DataTable dt)
        {
            if (G1.get_column_number(dt, "loc") < 0)
                dt.Columns.Add("loc");
            if (G1.get_column_number(dt, "trust") < 0)
                dt.Columns.Add("trust");
            DataTable locDt = new DataTable();
            locDt.Columns.Add("locations");
            DataTable trustDt = new DataTable();
            trustDt.Columns.Add("trusts");
            string locations = "";
            string trusts = "";
            string contract = "";
            string trust = "";
            string loc = "";
            string c = "";
            string ch = "";
            int idx = 0;
            double ibtrust = 0D;
            double sptrust = 0D;
            double xxtrust = 0D;
            string xtrust = "";
            bool dbr = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contract = dt.Rows[i]["contractNumber"].ObjToString();
                dbr = false;
                if (dt.Rows[i]["dbr"].ObjToString().ToUpper() == "DBR")
                    dbr = true;
                loc = "";
                trust = "";
                for (int j = 0; j < contract.Length; j++)
                {
                    c = contract.Substring(j, 1);
                    if (G1.validate_numeric(c))
                        break;
                    loc += c;
                }
                for (int j = (contract.Length - 1); j >= 0; j--)
                {
                    c = contract.Substring(j, 1);
                    if (G1.validate_numeric(c))
                        break;
                    trust = contract.Substring(j);
                }
                dt.Rows[i]["loc"] = loc;
                dt.Rows[i]["trust"] = trust;
                if (!locations.Contains(loc))
                    locations += loc + ",";
                if (!trusts.Contains(trust))
                    trusts += trust + ",";
                //if (dbr.ToUpper() == "DBR")
                //    continue;
                xtrust = dt.Rows[i]["xtrust"].ObjToString();
                ibtrust = 0D;
                sptrust = 0D;
                xxtrust = 0D;

                if (trust.Length > 0)
                {
                    idx = trust.Length - 1;
                    ch = trust.Substring(idx);
                    if (ch.ToUpper() == "I")
                        ibtrust = dt.Rows[i]["contractValue"].ObjToDouble();
                    else
                        sptrust = dt.Rows[i]["contractValue"].ObjToDouble();
                }
                else
                    sptrust = dt.Rows[i]["contractValue"].ObjToDouble();

                if (xtrust.ToUpper() == "Y")
                {
                    xxtrust = ibtrust + sptrust;
                    ibtrust = 0D;
                    sptrust = 0D;
                }

                if (dbr)
                {
                    ibtrust = 0D;
                    sptrust = 0D;
                    xxtrust = 0D;
                }

                dt.Rows[i]["ibtrust"] = ibtrust;
                dt.Rows[i]["sptrust"] = sptrust;
                dt.Rows[i]["xxtrust"] = xxtrust;
            }

            string[] Lines = locations.Split(',');
            for (int i = 0; i < Lines.Length; i++)
            {
                c = Lines[i].Trim();
                if (String.IsNullOrWhiteSpace(c))
                    continue;
                DataRow dRow = locDt.NewRow();
                dRow["locations"] = c;
                locDt.Rows.Add(dRow);
            }
            //            chkComboLocation.Properties.DataSource = locDt;

            Lines = trusts.Split(',');
            for (int i = 0; i < Lines.Length; i++)
            {
                c = Lines[i].Trim();
                if (String.IsNullOrWhiteSpace(c))
                    continue;
                DataRow dRow = trustDt.NewRow();
                dRow["trusts"] = c;
                trustDt.Rows.Add(dRow);
            }
            //            chkComboTrust.Properties.DataSource = trustDt;
        }
        /*******************************************************************************************/
        private string getAgentQuery(string agent = "")
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
            if (String.IsNullOrWhiteSpace(agent))
                agent = "agentNumber";
            return procLoc.Length > 0 ? " `" + agent + "` IN (" + procLoc + ") " : "";
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
        private string getAgentNameQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboAgentNames.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `agentName` IN (" + procLoc + ") " : "";
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
                }
            }
            return procLoc.Length > 0 ? " `loc` IN (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        //private void addTotalRow(DataTable dt, double payments, double downpayments, double trust85, double trust100)
        //{
        //    double totalPayments = payments + downpayments;
        //    DataRow dRow = dt.NewRow();
        //    dRow["paymentAmount"] = payments;
        //    dRow["downPayment"] = downpayments;
        //    dRow["trust85P"] = trust85;
        //    dRow["trust100P"] = trust100;
        //    dRow["customer"] = "   Totals";
        //    dRow["totalPayments"] = totalPayments;
        //    dt.Rows.InsertAt(dRow, 0);
        //}
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
            else if (dgv7.Visible)
                SetSpyGlass(gridMain7);
            else if (dgv8.Visible)
                SetSpyGlass(gridMain8);
            else if (dgv9.Visible)
                SetSpyGlass(gridMain9);
            else if (dgv10.Visible)
                SetSpyGlass(gridMain10);
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
        private void loadAgents(string cmdx)
        {
            //cmd += " GROUP by `agentNumber` order by `agentNumber`;";
            //_agentList = G1.get_db_data(cmd);
            //chkComboAgent.Properties.DataSource = _agentList;
            string cmd = "Select * from `agents` order by `agentCode`;";
            _agentList = G1.get_db_data(cmd);
            chkComboAgent.Properties.DataSource = _agentList;

            cmd = "Select * from `agents` GROUP by `lastName`,`firstName` order by `lastName`;";
            DataTable nameList = G1.get_db_data(cmd);
            nameList.Columns.Add("agentNames");
            string fname = "";
            string lname = "";
            for (int i = 0; i < nameList.Rows.Count; i++)
            {
                fname = nameList.Rows[i]["firstName"].ObjToString().Trim();
                lname = nameList.Rows[i]["lastName"].ObjToString().Trim();
                nameList.Rows[i]["agentNames"] = fname + " " + lname;
            }
            chkComboAgentNames.Properties.DataSource = nameList;
        }
        /***********************************************************************************************/
        private void loadLocatons()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);
            chkComboLocNames.Properties.DataSource = locDt;
            chkComboLocation.Properties.DataSource = locDt;
        }
        /****************************************************************************************/
        private void btnCalc_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Color backColor = btnCalc.BackColor;
            btnCalc.BackColor = Color.Green;
            btnCalc.Refresh();
            DataTable dt = (DataTable)dgv.DataSource;
            DateTime startDate = this.dateTimePicker1.Value;
            DateTime stopDate = this.dateTimePicker2.Value;

            CalcCommission(dt, startDate, stopDate);

            bool doSplits = false;
            if (chkMainDoSplits.Checked)
                chkDoSplits.Checked = true;
            else
                chkDoSplits.Checked = false;

            if (chkDoSplits.Checked)
                doSplits = true;
            RunCommissions(true, doSplits);
            gridMain10.RefreshData();
            dgv10.Refresh();
            commissionRan = true;

            RunMeetingCommissions();
            //if (doSplits)
            //{
            //    DialogResult result = MessageBox.Show("Do you want to SAVE these historic Commissions?\nThis will take about 30 Extra Seconds!", "Commissions Calculated Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //    if (result == DialogResult.Yes)
            //        btnSaveCommissions_Click(null, null);
            //}
            this.Cursor = Cursors.Default;
            tabControl1.SelectTab("tabCommission");
            btnCalc.BackColor = backColor;
            btnCalc.Refresh();
        }
        /****************************************************************************************/
        private void RunMeetingCommissions()
        {
            string cmd = "";
            DataTable mDt = null;

            DateTime issueDate = DateTime.Now;
            DateTime effectiveFromDate = DateTime.Now;
            DateTime effectiveToDate = DateTime.Now;
            double commissionPercent = 0D;
            double cashAdvance = 0D;
            double faceAmount = 0D;
            double splitCommissionPercent = 0D;
            double dValue = 0D;
            double contractValue = 0D;
            string agentLastName = "";
            string agentFirstName = "";

            DataTable dt = (DataTable)dgv.DataSource;
            dt.Columns.Add("faceAmount", Type.GetType("System.Double"));

            DateTime date1 = this.dateTimePicker1.Value;
            DateTime date2 = this.dateTimePicker2.Value;
            DateTime date3 = DateTime.Now;

            date1 = new DateTime(date1.Year, date1.Month, date1.Day);
            date2 = new DateTime(date2.Year, date2.Month, date2.Day);

            DataRow[] dRows = dt.Select("meetingNumber <> '' AND meetingNumber <> '0' AND contractValue > '0'");
            if (dRows.Length <= 0)
                return;
            DataTable meetingDt = dRows.CopyToDataTable();
            for (int i = meetingDt.Rows.Count - 1; i >= 0; i--)
            {
                contractValue = meetingDt.Rows[i]["contractValue"].ObjToDouble();
                cashAdvance = meetingDt.Rows[i]["cashAdvance"].ObjToDouble();
                faceAmount = contractValue + cashAdvance;
                meetingDt.Rows[i]["faceAmount"] = faceAmount;

                date3 = meetingDt.Rows[i]["issueDate8"].ObjToDateTime();
                if (date3 >= date1 && date3 <= date2)
                    continue;
                meetingDt.Rows.RemoveAt(i);
            }
            DataView tempview = meetingDt.DefaultView;
            tempview.Sort = "meetingNumber asc, contractNumber asc";
            meetingDt = tempview.ToTable();

            string meetingNumber = "";
            string oldMeetingNumber = "";
            string contractNumber = "";
            string location = "";
            string attendees = "";
            int row = 0;

            DataTable oldMeetingDt = meetingDt.Clone();
            try
            {
                oldMeetingDt.Columns.Add("MC", Type.GetType("System.Double"));
                oldMeetingDt.Columns.Add("effectiveFromDate", Type.GetType("System.DateTime"));
                oldMeetingDt.Columns.Add("effectiveToDate", Type.GetType("System.DateTime"));
                oldMeetingDt.Columns.Add("commissionPercent", Type.GetType("System.Double"));
                oldMeetingDt.Columns.Add("splitCommissionPercent", Type.GetType("System.Double"));
                oldMeetingDt.Columns.Add("attendees", Type.GetType("System.Double"));
                oldMeetingDt.Columns.Add("agent");

                for (int i = 0; i < meetingDt.Rows.Count; i++)
                {
                    try
                    {
                        meetingNumber = meetingDt.Rows[i]["meetingNumber"].ObjToString();
                        agentFirstName = meetingDt.Rows[i]["firstName1"].ObjToString();
                        agentLastName = meetingDt.Rows[i]["lastName1"].ObjToString();
                        if (String.IsNullOrWhiteSpace(oldMeetingNumber))
                            oldMeetingNumber = meetingNumber;
                        //oldMeetingDt.ImportRow(meetingDt.Rows[i]);
                        //row = oldMeetingDt.Rows.Count - 1;
                        issueDate = meetingDt.Rows[i]["issueDate8"].ObjToDateTime();
                        contractValue = meetingDt.Rows[i]["contractValue"].ObjToDouble();
                        contractValue = meetingDt.Rows[i]["faceAmount"].ObjToDouble();

                        cmd = "Select * from `agent_meetings` WHERE `meetingNumber`='" + meetingNumber + "' AND `agent` = '" + agentLastName + ", " + agentFirstName + "';";
                        cmd = "Select * from `agent_meetings` WHERE `meetingNumber`='" + meetingNumber + "';";
                        mDt = G1.get_db_data(cmd);
                        if (mDt.Rows.Count > 0)
                        {
                            for (int j = 0; j < mDt.Rows.Count; j++)
                            {
                                effectiveFromDate = mDt.Rows[j]["effectiveFromDate"].ObjToDateTime();
                                effectiveToDate = mDt.Rows[j]["effectiveToDate"].ObjToDateTime();
                                commissionPercent = mDt.Rows[j]["commissionPercent"].ObjToDouble();
                                splitCommissionPercent = mDt.Rows[j]["splitCommissionPercent"].ObjToDouble();
                                if (issueDate < effectiveFromDate || issueDate > effectiveToDate)
                                    continue;
                                location = mDt.Rows[j]["location"].ObjToString();
                                attendees = mDt.Rows[j]["attendees"].ObjToString();
                                dValue = contractValue * commissionPercent / 100D;
                                if (splitCommissionPercent > 0D)
                                    dValue = dValue * splitCommissionPercent;
                                dValue = G1.RoundValue(dValue);

                                agentLastName = mDt.Rows[j]["agentLastName"].ObjToString();
                                agentFirstName = mDt.Rows[j]["agentFirstName"].ObjToString();

                                oldMeetingDt.ImportRow(meetingDt.Rows[i]);
                                row = oldMeetingDt.Rows.Count - 1;

                                oldMeetingDt.Rows[row]["MC"] = dValue;
                                oldMeetingDt.Rows[row]["effectiveFromDate"] = G1.DTtoMySQLDT(effectiveFromDate);
                                oldMeetingDt.Rows[row]["effectiveToDate"] = G1.DTtoMySQLDT(effectiveToDate);
                                oldMeetingDt.Rows[row]["commissionPercent"] = commissionPercent;
                                oldMeetingDt.Rows[row]["splitCommissionPercent"] = splitCommissionPercent;
                                oldMeetingDt.Rows[row]["lastName1"] = meetingDt.Rows[i]["lastName1"].ObjToString() + ", " + meetingDt.Rows[i]["firstName1"].ObjToString();
                                oldMeetingDt.Rows[row]["firstName1"] = agentLastName + ", " + agentFirstName;
                                oldMeetingDt.Rows[row]["agent"] = agentLastName + ", " + agentFirstName;
                                oldMeetingDt.Rows[row]["location"] = location;
                                oldMeetingDt.Rows[row]["attendees"] = attendees;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
            G1.NumberDataTable(oldMeetingDt);
            dgv13.DataSource = oldMeetingDt;
            dgv13.Refresh();
            gridMain13.ExpandAllGroups();
            gridMain13.RefreshData();
            gridMain13.RefreshEditor(true);
            dgv13.Refresh();
        }
        /****************************************************************************************/
        private void ProcessCashAdvances(DataTable dt)
        {
            double payment = 0D;
            double cashAdvance = 0D;
            double contractValue = 0D;
            double debit = 0D;
            double credit = 0D;
            double interest = 0D;
            double downPayment = 0D;
            double totalPayment = 0D;
            double dbc = 0D;

            if (G1.get_column_number(dt, "dbc_5") < 0)
                dt.Columns.Add("dbc_5", Type.GetType("System.Double")); // Actual DBC Down Payment

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                    debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();

                    payment = dt.Rows[i]["paymentAmount"].ObjToDouble();

                    interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                    dbc = dt.Rows[i]["dbc"].ObjToDouble();
                    if (dbc > 0D)
                    {
                        totalPayment = payment + credit - debit - interest;
                        dt.Rows[i]["dbc_5"] = downPayment;
                    }
                    else
                        totalPayment = downPayment + payment + credit - debit - interest;
                    dt.Rows[i]["totalPayments"] = totalPayment;

                    //cashAdvance = dt.Rows[i]["cashAdvance"].ObjToDouble();
                    //payment = dt.Rows[i]["totalPayments"].ObjToDouble();
                    //if (cashAdvance > 0D && payment > 0D)
                    //{
                    //    contractValue = DailyHistory.GetContractValue(dt.Rows[i]);
                    //    if (payment == (contractValue + cashAdvance))
                    //        dt.Rows[i]["totalPayments"] = payment - cashAdvance;
                    //}
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Calculating Commission " + ex.Message.ToString());
                }
            }
        }
        /****************************************************************************************/
        private void CalcCommission(DataTable dt, DateTime startDate, DateTime stopDate)
        { // This is Step 1 of the 5% commission
            DataTable dx = G1.get_db_data("Select * from `agents` order by `agentCode`;");
            DataTable gDx = G1.get_db_data("Select * from `goals` order by `agentCode`,`effectiveDate`;");

            if (G1.get_column_number(dt, "dbcMoney") < 0)
                dt.Columns.Add("dbcMoney", Type.GetType("System.Double"));
            string agent = "";
            string status = "";
            string type = "";
            double payment = 0D;
            double comm = 0D;
            double money = 0D;
            double totals = 0D;
            double debit = 0D;
            double credit = 0D;
            double cashAdvance = 0D;
            double contractValue = 0D;
            double dbc = 0D;
            double fbi = 0D;
            double fbiMoney = 0D;
            string contractNumber = "";
            DateTime eDate = DateTime.Now;
            DateTime payDate8 = DateTime.Now;
            DateTime issueDate8 = DateTime.Now;
            bool standardPaid = false;
            double savePercent = 0D;
            bool payCommission = true;
            double newContract = 0D;
            double contractMoney = 0D;
            string creditReason = "";
            DataTable ddx = gDx.Clone();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    agent = dt.Rows[i]["agentNumber"].ObjToString();
                    if (agent == "V05")
                    {

                    }
                    if (agent == "V22")
                    {

                    }
                    DataRow[] dRows = dx.Select("agentCode='" + agent + "'");
                    if (dRows.Length <= 0)
                        continue;
                    status = dRows[0]["status"].ObjToString();
                    if (status.ToUpper() == "INACTIVE")
                        continue;
                    DataRow[] gRows = gDx.Select("agentCode='" + agent + "'");
                    if (gRows.Length <= 0)
                        continue;
                    standardPaid = false;
                    savePercent = 0D;
                    G1.ConvertToTable(gRows, ddx);
                    for (int j = 0; j < gRows.Length; j++)
                    {
                        status = gRows[j]["status"].ObjToString();
                        //if (status.ToUpper() != "CURRENT")
                        //    continue;
                        type = gRows[j]["type"].ObjToString();
                        if (type.ToUpper() != "STANDARD")
                            continue;
                        if (standardPaid)
                            continue;
                        eDate = gRows[j]["effectiveDate"].ObjToDateTime();
                        payDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();
                        issueDate8 = dt.Rows[i]["issueDate8"].ObjToDateTime();
                        if (issueDate8 >= eDate)
                        {
                            comm = gRows[j]["percent"].ObjToDouble();
                            if (comm <= 0D)
                                comm = 0.000001D;
                            comm = comm / 100D;
                            savePercent = comm;
                        }
                    }
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber == "L1264")
                    {
                    }
                    payCommission = true;
                    if (!Commission.ShouldCommissionBePaid(dt, i))
                        payCommission = false;
                    creditReason = dt.Rows[i]["creditReason"].ObjToString();
                    if (creditReason.ToUpper() == "TCA")
                        payCommission = false;
                    if (agent == "WF9" && contractNumber == "WF19071LI")
                    {
                    }
                    cashAdvance = dt.Rows[i]["cashAdvance"].ObjToDouble();
                    dbc = dt.Rows[i]["dbc"].ObjToDouble();
                    if (dbc > 0D)
                    {

                    }
                    contractValue = DailyHistory.GetContractValue(dt.Rows[i]);
                    newContract = 0D;
                    contractMoney = dt.Rows[i]["contractValue"].ObjToDouble();
                    if (contractMoney > 0D)
                        newContract = 1D;
                    payment = dt.Rows[i]["totalPayments"].ObjToDouble();
                    if (contractValue < 0D && payment > 0D)
                        contractValue = payment;
                    if (cashAdvance > 0D)
                    {
                        if (payment == (contractValue + cashAdvance))
                            payment = payment - cashAdvance;
                        else if (newContract > 0D)
                        {
                            if (payment - cashAdvance > 0D)
                                payment = payment - cashAdvance;
                        }
                    }
                    payment = G1.RoundValue(payment);
                    credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    if (credit != 0D) // Ramma Zamma
                    {
                        if (agent.ToUpper() == "XXX")
                            payment = 0D;
                    }
                    if (debit != 0D && !allowDebits) // Ramma Zamma
                        payment = 0D;
                    money = payment * comm;
                    //                    money = G1.RoundValue(money);
                    if (debit > 0D && allowDebits)
                        money = G1.RoundUp(money);
                    else
                        money = G1.RoundDown(money);
                    fbi = dt.Rows[i]["fbi"].ObjToDouble();
                    if (fbi > 0D)
                    {
                        agent = dt.Rows[i]["agentNumber"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(agent))
                        {
                            DataRow[] dR = dx.Select("agentCode = '" + agent + "'");
                            if (dR.Length > 0)
                            {
                                fbiMoney = dR[0]["fbiCommission"].ObjToDouble();
                                money = fbi * fbiMoney;
                            }
                        }
                    }

                    if (dbc > 0D) // Remove Commission if Dead Before Commission
                    {
                        dt.Rows[i]["dbcMoney"] = money;
                        money = 0D;
                    }
                    else if (!payCommission)
                        money = 0D;
                    if (payment > 0D && savePercent <= 0D)
                        money = 0.00001D;
                    dt.Rows[i]["commission"] = money;
                    totals += money;
                    standardPaid = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Calculating Commission " + ex.Message.ToString());
                }
            }

            double totalPayment = 0D;
            double downPayment = 0D;
            FindContract(dt, "N18021LI");
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    dbc = dt.Rows[i]["dbc"].ObjToDouble();
            //    if (dbc > 0D)
            //        dt.Rows[i]["commissiona"] = 0D;
            //    else
            //    {
            //        downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
            //        if (downPayment > 0D)
            //        {
            //            cashAdvance = dt.Rows[i]["cashAdvance"].ObjToDouble();
            //            totalPayment = downPayment - cashAdvance;
            //            if (totalPayment <= 0D)
            //                dt.Rows[i]["commissiona"] = 0D;
            //        }
            //    }
            //}
            FindContract(dt, "HC22001");


            gridMain.Columns["commission"].Visible = true;
            gridMain.Columns["dbcMoney"].Visible = true;
            gridMain.Columns["agentNumber"].Visible = true;
            //            dt.Rows[0]["commission"] = G1.RoundValue(totals);
            dgv.DataSource = dt;
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            DateTime start = DateTime.Now;
            DateTime stop = DateTime.Now;
            if (now < DailyHistory.as400Date)
            {
                now = now.AddMonths(-1);
                now = new DateTime(now.Year, now.Month, 1);
                this.dateTimePicker3.Value = now;
                int days = DateTime.DaysInMonth(now.Year, now.Month);
                this.dateTimePicker4.Value = new DateTime(now.Year, now.Month, days);
                start = now.AddDays(-1);
                stop = new DateTime(now.Year, now.Month, days - 1);
            }
            else
            {
                now = this.dateTimePicker1.Value;
                now = now.AddMonths(-1);
                start = now;
                int days = DateTime.DaysInMonth(now.Year, now.Month);
                stop = new DateTime(now.Year, now.Month, days);
                this.dateTimePicker3.Value = start;
                this.dateTimePicker4.Value = stop;
            }
            this.dateTimePicker1.Value = start;
            this.dateTimePicker2.Value = stop;
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker3.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker4.Value = new DateTime(now.Year, now.Month, days);
            DateTime start = now.AddDays(-1);
            DateTime stop = new DateTime(now.Year, now.Month, days - 1);
            this.dateTimePicker1.Value = this.dateTimePicker3.Value;
            this.dateTimePicker2.Value = this.dateTimePicker4.Value;
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
        private void commissionReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
        private void RunCommissions(bool batch, bool splits)
        { // Splits are forced into the Commission DataTable here rather than in the Commissions module.
            DataTable dt = (DataTable)dgv.DataSource;
            DataTable dt8 = (DataTable)dgv8.DataSource;
            DataTable dt9 = (DataTable)dgv9.DataSource;
            if (!batch)
            {
                this.Cursor = Cursors.WaitCursor;
                Commission commForm = new Commission(false, splits, this.dateTimePicker1.Value, this.dateTimePicker2.Value, dt, dt8, dt9, _agentList);
                commForm.Show();
                this.Cursor = Cursors.Default;
            }
            else
            {
                using (Commission commForm = new Commission(true, splits, this.dateTimePicker1.Value, this.dateTimePicker2.Value, dt, dt8, dt9, _agentList))
                {
                    if (splits)
                    {
                        gridMain10.Columns["splitCommission"].Visible = true;
                        gridMain10.Columns["splitBaseCommission"].Visible = true;
                        gridMain10.Columns["splitGoalCommission"].Visible = true;
                    }
                    else
                    {
                        gridMain10.Columns["splitCommission"].Visible = false;
                        gridMain10.Columns["splitBaseCommission"].Visible = false;
                        gridMain10.Columns["splitGoalCommission"].Visible = false;
                    }
                    dgv10.DataSource = Commission.commissionDt;
                    DoSplits(dt); //Ramma Zamma // Splits are prepared here and then forced into the Commission Table
                    // The problem was Commissions calculated a split based on Totals that did not match to the penny the tab Agent Details
                    // So, I go back through the Agent Details and replace what Commissions did; however, I have to find whether Commission put the split in the Standard or the Goal line.
                    // That's why I first check to see if Commission put it in the Standard. If it's zero, I check the Goal. If it's not zero I replace it.
                    if (dt != null)
                    {
                        DataRow[] dRows = null;
                        double splitBase = 0D;
                        string agent = "";
                        string[] Lines = null;
                        double dValue = 0D;

                        double splitTotalPayments = 0D;
                        double payment = 0D;
                        double splitPercent = 0D;


                        DataTable goalDt = G1.get_db_data("Select * from `goals` where `type` = 'standard' AND `status` = 'current' AND `splits` <> '';");
                        for (int i = 0; i < goalDt.Rows.Count; i++)
                        {
                            agent = goalDt.Rows[i]["agentCode"].ObjToString();
                            string agentSplits = goalDt.Rows[i]["splits"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(agentSplits))
                            {
                                Lines = agentSplits.Split('~');
                                for (int j = 0; j < Lines.Length; j = j + 2)
                                {
                                    try
                                    {
                                        agent = Lines[j].Trim();
                                        if (agent == "V15")
                                        {
                                        }
                                        splitBase = 0D;
                                        splitTotalPayments = 0D;
                                        if (G1.get_column_number(dt, agent) > 0)
                                        {
                                            splitBase = 0D;
                                            splitTotalPayments = 0D;
                                            for (int k = 0; k < dt.Rows.Count; k++)
                                            {
                                                dValue = dt.Rows[k][agent].ObjToDouble();
                                                if (dValue > 0D)
                                                {
                                                    splitBase += dt.Rows[k][agent].ObjToDouble();
                                                    if (dt.Rows[k][agent].ObjToDouble() != 0D)
                                                    {
                                                        payment = dt.Rows[k]["totalPayments"].ObjToDouble();
                                                        splitPercent = dt.Rows[k]["Split Payment"].ObjToDouble();
                                                        splitTotalPayments += payment * splitPercent;
                                                    }
                                                }
                                                else if (allowDebits)
                                                {
                                                    splitBase += dt.Rows[k][agent].ObjToDouble();
                                                    if (dt.Rows[k][agent].ObjToDouble() != 0D)
                                                    {
                                                        payment = dt.Rows[k]["totalPayments"].ObjToDouble();
                                                        splitPercent = dt.Rows[k]["Split Payment"].ObjToDouble();
                                                        splitTotalPayments += payment * splitPercent;
                                                    }
                                                }
                                            }
                                        }
                                        if (agent == "V15")
                                        {
                                        }
                                        dRows = Commission.commissionDt.Select("agentNumber='" + agent + "' AND type='Standard'"); // RAMMA ZAMMA
                                        if (dRows.Length > 0)
                                        {
                                            double ddd = dRows[0]["splitBaseCommission"].ObjToDouble();
                                            if (ddd > 0D)
                                            {
                                                dRows[0]["splitBaseCommission"] = splitBase;
                                                payment = dRows[0]["totalPayments"].ObjToDouble();
                                                payment = splitTotalPayments;
                                                dRows[0]["totalPayments"] = payment;
                                            }
                                            else
                                            {
                                                dRows = Commission.commissionDt.Select("agentNumber='" + agent + "' AND type='Goal'"); // RAMMA ZAMMA
                                                if (dRows.Length > 0)
                                                {
                                                    ddd = dRows[0]["splitBaseCommission"].ObjToDouble();
                                                    if (ddd > 0D)
                                                    {
                                                        dRows[0]["splitBaseCommission"] = splitBase;
                                                        payment = dRows[0]["totalPayments"].ObjToDouble();
                                                        payment = splitTotalPayments;
                                                        dRows[0]["totalPayments"] = payment;
                                                    }
                                                }
                                            }
                                            //dRows = Commission.commissionDt.Select("agentNumber='" + agent + "'");
                                            //DataTable dddc = dRows.CopyToDataTable();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                }
                            }
                        }
                    }
                    dgv10.DataSource = Commission.commissionDt;
                    dgv10.Refresh();
                    gridMain10.RefreshData();
                    dgv10.Refresh();
                }
            }
        }
        /****************************************************************************************/
        private double getEffectiveCommissionFive(DataTable gDx, string agent, DateTime issueDate8)
        {
            double savePercent = 0D;
            DataRow[] gRows = gDx.Select("agentCode='" + agent + "'");
            if (gRows.Length <= 0)
                return savePercent;
            savePercent = 0D;
            string status = "";
            string type = "";
            double comm = 0D;
            DateTime eDate = DateTime.Now;
            //            G1.ConvertToTable(gRows, ddx);
            for (int j = 0; j < gRows.Length; j++)
            {
                status = gRows[j]["status"].ObjToString();
                //if (status.ToUpper() != "CURRENT")
                //    continue;
                type = gRows[j]["type"].ObjToString();
                if (type.ToUpper() != "STANDARD")
                    continue;
                eDate = gRows[j]["effectiveDate"].ObjToDateTime();
                //payDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();
                //issueDate8 = dt.Rows[i]["issueDate8"].ObjToDateTime();
                if (issueDate8 >= eDate)
                {
                    comm = gRows[j]["percent"].ObjToDouble();
                    comm = comm / 100D;
                    savePercent = comm;
                }
            }
            return savePercent;
        }
        /****************************************************************************************/
        private void DoSplits(DataTable dt)
        { // This is the 5% detail commission
            DataTable cDt = (DataTable)dgv10.DataSource;
            if (cDt == null)
                return;
            if (G1.get_column_number(cDt, "SplitBaseCommission") < 0)
                return;
            DataTable gDx = G1.get_db_data("Select * from `goals` order by `agentCode`,`effectiveDate`;");

            int col = G1.get_column_number(dt, "dbc_5");
            for (int i = dt.Columns.Count - 1; i > col; i--)
                dt.Columns.RemoveAt(i);
            dt.Columns.Add("Split Payment", Type.GetType("System.Double"));
            dt.Columns.Add("Split DownPayment", Type.GetType("System.Double"));

            string agent = "";
            string agentNumber = "";
            string splits = "";
            string str = "";
            string type = "";
            string status = "";
            double percent = 0D;
            double contractValue = 0D;
            double agentPercent = 0D;
            double commission = 0D;
            double totalPayments = 0D;
            double paymentAmount = 0D;
            double totalPaymentAmounts = 0D;
            double fbiCommission = 0D;
            double fbi = 0D;
            double fbiMoney = 0D;
            string contractNumber = "";
            double newCommission = 0D;
            double oldCommission = 0D;
            double difference = 0D;
            double splitPayment = 0D;
            double totalSplits = 0D;
            int lastSplitRow = 0;
            bool first = true;
            double allPayments = 0D;
            double allSplits = 0D;
            double agentSplits = 0D;
            double cashAdvance = 0D;
            double downPayment = 0D;
            double debit = 0D;
            bool cashOnly = false;
            DateTime issueDate8 = DateTime.Now;
            DataRow[] aDR = null;
            DataRow[] xDR = null;
            try
            {
                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    cashOnly = false;
                    contractNumber = dt.Rows[k]["contractNumber"].ObjToString();
                    issueDate8 = dt.Rows[k]["issueDate8"].ObjToDateTime();
                    if (contractNumber == "C18063LI")
                    {

                    }
                    //FindContract(dt, "M15027UI");

                    agentNumber = dt.Rows[k]["agentNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(agentNumber))
                        continue;
                    if (agentNumber == "V25")
                    {
                        if (contractNumber == "M21001LI")
                        {
                        }

                    }

                    if (contractNumber == "L19082LI")
                    {
                    }
                    cashAdvance = dt.Rows[k]["cashAdvance"].ObjToDouble();
                    downPayment = dt.Rows[k]["downPayment"].ObjToDouble();
                    contractValue = dt.Rows[k]["contractValue"].ObjToDouble();
                    totalPayments = dt.Rows[k]["totalPayments"].ObjToDouble();
                    debit = dt.Rows[k]["debitAdjustment"].ObjToDouble();
                    if (debit != 0D)
                    {
                    }

                    //                    if ( contractValue > 0D)
                    if (downPayment > 0D)
                        cashAdvance = dt.Rows[k]["cashAdvance"].ObjToDouble();
                    if (cashAdvance > 0D)
                    {

                    }
                    totalPayments = totalPayments - cashAdvance;

                    paymentAmount = dt.Rows[k]["paymentAmount"].ObjToDouble();

                    allPayments += totalPayments;
                    totalPaymentAmounts += paymentAmount;
                    totalSplits = 0D;
                    lastSplitRow = -1;
                    first = true;
                    //                    dt.Rows[k]["Split Payment"] = totalPayments;
                    commission = dt.Rows[k]["commission"].ObjToDouble();
                    if (commission == 0D)
                        commission = 0.000001D;
                    fbiCommission = 0D;
                    fbiMoney = 0D;
                    fbi = dt.Rows[k]["fbi"].ObjToDouble();
                    // fbi = 0D;
                    if (fbi > 0D)
                    {
                        if (!String.IsNullOrWhiteSpace(agentNumber))
                        {
                            aDR = _agentList.Select("agentCode='" + agentNumber + "'");
                            if (aDR.Length > 0)
                            {
                                fbiCommission = aDR[0]["fbiCommission"].ObjToDouble();
                                fbiMoney = fbi * fbiCommission;
                                dt.Rows[k]["commission"] = fbiMoney;
                                commission = 0D;
                            }
                        }
                    }
                    //                    if (totalPayments == 0D && commission == 0D && fbi <= 0D)
                    if (totalPayments == 0D && commission == 0D && fbi <= 0D && cashAdvance <= 0D)
                        continue;
                    if (totalPayments > 0D && commission == 0D && fbi <= 0D)
                    {
                        cashAdvance = dt.Rows[k]["cashAdvance"].ObjToDouble();
                        downPayment = dt.Rows[k]["downPayment1"].ObjToDouble();
                        if (cashAdvance > 0D)
                        {
                            if (downPayment == cashAdvance)
                                cashOnly = true;
                        }
                    }
                    if (commission == 0D && cashAdvance >= 0D && downPayment >= 0D && totalPayments <= 0D)
                    {
                        commission = -999.99D;
                        if (fbi > 0D)
                            commission = 0D;
                    }
                    else if (cashAdvance == downPayment)
                    {
                        if (cashAdvance > 0D)
                            commission = -999.99D;
                        if (fbi > 0D)
                            commission = 0D;
                    }
                    if (G1.get_column_number(dt, agentNumber) < 0)
                    {
                        if (agentNumber == "115845")
                        {
                        }
                        dt.Columns.Add(agentNumber, Type.GetType("System.Double"));
                    }
                    dt.Rows[k][agentNumber] = commission + fbiMoney;
                    agentSplits = -1D;
                    oldCommission = commission + fbiMoney;
                    newCommission = 0D;
                    if (agentNumber == "N40" && fbi > 0D)
                    {

                    }
                    xDR = cDt.Select("agentCode='" + agentNumber + "'");
                    if (xDR.Length <= 0)
                        continue;

                    for (int i = 0; i < xDR.Length; i++)
                    {
                        type = xDR[i]["type"].ObjToString();
                        if (type.Trim().ToUpper() == "GOAL")
                            continue;
                        status = xDR[i]["status"].ObjToString();
                        if (status.Trim().ToUpper() == "HISTORIC")
                            continue;

                        splits = xDR[i]["splits"].ObjToString();
                        //                        splits = "";
                        if (String.IsNullOrWhiteSpace(splits))
                            continue;
                        if (splits.IndexOf("~") < 0)
                            continue;
                        if (!splits.Contains(agentNumber))
                            continue;
                        if (agentSplits == -1D)
                            agentSplits = 0D;
                        agentPercent = xDR[i]["percent"].ObjToDouble() / 100D;
                        agentPercent = getEffectiveCommissionFive(gDx, agentNumber, issueDate8);
                        string[] Lines = splits.Split('~');
                        for (int j = 0; j < Lines.Length; j = j + 2)
                        {
                            agent = Lines[j].Trim();
                            if (String.IsNullOrWhiteSpace(agent))
                                continue;
                            if (G1.get_column_number(dt, agent) < 0)
                                dt.Columns.Add(agent, Type.GetType("System.Double"));
                            if (agent == "V15")
                            {

                            }
                            str = Lines[j + 1].ObjToString();
                            if (!G1.validate_numeric(str))
                                continue;
                            percent = str.ObjToDouble() / 100D;
                            double test = percent / agentPercent;
                            commission = oldCommission * test;
                            //                            commission = totalPayments * percent;
                            splitPayment = totalPayments * (percent / agentPercent);
                            splitPayment = G1.RoundValue(splitPayment);
                            splitPayment = percent / agentPercent;
                            dt.Rows[k]["Split Payment"] = splitPayment;

                            agentSplits += splitPayment;
                            //totalSplits += splitPayment;
                            //lastSplitRow = k;
                            //if (fbiMoney > 0D && agentPercent > 0D)
                            //    commission += fbiMoney / (agentPercent / percent);
                            if (debit != 0D && allowDebits)
                                commission = G1.RoundDown(commission);
                            else
                                commission = G1.RoundValue(commission);
                            dt.Rows[k][agent] = commission;
                            newCommission += commission;
                            if (cashAdvance >= 0D && commission <= 0D)
                            {
                                if (debit == 0D && !allowDebits)
                                    dt.Rows[k][agent] = -999.99D;
                            }
                            first = false;
                        }
                        if (agentSplits == -1D)
                            agentSplits = totalPayments;
                        allSplits += agentSplits;
                        if (newCommission != oldCommission)
                        {
                            difference = oldCommission - newCommission;
                            difference = G1.RoundValue(difference);
                            for (int j = 0; j < Lines.Length; j = j + 2)
                            {
                                agent = Lines[j].Trim();
                                if (String.IsNullOrWhiteSpace(agent))
                                    continue;
                                commission = dt.Rows[k][agent].ObjToDouble();
                                commission += difference;
                                dt.Rows[k][agent] = commission;
                                break;
                            }
                        }
                        break;
                    }
                    if (cashOnly)
                    {
                        if (G1.get_column_number(dt, agentNumber) < 0)
                            dt.Columns.Add(agentNumber, Type.GetType("System.Double"));
                        dt.Rows[k][agentNumber] = -99999.99D;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            originalDt = dt;
            //FindContract(dt, "E19047L");
            col = G1.get_column_number(dt, "dbc_5");
            DataRow[] dRs = null;
            for (int i = dt.Columns.Count - 1; i > col; i--)
            {
                agent = dt.Columns[i].ColumnName.ObjToString();
                if (agent == "Split Payment")
                    continue;
                if (agent == "Split DownPayment")
                    continue;
                dRs = _agentList.Select("agentCode='" + agent + "'");
                if (dRs.Length <= 0)
                {
                    if (!oldData)
                        MessageBox.Show("***ERROR*** Agent Code " + agent + " is not in the Agent Table! Please Add Agent to Table!");
                }
            }

            DataRow[] dRows = dt.Select("agentName='Sally Leonard'");
            if (dRows.Length > 0)
            {
                DataTable dd = new DataTable();
                dd = dt.Clone();
                G1.ConvertToTable(dRows, dd);
                totalPayments = 0D;
                for (int i = 0; i < dd.Rows.Count; i++)
                {
                    agentNumber = dd.Rows[i]["agentNumber"].ObjToString();
                    //paymentAmount = dd.Rows[i][agentNumber].ObjToDouble(); // Commission???
                    paymentAmount = dd.Rows[i]["totalPayments"].ObjToDouble();
                    totalPayments += paymentAmount;
                }
            }
        }
        /****************************************************************************************/
        //private void btnTest_Click(object sender, EventArgs e)
        //{
        //    DataTable dt = CalcAgentExtraCommission();
        //}
        ///****************************************************************************************/
        //private DataTable CalcAgentExtraCommission ()
        //{
        //    string cmd = "Select * from `goals` GROUP by `agentCode` ORDER by `effectiveDate`;";
        //    DataTable agents = G1.get_db_data(cmd);
        //    if (agents.Rows.Count <= 0)
        //        return agents;
        //    agents.Columns.Add("Formula Sales", Type.GetType("System.Double"));
        //    agents.Columns.Add("Location Sales", Type.GetType("System.Double"));
        //    agents.Columns.Add("Total Sales", Type.GetType("System.Double"));
        //    agents.Columns.Add("Commission", Type.GetType("System.Double"));

        //    DataTable allAgentsDt = G1.get_db_data("Select * from `agents`;");

        //    DataTable dt = (DataTable)dgv.DataSource;
        //    string agentCode = "";
        //    string formula = "";
        //    string agent = "";
        //    double percent = 0D;
        //    double goal = 0D;
        //    int count = 0;
        //    bool rv = false;
        //    int position = -1;
        //    string lastDelimiter = "";
        //    string parameter = "";
        //    string delimiter = "";
        //    string delimiters = @"(?<=[.,;])+->";
        //    string[,] calc = new string[50,2];
        //    for (int i = 0; i < agents.Rows.Count; i++)
        //    {
        //        agentCode = agents.Rows[i]["agentCode"].ObjToString();
        //        formula = agents.Rows[i]["formula"].ObjToString();
        //        percent = agents.Rows[i]["percent"].ObjToDouble();
        //        goal = agents.Rows[i]["goal"].ObjToDouble();
        //        count = 0;
        //        for (;;)
        //        {
        //            try
        //            {
        //                rv = GetParameter(formula, delimiters, ref parameter, ref delimiter, ref position);
        //                if (!rv)
        //                    break;
        //                calc[count, 0] = parameter;
        //                calc[count, 1] = delimiter;
        //                count++;
        //                if (String.IsNullOrWhiteSpace(delimiter))
        //                    break;
        //                formula = formula.Substring((position + 1));
        //            }
        //            catch( Exception ex )
        //            {
        //                MessageBox.Show("***ERROR*** Parsing Formula! " + ex.Message.ToString());
        //            }
        //        }
        //        double formulaSales = 0D;
        //        double locationSales = 0D;
        //        double totalSales = 0D;
        //        double commission = 0D;
        //        lastDelimiter = "";
        //        double value = 0D;
        //        for ( int j=0; j<count; j++)
        //        {
        //            try
        //            {
        //                parameter = calc[j, 0];
        //                delimiter = calc[j, 1];
        //                if (isAgent(parameter, allAgentsDt))
        //                {
        //                    value = GetAgentSales(parameter, dt);
        //                    if (lastDelimiter == "+")
        //                        formulaSales += value;
        //                    else if (String.IsNullOrWhiteSpace(lastDelimiter))
        //                        formulaSales = value;
        //                }
        //                else
        //                {
        //                    value = GetLocationSales(parameter, dt);
        //                    if (lastDelimiter == "+")
        //                        locationSales += value;
        //                    else if (String.IsNullOrWhiteSpace(lastDelimiter))
        //                        locationSales = value;

        //                }
        //                lastDelimiter = delimiter;
        //            }
        //            catch ( Exception ex)
        //            {
        //                MessageBox.Show("***ERROR*** Gathering Sales and Location Data " + ex.Message.ToString());
        //            }
        //        }
        //        formulaSales = G1.RoundValue(formulaSales);
        //        locationSales = G1.RoundValue(locationSales);
        //        agents.Rows[i]["Formula Sales"] = formulaSales;
        //        agents.Rows[i]["Location Sales"] = locationSales;
        //        totalSales = formulaSales + locationSales;
        //        agents.Rows[i]["Total Sales"] = G1.RoundValue(totalSales);
        //        commission = totalSales * (percent / 100D);
        //        if ( totalSales > goal )
        //            agents.Rows[i]["Commission"] = commission;
        //    }
        //    return agents;
        //}
        /****************************************************************************************/
        private double GetAgentSales(string parameter, DataTable dt)
        {
            double total = 0D;
            double value = 0D;
            DataRow[] dRows = dt.Select("agentNumber='" + parameter + "'");
            for (int i = 0; i < dRows.Length; i++)
            {
                value = dRows[i]["contractValue"].ObjToDouble();
                total += value;
            }
            return total;
        }
        /****************************************************************************************/
        private double GetLocationSales(string parameter, DataTable dt)
        {
            double total = 0D;
            double value = 0D;
            DataRow[] dRows = dt.Select("loc='" + parameter + "'");
            for (int i = 0; i < dRows.Length; i++)
            {
                value = dRows[i]["contractValue"].ObjToDouble();
                total += value;
            }
            return total;
        }
        /****************************************************************************************/
        public static bool isAgent(string parameter, DataTable agents)
        {
            bool rv = false;
            for (int i = 0; i < agents.Rows.Count; i++)
            {
                if (parameter.ToUpper() == agents.Rows[i]["agentCode"].ObjToString().ToUpper())
                {
                    rv = true;
                    break;
                }
            }
            return rv;
        }
        /****************************************************************************************/
        private bool GetParameter(string formula, string delimiters, ref string parameter, ref string delimiter, ref int position)
        {
            bool rv = false;
            position = -1;
            parameter = "";
            delimiter = "";
            string c = "";
            for (int i = 0; i < formula.Length; i++)
            {
                c = formula.Substring(i, 1);
                if (delimiters.Contains(c))
                {
                    parameter = formula.Substring(0, i);
                    delimiter = c;
                    position = i;
                    rv = true;
                    break;
                }
            }
            if (!rv && formula.Length > 0)
            {
                parameter = formula;
                delimiter = "";
                position = formula.Length;
                rv = true;
            }
            return rv;
        }
        /****************************************************************************************/
        private void chkComboLocation_EditValueChanged(object sender, EventArgs e)
        {
            string names = getLocationQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            //            btnRun_Click(null, null);
        }
        /****************************************************************************************/
        private void chkComboAgentNames_EditValueChanged(object sender, EventArgs e)
        {
            string names = getAgentNameQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /****************************************************************************************/
        private void chkComboLocNames_EditValueChanged(object sender, EventArgs e)
        {
            string names = getAgentNameQuery();
            if (!String.IsNullOrWhiteSpace(names))
            {
                DataRow[] dRows = originalDt.Select(names);
                DataTable dt = originalDt.Clone();
                for (int i = 0; i < dRows.Length; i++)
                    dt.ImportRow(dRows[i]);

                names = getLocationNameQuery();
                dRows = dt.Select(names);
                DataTable newdt = originalDt.Clone();
                for (int i = 0; i < dRows.Length; i++)
                    newdt.ImportRow(dRows[i]);
                G1.NumberDataTable(newdt);
                dgv.DataSource = newdt;
            }
            else
            {

                names = getLocationNameQuery();
                DataRow[] dRows = originalDt.Select(names);
                DataTable dt = originalDt.Clone();
                for (int i = 0; i < dRows.Length; i++)
                    dt.ImportRow(dRows[i]);
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;
            }
        }
        /****************************************************************************************/
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage current = (sender as TabControl).SelectedTab;
            if (current.Name.Trim().ToUpper() == "TABLOCATIONS")
                LoadTabLocations();
            else if (current.Name.Trim().ToUpper() == "TABAGENTTOTALS")
                LoadTabAgents();
            else if (current.Name.Trim().ToUpper() == "TABAGENTLOCATIONS")
                LoadTabAgentsLocations();
            else if (current.Name.Trim().ToUpper() == "TABLOCATIONTOTALS")
                LoadTabAllLocations();
            else if (current.Name.Trim().ToUpper() == "TABCONTRACTLOCATIONS")
                LoadTabContractsByLocations();
        }
        /****************************************************************************************/
        private void LoadTabLocations()
        {
            DataTable dx = (DataTable)dgv.DataSource;
            DataTable dt = dx.Copy();
            DataView tempview = dt.DefaultView;
            //            tempview.Sort = "loc asc, agentName asc";
            tempview.Sort = "loc asc, agentNumber asc";
            dt = tempview.ToTable();

            //DataRow[] dRRR = dt.Select("loc='MC'");
            //DataTable testdt = dt.Clone();
            //for (int i = 0; i < dRRR.Length; i++)
            //    testdt.ImportRow(dRRR[i]);

            double fbi = 0D;
            double contractValue = 0D;
            string agentNumber = "";
            DataRow[] dRows = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                fbi = dt.Rows[i]["fbi"].ObjToDouble();
                contractValue = dt.Rows[i]["contractValue"].ObjToDouble();
                if (contractValue < 0D && fbi == 1D)
                    dt.Rows[i]["contractValue"] = 0D;
                agentNumber = dt.Rows[i]["agentNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(agentNumber))
                {
                    dt.Rows[i]["agentNumber"] = "XXX";
                    dt.Rows[i]["agentName"] = "Agent XXX";
                }
                else
                {
                    dRows = _agentList.Select("agentCode='" + agentNumber + "'");
                    if (dRows.Length <= 0)
                    {
                        dt.Rows[i]["agentNumber"] = "XXX";
                        dt.Rows[i]["agentName"] = "Agent XXX";
                    }
                }
            }


            DataTable lDt = dt.Clone();

            //            lDt.Columns.Add("ibtrust", Type.GetType("System.Double"));
            //          lDt.Columns.Add("sptrust", Type.GetType("System.Double"));
            lDt.Columns.Add("total", Type.GetType("System.Double"));

            string lastLocation = "";
            string lastAgent = "";
            string lastAgentName = "";
            string location = "";
            string agent = "";
            string lloc = "";
            //double contractValue = 0D;
            double downPayment = 0D;
            double payment = 0D;
            double ccFee = 0D;
            double cashAdvance = 0D;
            double debit = 0D;
            double credit = 0D;
            double dbc_5 = 0D;
            double dpr = 0D;
            double dbc = 0D;
            double interest = 0D;
            double ibtrust = 0D;
            double sptrust = 0D;
            double xxtrust = 0D;
            string xtrust = "";
            string contractNumber = "";
            DateTime deceasedDate = DateTime.Now;
            int idx = 0;
            string ch;
            //string agentNumber = "";
            string trust = "";
            bool dbr = false;
            bool contractsOnly = false;
            if (chkShowOnlyContractValues.Checked)
                contractsOnly = true;
            bool doit = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                doit = false;
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                agent = dt.Rows[i]["agentName"].ObjToString();
                location = dt.Rows[i]["loc"].ObjToString();
                agentNumber = dt.Rows[i]["agentNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(lastLocation))
                    lastLocation = location;
                if (String.IsNullOrWhiteSpace(lastAgent))
                    lastAgent = agentNumber;
                if (String.IsNullOrWhiteSpace(lastAgentName))
                    lastAgentName = agent;
                if (contractsOnly)
                {
                    deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                    if (dt.Rows[i]["DBR"].ObjToString() == "DBR")
                    {
                        //if ( deceasedDate <= this.dateTimePicker2.Value )
                        continue;
                    }
                    contractValue = dt.Rows[i]["contractValue"].ObjToDouble();
                    if (contractValue <= 0D)
                        continue;
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    cashAdvance = dt.Rows[i]["cashAdvance"].ObjToDouble();
                    trust = dt.Rows[i]["trust"].ObjToString();
                    xtrust = dt.Rows[i]["xtrust"].ObjToString();
                    ibtrust = 0D;
                    sptrust = 0D;
                    if (xtrust.ToUpper() == "Y")
                        xxtrust = dt.Rows[i]["contractValue"].ObjToDouble();
                    else
                    {
                        if (trust.Length > 0)
                        {
                            idx = trust.Length - 1;
                            ch = trust.Substring(idx);
                            if (ch.ToUpper() == "I")
                                ibtrust += dt.Rows[i]["contractValue"].ObjToDouble();
                            else
                                sptrust += dt.Rows[i]["contractValue"].ObjToDouble();
                        }
                        else
                            sptrust += dt.Rows[i]["contractValue"].ObjToDouble();
                    }

                    DataRow drow = lDt.NewRow();
                    drow["contractNumber"] = contractNumber;
                    drow["loc"] = location;
                    drow["agentNumber"] = agentNumber;
                    drow["agentName"] = agent;
                    drow["downPayment"] = dpr;
                    drow["contractValue"] = contractValue;
                    drow["paymentAmount"] = payment;
                    drow["ccFee"] = ccFee;
                    drow["cashAdvance"] = cashAdvance;
                    drow["totalPayments"] = contractValue;
                    drow["ibtrust"] = ibtrust;
                    drow["sptrust"] = sptrust;
                    drow["xxtrust"] = xxtrust;
                    drow["total"] = ibtrust + sptrust;
                    lDt.Rows.Add(drow);
                    lastLocation = location;
                    lastAgent = agentNumber;
                    lastAgentName = agent;
                    contractValue = 0D;
                    payment = 0D;
                    ccFee = 0D;
                    downPayment = 0D;
                    debit = 0D;
                    credit = 0D;
                    interest = 0D;
                    cashAdvance = 0D;
                    ibtrust = 0D;
                    sptrust = 0D;
                    xxtrust = 0D;
                    dpr = 0D;
                    dbc_5 = 0D;
                    lloc = "";
                    continue;
                }
                if (location != lastLocation || doit)
                {
                    DataRow drow = lDt.NewRow();
                    drow["loc"] = lastLocation;
                    drow["agentNumber"] = lastAgent;
                    drow["agentName"] = lastAgentName;
                    drow["downPayment"] = dpr;
                    drow["contractValue"] = contractValue;
                    drow["paymentAmount"] = payment;
                    drow["ccFee"] = ccFee;
                    drow["cashAdvance"] = cashAdvance;
                    drow["totalPayments"] = dpr + payment + credit - debit - interest - dbc_5;
                    drow["ibtrust"] = ibtrust;
                    drow["sptrust"] = sptrust;
                    drow["xxtrust"] = xxtrust;
                    drow["total"] = ibtrust + sptrust;
                    lDt.Rows.Add(drow);
                    lastLocation = location;
                    lastAgent = agentNumber;
                    lastAgentName = agent;
                    contractValue = 0D;
                    payment = 0D;
                    ccFee = 0D;
                    downPayment = 0D;
                    debit = 0D;
                    credit = 0D;
                    interest = 0D;
                    cashAdvance = 0D;
                    ibtrust = 0D;
                    sptrust = 0D;
                    xxtrust = 0D;
                    dpr = 0D;
                    dbc_5 = 0D;
                    lloc = "";
                }
                else if (agentNumber != lastAgent)
                {
                    DataRow drow = lDt.NewRow();
                    drow["loc"] = lastLocation;
                    drow["agentNumber"] = lastAgent;
                    drow["agentName"] = lastAgentName;
                    lDt.Rows.Add(drow);
                    lastAgent = agentNumber;
                    drow["downPayment"] = dpr;
                    drow["contractValue"] = contractValue;
                    drow["paymentAmount"] = payment;
                    drow["ccFee"] = ccFee;
                    drow["cashAdvance"] = cashAdvance;
                    drow["totalPayments"] = dpr + payment + credit - debit - interest - dbc_5;
                    drow["ibtrust"] = ibtrust;
                    drow["sptrust"] = sptrust;
                    drow["xxtrust"] = xxtrust;
                    drow["total"] = ibtrust + sptrust;
                    contractValue = 0D;
                    payment = 0D;
                    ccFee = 0D;
                    downPayment = 0D;
                    debit = 0D;
                    credit = 0D;
                    interest = 0D;
                    cashAdvance = 0D;
                    ibtrust = 0D;
                    sptrust = 0D;
                    xxtrust = 0D;
                    dpr = 0D;
                    dbc_5 = 0D;
                }
                dbr = false;
                if (dt.Rows[i]["DBR"].ObjToString() == "DBR")
                    dbr = true;
                dbc = dt.Rows[i]["dbc"].ObjToDouble();
                dbc_5 += dt.Rows[i]["dbc_5"].ObjToDouble();
                dpr += dt.Rows[i]["downPayment"].ObjToDouble();
                contractValue += dt.Rows[i]["contractValue"].ObjToDouble();
                if (!dbr)
                {
                    payment += dt.Rows[i]["paymentAmount"].ObjToDouble();
                    ccFee += dt.Rows[i]["ccFee"].ObjToDouble();
                    downPayment += dt.Rows[i]["downPayment"].ObjToDouble();
                    cashAdvance += dt.Rows[i]["cashAdvance"].ObjToDouble();
                    debit += dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    credit += dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    interest += dt.Rows[i]["interestPaid"].ObjToDouble();
                    trust = dt.Rows[i]["trust"].ObjToString();
                    xtrust = dt.Rows[i]["xtrust"].ObjToString();
                    if (xtrust.ToUpper() == "Y")
                        xxtrust = dt.Rows[i]["contractValue"].ObjToDouble();
                    else
                    {
                        if (trust.Length > 0)
                        {
                            idx = trust.Length - 1;
                            ch = trust.Substring(idx);
                            if (ch.ToUpper() == "I")
                                ibtrust += dt.Rows[i]["contractValue"].ObjToDouble();
                            else
                                sptrust += dt.Rows[i]["contractValue"].ObjToDouble();
                        }
                        else
                            sptrust += dt.Rows[i]["contractValue"].ObjToDouble();
                    }
                }
            }
            if (!contractsOnly)
            {
                DataRow ddr = lDt.NewRow();
                ddr["loc"] = lastLocation;
                ddr["agentNumber"] = lastAgent;
                ddr["agentName"] = lastAgentName;
                ddr["downPayment"] = dpr;
                ddr["contractValue"] = contractValue;
                ddr["paymentAmount"] = payment;
                ddr["ccFee"] = ccFee;
                ddr["cashAdvance"] = cashAdvance;
                ddr["totalPayments"] = dpr + payment - ccFee + credit - debit - interest - dbc_5;
                ddr["ibtrust"] = ibtrust;
                ddr["sptrust"] = sptrust;
                ddr["xxtrust"] = xxtrust;
                ddr["total"] = ibtrust + sptrust;
                lDt.Rows.Add(ddr);
            }

            DataTable dd = (DataTable)chkComboLocNames.Properties.DataSource;
            dx = G1.get_db_data("Select * from `agents`;");

            lastLocation = "";
            bool first = true;
            for (int i = 0; i < lDt.Rows.Count; i++)
            {
                agent = lDt.Rows[i]["agentNumber"].ObjToString();
                DataRow[] dR = dx.Select("agentCode='" + agent + "'");
                if (dR.Length > 0)
                    lDt.Rows[i]["agentName"] = dR[0]["firstName"].ObjToString().Trim() + " " + dR[0]["lastName"].ObjToString().Trim();

                location = lDt.Rows[i]["loc"].ObjToString();
                if (contractsOnly)
                {
                    lastLocation = location;
                    DataRow[] dr = dd.Select("keycode='" + location + "'");
                    if (dr.Length > 0)
                        lDt.Rows[i]["Location Name"] = dr[0]["name"].ObjToString();
                    continue;
                }
                if (location == lastLocation)
                {
                    //                    lDt.Rows[i]["loc"] = "";
                }
                else
                {
                    lastLocation = location;
                    DataRow[] dr = dd.Select("keycode='" + location + "'");
                    if (dr.Length > 0)
                        lDt.Rows[i]["Location Name"] = dr[0]["name"].ObjToString();
                }
            }

            lastLocation = "";
            if (!contractsOnly)
            {
                for (int i = (lDt.Rows.Count - 1); i >= 0; i--)
                {
                    location = lDt.Rows[i]["loc"].ObjToString();
                    if (String.IsNullOrWhiteSpace(lastLocation))
                        lastLocation = location;
                    if (location != lastLocation)
                    {
                        DataRow dRow = lDt.NewRow();
                        lDt.Rows.InsertAt(dRow, (i + 1));
                        lastLocation = location;
                    }
                }
            }

            DailyHistory.AddAP(lDt);
            DailyHistory.CleanupVisibility(gridMain2);
            G1.NumberDataTable(lDt);
            dgv2.DataSource = lDt;
        }
        /****************************************************************************************/
        private void LoadTabAgents()
        {
            chkNoSummary.Hide();
            chkMonthly.Hide();
            btnMatch.Hide();
            if (chkShowCommissions.Checked)
            {
                LoadTabDetailAgents();
                return;
            }
            try
            {
                gridMain3.Columns["commission"].Visible = false;
                gridMain3.Columns["Split Payment"].Visible = false;
                gridMain3.Columns["Split DownPayment"].Visible = false;
                gridMain3.Columns["interestPaid"].Visible = false;
                gridMain3.Columns["dbc"].Visible = false;
                gridMain3.Columns["lastName"].Visible = false;
                gridMain3.Columns["firstName"].Visible = false;
                gridMain3.Columns["depositNumber"].Visible = false;
                gridMain3.Columns["userId"].Visible = false;
                gridMain3.Columns["edited"].Visible = false;
                gridMain3.Columns["percentComm"].Visible = false;

                gridMain3.Columns["debitAdjustment"].Visible = false;
                gridMain3.Columns["creditAdjustment"].Visible = false;
                gridMain3.Columns["num"].Visible = true;
                gridMain3.Columns["recapAmount"].Visible = true;
                gridMain3.Columns["Reins"].Visible = true;
                gridMain3.Columns["Recap"].Visible = true;
                gridMain3.Columns["cashAdvance"].Visible = true;

                DataTable dx = (DataTable)dgv.DataSource;
                DataTable dt = dx.Copy();
                double fbi = 0D;
                double fbiMoney = 0D;
                double contractValue = 0D;
                string agentNumber = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    fbi = dt.Rows[i]["fbi"].ObjToDouble();
                    contractValue = dt.Rows[i]["contractValue"].ObjToDouble();
                    if (contractValue < 0D && fbi == 1D)
                        dt.Rows[i]["contractValue"] = 0D;
                    agentNumber = dt.Rows[i]["agentNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(agentNumber))
                    {
                        dt.Rows[i]["agentNumber"] = "XXX";
                        dt.Rows[i]["agentName"] = "Agent XXX";
                    }
                }


                DataView tempview = dt.DefaultView;
                if (chkMonthly.Checked)
                {
                    tempview.Sort = "agentNumber asc, payDate8 asc";
                    gridMain3.Columns["payDate8"].Visible = true;
                }
                else
                {
                    tempview.Sort = "agentName asc";
                    gridMain3.Columns["payDate8"].Visible = false;
                }
                dt = tempview.ToTable();

                DataRow[] dRows = dt.Select("agentNumber='XXX'");
                if (dRows.Length > 0)
                {
                    DataTable ddddd = dRows.CopyToDataTable();
                }

                DataTable dt8 = (DataTable)dgv8.DataSource;
                if (chkMonthly.Checked)
                {
                    DataTable ddd = new DataTable();
                    ddd.Columns.Add("agent");
                    ddd.Columns.Add("payDate8");
                    ddd.Columns.Add("payAmount", Type.GetType("System.Double"));
                    ddd.Columns.Add("downPayment", Type.GetType("System.Double"));
                    ddd.Columns.Add("contractValue", Type.GetType("System.Double"));
                    ddd.Columns.Add("Recap", Type.GetType("System.Decimal"));
                    ddd.Columns.Add("Reins", Type.GetType("System.Decimal"));
                    DateTime ddDate = DateTime.Now;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow dRow = ddd.NewRow();
                        ddDate = dt.Rows[i]["payDate8"].ObjToDateTime();

                        dRow["agent"] = dt.Rows[i]["agentNumber"].ObjToString();
                        dRow["payDate8"] = ddDate.Year.ToString("D4") + ddDate.Month.ToString("D2");

                        dRow["payAmount"] = dt.Rows[i]["paymentAmount"].ObjToDouble() - dt.Rows[i]["ccFee"].ObjToDouble();

                        dRow["downPayment"] = dt.Rows[i]["downPayment"].ObjToDouble();
                        dRow["contractValue"] = dt.Rows[i]["contractValue"].ObjToDouble();
                        dRow["Recap"] = dt.Rows[i]["Recap"].ObjToDouble();
                        dRow["Reins"] = dt.Rows[i]["Reins"].ObjToDouble();
                        ddd.Rows.Add(dRow);
                    }
                    ddd.AcceptChanges();
                }
                DataTable lDt = dt.Clone();
                if (G1.get_column_number(lDt, "fbiCommission") < 0)
                    lDt.Columns.Add("fbiCommission", Type.GetType("System.Decimal"));
                if (G1.get_column_number(lDt, "dpr") < 0)
                    lDt.Columns.Add("dpr", Type.GetType("System.Double")); //Actual Down Payment
                if (G1.get_column_number(lDt, "dbc_5") < 0)
                    lDt.Columns.Add("dbc_5", Type.GetType("System.Double")); // Actual DBC Down Payment
                if (G1.get_column_number(lDt, "ccFee") < 0)
                    lDt.Columns.Add("ccFee", Type.GetType("System.Double")); // Actual DBC Down Payment

                //DataRow[] dddRows = dt.Select("agentName='Ronnie Knotts'");
                //DataTable xDt = dt.Clone();
                //for (int i = 0; i < dddRows.Length; i++)
                //    xDt.ImportRow(dddRows[i]);
                //dt = xDt.Copy();

                //if (!chkNoSummary.Checked)
                //    gridMain3.Columns["contractNumber"].Visible = false;
                //else
                //    gridMain3.Columns["contractNumber"].Visible = true;
                string lastLocation = "";
                string lastAgent = "";
                string lastNumber = "";
                string location = "";
                string agent = "";
                string lloc = "";
                //double contractValue = 0D;
                double downPayment = 0D;
                double dpr = 0D;
                double dbc_5 = 0D;
                double payment = 0D;
                double ccFee = 0D;
                double debit = 0D;
                double credit = 0D;
                double interest = 0D;
                double dbrValue = 0D;
                double reins = 0D;
                double cashAdvance = 0D;
                fbi = 0D;
                fbiMoney = 0D;
                double dbc = 0D;
                //string agentNumber = "";
                DateTime date = DateTime.Now;
                DateTime newDate = DateTime.Now;
                string issueDate = "";
                string lapseDate = "";
                string contract = "";
                bool dateChange = false;
                bool agentChange = false;
                bool dbr = false;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    agent = dt.Rows[i]["agentName"].ObjToString();
                    location = dt.Rows[i]["loc"].ObjToString();
                    agentNumber = dt.Rows[i]["agentNumber"].ObjToString();
                    newDate = dt.Rows[i]["paydate8"].ObjToDateTime();

                    dateChange = false;
                    agentChange = false;

                    if (String.IsNullOrWhiteSpace(lastLocation))
                    {
                        lastLocation = location;
                        date = newDate;
                    }
                    if (String.IsNullOrWhiteSpace(lastAgent))
                    {
                        lastAgent = agent;
                        lastNumber = agentNumber;
                        date = newDate;
                    }
                    if (chkMonthly.Checked)
                    {
                        string oldDate = date.Year.ToString("D4") + date.Month.ToString("D2");
                        string nowDate = newDate.Year.ToString("D4") + newDate.Month.ToString("D2");
                        if (nowDate != oldDate)
                            dateChange = true;
                    }
                    if (agent != lastAgent)
                        agentChange = true;
                    if (chkNoSummary.Checked)
                    {
                        fbi = dt.Rows[i]["fbi"].ObjToDouble();
                        contractValue = dt.Rows[i]["contractValue"].ObjToDouble();

                        payment = dt.Rows[i]["paymentAmount"].ObjToDouble();

                        downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                        dpr = dt.Rows[i]["downPayment"].ObjToDouble();
                        ccFee = dt.Rows[i]["ccFee"].ObjToDouble();
                        debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                        credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                        interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                        dbrValue = dt.Rows[i]["Recap"].ObjToDouble();
                        reins = dt.Rows[i]["Reins"].ObjToDouble();
                        cashAdvance = dt.Rows[i]["cashAdvance"].ObjToDouble();
                        if (contractValue == 0D && downPayment == 0D)
                            continue;
                    }
                    if (dateChange || agentChange || chkNoSummary.Checked)
                    {
                        DataRow drow = lDt.NewRow();
                        drow["loc"] = lastLocation;
                        drow["agentNumber"] = lastNumber;
                        lDt.Rows.Add(drow);
                        drow["downPayment"] = downPayment;
                        drow["dpr"] = dpr;
                        drow["dbc_5"] = dbc_5;
                        drow["fbi"] = fbi;
                        drow["contractValue"] = contractValue;
                        drow["paymentAmount"] = payment;
                        drow["totalPayments"] = dpr + payment + credit - debit - interest - dbc_5;
                        drow["Recap"] = dbrValue;
                        drow["cashAdvance"] = cashAdvance;
                        if (chkSummarize.Checked)
                            drow["Reins"] = reins;
                        drow["payDate8"] = G1.DTtoMySQLDT(date);
                        if (chkNoSummary.Checked)
                        {
                            contract = dt.Rows[i]["contractNumber"].ObjToString();
                            issueDate = G1.GetSQLDate(dt, i, "issueDate8");
                            lapseDate = G1.GetSQLDate(dt, i, "lapseDate8");
                            drow["contractNumber"] = contract + " (I" + issueDate + ")";
                            drow["agentNumber"] = agentNumber;
                            drow["agentName"] = agent;
                        }
                        contractValue = 0D;
                        payment = 0D;
                        downPayment = 0D;
                        dpr = 0D;
                        dbc_5 = 0D;
                        ccFee = 0D;
                        fbi = 0D;
                        debit = 0D;
                        credit = 0D;
                        interest = 0D;
                        dbrValue = 0D;
                        reins = 0D;
                        cashAdvance = 0D;
                        lastAgent = agent;
                        lastNumber = agentNumber;
                        date = newDate;
                    }
                    dbr = false;
                    if (dt.Rows[i]["dbr"].ObjToString().ToUpper() == "DBR")
                        dbr = true;
                    dbc = dt.Rows[i]["dbc"].ObjToDouble();
                    dbc_5 += dt.Rows[i]["dbc_5"].ObjToDouble();
                    dpr += dt.Rows[i]["downPayment"].ObjToDouble();
                    contractValue += dt.Rows[i]["contractValue"].ObjToDouble();
                    cashAdvance += dt.Rows[i]["cashAdvance"].ObjToDouble();
                    fbi += dt.Rows[i]["fbi"].ObjToDouble();
                    if (!dbr)
                    {
                        //contractValue += dt.Rows[i]["contractValue"].ObjToDouble();

                        payment += dt.Rows[i]["paymentAmount"].ObjToDouble();

                        downPayment += dt.Rows[i]["downPayment"].ObjToDouble();
                        ccFee += dt.Rows[i]["ccFee"].ObjToDouble();
                        debit += dt.Rows[i]["debitAdjustment"].ObjToDouble();
                        credit += dt.Rows[i]["creditAdjustment"].ObjToDouble();
                        interest += dt.Rows[i]["interestPaid"].ObjToDouble();
                        dbrValue += dt.Rows[i]["Recap"].ObjToDouble();
                        reins += dt.Rows[i]["Reins"].ObjToDouble();
                    }
                }
                if (!chkNoSummary.Checked)
                {
                    DataRow ddr = lDt.NewRow();
                    ddr["loc"] = lastLocation;
                    ddr["agentNumber"] = agentNumber;
                    ddr["downPayment"] = downPayment;
                    ddr["dpr"] = dpr;
                    ddr["fbi"] = fbi;
                    ddr["contractValue"] = contractValue;
                    ddr["paymentAmount"] = payment;
                    ddr["totalPayments"] = dpr + payment + credit - debit - interest - dbc_5;
                    ddr["Recap"] = dbrValue;
                    if (chkSummarize.Checked)
                        ddr["Reins"] = reins;
                    ddr["cashAdvance"] = cashAdvance;
                    ddr["payDate8"] = G1.DTtoMySQLDT(date);
                    lDt.Rows.Add(ddr);
                }

                DataTable dd = (DataTable)chkComboLocNames.Properties.DataSource;
                dx = G1.get_db_data("Select * from `agents`;");

                lastLocation = "";
                double fbiCommission = 0D;
                bool first = true;
                for (int i = 0; i < lDt.Rows.Count; i++)
                {
                    fbiMoney = 0D;
                    fbiCommission = 0D;
                    agent = lDt.Rows[i]["agentNumber"].ObjToString();
                    DataRow[] dR = dx.Select("agentCode='" + agent + "'");
                    if (dR.Length > 0)
                    {
                        lDt.Rows[i]["agentName"] = dR[0]["firstName"].ObjToString().Trim() + " " + dR[0]["lastName"].ObjToString().Trim();
                        fbiCommission = dR[0]["fbiCommission"].ObjToDouble();
                    }
                    fbi = lDt.Rows[i]["fbi"].ObjToDouble();
                    fbiMoney = fbi * fbiCommission;
                    lDt.Rows[i]["fbiCommission"] = fbiMoney;

                    location = lDt.Rows[i]["loc"].ObjToString();
                    if (String.IsNullOrWhiteSpace(lastLocation))
                        lastLocation = location;
                    if (location == lastLocation && i != 0)
                    {
                        lDt.Rows[i]["loc"] = "";
                    }
                    else
                    {
                        lastLocation = location;
                        DataRow[] dr = dd.Select("keycode='" + location + "'");
                        if (dr.Length > 0)
                            lDt.Rows[i]["Location Name"] = dr[0]["name"].ObjToString();
                    }
                }
                if (chkMonthly.Checked)
                {
                    tempview = lDt.DefaultView;
                    tempview.Sort = "agentName asc, payDate8 asc";
                    lDt = tempview.ToTable();
                    lDt = SummarizeByAgentName(lDt);
                    G1.NumberDataTable(lDt);

                    DailyHistory.AddAP(lDt);
                    DailyHistory.CleanupVisibility(gridMain3);

                    dgv3.DataSource = lDt;
                    return;
                }

                lastLocation = "";
                for (int i = (lDt.Rows.Count - 1); i >= 0; i--)
                {
                    location = lDt.Rows[i]["agentName"].ObjToString();
                    if (String.IsNullOrWhiteSpace(lastLocation))
                        lastLocation = location;
                    if (location != lastLocation)
                    {
                        DataRow dRow = lDt.NewRow();
                        lDt.Rows.InsertAt(dRow, (i + 1));
                        lastLocation = location;
                    }
                }
                if (!chkSummarize.Checked)
                {
                    gridMain3.Columns["contractNumber"].Visible = true;
                    gridMain3.Columns["recapAmount"].Visible = true;
                    lDt.Columns.Add("recapAmount", Type.GetType("System.Decimal"));
                    CheckForLapse(lDt);
                }
                else
                {
                    gridMain3.Columns["contractNumber"].Visible = false;
                    gridMain3.Columns["recapAmount"].Visible = false;
                }

                DailyHistory.AddAP(lDt);
                DailyHistory.CleanupVisibility(gridMain3);

                G1.NumberDataTable(lDt);
                dgv3.DataSource = lDt;
                gridMain3.Columns["agentName"].GroupIndex = -1;
                gridMain3.OptionsBehavior.AutoExpandAllGroups = false;
                gridMain3.CollapseAllGroups();
                gridMain3.OptionsPrint.ExpandAllGroups = false;
                gridMain3.OptionsPrint.PrintGroupFooter = false;
            }
            catch (Exception ex)
            {

            }
        }
        /****************************************************************************************/
        private void SetupSplitInfo(DataTable agentDt)
        {
            string agentCode = "";
            DataTable goalDt = G1.get_db_data("Select * from `goals` where `status` = 'CURRENT' ORDER by `effectiveDate`;");
            if (goalDt.Rows.Count <= 0)
                return;
            string splits = "";
            for (int i = 0; i < agentDt.Rows.Count; i++)
            {
                agentCode = agentDt.Rows[i]["agentCode"].ObjToString();
                DataRow[] dRows = goalDt.Select("agentCode='" + agentCode + "'");
                for (int j = 0; j < dRows.Length; j++)
                {
                    splits = dRows[j]["splits"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(splits))
                        agentDt.Rows[i]["splits"] = splits;
                }
            }
        }
        /****************************************************************************************/
        private void LoadTabDetailAgents()
        {
            chkNoSummary.Hide();
            chkMonthly.Hide();
            btnMatch.Show();
            gridMain3.Columns["commission"].Visible = true;
            gridMain3.Columns["percentComm"].Visible = true;
            gridMain3.Columns["Split Payment"].Visible = true;
            gridMain3.Columns["Split DownPayment"].Visible = true;
            gridMain3.Columns["interestPaid"].Visible = true;
            gridMain3.Columns["dpr"].Visible = true;
            gridMain3.Columns["downPayment"].Visible = false;
            gridMain3.Columns["dbc"].Visible = false;
            gridMain3.Columns["dbc_5"].Visible = true;
            gridMain3.Columns["lastName"].Visible = true;
            gridMain3.Columns["firstName"].Visible = true;
            gridMain3.Columns["depositNumber"].Visible = true;
            gridMain3.Columns["userId"].Visible = true;
            gridMain3.Columns["edited"].Visible = true;
            gridMain3.Columns["cashAdvance"].Visible = true;
            gridMain3.Columns["agentName"].Visible = false;
            gridMain3.Columns["debitAdjustment"].Visible = true;
            gridMain3.Columns["creditAdjustment"].Visible = true;
            gridMain3.Columns["recapAmount"].Visible = false;
            gridMain3.Columns["Reins"].Visible = false;
            gridMain3.Columns["Recap"].Visible = false;
            gridMain3.Columns["num"].Visible = false;

            DataTable dx = (DataTable)dgv.DataSource;
            DataTable dt = dx.Copy();
            if (G1.get_column_number(dt, "Split Payment") < 0)
            {
                MessageBox.Show("***Warning*** You may need to run Split Commissions first!");
                dt.Columns.Add("Split Payment", Type.GetType("System.Double"));
            }
            if (G1.get_column_number(dt, "Split DownPayment") < 0)
                dt.Columns.Add("Split DownPayment", Type.GetType("System.Double"));

            DataView tempview = dt.DefaultView;
            tempview.Sort = "agentName asc, agentNumber asc";
            dt = tempview.ToTable();


            double fbi = 0D;
            double contractValue = 0D;
            string agentNumber = "";
            bool addedBadAgent = false;
            DataRow[] dRows = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                fbi = dt.Rows[i]["fbi"].ObjToDouble();
                contractValue = dt.Rows[i]["contractValue"].ObjToDouble();
                if (contractValue < 0D && fbi == 1D)
                    dt.Rows[i]["contractValue"] = 0D;
                agentNumber = dt.Rows[i]["agentNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(agentNumber))
                {
                    dt.Rows[i]["agentNumber"] = "XXX";
                    dt.Rows[i]["agentName"] = "Agent XXX";
                    addedBadAgent = true;
                }
                else
                {
                    dRows = _agentList.Select("agentCode='" + agentNumber + "'");
                    if (dRows.Length <= 0)
                    {
                        dt.Rows[i]["agentNumber"] = "XXX";
                        dt.Rows[i]["agentName"] = "Agent XXX";
                        addedBadAgent = true;
                    }
                }
            }


            tempview = dt.DefaultView;
            tempview.Sort = "agentName asc, agentNumber asc";
            gridMain3.Columns["payDate8"].Visible = false;
            dt = tempview.ToTable();

            dRows = dt.Select("agentNumber='XXX'");
            if (dRows.Length > 0)
            {
                DataTable dddd = dRows.CopyToDataTable();
            }

            string agentCode = "";
            string location = "";
            string lastName = "";
            string firstName = "";
            //double contractValue = 0D;
            double downPayment = 0D;
            double payment = 0D;
            double totalPayment = 0D;
            double paymentAmount = 0D;
            double splitPayment = 0D;
            double splitDownPayment = 0D;
            double debit = 0D;
            double credit = 0D;
            double interest = 0D;
            double dbrValue = 0D;
            double reins = 0D;
            double dbc = 0D;
            DateTime date = DateTime.Now;
            DateTime newDate = DateTime.Now;
            string contract = "";
            double fraction = 0D;
            double commission = 0D;
            double actualCommission = 0D;
            double splitCommission = 0D;
            double splitFraction = 0D;
            string splits = "";
            string actualAgent = "";
            string depositNumber = "";
            string userId = "";
            string edited = "";
            bool first = true;
            //double fbi = 0D;
            double fbiMoney = 0D;
            double fbiCommission = 0D;
            double cashAdvance = 0D;
            double percent = 0D;
            double nonSplitPayment = 0D;

            double ccFee = 0D;

            double dpr = 0D;
            double dbc_5 = 0D;

            string cmd = "Select * from `agents` order by `lastName`, `firstName`;";
            DataTable agentList = G1.get_db_data(cmd);
            agentList.Columns.Add("agentNames");
            string fname = "";
            string lname = "";
            for (int i = 0; i < agentList.Rows.Count; i++)
            {
                fname = agentList.Rows[i]["firstName"].ObjToString().Trim();
                lname = agentList.Rows[i]["lastName"].ObjToString().Trim();
                agentCode = agentList.Rows[i]["agentCode"].ObjToString();
                //agentList.Rows[i]["agentNames"] = "(" + agentCode + ") " + fname + " " + lname;
                //agentList.Rows[i]["agentNames"] = fname + " " + lname + " (" + agentCode + ")";
                agentList.Rows[i]["agentNames"] = lname + ", " + fname + " (" + agentCode + ")";
            }
            if (addedBadAgent)
            {
                DataRow ddr = agentList.NewRow();
                ddr["agentCode"] = "XXX";
                //ddr["agentNames"] = "(XXX) Agent XXX";
                ddr["agentNames"] = "Agent XXX (XXX)";
                agentList.Rows.Add(ddr);
                if (G1.get_column_number(dt, "XXX") < 0)
                    dt.Columns.Add("XXX", Type.GetType("System.Double")); // Actual XXX Agent
            }

            SetupSplitInfo(agentList);

            agentCode = "";
            bool theForce = false;
            DataTable lDt = dt.Clone();
            if (G1.get_column_number(lDt, "fbiCommission") < 0)
                lDt.Columns.Add("fbiCommission", Type.GetType("System.Decimal"));
            if (G1.get_column_number(lDt, "percentComm") < 0)
                lDt.Columns.Add("percentComm", Type.GetType("System.Decimal"));

            if (G1.get_column_number(lDt, "dpr") < 0)
                lDt.Columns.Add("dpr", Type.GetType("System.Double")); //Actual Down Payment
            if (G1.get_column_number(lDt, "dbc_5") < 0)
                lDt.Columns.Add("dbc_5", Type.GetType("System.Double")); // Actual DBC Down Payment
            if (G1.get_column_number(lDt, "ccFee") < 0)
                lDt.Columns.Add("ccFee", Type.GetType("System.Double")); // Credit Card Fee

            try
            {
                int j = 0;
                double mainPercent = 0D;
                double newFBI = 0D;
                for (int i = 0; i < agentList.Rows.Count; i++)
                {
                    try
                    {
                        fbiCommission = agentList.Rows[i]["fbiCommission"].ObjToDouble();
                        lname = agentList.Rows[i]["agentNames"].ObjToString();
                        agentCode = agentList.Rows[i]["agentCode"].ObjToString();
                        mainPercent = agentList.Rows[i]["commission"].ObjToDouble();

                        if (agentCode == "XXX" || agentCode == "L15")
                        {
                        }
                        if (G1.get_column_number(dt, agentCode) < 0)
                            continue;
                        actualCommission = agentList.Rows[i]["commission"].ObjToDouble();
                        splits = agentList.Rows[i]["splits"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(splits))
                        {

                        }
                        first = true;
                        for (j = 0; j < dt.Rows.Count; j++)
                        {
                            try
                            {
                                if (j == 633)
                                {

                                }
                                splitDownPayment = 0D;
                                splitCommission = 0D;
                                contract = dt.Rows[j]["contractNumber"].ObjToString();
                                if (String.IsNullOrWhiteSpace(contract))
                                    continue;
                                if (contract == "L24808" && agentCode == "L15")
                                {
                                }
                                if (agentCode == "XXX" && contract == "HC22001")
                                {
                                    FindContract(dt, contract);
                                }
                                if (agentCode == "S46" && contract == "HC22001")
                                {
                                    FindContract(dt, contract);
                                }
                                theForce = false;
                                downPayment = dt.Rows[j]["downPayment"].ObjToDouble();
                                dpr = downPayment;
                                splitCommission = dt.Rows[j][agentCode].ObjToDouble();
                                if (splitCommission == -999.99D || splitCommission == -999.98D)
                                {
                                    splitCommission = dt.Rows[j]["commission"].ObjToDouble();
                                    if (splitCommission != 0D)
                                    {
                                        splitFraction = dt.Rows[j]["split payment"].ObjToDouble();
                                        if (splitFraction > 0D)
                                        {

                                        }
                                        splitCommission = splitCommission * splitFraction;
                                        splitCommission = G1.RoundValue(splitCommission);
                                    }
                                    theForce = true;
                                }
                                contractValue = dt.Rows[j]["contractValue"].ObjToDouble();
                                fbi = dt.Rows[j]["fbi"].ObjToDouble();
                                //fbi = 0D;
                                fbiMoney = 0D;
                                if (fbi > 0D)
                                {
                                    fbiMoney = fbiCommission * fbi;
                                }
                                actualAgent = dt.Rows[j]["agentNumber"].ObjToString();
                                if (splitCommission == 0D && dbc == 0D && fbi <= 0D)
                                {
                                    if (actualAgent == "XXX" && agentCode == "XXX")
                                    {
                                        //if (contractValue <= 0D)
                                        //    continue;
                                    }
                                    else
                                    {
                                        if (!theForce)
                                            continue;
                                    }
                                }
                                if (splitCommission > 0D && fbi > 0D)
                                {
                                }
                                if (splitCommission < 0D)
                                {
                                    if (splitCommission == -99999.99D)
                                        splitCommission = 0D;
                                }
                                actualAgent = dt.Rows[j]["agentNumber"].ObjToString();
                                if (actualAgent != "XXX")
                                {
                                    if (splitCommission == 0D && dbc > 0D)
                                    {
                                        if (actualAgent != agentCode && !theForce)
                                            continue;
                                    }
                                    else if (splitCommission == 0D && fbi > 0D)
                                    {
                                        if (actualAgent != agentCode && !theForce)
                                            continue;
                                    }
                                }
                                else if (agentCode != "XXX")
                                    continue;


                                lastName = dt.Rows[j]["lastName"].ObjToString();
                                firstName = dt.Rows[j]["firstName"].ObjToString();
                                date = dt.Rows[j]["payDate8"].ObjToDateTime();
                                location = dt.Rows[j]["loc"].ObjToString();
                                contractValue = dt.Rows[j]["contractValue"].ObjToDouble();

                                payment = dt.Rows[j]["paymentAmount"].ObjToDouble();

                                totalPayment = dt.Rows[j]["totalPayments"].ObjToDouble();
                                if (totalPayment < -148.68D)
                                {

                                }
                                dbc = dt.Rows[j]["dbc"].ObjToDouble();
                                dbc_5 = dt.Rows[j]["dbc_5"].ObjToDouble();
                                if (dbc_5 > 0D)
                                {

                                }

                                paymentAmount = dt.Rows[j]["paymentAmount"].ObjToDouble();

                                downPayment = dt.Rows[j]["downPayment"].ObjToDouble();
                                //if ( dbc_5 > 0D)
                                //    dpr = downPayment;
                                ccFee = dt.Rows[j]["ccFee"].ObjToDouble();
                                debit = dt.Rows[j]["debitAdjustment"].ObjToDouble();
                                credit = dt.Rows[j]["creditAdjustment"].ObjToDouble();
                                interest = dt.Rows[j]["interestPaid"].ObjToDouble();
                                splitPayment = dt.Rows[j]["Split Payment"].ObjToDouble();
                                percent = splitPayment;
                                percent = 0D;
                                cashAdvance = dt.Rows[j]["cashAdvance"].ObjToDouble();
                                nonSplitPayment = 0D;
                                if (splitPayment == 0D)
                                {
                                    //                        splitPayment = totalPayment;
                                    //                        splitPayment = paymentAmount;
                                    //                        payment = 0D;
                                    splitPayment = payment - interest;
                                    if (downPayment > 0D)
                                        splitDownPayment = downPayment - cashAdvance;
                                    totalPayment = splitPayment + splitDownPayment;
                                    nonSplitPayment = totalPayment;
                                }
                                else
                                {
                                    //                        splitPayment = totalPayment * splitPayment;
                                    fraction = splitPayment;
                                    //                            splitPayment = paymentAmount * splitPayment;
                                    splitPayment = totalPayment * splitPayment;
                                    //payment = splitPayment;
                                    if (downPayment > 0D)
                                    {
                                        dpr = downPayment * fraction;
                                        if (downPayment > cashAdvance)
                                            dpr = (downPayment - cashAdvance) * fraction;
                                        dbc_5 = dbc_5 * fraction;
                                        //                                splitDownPayment = (downPayment - cashAdvance) * fraction;
                                        splitDownPayment = dpr;
                                    }
                                    payment = payment * fraction;
                                    downPayment = splitDownPayment;
                                    interest = interest * fraction;
                                    debit = debit * fraction;
                                    credit = credit * fraction;
                                    if (ccFee > 0D)
                                    {

                                    }
                                    ccFee = ccFee * fraction;
                                    contractValue = contractValue * fraction;
                                    cashAdvance = cashAdvance * fraction;
                                    //                        splitPayment = G1.RoundValue(splitPayment);
                                }
                                //                    downPayment = dt.Rows[j]["downPayment"].ObjToDouble();
                                //debit = dt.Rows[j]["debitAdjustment"].ObjToDouble();
                                //credit = dt.Rows[j]["creditAdjustment"].ObjToDouble();
                                //interest = dt.Rows[j]["interestPaid"].ObjToDouble();
                                dbrValue = dt.Rows[j]["Recap"].ObjToDouble();
                                reins = dt.Rows[j]["Reins"].ObjToDouble();
                                //dbc = dt.Rows[j]["dbc"].ObjToDouble();
                                commission = dt.Rows[j]["commission"].ObjToDouble();
                                if (splitCommission >= 99999.00D)
                                    splitCommission = 0D;
                                depositNumber = dt.Rows[j]["depositNumber"].ObjToString();
                                userId = dt.Rows[j]["userId"].ObjToString();
                                edited = dt.Rows[j]["edited"].ObjToString();

                                //                    totalPayment = downPayment + payment - debit + credit - interest + fbiMoney;
                                totalPayment = downPayment + payment - debit + credit - interest - dbc_5; // Add fbiMoney to commission but not total payments
                                if (contractValue == 0D)
                                    downPayment = 0D;
                                if (contract == "HC22001")
                                {

                                }
                                DataRow drow = lDt.NewRow();
                                drow["loc"] = location;
                                drow["agentNumber"] = agentCode;
                                //                        drow["agentNumber"] = actualAgent;
                                drow["agentName"] = lname;
                                drow["contractValue"] = contractValue;
                                drow["downPayment"] = downPayment;
                                drow["dpr"] = dpr;
                                drow["paymentAmount"] = payment;
                                drow["ccFee"] = ccFee;
                                drow["debitAdjustment"] = debit;
                                drow["creditAdjustment"] = credit;
                                drow["interestPaid"] = interest;
                                drow["totalPayments"] = totalPayment;
                                drow["cashAdvance"] = cashAdvance;
                                drow["Split Payment"] = splitPayment;
                                drow["Split DownPayment"] = splitDownPayment;
                                drow["fbi"] = fbi;
                                drow["fbiCommission"] = fbiMoney;
                                //drow["Recap"] = dbrValue;
                                //drow["Reins"] = reins;
                                drow["payDate8"] = G1.DTtoMySQLDT(date);
                                drow["contractNumber"] = contract;
                                drow["lastName"] = lastName;
                                drow["firstName"] = firstName;
                                splitCommission = G1.RoundValue(splitCommission);
                                drow["commission"] = splitCommission;
                                if (dbc > 0D)
                                    drow["dbc"] = dbc;
                                drow["dbc_5"] = dbc_5;
                                drow["depositNumber"] = depositNumber;
                                drow["userId"] = userId;
                                drow["edited"] = edited;
                                if (dbc > 0D)
                                    drow["commission"] = 0D;
                                if (percent <= 0D)
                                {
                                    if (nonSplitPayment > 0D)
                                        totalPayment = nonSplitPayment;
                                    if (totalPayment != 0D)
                                    {
                                        //if (fraction != 0D && cashAdvance > 0D && splitDownPayment > 0D )
                                        //    totalPayment = totalPayment - cashAdvance;
                                        percent = splitCommission / totalPayment;
                                    }
                                    percent = G1.RoundValue(percent);
                                    if (percent != 0.05D)
                                    {

                                    }
                                }
                                //percent = 0D;
                                drow["percentComm"] = percent;
                                lDt.Rows.Add(drow);
                                first = false;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("***ERROR*** i=" + i.ToString() + " j=" + j.ToString() + " AgentCode=" + agentCode + " " + ex.Message.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("A critical exception has occurred while attempting to run Commissions for AgentCode : " + agentCode + "\n" + ex.Message + "\n", "5% Commission Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        //MessageBox.Show("***ERROR*** i=" + i.ToString() + " j=" + j.ToString() + " AgentCode=" + agentCode + " " + ex.Message.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A critical exception has occurred while attempting to run Commissions for AgentCode : " + agentCode + "\n" + ex.Message, "5% Commission Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                //MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }

            FindContract(lDt, "HC22001");

            ProcessAllFBIsplits(dt, agentList, lDt );

            FindContract(lDt, "HC22001");
            for (int i = 0; i < lDt.Rows.Count; i++)
            {
                downPayment = lDt.Rows[i]["dpr"].ObjToDouble();
                if (downPayment > 0D)
                {
                    cashAdvance = lDt.Rows[i]["cashAdvance"].ObjToDouble();
                    if (cashAdvance > 0D)
                    {
                        totalPayment = lDt.Rows[i]["totalPayments"].ObjToDouble();
                        if (totalPayment == 0D)
                        {
                            lDt.Rows[i]["commission"] = 0D;
                        }
                    }
                }
            }

            //tempview = lDt.DefaultView;
            //tempview.Sort = "agentName asc, lastName asc, firstName asc";
            //lDt = tempview.ToTable();


            gridMain3.Columns["contractNumber"].Visible = true;
            if (G1.get_column_number(lDt, "percentComm") < 0)
                lDt.Columns.Add("percentComm", Type.GetType("System.Decimal"));

            FindContract(lDt, "FF19009L");

            //for ( int i=0; i<lDt.Rows.Count; i++)
            //{
            //    commission = lDt.Rows[i]["commission"].ObjToDouble();
            //    cashAdvance = lDt.Rows[i]["cashAdvance"].ObjToDouble();
            //    totalPayment = lDt.Rows[i]["totalPayments"].ObjToDouble();
            //    percent = 0D;
            //    if ( totalPayment != 0D)
            //        percent = commission / (totalPayment+cashAdvance);
            //    percent = G1.RoundValue(percent);
            //    lDt.Rows[i]["percentComm"] = percent;
            //}

            DailyHistory.AddAP(lDt);
            //DailyHistory.CleanupVisibility(gridMain3);
            gridMain3.Columns["dpp"].Visible = false;


            G1.NumberDataTable(lDt);
            dgv3.DataSource = lDt;
            gridMain3.Columns["agentName"].GroupIndex = 0;
            gridMain3.OptionsBehavior.AutoExpandAllGroups = true;
            gridMain3.ExpandAllGroups();
            gridMain3.OptionsPrint.ExpandAllGroups = true;
            gridMain3.OptionsPrint.PrintGroupFooter = true;

            gridMain3.Columns["Split Payment"].Visible = false; // Just for debugging
            gridMain3.Columns["Split DownPayment"].Visible = false; // Just for debugging
                                                                    //            gridMain3.Columns["interestPaid"].Visible = false; // Just for debugging
        }
        /****************************************************************************************/
        private void ProcessAllFBIsplits(DataTable dt, DataTable agentList, DataTable lDt )
        {
            try
            {
                int j = 0;
                double mainPercent = 0D;
                double newFBI = 0D;
                string splits = "";
                string agentCode = "";
                string actualAgent = "";
                double fbiCommission = 0D;
                string contract = "";
                double fbi = 0D;
                double fbiMoney = 0D;
                double money = 0D;
                string splitDone = "";
                DataRow[] dRows = null;

                lDt.Columns.Add("splitDone");

                for (j = 0; j < lDt.Rows.Count; j++)
                    lDt.Rows[j]["fbiCommission"] = 0D;

                for (int i = 0; i < agentList.Rows.Count; i++)
                {
                    try
                    {
                        fbiCommission = agentList.Rows[i]["fbiCommission"].ObjToDouble();
                        agentCode = agentList.Rows[i]["agentCode"].ObjToString();
                        mainPercent = agentList.Rows[i]["commission"].ObjToDouble();

                        if (agentCode == "XXX" || agentCode == "L15")
                        {
                        }
                        if (G1.get_column_number(dt, agentCode) < 0)
                            continue;
                        splits = agentList.Rows[i]["splits"].ObjToString();
                        for (j = 0; j < dt.Rows.Count; j++)
                        {
                            try
                            {
                                contract = dt.Rows[j]["contractNumber"].ObjToString();
                                if (String.IsNullOrWhiteSpace(contract))
                                    continue;
                                if (contract == "L24808" && agentCode == "L15")
                                {
                                }
                                actualAgent = dt.Rows[j]["agentNumber"].ObjToString();
                                fbi = dt.Rows[j]["fbi"].ObjToDouble();
                                //dt.Rows[j]["fbiMoney"] = 0D;
                                fbiMoney = 0D;
                                if (fbi > 0D)
                                {
                                    fbiMoney = fbiCommission * fbi;

                                    if (!String.IsNullOrWhiteSpace(splits))
                                        ProcessSplitFBI(contract, fbiMoney, agentCode, splits, mainPercent, lDt);
                                    else
                                    {
                                        dRows = lDt.Select("contractNumber='" + contract + "' AND `agentNumber` = '" + agentCode + "'");
                                        if (dRows.Length > 0)
                                        {
                                            splitDone = dRows[0]["splitDone"].ObjToString();
                                            if (splitDone != "Y")
                                            {
                                                money = dRows[0]["fbiCommission"].ObjToDouble();
                                                money += fbiMoney;
                                                dRows[0]["fbiCommission"] = money;
                                            }
                                        }

                                        //dt.Rows[j]["fbiCommission"] = fbiMoney;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("***ERROR*** i=" + i.ToString() + " j=" + j.ToString() + " AgentCode=" + agentCode + " " + ex.Message.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** i=" + i.ToString() + " j=" + j.ToString() + " AgentCode=" + agentCode + " " + ex.Message.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A critical exception has occurred while attempting to run Split Commissions :\n" + ex.Message + "\n", "5% Split Commission Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        /****************************************************************************************/
        private double ProcessSplitFBI ( string contractNumber, double fbi, string thisAgent, string splits, double mainPercent, DataTable dt )
        {
            string[] Lines = splits.Split('~');
            string agent = "";
            string str = "";
            double percent = 0D;
            double commission = 0D;
            double fbiMoney = 0D;
            double money = 0D;
            DataRow[] dRows = null;
            DataTable tempDt = null;

            for (int j = 0; j < Lines.Length; j = j + 2)
            {
                try
                {
                    agent = Lines[j].Trim();
                    if (String.IsNullOrWhiteSpace(agent))
                        continue;
                    if (agent == "L15")
                    {
                    }
                    str = Lines[j + 1].ObjToString();
                    if (G1.validate_numeric(str))
                    {
                        percent = str.ObjToDouble() / 100D;
                        fbiMoney = fbi * percent * 100D / mainPercent;
                        dRows = dt.Select("contractNumber='" + contractNumber + "' AND `agentNumber` = '" + agent + "'");
                        if ( dRows.Length > 0 )
                        {
                            money = dRows[0]["fbiCommission"].ObjToDouble();
                            money += fbiMoney;
                            dRows[0]["fbiCommission"] = money;
                            dRows[0]["splitDone"] = "Y";
                            if (contractNumber == "L24808")
                            {
                                tempDt = dRows.CopyToDataTable();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return fbiMoney;
        }
        /****************************************************************************************/
        private bool summaryPressed = false;
        private void chkSummarize_CheckedChanged(object sender, EventArgs e)
        {
            if (summaryPressed)
                summaryPressed = false;
            else
                summaryPressed = true;
            LoadTabAgents();
        }
        /****************************************************************************************/
        private DataTable SummarizeByAgentName(DataTable dt)
        {
            DataTable lDt = dt.Clone();
            string lastLocation = "";
            string lastAgent = "";
            string lastNumber = "";
            string location = "";
            string agent = "";
            string lloc = "";
            double contractValue = 0D;
            double downPayment = 0D;
            double payment = 0D;
            double debit = 0D;
            double credit = 0D;
            double interest = 0D;
            string agentNumber = "";
            DateTime date = DateTime.Now;
            DateTime newDate = DateTime.Now;
            bool dateChange = false;
            bool agentChange = false;
            bool dbr = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                agent = dt.Rows[i]["agentName"].ObjToString();
                location = dt.Rows[i]["loc"].ObjToString();
                agentNumber = dt.Rows[i]["agentNumber"].ObjToString();
                newDate = dt.Rows[i]["paydate8"].ObjToDateTime();

                dateChange = false;
                agentChange = false;

                if (String.IsNullOrWhiteSpace(lastLocation))
                {
                    lastLocation = location;
                    date = newDate;
                }
                if (String.IsNullOrWhiteSpace(lastAgent))
                {
                    lastAgent = agent;
                    lastNumber = agentNumber;
                    date = newDate;
                }
                if (chkMonthly.Checked)
                {
                    string oldDate = date.Year.ToString("D4") + date.Month.ToString("D2");
                    string nowDate = newDate.Year.ToString("D4") + newDate.Month.ToString("D2");
                    if (nowDate != oldDate)
                        dateChange = true;
                }
                if (agent != lastAgent)
                    agentChange = true;
                if (dateChange || agentChange)
                {
                    DataRow drow = lDt.NewRow();
                    drow["loc"] = lastLocation;
                    drow["agentNumber"] = lastNumber;
                    drow["agentName"] = lastAgent;
                    lDt.Rows.Add(drow);
                    drow["downPayment"] = downPayment;
                    drow["contractValue"] = contractValue;
                    drow["paymentAmount"] = payment;
                    drow["totalPayments"] = downPayment + payment + credit - debit - interest;
                    drow["payDate8"] = G1.DTtoMySQLDT(date);
                    contractValue = 0D;
                    payment = 0D;
                    downPayment = 0D;
                    debit = 0D;
                    credit = 0D;
                    interest = 0D;
                    lastAgent = agent;
                    lastNumber = agentNumber;
                    date = newDate;
                }
                dbr = false;
                if (dt.Rows[i]["DBR"].ObjToString().ToUpper() == "DBR")
                    dbr = true;
                if (!dbr)
                {
                    contractValue += dt.Rows[i]["contractValue"].ObjToDouble();

                    payment += dt.Rows[i]["paymentAmount"].ObjToDouble();

                    downPayment += dt.Rows[i]["downPayment"].ObjToDouble();
                    debit += dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    credit += dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    interest += dt.Rows[i]["interestPaid"].ObjToDouble();
                }
            }
            DataRow ddr = lDt.NewRow();
            ddr["loc"] = lastLocation;
            ddr["agentNumber"] = agentNumber;
            ddr["agentName"] = agent;
            ddr["downPayment"] = downPayment;
            ddr["contractValue"] = contractValue;
            ddr["paymentAmount"] = payment;
            ddr["totalPayments"] = downPayment + payment + credit - debit - interest;
            ddr["payDate8"] = G1.DTtoMySQLDT(date);
            lDt.Rows.Add(ddr);
            return lDt;
        }
        /****************************************************************************************/
        private void CheckForLapse(DataTable dt)
        {
            double contractValue = 0D;
            double recapAmount = 0D;
            double reinsAmount = 0D;
            string agent = "";
            string agentName = "";
            string contract = "";
            string name = "";
            string miniContract = "";
            string trust = "";
            string loc = "";
            string lapseDate8 = "";
            string issueDate8 = "";
            string reinstateDate8 = "";

            DateTime lapseDate = this.dateTimePicker1.Value;
            lapseDate = lapseDate.AddMonths(1);
            string start = lapseDate.Year.ToString("D4") + "-" + lapseDate.Month.ToString("D2") + "-1";

            lapseDate = this.dateTimePicker2.Value;
            lapseDate = lapseDate.AddMonths(1);
            int days = DateTime.DaysInMonth(lapseDate.Year, lapseDate.Month);
            string end = lapseDate.Year.ToString("D4") + "-" + lapseDate.Month.ToString("D2") + "-" + lapseDate.Day.ToString("D2");

            string cmd = "Select * from `contracts` where `lapsedate8` >= '" + start + "' AND `lapsedate8` <= '" + end + "';";
            //DataTable lapseDt = G1.get_db_data(cmd);

            string where = "";
            DataTable dt9 = (DataTable)dgv9.DataSource;
            DataTable dx = dt9.Copy();
            dx.Columns.Add("Where");
            dx.Columns.Add("Done");
            for (int i = 0; i < dx.Rows.Count; i++)
                dx.Rows[i]["Where"] = "9";

            DataTable dt8 = (DataTable)dgv8.DataSource;
            for (int i = 0; i < dt8.Rows.Count; i++)
                dx.ImportRow(dt8.Rows[i]);

            DataTable lapseDt = dx.Copy();

            if (G1.get_column_number(lapseDt, "loc") < 0)
                lapseDt.Columns.Add("loc");
            for (int i = 0; i < lapseDt.Rows.Count; i++)
            {
                contract = lapseDt.Rows[i]["contractNumber"].ObjToString();
                miniContract = decodeContractNumber(contract, ref trust, ref loc);
                lapseDt.Rows[i]["loc"] = loc;
            }

            DataView tempview = lapseDt.DefaultView;
            tempview.Sort = "loc asc";
            lapseDt = tempview.ToTable();

            for (int i = 0; i < lapseDt.Rows.Count; i++)
            {
                contractValue = DailyHistory.GetContractValue(lapseDt.Rows[i]);
                contractValue = G1.RoundValue(contractValue);
                contract = lapseDt.Rows[i]["contractNumber"].ObjToString();
                loc = lapseDt.Rows[i].ObjToString();
                agent = GetContractAgent(contract);
                agentName = GetAgentName(agent);
                if (String.IsNullOrWhiteSpace(agentName))
                    continue;
                recapAmount = lapseDt.Rows[i]["Recap"].ObjToDouble();
                reinsAmount = lapseDt.Rows[i]["Reins"].ObjToDouble();
                lapseDate8 = G1.GetSQLDate(lapseDt, i, "lapseDate8");
                issueDate8 = G1.GetSQLDate(lapseDt, i, "issueDate8");
                reinstateDate8 = G1.GetSQLDate(lapseDt, i, "reinstateDate8");
                where = lapseDt.Rows[i]["where"].ObjToString();
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    name = dt.Rows[j]["agentName"].ObjToString();
                    if (name == agentName)
                    {
                        DataRow dRow = dt.NewRow();
                        dRow["loc"] = loc;
                        dRow["agentNumber"] = agent;
                        dRow["agentName"] = name;
                        if (where == "9")
                        {
                            dRow["contractNumber"] = "   " + contract + " (R" + issueDate8 + ")";
                            reinsAmount = G1.RoundValue(reinsAmount);
                            dRow["Reins"] = reinsAmount;
                        }
                        else
                        {
                            dRow["contractNumber"] = "   " + contract + " (L" + issueDate8 + ")";
                            recapAmount = G1.RoundValue(recapAmount);
                            dRow["recapAmount"] = recapAmount;
                        }
                        dt.Rows.InsertAt(dRow, j + 1);
                        break;
                    }
                }
            }
        }
        /****************************************************************************************/
        public static DataTable RunLapseReport(ref DateTime d1, ref DateTime d2)
        {
            //DateTime now = DateTime.Now;
            //DateTime lastMonth = now.AddMonths(-2);
            //int days = DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month);
            //DateTime date11 = new DateTime(lastMonth.Year, 3, 27);
            //string date1 = G1.DateTimeToSQLDateTime(date11);
            //DateTime date22 = new DateTime(lastMonth.Year, lastMonth.Month, days);
            //string date2 = G1.DateTimeToSQLDateTime(date22);

            DateTime now = DateTime.Now;
            now = now.AddMonths(-2);
            now = new DateTime(now.Year, now.Month, 1);
            d1 = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            d2 = new DateTime(now.Year, now.Month, days);

            string date1 = G1.DateTimeToSQLDateTime(d1);
            string date2 = G1.DateTimeToSQLDateTime(d2);

            string paidDate = "`payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' ";
            string cmd = "Select p.*,d.*,a.`firstName`,a.`lastName` from `payments` p LEFT JOIN `contracts` d on p.`contractNumber` = d.`contractNumber` LEFT JOIN `agents` a ON p.`agentNumber` = a.`agentCode` ";
            cmd += " WHERE ";
            cmd += paidDate;
            DataTable dt = G1.get_db_data(cmd);
            DataTable dt8 = CheckForMainLapse(dt, d1, d2);
            return dt8;
        }
        /****************************************************************************************/
        public static DataTable CheckForMainLapse(DataTable dt, DateTime date1, DateTime date2)
        {
            if (G1.get_column_number(dt, "Recap") < 0)
                dt.Columns.Add("Recap", Type.GetType("System.Decimal"));
            if (G1.get_column_number(dt, "RecapContracts") < 0)
                dt.Columns.Add("RecapContracts", Type.GetType("System.Decimal"));

            DateTime issueDate;

            if (date1 == date2)
            {
                DateTime lapseDate = date1;
                int days = lapseDate.Day;
                if (date1 == date2)
                    days = 1;
                string start = lapseDate.Year.ToString("D4") + "-" + lapseDate.Month.ToString("D2") + "-" + days.ToString("D2");
                date1 = start.ObjToDateTime();

                lapseDate = date2;
                if (date1 == date2)
                    days = DateTime.DaysInMonth(lapseDate.Year, lapseDate.Month);
                string end = lapseDate.Year.ToString("D4") + "-" + lapseDate.Month.ToString("D2") + "-" + days.ToString("D2");
                date2 = end.ObjToDateTime();
            }

            DataTable dt8 = CalculateLapses(dt, date1, date2);

            DataView tempview = dt8.DefaultView;
            tempview.Sort = "agentName asc, loc asc";
            dt8 = tempview.ToTable();
            string agent = "";
            string agentName = "";
            double commission = 0D;
            string formula = "";
            string splits = "";
            double percent = 0D;
            double goal = 0D;
            double totalContracts = 0D;
            double recaps = 0D;
            double recapContracts = 0D;
            double dbrSales = 0D;
            AddColumnToTable(dt8, "commission", "System.Double");
            AddColumnToTable(dt8, "totalContracts", "System.Double");
            AddColumnToTable(dt8, "lapseRecaps", "System.Double");
            AddColumnToTable(dt8, "dbrSales", "System.Double");

            DateTime lapseDate8 = DateTime.Now;
            string contractNumber = "";

            for (int i = 0; i < dt8.Rows.Count; i++)
            {
                contractNumber = dt8.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "M14130UI")
                {
                }
                agent = dt8.Rows[i]["agentNumber"].ObjToString();
                agentName = dt8.Rows[i]["agentName"].ObjToString();
                issueDate = dt8.Rows[i]["issueDate8"].ObjToDateTime();
                lapseDate8 = dt8.Rows[i]["lapseDate8"].ObjToDateTime();
                formula = dt8.Rows[i]["formula"].ObjToString();
                splits = dt8.Rows[i]["splits"].ObjToString();
                percent = dt8.Rows[i]["percent"].ObjToDouble();
                goal = dt8.Rows[i]["goal"].ObjToDouble();

                if (agentName.ToUpper() == "HAROLD LAIRD")
                {
                    if (issueDate.Year == 2015 && issueDate.Month == 7 && formula == "M")
                    {

                    }
                }
                commission = CalculatePastCommissions("lapseDate8", agent, formula, splits, percent, goal, lapseDate8, issueDate, issueDate, ref totalContracts, ref recaps, ref recapContracts, ref dbrSales);
                dt8.Rows[i]["commission"] = commission;
                dt8.Rows[i]["totalContracts"] = totalContracts;
                dt8.Rows[i]["lapseRecaps"] = recaps;
                dt8.Rows[i]["dbrSales"] = dbrSales;
            }
            dt8.Columns.Add("LLoc");
            for (int i = 0; i < dt8.Rows.Count; i++)
                dt8.Rows[i]["LLoc"] = "M";
            int maxRows = dt8.Rows.Count;
            //dt8 = CalculatePastAgents(dt8);
            maxRows = dt8.Rows.Count;
            return dt8;
        }
        /****************************************************************************************/
        public static void AdjustRecaps(DataTable dt, DataTable dt8)
        {
            string contractNumber = "";
            string contract = "";
            double totalContracts = 0D;
            double contractValue = 0D;
            double goal = 0D;
            double recap = 0D;
            double value = 0D;
            for (int i = 0; i < dt8.Rows.Count; i++)
            {
                contractNumber = dt8.Rows[i]["contractNumber"].ObjToString();
                totalContracts = dt8.Rows[i]["totalContracts"].ObjToDouble();
                if (contractNumber.IndexOf("C13197UI") >= 0)
                {

                }
                contractValue = dt8.Rows[i]["contractValue"].ObjToDouble();
                goal = dt8.Rows[i]["goal"].ObjToDouble();
                recap = dt8.Rows[i]["Recap"].ObjToDouble();
                if ((totalContracts - contractValue) < goal)
                {
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        contract = dt.Rows[j]["contractNumber"].ObjToString();
                        if (contract == contractNumber)
                        {
                            value = dt.Rows[j]["Recap"].ObjToDouble();
                            if (value == recap)
                            {
                                dt.Rows[j]["Recap"] = 0D;
                                break;
                            }
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
        public static DataTable CalculatePastFormulas(DataTable dt8, string what = "")
        {
            DataTable ttDt = null;
            string tDate = "";
            string ttDate = "";
            DataRow[] dddR = null;
            string agent = "";
            string formula = "";
            DateTime eDate = DateTime.Now;
            string cmd = "";
            string contract = "";
            string miniContract = "";
            string trust = "";
            string loc = "";
            string location = "";
            string issueDate = "";
            DateTime iDate = DateTime.Now;
            DateTime lapseDate8 = DateTime.Now;
            string yearMonth = "";
            double contractValue = 0D;
            string splits = "";
            double percent = 0D;
            string goal = "";
            string aGoal = "";
            string formulaAgent = "";
            double dGoal = 0D;
            int count = 0;
            string[,] calc = new string[100, 2];
            bool processed = false;
            string[] Lines = null;
            int row = 0;
            DataRow[] dR = null;
            double dbrSales = 0D;
            double commission = 0D;
            double totalContracts = 0D;
            double recapContracts = 0D;
            double recaps = 0D;
            string lastAgent = "";
            DateTime lastEdate = DateTime.Now;

            DataTable contractDt = dt8.Clone();

            cmd = "SELECT * FROM `goals` g JOIN `agents` a ON g.`agentCode` = a.`agentCode` WHERE  g.`type` = 'GOAL' AND g.`formula` <> '' ORDER BY g.`agentCode`,g.`effectiveDate` DESC;";
            DataTable agentsDt = G1.get_db_data(cmd);

            for (int i = 0; i < agentsDt.Rows.Count; i++)
            {
                try
                {
                    agent = agentsDt.Rows[i]["agentCode"].ObjToString();
                    if (agent == "L15")
                    {
                    }
                    if (agent != lastAgent)
                    {
                        lastEdate = DateTime.Now;
                        lastEdate = lastEdate.AddMonths(1);
                        lastAgent = agent;
                    }
                    eDate = agentsDt.Rows[i]["effectiveDate"].ObjToDateTime();
                    formula = agentsDt.Rows[i]["formula"].ObjToString();
                    percent = agentsDt.Rows[i]["percent"].ObjToDouble();
                    if (percent >= 1D)
                        percent = percent / 100D;
                    if (percent <= 0D)
                    {
                        lastEdate = eDate;
                        continue;
                    }
                    tDate = eDate.Year.ToString("D4") + eDate.Month.ToString("D2");
                    ttDate = lastEdate.Year.ToString("D4") + lastEdate.Month.ToString("D2");
                    dddR = dt8.Select("YearMonth >= '" + tDate + "' AND YearMonth < '" + ttDate + "'");
                    lastEdate = eDate;
                    if (dddR.Length <= 0)
                        continue;
                    ttDt = contractDt.Clone();
                    G1.ConvertToTable(dddR, ttDt);
                    for (int k = 0; k < ttDt.Rows.Count; k++)
                    {
                        contract = ttDt.Rows[k]["contractNumber"].ObjToString();
                        if (contract == "M14130UI" && agent == "V25")
                        {
                        }
                        miniContract = decodeContractNumber(contract, ref trust, ref loc);
                        issueDate = G1.GetSQLDate(ttDt, k, "issueDate8");
                        iDate = issueDate.ObjToDateTime();
                        lapseDate8 = ttDt.Rows[k]["lapseDate8"].ObjToDateTime();
                        yearMonth = iDate.Year.ToString("D4") + iDate.Month.ToString("D2");

                        dddR = dt8.Select("contractNumber='" + contract + "' AND agentNumber='" + agent + "' AND YearMonth='" + yearMonth + "'");
                        if (dddR.Length > 0)
                            continue;

                        contractValue = DailyHistory.GetContractValue(contract);
                        contractValue = G1.RoundValue(contractValue);

                        count = Commission.ParseOutFormula(formulaAgent, dGoal, percent, formula, ref calc);
                        count = CondenseFormula(count, ref calc);
                        processed = false;
                        for (int l = 0; l < count; l++)
                        {
                            location = calc[l, 0];
                            aGoal = calc[l, 1];
                            if (G1.validate_numeric(aGoal))
                            {
                                goal = aGoal;
                                dGoal = aGoal.ObjToDouble();
                            }
                            else
                            {
                                goal = agentsDt.Rows[i]["goal1"].ObjToString();
                                dGoal = agentsDt.Rows[i]["goal1"].ObjToDouble();
                            }
                            location = location.Replace(",", "+");
                            Lines = location.Split('+');
                            for (int m = 0; m < Lines.Length; m++)
                            {
                                location = Lines[m];
                                if (!String.IsNullOrWhiteSpace(location))
                                {
                                    // if (loc.Contains(location))
                                    if (loc == location)
                                    {
                                        G1.copy_dt_row(ttDt, k, contractDt, contractDt.Rows.Count);
                                        row = contractDt.Rows.Count - 1;
                                        contractDt.Rows[row]["agentNumber"] = agent;
                                        dR = agentsDt.Select("agentCode='" + agent + "'");
                                        if (dR.Length > 0)
                                            contractDt.Rows[row]["agentName"] = dR[0]["firstName"].ObjToString().Trim() + " " + dR[0]["lastName"].ObjToString().Trim();
                                        else
                                            contractDt.Rows[row]["agentName"] = agent;
                                        contractDt.Rows[row]["formula"] = formula;
                                        contractDt.Rows[row]["percent"] = percent;
                                        contractDt.Rows[row]["goal"] = goal;
                                        if (yearMonth == "201507" && location == "M")
                                        {

                                        }
                                        if (agent == "V25" && location == "L")
                                        {
                                        }
                                        commission = CalculatePastCommissions(what, agent, location, splits, percent, dGoal, lapseDate8, iDate, iDate, ref totalContracts, ref recaps, ref recapContracts, ref dbrSales);
                                        contractDt.Rows[row]["dbrSales"] = dbrSales;
                                        contractDt.Rows[row]["totalContracts"] = totalContracts;
                                        contractDt.Rows[row]["commission"] = commission;
                                        contractDt.Rows[row]["RecapContracts"] = recapContracts;

                                        if (what.ToUpper() == "LAPSEDATE8")
                                            contractDt.Rows[row]["Recap"] = contractValue * percent;
                                        else
                                            contractDt.Rows[row]["Reins"] = contractValue * percent;

                                        //if (what.ToUpper() == "LAPSEDATE8")
                                        //    contractDt.Rows[row]["Recap"] = recaps;
                                        //else
                                        //    contractDt.Rows[row]["Reins"] = recaps;
                                        processed = true;
                                        break;
                                    }
                                }
                            }
                            //if (processed)
                            //    break;
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            row = dt8.Rows.Count;
            for (int i = 0; i < contractDt.Rows.Count; i++)
            {
                contract = contractDt.Rows[i]["contractNumber"].ObjToString();
                agent = contractDt.Rows[i]["agentNumber"].ObjToString();
                if (contract == "L17045UI" && agent == "V25")
                {
                }
                contractDt.Rows[i]["LLoc"] = "L";
                dt8.ImportRow(contractDt.Rows[i]);
            }
            return dt8;
        }
        /****************************************************************************************/
        public static DataTable CalculatePastAgents(DataTable dt8)
        {
            DataTable allAgentsDt = G1.get_db_data("Select * from `agents`;");
            string agent = "";
            string contract = "";
            string miniContract = "";
            string trust = "";
            string loc = "";
            string location = "";
            string issueDate = "";
            DateTime iDate;
            string yearMonth = "";
            double contractValue = 0D;
            double downPayment = 0D;
            bool found = false;
            double percent = 0D;
            string formula = "";
            string splits = "";
            string goal = "";
            double dGoal = 0D;
            string formulaAgent = "";
            string[,] calc = new string[100, 2];
            double commission = 0D;
            double totalContracts = 0D;
            double recaps = 0D;
            double recapContracts = 0D;
            double dbrSales = 0D;
            bool processed = false;
            int count = 0;
            int row = 0;
            string aGoal = "";
            string[] Lines = null;
            DateTime lapseDate8 = DateTime.Now;
            DataTable agentDt = dt8.Clone();
            DataTable contractDt = dt8.Clone();
            DataTable copyDt8 = dt8.Copy();
            for (int i = 0; i < allAgentsDt.Rows.Count; i++)
            {
                agent = allAgentsDt.Rows[i]["agentCode"].ObjToString();
                //                agent = "N30";
                //if (agent != "N30")
                //    continue;
                try
                {
                    DataRow[] dR = copyDt8.Select("agentNumber='" + agent + "'");
                    if (dR.Length <= 0)
                        continue;
                    agentDt.Rows.Clear();
                    G1.ConvertToTable(dR, agentDt);
                    for (int j = 0; j < agentDt.Rows.Count; j++)
                    {
                        contract = agentDt.Rows[j]["contractNumber"].ObjToString();
                        if (contract == "M16137UI")
                        {
                        }
                        miniContract = decodeContractNumber(contract, ref trust, ref loc);
                        issueDate = G1.GetSQLDate(agentDt, j, "issueDate8");
                        iDate = issueDate.ObjToDateTime();
                        lapseDate8 = agentDt.Rows[j]["lapseDate8"].ObjToDateTime();
                        yearMonth = iDate.Year.ToString("D4") + iDate.Month.ToString("D2");

                        //                    agent = dt8.Rows[j]["agentCode"].ObjToString();
                        contractValue = DailyHistory.GetContractValue(contract);
                        contractValue = G1.RoundValue(contractValue);
                        //downPayment = dt8.Rows[j]["downPayment"].ObjToDouble();
                        //downPayment = G1.RoundValue(downPayment);
                        found = GetLapseAgent(agent, iDate, ref percent, ref goal, ref formula, ref splits);
                        double lapseAmount = DailyHistory.ProcessLapseAmount(false, agent, contractValue, percent, splits);
                        for (int k = 0; k < allAgentsDt.Rows.Count; k++)
                        {
                            formulaAgent = allAgentsDt.Rows[k]["agentCode"].ObjToString();
                            if (formulaAgent == agent)
                                continue;
                            if (formulaAgent == "V25")
                            {
                            }
                            //    continue;
                            //                        formulaAgent = "N07";
                            found = GetLapseAgent(formulaAgent, iDate, ref percent, ref goal, ref formula, ref splits);
                            if (!found)
                                continue;
                            if (String.IsNullOrWhiteSpace(formula))
                                continue;
                            if (!G1.validate_numeric(goal))
                                continue;
                            dGoal = goal.ObjToDouble();
                            count = Commission.ParseOutFormula(formulaAgent, dGoal, percent, formula, ref calc);
                            count = CondenseFormula(count, ref calc);
                            //                            count = Commission.ParseFormula(formula, ref calc);
                            processed = false;
                            for (int l = 0; l < count; l++)
                            {
                                location = calc[l, 0];
                                aGoal = calc[l, 1];
                                if (G1.validate_numeric(aGoal))
                                {
                                    goal = aGoal;
                                    dGoal = aGoal.ObjToDouble();
                                }
                                Lines = location.Split('+');
                                for (int m = 0; m < Lines.Length; m++)
                                {
                                    location = Lines[m];
                                    if (!String.IsNullOrWhiteSpace(location))
                                    {
                                        if (loc.Contains(location))
                                        {
                                            contractDt.Rows.Clear();
                                            G1.copy_dt_row(agentDt, j, dt8, dt8.Rows.Count);
                                            row = dt8.Rows.Count - 1;
                                            dt8.Rows[row]["agentNumber"] = formulaAgent;
                                            dR = allAgentsDt.Select("agentCode='" + formulaAgent + "'");
                                            if (dR.Length > 0)
                                                dt8.Rows[row]["agentName"] = dR[0]["firstName"].ObjToString().Trim() + " " + dR[0]["lastName"].ObjToString().Trim();
                                            else
                                                dt8.Rows[row]["agentName"] = formulaAgent;
                                            dt8.Rows[row]["formula"] = formula;
                                            dt8.Rows[row]["percent"] = percent;
                                            dt8.Rows[row]["goal"] = goal;
                                            commission = CalculatePastCommissions("lapseDate8", formulaAgent, location, splits, percent, dGoal, lapseDate8, iDate, iDate, ref totalContracts, ref recaps, ref recapContracts, ref dbrSales);
                                            dt8.Rows[row]["dbrSales"] = dbrSales;
                                            dt8.Rows[row]["totalContracts"] = totalContracts;
                                            dt8.Rows[row]["commission"] = commission;
                                            dt8.Rows[row]["RecapContracts"] = recapContracts;
                                            dt8.Rows[row]["Recap"] = recaps;
                                            processed = true;
                                            break;
                                        }
                                    }
                                }
                                if (processed)
                                    break;
                            }

                            //commission = CalculatePastCommissions("lapseDate8", formulaAgent, formula, splits, percent, dGoal, lapseDate8, iDate, iDate, ref totalContracts, ref recaps, ref recapContracts, ref dbrSales);
                            //Commission.ParseOutFormula(formulaAgent, dGoal, percent, formula, ref calc);
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return dt8;
        }
        /****************************************************************************************/
        public static int CondenseFormula(int count, ref string[,] calc)
        {
            string[,] newCalc = new string[100, 2];
            int newCount = 0;
            string left = "";
            string right = "";
            bool Switch = false;
            string formula = "";
            string delimiter = "";
            for (int i = 0; i < count; i++)
            {
                formula = calc[i, 0];
                delimiter = calc[i, 1];
                if (formula == "=" || delimiter == "=" ||
                     formula == ">" || delimiter == ">")
                {
                    Switch = true;
                    if (delimiter == "=" || delimiter == ">")
                        left += formula;
                    continue;
                }
                else
                {
                    if (Switch)
                    {
                        right += formula + delimiter;
                        left = left.Replace("(", "");
                        left = left.Replace(")", "");
                        left = left.Replace("+", ",");
                        newCalc[newCount, 0] = left;
                        right = right.Replace("+", "");
                        right = right.Replace(",", "");
                        newCalc[newCount, 1] = right;
                        newCount++;
                        left = "";
                        right = "";
                        Switch = false;
                    }
                    else
                        left += formula + delimiter;
                }
            }
            if (newCount == 0 && !String.IsNullOrWhiteSpace(left))
            {
                newCalc[0, 0] = left;
                newCalc[0, 1] = "";
                newCount++;
            }
            for (int i = 0; i < newCount; i++)
            {
                calc[i, 0] = newCalc[i, 0];
                calc[i, 1] = newCalc[i, 1];
            }
            return newCount;
        }
        /****************************************************************************************/
        //private DataTable LoadNewContracts ( DataTable dt, DateTime date1, DateTime date2 )
        //{
        //    AddColumnToTable(dt, "num");
        //    AddColumnToTable(dt, "customer");
        //    AddColumnToTable(dt, "agentName");
        //    AddColumnToTable(dt, "Location Name");
        //    AddColumnToTable(dt, "totalPayments", "System.Double");
        //    AddColumnToTable(dt, "commission", "System.Double");
        //    AddColumnToTable(dt, "contractValue", "System.Double");
        //    AddColumnToTable(dt, "ibtrust", "System.Double");
        //    AddColumnToTable(dt, "sptrust", "System.Double");
        //    AddColumnToTable(dt, "xxtrust", "System.Double");
        //    AddColumnToTable(dt, "dbr");

        //    DateTime lapseDate = date1;
        //    int days = lapseDate.Day;
        //    if (date1 == date2)
        //        days = 1;
        //    string start = lapseDate.Year.ToString("D4") + "-" + lapseDate.Month.ToString("D2") + "-" + days.ToString("D2");

        //    lapseDate = date2;
        //    if (date1 == date2)
        //        days = DateTime.DaysInMonth(lapseDate.Year, lapseDate.Month);
        //    string end = lapseDate.Year.ToString("D4") + "-" + lapseDate.Month.ToString("D2") + "-" + days.ToString("D2");

        //    string cmd = "Select * from `contracts` c JOIN `customers` a ON c.`contractNumber` = a.`ContractNumber` where `issueDate8` >= '" + start + "' AND `issueDate8` <= '" + end + "' ";
        //    cmd += ";";
        //    DataTable cDt = G1.get_db_data(cmd);

        //    CalculateNewContracts(dt, date1, date2);

        //    DataTable dt8 = CheckForMainLapse(dt, date1, date2);

        //    AddColumnToTable(cDt, "num");
        //    AddColumnToTable(cDt, "customer");
        //    AddColumnToTable(cDt, "agentName");
        //    AddColumnToTable(cDt, "Location Name");
        //    AddColumnToTable(cDt, "totalPayments", "System.Double");
        //    AddColumnToTable(cDt, "commission", "System.Double");
        //    AddColumnToTable(cDt, "contractValue", "System.Double");
        //    AddColumnToTable(cDt, "ibtrust", "System.Double");
        //    AddColumnToTable(cDt, "sptrust", "System.Double");
        //    AddColumnToTable(cDt, "xxtrust", "System.Double");
        //    AddColumnToTable(cDt, "dbr");

        //    double contractValue = 0D;
        //    double dbrSales = 0D;
        //    for (int i = 0; i < cDt.Rows.Count; i++)
        //    {
        //        contractValue = DailyHistory.GetContractValue(cDt.Rows[i]);
        //        if (!Commission.CheckDeathDateCommission(cDt, i))
        //            dbrSales += contractValue;
        //        cDt.Rows[i]["contractValue"] = contractValue;
        //        dt.ImportRow(cDt.Rows[i]);
        //    }

        //    return dt;
        //}
        /****************************************************************************************/
        public static Double CalculatePastCommissions(string what, string agent, string formula, string splits, double percent, double goal, DateTime lapseDate8, DateTime date1, DateTime date2, ref Double totalContracts, ref double recaps, ref double recapContracts, ref double dbrSales)
        {
            double commission = 0D;
            string contract = "";
            string trust = "";
            string loc = "";
            recaps = 0D;
            dbrSales = 0D;
            DateTime pastDate = date1;
            int days = pastDate.Day;
            if (date1 == date2)
                days = 1;
            string start = pastDate.Year.ToString("D4") + "-" + pastDate.Month.ToString("D2") + "-" + days.ToString("D2");

            pastDate = date2;
            if (date1 == date2)
                days = DateTime.DaysInMonth(pastDate.Year, pastDate.Month);
            string end = pastDate.Year.ToString("D4") + "-" + pastDate.Month.ToString("D2") + "-" + days.ToString("D2");

            bool doContracts = false;
            if (date1 == date2)
                doContracts = true;

            bool doLocation = false;

            date1 = start.ObjToDateTime();
            date2 = end.ObjToDateTime();

            bool stop = false;
            if (!String.IsNullOrWhiteSpace(formula))
                stop = true;

            if (String.IsNullOrWhiteSpace(formula))
                formula = "('" + agent + "')";
            else
            {
                string[] Lines = formula.Split('+');
                formula = "(";
                for (int i = 0; i < Lines.Length; i++)
                {
                    if (!isAgent(Lines[i].Trim(), allAgentsDt))
                        doLocation = true;
                    formula += "'" + Lines[i] + "',";
                }
                formula = formula.TrimEnd(',');
                formula += ")";
            }

            //            string paidDate = "`payDate8` >= '" + start + "' and `payDate8` <= '" + end + "' ";
            string paidDate = "`issueDate8` >= '" + start + "' and `issueDate8` <= '" + end + "' ";

            string cmd = "Select p.*,d.*,a.`firstName`,a.`lastName` from `payments` p LEFT JOIN `contracts` d on p.`contractNumber` = d.`contractNumber` LEFT JOIN `agents` a ON p.`agentNumber` = a.`agentCode` ";
            cmd += " WHERE ";
            cmd += paidDate;

            cmd = "Select * from `contracts` c JOIN `customers` a ON c.`contractNumber` = a.`ContractNumber` where `issueDate8` >= '" + start + "' AND `issueDate8` <= '" + end + "' ";

            if (!doLocation)
                cmd += " AND `agentCode` IN " + formula + ";";

            DataTable cDt = G1.get_db_data(cmd);

            if (G1.get_column_number(cDt, "loc") < 0)
                cDt.Columns.Add("loc");

            if (doLocation)
            {
                for (int i = 0; i < cDt.Rows.Count; i++)
                {
                    contract = cDt.Rows[i]["contractNumber"].ObjToString();
                    contract = Trust85.decodeContractNumber(contract, ref trust, ref loc);
                    cDt.Rows[i]["loc"] = loc;
                }
                DataRow[] dRows = cDt.Select("loc IN " + formula);
                DataTable xDt = cDt.Clone();
                G1.ConvertToTable(dRows, xDt);
                cDt = xDt.Copy();
            }


            //if ( doContracts )
            //    LoadPaidContracts(cDt, date1, date2);

            double contractValue = 0D;
            totalContracts = 0D;
            double downPayment = 0D;
            //DataTable tdt = cDt.Clone();
            //for (int i = 0; i < cDt.Rows.Count; i++)
            //{
            //    downPayment = cDt.Rows[i]["downPayment"].ObjToDouble();
            //    if (downPayment != 0D)
            //    {
            //        contractValue = DailyHistory.GetContractValue(cDt.Rows[i]);
            //        if (!Commission.CheckDeathDateCommission(cDt, i))
            //            dbrSales += contractValue;
            //        totalContracts += contractValue;
            //        tdt.ImportRow(cDt.Rows[i]);
            //    }
            //}
            //if ( stop )
            //{

            //}

            double recap = 0D;
            double recapCon = 0D;
            DataTable dt8 = CalculateLapses(cDt, date1, date2, formula);
            if (what.ToUpper() != "REINSTATEDATE8")
            {
                for (int i = 0; i < dt8.Rows.Count; i++)
                {
                    recap += dt8.Rows[i]["Recap"].ObjToDouble();
                }
            }
            contract = "";
            recap = 0D;
            DateTime lapseDate = DateTime.Now;
            DataTable tdt = cDt.Clone();
            for (int i = 0; i < cDt.Rows.Count; i++)
            {
                contract = cDt.Rows[i]["contractNumber"].ObjToString();
                if (contract.IndexOf("M14130UI") >= 0)
                {

                }
                lapseDate = cDt.Rows[i][what].ObjToDateTime();
                downPayment = cDt.Rows[i]["downPayment"].ObjToDouble();
                contractValue = DailyHistory.GetContractValue(cDt.Rows[i]);
                if (!Commission.CheckDeathDateCommission(cDt, i, date1, date2))
                    dbrSales += contractValue;
                else
                {
                    if (lapseDate.Year > 1975)
                    {
                        if (lapseDate < lapseDate8)
                        {
                            cDt.Rows[i]["recapContracts"] = contractValue;
                            cDt.Rows[i]["recap"] = contractValue * percent;
                            recap += contractValue;
                        }
                    }
                }
                totalContracts += contractValue;
                tdt.ImportRow(cDt.Rows[i]);
            }

            commission = (totalContracts - dbrSales) * percent;
            recap = recap * percent;
            commission = commission - recap;

            recaps = recap;
            recapContracts = recapCon;

            return commission;
        }
        /****************************************************************************************/
        public static DataTable CalculateLapses(DataTable dt, DateTime date1, DateTime date2, string agentFormula = "")
        {
            if (G1.get_column_number(dt, "Recap") < 0)
                dt.Columns.Add("Recap", Type.GetType("System.Decimal"));
            if (G1.get_column_number(dt, "RecapContracts") < 0)
                dt.Columns.Add("RecapContracts", Type.GetType("System.Decimal"));

            DateTime lapseDate = date1;
            int days = lapseDate.Day;
            if (date1 == date2)
                days = 1;
            lapseDate = lapseDate.AddMonths(1);
            string start = lapseDate.Year.ToString("D4") + "-" + lapseDate.Month.ToString("D2") + "-" + days.ToString("D2");

            lapseDate = date2;
            lapseDate = lapseDate.AddMonths(1);
            //            if (date1 == date2)
            days = DateTime.DaysInMonth(lapseDate.Year, lapseDate.Month);
            string end = lapseDate.Year.ToString("D4") + "-" + lapseDate.Month.ToString("D2") + "-" + days.ToString("D2");

            //start = date1.Year.ToString("D4") + "-" + date1.Month.ToString("D2") + "-" + date1.Day.ToString("D2");
            //end = date2.Year.ToString("D4") + "-" + date2.Month.ToString("D2") + "-" + date2.Day.ToString("D2");

            string cmd = "Select * from `contracts` c JOIN `customers` a ON c.`contractNumber` = a.`ContractNumber` where `lapsedate8` >= '" + start + "' AND `lapsedate8` <= '" + end + "' ";
            if (!String.IsNullOrWhiteSpace(agentFormula))
                cmd += " AND `agentCode` IN " + agentFormula + " ";
            //            cmd += " and c.`contractNumber` = 'XXXXXXX' "; // Just for Debug
            cmd += " ;";

            DataTable lapseDt = G1.get_db_data(cmd);
            FindContract(lapseDt, "B19010LI");
            double lapseRecap = 0D;
            double contractValue = 0D;
            double downPayment = 0D;
            DataTable dt8 = dt.Clone();
            if (G1.get_column_number(dt8, "percent") < 0)
                dt8.Columns.Add("percent", Type.GetType("System.Decimal"));
            if (G1.get_column_number(dt8, "goal") < 0)
                dt8.Columns.Add("goal", Type.GetType("System.Decimal"));
            if (G1.get_column_number(dt8, "formula") < 0)
                dt8.Columns.Add("formula");
            if (G1.get_column_number(dt8, "splits") < 0)
                dt8.Columns.Add("splits");
            if (G1.get_column_number(dt8, "YearMonth") < 0)
                dt8.Columns.Add("YearMonth");


            AddColumnToTable(dt8, "num");
            AddColumnToTable(dt8, "customer");
            AddColumnToTable(dt8, "agentName");
            AddColumnToTable(dt8, "Location Name");
            AddColumnToTable(dt8, "agentNumber");
            AddColumnToTable(dt8, "loc");
            AddColumnToTable(dt8, "trust");
            AddColumnToTable(dt8, "totalPayments", "System.Double");
            AddColumnToTable(dt8, "commission", "System.Double");
            AddColumnToTable(dt8, "contractValue", "System.Double");

            AddColumnToTable(dt, "num");
            AddColumnToTable(dt, "customer");
            AddColumnToTable(dt, "agentName");
            AddColumnToTable(dt, "Location Name");
            AddColumnToTable(dt, "agentNumber");
            AddColumnToTable(dt, "loc");
            AddColumnToTable(dt, "trust");
            AddColumnToTable(dt, "totalPayments", "System.Double");
            AddColumnToTable(dt, "commission", "System.Double");
            AddColumnToTable(dt, "contractValue", "System.Double");

            for (int i = 0; i < lapseDt.Rows.Count; i++)
            {
                contractValue = DailyHistory.GetContractValue(lapseDt.Rows[i]);
                lapseRecap += contractValue;
            }
            double rr = lapseRecap * 0.01D;
            contractValue = 0D;
            double recapAmount = 0D;
            string agent = "";
            string agentName = "";
            string contract = "";
            string miniContract = "";
            string name = "";
            string trust = "";
            string loc = "";
            double percent = 0.01D;
            double goal = 0D;
            string formula = "";
            string splits = "";
            bool found = false;
            string issueDate = "";
            DateTime iDate;
            string yearMonth = "";
            string meetingNumber = "";

            for (int i = 0; i < lapseDt.Rows.Count; i++)
            {
                contract = lapseDt.Rows[i]["contractNumber"].ObjToString();
                if (contract == "B19010LI")
                {

                }
                meetingNumber = lapseDt.Rows[i]["meetingNumber"].ObjToString();
                if (!String.IsNullOrWhiteSpace(meetingNumber))
                    continue;

                issueDate = G1.GetSQLDate(lapseDt, i, "issueDate8");
                iDate = issueDate.ObjToDateTime();
                iDate = issueDate.ObjToDateTime();
                if (iDate.Year < 1900)
                {
                    iDate = DailyHistory.GetIssueDate(iDate, contract, null);
                    issueDate = iDate.ToString("MM/dd/yyyy");
                    lapseDt.Rows[i]["issueDate8"] = G1.DTtoMySQLDT(issueDate);
                }
                yearMonth = iDate.Year.ToString("D4") + iDate.Month.ToString("D2");

                agent = lapseDt.Rows[i]["agentCode"].ObjToString();
                contractValue = DailyHistory.GetContractValue(lapseDt.Rows[i]);
                contractValue = G1.RoundValue(contractValue);
                downPayment = lapseDt.Rows[i]["downPayment"].ObjToDouble();
                downPayment = G1.RoundValue(downPayment);
                contract = lapseDt.Rows[i]["contractNumber"].ObjToString();
                DataRow[] dRows = dt.Select("contractNumber='" + contract + "'");
                if (contract == "E17070LI")
                {

                }
                if (dRows.Length > 0)
                {
                    found = GetLapseRecap(contract, ref percent, ref goal, ref formula, ref splits);
                    double lapseAmount = DailyHistory.ProcessLapseAmount(false, agent, contractValue, percent, splits);
                    //                    double newlapse = contractValue * percent;
                    dRows[0]["Recap"] = lapseAmount;
                    dRows[0]["RecapContracts"] = contractValue;
                    dt8.ImportRow(dRows[0]);
                    int rows = dt8.Rows.Count - 1;
                    dt8.Rows[rows]["contractValue"] = contractValue;
                    dt8.Rows[rows]["downPayment"] = downPayment;
                    dt8.Rows[rows]["percent"] = percent;
                    dt8.Rows[rows]["goal"] = goal;
                    dt8.Rows[rows]["formula"] = formula;
                    dt8.Rows[rows]["splits"] = splits;
                    dt8.Rows[rows]["YearMonth"] = yearMonth;
                    dt8.Rows[rows]["issueDate8"] = G1.DTtoMySQLDT(issueDate);
                }
                else
                {
                    agentName = GetAgentName(agent);
                    name = lapseDt.Rows[i]["firstName"].ObjToString() + " " + lapseDt.Rows[i]["lastName"].ObjToString();
                    DataRow dRow = dt.NewRow();
                    dRow["contractNumber"] = contract;
                    //                    dRow["contractValue"] = contractValue;
                    dRow["agentName"] = agentName;
                    dRow["customer"] = name;
                    dRow["firstName"] = lapseDt.Rows[i]["firstName"].ObjToString();
                    dRow["lastName"] = lapseDt.Rows[i]["lastName"].ObjToString();
                    dRow["agentNumber"] = agent;
                    dRow["issueDate8"] = G1.DTtoMySQLDT(lapseDt.Rows[i]["issueDate8"]);
                    dRow["dueDate8"] = G1.DTtoMySQLDT(lapseDt.Rows[i]["lapseDate8"]);
                    dRow["lapseDate8"] = G1.DTtoMySQLDT(lapseDt.Rows[i]["lapseDate8"]);

                    miniContract = decodeContractNumber(contract, ref trust, ref loc);
                    dRow["loc"] = loc;
                    dRow["trust"] = trust;

                    found = GetLapseRecap(contract, ref percent, ref goal, ref formula, ref splits);
                    double lapseAmount = DailyHistory.ProcessLapseAmount(false, agent, contractValue, percent, splits);
                    dRow["Recap"] = lapseAmount;
                    dRow["RecapContracts"] = contractValue;
                    dt.Rows.Add(dRow);
                    dt8.ImportRow(dRow);
                    int rows = dt8.Rows.Count - 1;
                    dt8.Rows[rows]["contractValue"] = contractValue;
                    dt8.Rows[rows]["percent"] = percent;
                    dt8.Rows[rows]["goal"] = goal;
                    dt8.Rows[rows]["formula"] = formula;
                    dt8.Rows[rows]["splits"] = splits;
                    dt8.Rows[rows]["downPayment"] = downPayment;
                    dt8.Rows[rows]["YearMonth"] = yearMonth;
                }
            }
            G1.NumberDataTable(dt8);
            int axRow = dt8.Rows.Count;
            return dt8;
        }
        /****************************************************************************************/
        public static bool GetLapseAgent(string agent, DateTime issueDate, ref double percent, ref string goal, ref string formula, ref string splits)
        {
            formula = "";
            splits = "";
            goal = "";
            percent = 0.01;

            bool found = GetRecapAgentGoal(agent, issueDate, ref percent, ref goal, ref formula, ref splits);
            return found;
        }
        /****************************************************************************************/
        public static bool GetLapseRecap(string contract, ref double percent, ref double goal, ref string formula, ref string splits)
        {
            formula = "";
            splits = "";
            goal = 0D;
            percent = 0.01;
            string cmd = "Select * from `contracts` c JOIN `customers` a ON c.`contractNumber` = a.`ContractNumber` where c.`contractNumber` = '" + contract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;
            string agent = dt.Rows[0]["agentCode"].ObjToString();
            DateTime date = dt.Rows[0]["issueDate8"].ObjToDateTime();
            string sGoal = "";

            bool found = GetRecapAgentGoal(agent, date, ref percent, ref sGoal, ref formula, ref splits);
            goal = sGoal.ObjToDouble();
            return found;
        }
        /****************************************************************************************/
        public static bool GetRecapAgentGoal(string agent, DateTime date, ref double percent, ref string goal, ref string formula, ref string splits)
        {
            bool found = false;
            percent = 0.01D;
            goal = "";
            formula = "";
            splits = "";
            DateTime date2;
            string cmd = "Select * from `goals` where `agentCode` = '" + agent + "' AND `type` = 'Goal' order by `agentCode`,`effectiveDate`;";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date2 = dt.Rows[i]["effectiveDate"].ObjToDateTime();
                if (date2 <= date)
                {
                    found = true;
                    percent = dt.Rows[i]["percent"].ObjToDouble();
                    if (percent >= 1.0)
                        percent = percent / 100D;
                    goal = dt.Rows[i]["Goal"].ObjToString();
                    formula = dt.Rows[i]["formula"].ObjToString();
                    splits = dt.Rows[i]["splits"].ObjToString();
                    break;
                }
            }
            if (found)
                return found;
            cmd = "Select * from `goals` where `formula` LIKE '%" + agent + "%' AND `type` = 'Goal' order by `agentCode`,`effectiveDate`;";
            dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date2 = dt.Rows[i]["effectiveDate"].ObjToDateTime();
                if (date2 <= date)
                {
                    found = true;
                    percent = dt.Rows[i]["percent"].ObjToDouble();
                    if (percent >= 1.0)
                        percent = percent / 100D;
                    goal = dt.Rows[i]["Goal"].ObjToString();
                    formula = dt.Rows[i]["formula"].ObjToString();
                    splits = dt.Rows[i]["splits"].ObjToString();
                    break;
                }
            }
            return found;
        }
        /****************************************************************************************/
        public static DataTable CheckForMainReinstate(DataTable dt, DateTime date1, DateTime date2)
        {
            if (G1.get_column_number(dt, "Reins") < 0)
                dt.Columns.Add("Reins", Type.GetType("System.Decimal"));

            DateTime issueDate;

            if (date1 == date2)
            {
                DateTime lapseDate = date1;
                int days = lapseDate.Day;
                if (date1 == date2)
                    days = 1;
                string start = lapseDate.Year.ToString("D4") + "-" + lapseDate.Month.ToString("D2") + "-" + days.ToString("D2");
                date1 = start.ObjToDateTime();

                lapseDate = date2;
                if (date1 == date2)
                    days = DateTime.DaysInMonth(lapseDate.Year, lapseDate.Month);
                string end = lapseDate.Year.ToString("D4") + "-" + lapseDate.Month.ToString("D2") + "-" + days.ToString("D2");
                date2 = end.ObjToDateTime();
            }

            DataTable dt9 = CalculateReinstate(dt, date1, date2);

            DataView tempview = dt9.DefaultView;
            tempview.Sort = "agentName asc, loc asc";
            dt9 = tempview.ToTable();
            string agent = "";
            string agentName = "";
            double commission = 0D;
            string formula = "";
            string splits = "";
            double percent = 0D;
            double goal = 0D;
            double totalContracts = 0D;
            double recaps = 0D;
            double recapContracts = 0D;
            double dbrSales = 0D;
            AddColumnToTable(dt9, "commission", "System.Double");
            AddColumnToTable(dt9, "totalContracts", "System.Double");
            AddColumnToTable(dt9, "lapseRecaps", "System.Double");
            AddColumnToTable(dt9, "dbrSales", "System.Double");
            //if (1 == 1)
            //    return dt9;

            DateTime reinstateDate8 = DateTime.Now;

            for (int i = 0; i < dt9.Rows.Count; i++)
            {
                agent = dt9.Rows[i]["agentNumber"].ObjToString();
                agentName = dt9.Rows[i]["agentName"].ObjToString();
                //if (agentName.ToUpper() != "RONNIE KNOTTS")
                //    continue;
                issueDate = dt9.Rows[i]["issueDate8"].ObjToDateTime();
                reinstateDate8 = dt9.Rows[i]["reinstateDate8"].ObjToDateTime();
                formula = dt9.Rows[i]["formula"].ObjToString();
                splits = dt9.Rows[i]["splits"].ObjToString();
                percent = dt9.Rows[i]["percent"].ObjToDouble();
                goal = dt9.Rows[i]["goal"].ObjToDouble();
                if (formula.IndexOf("+") > 0)
                {

                }
                if (agent == "C40")
                {

                }
                commission = CalculatePastCommissions("reinstateDate8", agent, formula, splits, percent, goal, reinstateDate8, issueDate, issueDate, ref totalContracts, ref recaps, ref recapContracts, ref dbrSales);
                dt9.Rows[i]["commission"] = commission;
                dt9.Rows[i]["totalContracts"] = totalContracts;
                dt9.Rows[i]["lapseRecaps"] = recaps;
                dt9.Rows[i]["dbrSales"] = dbrSales;
                if (dbrSales > 0D)
                {

                }
            }
            dt9.Columns.Add("LLoc");
            for (int i = 0; i < dt9.Rows.Count; i++)
                dt9.Rows[i]["LLoc"] = "M";

            //            dt8 = CalculatePastAgents(dt8);
            return dt9;
        }
        /****************************************************************************************/
        public static DataTable CalculateReinstate(DataTable dt, DateTime date1, DateTime date2, string agentFormula = "")
        {
            if (G1.get_column_number(dt, "Reins") < 0)
                dt.Columns.Add("Reins", Type.GetType("System.Decimal"));

            DateTime lapseDate = date1;
            int days = lapseDate.Day;
            //            if (date1 == date2)
            days = 1;
            string start = lapseDate.Year.ToString("D4") + "-" + lapseDate.Month.ToString("D2") + "-" + days.ToString("D2");

            lapseDate = date2;
            //            if (date1 == date2)
            days = DateTime.DaysInMonth(lapseDate.Year, lapseDate.Month);
            string end = lapseDate.Year.ToString("D4") + "-" + lapseDate.Month.ToString("D2") + "-" + days.ToString("D2");

            start = date1.Year.ToString("D4") + "-" + date1.Month.ToString("D2") + "-" + date1.Day.ToString("D2");
            end = date2.Year.ToString("D4") + "-" + date2.Month.ToString("D2") + "-" + date2.Day.ToString("D2");

            string cmd = "Select * from `contracts` c JOIN `customers` a ON c.`contractNumber` = a.`ContractNumber` where `reinstatedate8` >= '" + start + "' AND `reinstatedate8` <= '" + end + "' ";
            if (!String.IsNullOrWhiteSpace(agentFormula))
                cmd += " AND `agentCode` IN " + agentFormula + " ";
            cmd += ";";

            DataTable lapseDt = G1.get_db_data(cmd);
            double lapseRecap = 0D;
            double contractValue = 0D;
            double downPayment = 0D;
            DataTable dt9 = dt.Clone();
            if (G1.get_column_number(dt9, "percent") < 0)
                dt9.Columns.Add("percent", Type.GetType("System.Decimal"));
            if (G1.get_column_number(dt9, "goal") < 0)
                dt9.Columns.Add("goal", Type.GetType("System.Decimal"));
            if (G1.get_column_number(dt9, "formula") < 0)
                dt9.Columns.Add("formula");
            if (G1.get_column_number(dt9, "splits") < 0)
                dt9.Columns.Add("splits");
            if (G1.get_column_number(dt9, "YearMonth") < 0)
                dt9.Columns.Add("YearMonth");


            AddColumnToTable(dt9, "num");
            AddColumnToTable(dt9, "customer");
            AddColumnToTable(dt9, "agentName");
            AddColumnToTable(dt9, "Location Name");
            AddColumnToTable(dt9, "agentNumber");
            AddColumnToTable(dt9, "loc");
            AddColumnToTable(dt9, "trust");
            AddColumnToTable(dt9, "totalPayments", "System.Double");
            AddColumnToTable(dt9, "commission", "System.Double");
            AddColumnToTable(dt9, "contractValue", "System.Double");

            AddColumnToTable(dt, "num");
            AddColumnToTable(dt, "customer");
            AddColumnToTable(dt, "agentName");
            AddColumnToTable(dt, "Location Name");
            AddColumnToTable(dt, "agentNumber");
            AddColumnToTable(dt, "loc");
            AddColumnToTable(dt, "trust");
            AddColumnToTable(dt, "totalPayments", "System.Double");
            AddColumnToTable(dt, "commission", "System.Double");
            AddColumnToTable(dt, "contractValue", "System.Double");

            for (int i = 0; i < lapseDt.Rows.Count; i++)
            {
                contractValue = DailyHistory.GetContractValue(lapseDt.Rows[i]);
                lapseRecap += contractValue;
            }
            double rr = lapseRecap * 0.01D;
            contractValue = 0D;
            double recapAmount = 0D;
            string agent = "";
            string agentName = "";
            string contract = "";
            string miniContract = "";
            string name = "";
            string trust = "";
            string loc = "";
            double percent = 0.01D;
            double goal = 0D;
            string formula = "";
            string splits = "";
            bool found = false;
            string issueDate = "";
            DateTime iDate;
            string yearMonth = "";
            string meetingNumber = "";

            for (int i = 0; i < lapseDt.Rows.Count; i++)
            {
                contract = lapseDt.Rows[i]["contractNumber"].ObjToString();
                if (contract == "N18021LI")
                {

                }
                meetingNumber = lapseDt.Rows[i]["meetingNumber"].ObjToString();
                if (!String.IsNullOrWhiteSpace(meetingNumber))
                    continue;
                issueDate = G1.GetSQLDate(lapseDt, i, "issueDate8");
                iDate = issueDate.ObjToDateTime();
                if (iDate.Year < 1900)
                {
                    iDate = DailyHistory.GetIssueDate(iDate, contract, null);
                    issueDate = iDate.ToString("MM/dd/yyyy");
                    lapseDt.Rows[i]["issueDate8"] = G1.DTtoMySQLDT(issueDate);
                }
                yearMonth = iDate.Year.ToString("D4") + iDate.Month.ToString("D2");

                agent = lapseDt.Rows[i]["agentCode"].ObjToString();
                contractValue = DailyHistory.GetContractValue(lapseDt.Rows[i]);
                contractValue = G1.RoundValue(contractValue);
                downPayment = lapseDt.Rows[i]["downPayment"].ObjToDouble();
                downPayment = G1.RoundValue(downPayment);
                contract = lapseDt.Rows[i]["contractNumber"].ObjToString();
                DataRow[] dRows = dt.Select("contractNumber='" + contract + "'");
                if (dRows.Length > 0)
                {
                    found = GetLapseRecap(contract, ref percent, ref goal, ref formula, ref splits);
                    double lapseAmount = DailyHistory.ProcessLapseAmount(false, agent, contractValue, percent, splits);
                    //                    double newlapse = contractValue * percent;
                    dRows[0]["Reins"] = lapseAmount;
                    dt9.ImportRow(dRows[0]);
                    int rows = dt9.Rows.Count - 1;
                    dt9.Rows[rows]["contractValue"] = contractValue;
                    dt9.Rows[rows]["downPayment"] = downPayment;
                    dt9.Rows[rows]["percent"] = percent;
                    dt9.Rows[rows]["goal"] = goal;
                    dt9.Rows[rows]["formula"] = formula;
                    dt9.Rows[rows]["splits"] = splits;
                    dt9.Rows[rows]["YearMonth"] = yearMonth;
                    dt9.Rows[rows]["issueDate8"] = G1.DTtoMySQLDT(issueDate);
                }
                else
                {
                    agentName = GetAgentName(agent);
                    name = lapseDt.Rows[i]["firstName"].ObjToString() + " " + lapseDt.Rows[i]["lastName"].ObjToString();
                    DataRow dRow = dt.NewRow();
                    dRow["contractNumber"] = contract;
                    //                    dRow["contractValue"] = contractValue;
                    dRow["agentName"] = agentName;
                    dRow["customer"] = name;
                    dRow["firstName"] = lapseDt.Rows[i]["firstName"].ObjToString();
                    dRow["lastName"] = lapseDt.Rows[i]["lastName"].ObjToString();
                    dRow["agentNumber"] = agent;
                    dRow["issueDate8"] = G1.DTtoMySQLDT(lapseDt.Rows[i]["issueDate8"]);
                    dRow["dueDate8"] = G1.DTtoMySQLDT(lapseDt.Rows[i]["lapseDate8"]);
                    dRow["lapseDate8"] = G1.DTtoMySQLDT(lapseDt.Rows[i]["lapseDate8"]);
                    dRow["reinstateDate8"] = G1.DTtoMySQLDT(lapseDt.Rows[i]["reinstateDate8"]);

                    miniContract = decodeContractNumber(contract, ref trust, ref loc);
                    dRow["loc"] = loc;
                    dRow["trust"] = trust;

                    found = GetLapseRecap(contract, ref percent, ref goal, ref formula, ref splits);
                    double lapseAmount = DailyHistory.ProcessLapseAmount(false, agent, contractValue, percent, splits);
                    dRow["Reins"] = lapseAmount;
                    dt.Rows.Add(dRow);
                    dt9.ImportRow(dRow);
                    int rows = dt9.Rows.Count - 1;
                    dt9.Rows[rows]["contractValue"] = contractValue;
                    dt9.Rows[rows]["percent"] = percent;
                    dt9.Rows[rows]["goal"] = goal;
                    dt9.Rows[rows]["formula"] = formula;
                    dt9.Rows[rows]["splits"] = splits;
                    dt9.Rows[rows]["downPayment"] = downPayment;
                    dt9.Rows[rows]["YearMonth"] = yearMonth;
                }
            }
            G1.NumberDataTable(dt9);
            return dt9;
        }
        /****************************************************************************************/
        private string GetContractAgent(string contract)
        {
            string agent = "";
            string cmd = "Select `agentCode` from `customers` where `contractNumber` = '" + contract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                agent = dt.Rows[0]["agentCode"].ObjToString();
            return agent;
        }
        /****************************************************************************************/
        public static string GetAgentName(string agent)
        {
            if (String.IsNullOrWhiteSpace(agent))
                return "";
            string name = "";
            string cmd = "Select * from `agents` where `agentCode` = '" + agent + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                name = dt.Rows[0]["firstName"].ObjToString() + " " + dt.Rows[0]["lastName"].ObjToString();
            return name;
        }
        /****************************************************************************************/
        private void LoadTabAgentsLocations()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataView tempview = dt.DefaultView;
            tempview.Sort = "agentName asc, loc asc";
            //            tempview.Sort = "loc asc, agentName asc";
            dt = tempview.ToTable();
            DataTable lDt = dt.Clone();
            if (G1.get_column_number(lDt, "totalTrusts") < 0)
                lDt.Columns.Add("totalTrusts", Type.GetType("System.Double"));
            string lastLocation = "";
            string lastAgent = "";
            string lastNumber = "";
            string location = "";
            string agent = "";
            string lloc = "";
            double contractValue = 0D;
            double downPayment = 0D;
            double payment = 0D;
            double debit = 0D;
            double credit = 0D;
            double interest = 0D;
            string agentNumber = "";
            double ibtrust = 0D;
            double sptrust = 0D;
            double xxtrust = 0D;
            string trust = "";
            string xtrust = "";
            int idx = 0;
            string ch = "";
            bool dbr = false;
            double dpr = 0D;
            double dbc_5 = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                agent = dt.Rows[i]["agentName"].ObjToString();
                location = dt.Rows[i]["loc"].ObjToString();
                agentNumber = dt.Rows[i]["agentNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(lastLocation))
                    lastLocation = location;
                if (String.IsNullOrWhiteSpace(lastAgent))
                {
                    lastAgent = agent;
                    lastNumber = agentNumber;
                }
                if (agent != lastAgent)
                {
                    DataRow drow = lDt.NewRow();
                    drow["loc"] = lastLocation;
                    drow["agentNumber"] = lastNumber;
                    drow["downPayment"] = dpr;
                    drow["contractValue"] = contractValue;
                    drow["paymentAmount"] = payment;
                    drow["totalPayments"] = dpr + payment + credit - debit - interest - dbc_5;
                    drow["ibtrust"] = ibtrust;
                    drow["sptrust"] = sptrust;
                    drow["xxtrust"] = xxtrust;
                    drow["totalTrusts"] = sptrust + ibtrust;
                    lDt.Rows.Add(drow);
                    lastAgent = agent;
                    lastNumber = agentNumber;
                    lastLocation = location;
                    contractValue = 0D;
                    payment = 0D;
                    downPayment = 0D;
                    debit = 0D;
                    credit = 0D;
                    interest = 0D;
                    dpr = 0D;
                    dbc_5 = 0D;
                    ibtrust = 0D;
                    sptrust = 0D;
                    xxtrust = 0D;
                }
                else if (location != lastLocation)
                {
                    DataRow drow = lDt.NewRow();
                    drow["loc"] = lastLocation;
                    drow["agentNumber"] = agentNumber;
                    drow["downPayment"] = dpr;
                    drow["contractValue"] = contractValue;
                    drow["paymentAmount"] = payment;
                    drow["totalPayments"] = dpr + payment + credit - debit - interest - dbc_5;
                    drow["ibtrust"] = ibtrust;
                    drow["sptrust"] = sptrust;
                    drow["xxtrust"] = xxtrust;
                    drow["totalTrusts"] = sptrust + ibtrust;
                    lDt.Rows.Add(drow);
                    lastLocation = location;
                    contractValue = 0D;
                    payment = 0D;
                    downPayment = 0D;
                    debit = 0D;
                    credit = 0D;
                    interest = 0D;
                    dpr = 0D;
                    dbc_5 = 0D;
                    ibtrust = 0D;
                    sptrust = 0D;
                    xxtrust = 0D;
                    lloc = "";
                }
                dbr = false;
                if (dt.Rows[i]["dbr"].ObjToString().ToUpper() == "DBR")
                    dbr = true;
                contractValue += dt.Rows[i]["contractValue"].ObjToDouble();
                dpr += dt.Rows[i]["downPayment"].ObjToDouble();
                dbc_5 += dt.Rows[i]["dbc_5"].ObjToDouble();
                if (!dbr)
                {
                    payment += dt.Rows[i]["paymentAmount"].ObjToDouble();

                    downPayment += dt.Rows[i]["downPayment"].ObjToDouble();
                    debit += dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    credit += dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    interest += dt.Rows[i]["interestPaid"].ObjToDouble();
                    trust = dt.Rows[i]["trust"].ObjToString();
                    xtrust = dt.Rows[i]["xtrust"].ObjToString();
                    if (xtrust.ToUpper() == "Y")
                        xxtrust = dt.Rows[i]["contractValue"].ObjToDouble();
                    else
                    {
                        if (trust.Length > 0)
                        {
                            idx = trust.Length - 1;
                            ch = trust.Substring(idx);
                            if (ch.ToUpper() == "I")
                                ibtrust += dt.Rows[i]["contractValue"].ObjToDouble();
                            else
                                sptrust += dt.Rows[i]["contractValue"].ObjToDouble();
                        }
                        else
                            sptrust += dt.Rows[i]["contractValue"].ObjToDouble();
                    }
                }
            }
            DataRow ddr = lDt.NewRow();
            ddr["loc"] = lastLocation;
            ddr["agentNumber"] = agentNumber;
            ddr["downPayment"] = dpr;
            ddr["contractValue"] = contractValue;
            ddr["paymentAmount"] = payment;
            ddr["totalPayments"] = dpr + payment + credit - debit - interest - dbc_5;
            ddr["ibtrust"] = ibtrust;
            ddr["sptrust"] = sptrust;
            ddr["xxtrust"] = xxtrust;
            ddr["totalTrusts"] = sptrust + ibtrust;
            lDt.Rows.Add(ddr);

            DataTable dd = (DataTable)chkComboLocNames.Properties.DataSource;
            DataTable dx = G1.get_db_data("Select * from `agents`;");

            lastAgent = "";
            bool first = true;
            for (int i = 0; i < lDt.Rows.Count; i++)
            {
                agent = lDt.Rows[i]["agentNumber"].ObjToString();
                DataRow[] dR = dx.Select("agentCode='" + agent + "'");
                if (dR.Length > 0)
                    lDt.Rows[i]["agentName"] = dR[0]["firstName"].ObjToString().Trim() + " " + dR[0]["lastName"].ObjToString().Trim();
                agent = lDt.Rows[i]["agentName"].ObjToString();
                if (String.IsNullOrWhiteSpace(lastAgent))
                    lastAgent = lDt.Rows[i]["agentName"].ObjToString();
                //if (lastAgent == agent && i != 0)
                //    lDt.Rows[i]["agentName"] = "";
                lastAgent = agent;

                location = lDt.Rows[i]["loc"].ObjToString();
                DataRow[] dr = dd.Select("keycode='" + location + "'");
                if (dr.Length > 0)
                    lDt.Rows[i]["Location Name"] = dr[0]["name"].ObjToString();
            }

            lastLocation = "";
            for (int i = (lDt.Rows.Count - 1); i >= 0; i--)
            {
                location = lDt.Rows[i]["agentName"].ObjToString();
                if (String.IsNullOrWhiteSpace(lastLocation))
                    lastLocation = location;
                if (location != lastLocation)
                {
                    DataRow dRow = lDt.NewRow();
                    lDt.Rows.InsertAt(dRow, (i + 1));
                    lastLocation = location;
                }
            }

            DailyHistory.AddAP(lDt);
            DailyHistory.CleanupVisibility(gridMain4);

            G1.NumberDataTable(lDt);
            dgv4.DataSource = lDt;
        }
        /****************************************************************************************/
        private void AddToLocationCombo(DataTable locationDt, string text)
        {
            DataRow ddrx = locationDt.NewRow();
            ddrx["options"] = text;
            locationDt.Rows.Add(ddrx);
        }
        /****************************************************************************************/
        private void LoadTabAllLocations()
        {
            DataTable locationDt = new DataTable();
            locationDt.Columns.Add("options");
            AddToLocationCombo(locationDt, "Normal");
            AddToLocationCombo(locationDt, "Trusts");
            AddToLocationCombo(locationDt, "Lapses");
            AddToLocationCombo(locationDt, "Reinstates");
            cmbLocationTotals.Properties.DataSource = locationDt;

            DataTable dt = (DataTable)dgv.DataSource;
            DataView tempview = dt.DefaultView;
            tempview.Sort = "loc asc";
            dt = tempview.ToTable();
            DataTable lDt = dt.Clone();
            lDt.Columns.Add("contracts");
            //lDt.Columns.Add("ibtrust", Type.GetType("System.Double"));
            //lDt.Columns.Add("sptrust", Type.GetType("System.Double"));
            lDt.Columns.Add("total", Type.GetType("System.Double"));

            string lastLocation = "";
            string lastAgent = "";
            string lastNumber = "";
            string location = "";
            string agent = "";
            string lloc = "";
            double contractValue = 0D;
            double cValue = 0D;
            double downPayment = 0D;
            double payment = 0D;
            double ccFee = 0D;
            double dpp = 0D;
            double ap = 0D;
            double cc_Fee = 0D;
            double cc_ap = 0D;
            double cc_dpp = 0D;
            double debit = 0D;
            double credit = 0D;
            double interest = 0D;
            double ibtrust = 0D;
            double sptrust = 0D;
            double xxtrust = 0D;
            double dbc = 0D;
            double dpr = 0D;
            double dbc_5 = 0D;
            string xtrust = "";
            string agentNumber = "";
            string trust = "";
            string contracts = "";
            string contract = "";
            int idx = 0;
            string ch = "";
            int contractCount = 0;
            int contractRow = -1;
            bool dbr = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                agent = dt.Rows[i]["agentName"].ObjToString();
                location = dt.Rows[i]["loc"].ObjToString();
                agentNumber = dt.Rows[i]["agentNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(lastLocation))
                    lastLocation = location;
                if (String.IsNullOrWhiteSpace(lastAgent))
                {
                    lastAgent = agent;
                    lastNumber = agentNumber;
                }
                if (location != lastLocation)
                {
                    if (location == "L")
                    {

                    }
                    if (contractRow >= 0)
                    {
                        lDt.Rows[contractRow - 1]["downPayment"] = dpr;
                        lDt.Rows[contractRow - 1]["contractValue"] = contractValue;
                        lDt.Rows[contractRow - 1]["paymentAmount"] = payment;
                        lDt.Rows[contractRow - 1]["ccFee"] = ccFee;
                        lDt.Rows[contractRow - 1]["ap"] = cc_ap;
                        lDt.Rows[contractRow - 1]["dpp"] = cc_dpp;
                        lDt.Rows[contractRow - 1]["totalPayments"] = dpr + payment + credit - debit - interest - dbc_5;
                        lDt.Rows[contractRow - 1]["ibtrust"] = ibtrust;
                        lDt.Rows[contractRow - 1]["sptrust"] = sptrust;
                        lDt.Rows[contractRow - 1]["xxtrust"] = xxtrust;
                        lDt.Rows[contractRow - 1]["total"] = sptrust + ibtrust;
                    }
                    DataRow drow = lDt.NewRow();
                    if (contractRow < 0)
                    {
                        drow["loc"] = lastLocation;
                        drow["agentNumber"] = agentNumber;
                        drow["downPayment"] = dpr;
                        drow["contractValue"] = contractValue;
                        drow["paymentAmount"] = payment;
                        drow["ccFee"] = ccFee;
                        drow["ap"] = cc_ap;
                        drow["dpp"] = cc_dpp;
                        drow["totalPayments"] = dpr + payment + credit - debit - interest - dbc_5;
                        drow["ibtrust"] = ibtrust;
                        drow["sptrust"] = sptrust;
                        drow["xxtrust"] = xxtrust;
                        drow["total"] = sptrust + ibtrust;
                    }
                    drow["contracts"] = contracts.Trim().TrimEnd(',');
                    lDt.Rows.Add(drow);
                    lastLocation = location;
                    lastAgent = agent;
                    contractValue = 0D;
                    payment = 0D;
                    ccFee = 0D;
                    cc_ap = 0D;
                    cc_dpp = 0D;
                    downPayment = 0D;
                    debit = 0D;
                    credit = 0D;
                    interest = 0D;
                    ibtrust = 0D;
                    sptrust = 0D;
                    xxtrust = 0D;
                    contracts = "";
                    lloc = "";
                    contractRow = -1;
                    contractCount = 0;
                    dpr = 0D;
                    dbc_5 = 0D;
                }
                dbr = false;
                if (dt.Rows[i]["dbr"].ObjToString().ToUpper() == "DBR")
                    dbr = true;
                dbc = dt.Rows[i]["dbc"].ObjToDouble();
                //if (dbc > 0D)
                //    continue;
                dpr += dt.Rows[i]["downPayment"].ObjToDouble();
                cc_dpp += dt.Rows[i]["dpp"].ObjToDouble();
                dbc_5 += dt.Rows[i]["dbc_5"].ObjToDouble();
                contractValue += dt.Rows[i]["contractValue"].ObjToDouble();
                if (!dbr)
                {
                    cValue = dt.Rows[i]["contractValue"].ObjToDouble();

                    ccFee += dt.Rows[i]["ccFee"].ObjToDouble();
                    cc_ap += dt.Rows[i]["ap"].ObjToDouble();
                    //cc_dpp += dt.Rows[i]["dpp"].ObjToDouble();
                    payment += dt.Rows[i]["paymentAmount"].ObjToDouble();
                    downPayment += dt.Rows[i]["downPayment"].ObjToDouble();
                    debit += dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    credit += dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    interest += dt.Rows[i]["interestPaid"].ObjToDouble();
                    trust = dt.Rows[i]["trust"].ObjToString();
                    xtrust = dt.Rows[i]["xtrust"].ObjToString();
                    if (xtrust.ToUpper() == "Y")
                        xxtrust = dt.Rows[i]["contractValue"].ObjToDouble();
                    else
                    {
                        if (trust.Length > 0)
                        {
                            idx = trust.Length - 1;
                            ch = trust.Substring(idx);
                            if (ch.ToUpper() == "I")
                                ibtrust += dt.Rows[i]["contractValue"].ObjToDouble();
                            else
                                sptrust += dt.Rows[i]["contractValue"].ObjToDouble();
                        }
                        else
                            sptrust += dt.Rows[i]["contractValue"].ObjToDouble();
                    }
                }
                if (cValue > 0.0D)
                {
                    contract = dt.Rows[i]["contractNumber"].ObjToString();
                    contract = decodeContractNumber(contract, ref trust, ref lloc);
                    contracts += lloc + contract + ", ";
                    contractCount++;
                    if (contractCount >= 4)
                    {
                        DataRow drow = lDt.NewRow();
                        if (contractRow < 0)
                        {
                            drow["loc"] = lastLocation;
                            drow["agentNumber"] = agentNumber;
                            drow["downPayment"] = dpr;
                            drow["contractValue"] = contractValue;
                            drow["paymentAmount"] = payment;
                            drow["ccFee"] = ccFee;
                            drow["ap"] = cc_ap;
                            drow["dpp"] = cc_dpp;
                            drow["totalPayments"] = downPayment + payment + credit - debit - interest - dbc_5;
                            drow["ibtrust"] = ibtrust;
                            drow["sptrust"] = sptrust;
                            drow["xxtrust"] = xxtrust;
                            drow["total"] = sptrust + ibtrust;
                        }
                        drow["contracts"] = contracts.Trim().TrimEnd(',');
                        lDt.Rows.Add(drow);
                        if (contractRow < 0)
                            contractRow = lDt.Rows.Count;
                        contractCount = 0;
                        contracts = "";
                    }
                }
            }
            DataRow ddr = lDt.NewRow();
            if (contractRow >= 0)
            {
                lDt.Rows[contractRow - 1]["downPayment"] = dpr;
                lDt.Rows[contractRow - 1]["contractValue"] = contractValue;
                lDt.Rows[contractRow - 1]["paymentAmount"] = payment;
                lDt.Rows[contractRow - 1]["ccFee"] = ccFee;
                lDt.Rows[contractRow - 1]["ap"] = cc_ap;
                lDt.Rows[contractRow - 1]["dpp"] = cc_dpp;
                lDt.Rows[contractRow - 1]["totalPayments"] = dpr + payment + credit - debit - interest - dbc_5;
                lDt.Rows[contractRow - 1]["ibtrust"] = ibtrust;
                lDt.Rows[contractRow - 1]["sptrust"] = sptrust;
                lDt.Rows[contractRow - 1]["xxtrust"] = xxtrust;
                lDt.Rows[contractRow - 1]["total"] = sptrust + ibtrust;
            }
            else
            {
                ddr["loc"] = lastLocation;
                ddr["agentNumber"] = agentNumber;
                ddr["downPayment"] = dpr;
                ddr["contractValue"] = contractValue;
                ddr["paymentAmount"] = payment;
                ddr["ccFee"] = ccFee;
                ddr["ap"] = cc_ap;
                ddr["dpp"] = cc_dpp;
                ddr["totalPayments"] = dpr + payment + credit - debit - interest - dbc_5;
                ddr["ibtrust"] = ibtrust;
                ddr["sptrust"] = sptrust;
                ddr["xxtrust"] = xxtrust;
                ddr["total"] = sptrust + ibtrust;
            }
            ddr["contracts"] = contracts.Trim().TrimEnd(',');
            lDt.Rows.Add(ddr);

            DataTable dd = (DataTable)chkComboLocNames.Properties.DataSource;
            DataTable dx = G1.get_db_data("Select * from `agents`;");

            lastLocation = "";
            bool first = true;
            for (int i = 0; i < lDt.Rows.Count; i++)
            {
                agent = lDt.Rows[i]["agentNumber"].ObjToString();
                DataRow[] dR = dx.Select("agentCode='" + agent + "'");
                if (dR.Length > 0)
                    lDt.Rows[i]["agentName"] = dR[0]["firstName"].ObjToString().Trim() + " " + dR[0]["lastName"].ObjToString().Trim();

                location = lDt.Rows[i]["loc"].ObjToString();
                if (String.IsNullOrWhiteSpace(lastLocation))
                    lastLocation = location;
                lastLocation = location;
                DataRow[] dr = dd.Select("keycode='" + location + "'");
                if (dr.Length > 0)
                    lDt.Rows[i]["Location Name"] = dr[0]["name"].ObjToString();
            }

            lastLocation = "";
            for (int i = (lDt.Rows.Count - 1); i >= 0; i--)
            {
                location = lDt.Rows[i]["loc"].ObjToString();
                if (String.IsNullOrWhiteSpace(lastLocation))
                    lastLocation = location;
                if (location != lastLocation)
                {
                    DataRow dRow = lDt.NewRow();
                    lDt.Rows.InsertAt(dRow, (i + 1));
                    lastLocation = location;
                }
            }

            //DailyHistory.AddAP(lDt);
            DailyHistory.CleanupVisibility(gridMain5);

            G1.NumberDataTable(lDt);
            dgv5.DataSource = lDt;
        }
        /****************************************************************************************/
        private DataTable BuildExcelDataTable()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataTable dx = new DataTable();
            dx.Columns.Add("contractNumber");
            dx.Columns.Add("customer");
            dx.Columns.Add("agentNumber");
            dx.Columns.Add("loc");
            dx.Columns.Add("trust");
            dx.Columns.Add("contractValue", Type.GetType("System.Double"));
            dx.Columns.Add("downPayment", Type.GetType("System.Decimal"));
            dx.Columns.Add("paymentAmount", Type.GetType("System.Decimal"));
            dx.Columns.Add("issueDate8");
            dx.Columns.Add("dueDate8");
            dx.Columns.Add("payDate8");
            try
            {
                CopyColumn(dt, "contractNumber", dx);
                CopyColumn(dt, "customer", dx);
                CopyColumn(dt, "agentNumber", dx);
                CopyColumn(dt, "loc", dx);
                CopyColumn(dt, "trust", dx);
                CopyColumn(dt, "contractValue", dx);
                CopyColumn(dt, "downPayment", dx);
                CopyColumn(dt, "paymentAmount", dx);
                CopyColumn(dt, "issueDate8", dx);
                CopyColumn(dt, "dueDate8", dx);
                CopyColumn(dt, "payDate8", dx);
            }
            catch (Exception ex)
            {

            }
            return dx;
        }
        /****************************************************************************************/
        private void CopyColumn(DataTable fromDt, string fromColumn, DataTable toDt, string toColumn = "")
        {
            if (String.IsNullOrWhiteSpace(toColumn))
                toColumn = fromColumn;
            if (G1.get_column_number(fromDt, fromColumn) < 0)
                return;
            if (G1.get_column_number(toDt, toColumn) < 0)
                return;
            string type = fromDt.Columns[fromColumn].DataType.ToString().ToUpper();
            string to_type = toDt.Columns[toColumn].DataType.ToString().ToUpper();
            if (type != to_type)
            {

            }
            for (int i = 0; i < fromDt.Rows.Count; i++)
            {
                if (i > (toDt.Rows.Count - 1))
                {
                    DataRow dRow = toDt.NewRow();
                    toDt.Rows.Add(dRow);
                }
                if (type.IndexOf("MYSQLDATETIME") >= 0)
                    toDt.Rows[i][toColumn] = G1.DTtoMySQLDT(fromDt.Rows[i][fromColumn]);
                else if (type.IndexOf("DOUBLE") >= 0)
                    toDt.Rows[i][toColumn] = fromDt.Rows[i][fromColumn].ObjToDouble();
                else if (type.IndexOf("DECIMAL") >= 0)
                    toDt.Rows[i][toColumn] = fromDt.Rows[i][fromColumn].ObjToDouble();
                else
                    toDt.Rows[i][toColumn] = fromDt.Rows[i][fromColumn].ToString();
            }
        }
        /****************************************************************************************/
        private void btnExcel_Click(object sender, EventArgs e)
        {
            DataTable dx = BuildExcelDataTable();
            this.Hide();
            string directory = @"C:\\SMFS";
            //string directory = G1.GetAdminOption("Bulk Upload Create Directory");
            //if (String.IsNullOrWhiteSpace(directory))
            //    directory = @"C:\\rag";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            //            string filename = G1.GetAdminOption("Bulk Upload Filename");
            string filename = "PaymentFile";
            if (filename.ToUpper().IndexOf(".XLSX") >= 0)
            {
                filename.Replace(".xlsx", "");
                filename.Replace(".XLSX", "");
            }
            else if (filename.ToUpper().IndexOf(".XLS") >= 0)
            {
                filename.Replace(".xls", "");
                filename.Replace(".XLS", "");
            }
            if (radioXLS.Checked)
                filename += ".xls";
            else
                filename += ".xlsx";

            if (1 == 1)
            {
                DataTable[] dts = new DataTable[6];
                for (int i = 0; i < dts.Length; i++)
                    dts[i] = null;
                AddToDtTable(dts, dx, "Payments");
                //AddToDtTable(dts, (DataTable)dgv2.DataSource, "Smoking History");
                //AddToDtTable(dts, (DataTable)dgv3.DataSource, "Personal Rep");
                //AddToDtTable(dts, (DataTable)dgv4.DataSource, "Exposure");
                //AddToDtTable(dts, (DataTable)dgv5.DataSource, "Dependent");
                GenerateExcelFile(directory, filename, dts);
                //                AnotherExcelExport(bulkFilename, dts);
                //                ExcelCreator.Create(dts, @"C:\\rag\Export.xls");
                //                ExcelCreator.Create(dts, @bulkFilename);
                MessageBox.Show("Contratulations! Excel File has been created!");
                this.Show();
                if (chkPresent.Checked)
                {
                    string fullname = directory + "\\" + filename;
                    System.Diagnostics.Process.Start(@fullname);
                }
                return;
            }
            string columnName = "";
            string data = "";
            DataTable dt = (DataTable)dgv.DataSource;
            string outfile = @"c:\rag\demo.xls";
            FileStream stream = new FileStream(@outfile, FileMode.OpenOrCreate);
            ExcelWriter writer = new ExcelWriter(stream);
            writer.BeginWrite();
            for (int j = 1; j < dt.Columns.Count; j++)
            {
                columnName = dt.Columns[j].ColumnName.Trim().ObjToString();
                if (!String.IsNullOrWhiteSpace(columnName))
                    writer.WriteCell(0, j - 1, columnName);
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 1; j < dt.Columns.Count; j++)
                {
                    data = dt.Rows[i][j].ObjToString();
                    if (!String.IsNullOrWhiteSpace(data))
                        writer.WriteCell(i + 1, (j - 1), data);
                }
            }
            writer.EndWrite();
            stream.Close();
            if (chkPresent.Checked)
                System.Diagnostics.Process.Start(@outfile);
        }
        /***********************************************************************************************/
        private void AddToDtTable(DataTable[] dts, DataTable dt, string name)
        {
            for (int i = 0; i < dts.Length; i++)
            {
                if (dts[i] == null)
                {
                    dt.TableName = name;
                    dts[i] = dt;
                    break;
                }
            }
        }
        /***********************************************************************************************/
        public void GenerateExcelFile(string directory, string filename, DataTable[] dts)
        {
            bool gotit = false;
            if (filename.ToUpper().IndexOf(".XLSX") > 0)
                gotit = true;
            else if (filename.ToUpper().IndexOf(".XLS") > 0)
                gotit = true;
            if (!gotit)
                filename += ".XLSX";
            if (filename.ToUpper().IndexOf(".XLSX") < 0)
            {
                string newfile = directory + "/" + filename;
                AnotherExcelExport(newfile, dts);
                return;
            }

            DirectoryInfo outputDir = new DirectoryInfo(@directory);
            FileInfo newFile = new FileInfo(outputDir.FullName + @"\\" + filename);
            if (newFile.Exists)
            {
                newFile.Delete();  // ensures we create a new workbook
                newFile = new FileInfo(outputDir.FullName + @"\\" + filename);
            }
            using (ExcelPackage package = new ExcelPackage(newFile))
            {
                // add a new worksheet to the empty workbook
                for (int i = 0; i < dts.Length; i++)
                {
                    DataTable dt = dts[i];
                    if (dt == null)
                        continue;
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(dt.TableName);

                    //for (int col = 1; col < dt.Columns.Count; col++)
                    //{
                    //    worksheet.Cells[1, col].Value = dt.Columns[col].ColumnName.Trim();
                    //}

                    string colstr = "";
                    char c = 'A';
                    string cell = "";
                    string data = "";
                    for (int col = 1; col < dt.Columns.Count; col++)
                    {
                        cell = colstr + c + "1";
                        data = dt.Columns[col].Caption.Trim();
                        if (String.IsNullOrWhiteSpace(data))
                            data = dt.Columns[col].ColumnName.Trim();
                        if (!String.IsNullOrWhiteSpace(data))
                            worksheet.Cells[cell].Value = data;
                        if (c == 'Z')
                        {
                            c = 'A';
                            colstr += "A";
                            continue;
                        }
                        c++;
                    }
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        colstr = "";
                        c = 'A';
                        cell = "";
                        data = "";
                        for (int col = 1; col < dt.Columns.Count; col++)
                        {
                            cell = colstr + c + (j + 2).ToString();
                            data = dt.Rows[j][col].ObjToString();
                            if (!String.IsNullOrWhiteSpace(data))
                                worksheet.Cells[cell].Value = data;
                            if (c == 'Z')
                            {
                                c = 'A';
                                colstr += "A";
                                continue;
                            }
                            c++;
                            //                            worksheet.Cells[j + 1, col - 1].Value = dt.Rows[j][col].ObjToString();
                        }
                    }
                    worksheet.Cells.AutoFitColumns(0);  //Autofit columns for all cells

                    // lets set the header text 
                    worksheet.HeaderFooter.OddHeader.CenteredText = "&24&U&\"Arial,Regular Bold\" Trust85 Report";
                    // add the page number to the footer plus the total number of pages
                    worksheet.HeaderFooter.OddFooter.RightAlignedText =
                        string.Format("Page {0} of {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);
                    // add the sheet name to the footer
                    worksheet.HeaderFooter.OddFooter.CenteredText = ExcelHeaderFooter.SheetName;
                    // add the file path to the footer
                    worksheet.HeaderFooter.OddFooter.LeftAlignedText = ExcelHeaderFooter.FilePath + ExcelHeaderFooter.FileName;

                    //worksheet.PrinterSettings.RepeatRows = worksheet.Cells["1:2"];
                    //worksheet.PrinterSettings.RepeatColumns = worksheet.Cells["A:G"];

                    // Change the sheet view to show it in page layout mode
                    worksheet.View.PageLayoutView = true;
                }
                package.Workbook.Properties.Title = "Trust85 Report";
                package.Workbook.Properties.Author = "Robby Graham";
                package.Workbook.Properties.Comments = "This sample demonstrates how to create an Excel 2007 workbook using EPPlus";

                // set some extended property values
                package.Workbook.Properties.Company = "RAGWARE";

                // set some custom property values
                package.Workbook.Properties.SetCustomPropertyValue("Checked by", "Robby Graham");
                package.Workbook.Properties.SetCustomPropertyValue("AssemblyName", "EPPlus");
                package.Save();
            }
        }
        /***********************************************************************************************/
        public void AnotherExcelExport(string filename, DataTable[] dts)
        {
            Workbook workbook = new Workbook();
            for (int i = 0; i < dts.Length; i++)
            {
                DataTable dt = dts[i];
                if (dt == null)
                    continue;
                Worksheet worksheet = new Worksheet(dt.TableName);
                for (int col = 1; col < dt.Columns.Count; col++)
                {
                    worksheet.Cells[0, col - 1] = new Cell(dt.Columns[col].ColumnName.Trim());
                }
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    for (int col = 1; col < dt.Columns.Count; col++)
                    {
                        worksheet.Cells[j + 1, col - 1] = new Cell(dt.Rows[j][col].ObjToString());
                    }
                }
                //worksheet.Cells[0, 1] = new Cell((short)1);
                //worksheet.Cells[2, 0] = new Cell(9999999);
                //worksheet.Cells[3, 3] = new Cell((decimal)3.45);
                //worksheet.Cells[2, 2] = new Cell("Text string");
                //worksheet.Cells[2, 4] = new Cell("Second string");
                //worksheet.Cells[4, 0] = new Cell(32764.5, "#,##0.00");
                //worksheet.Cells[5, 1] = new Cell(DateTime.Now, @"YYYY-MM-DD");
                worksheet.Cells.ColumnWidth[0, 1] = 3000;
                workbook.Worksheets.Add(worksheet);
            }
            workbook.Save(filename);
        }
        /***********************************************************************************************/
        private void LoadLocations(DataTable lDt)
        {
            DataTable dd = (DataTable)chkComboLocNames.Properties.DataSource;
            string location = "";
            for (int i = 0; i < lDt.Rows.Count; i++)
            {
                location = lDt.Rows[i]["loc"].ObjToString();
                DataRow[] dr = dd.Select("keycode='" + location + "'");
                if (dr.Length > 0)
                    lDt.Rows[i]["Location Name"] = dr[0]["name"].ObjToString();
            }
        }
        public void pb(DetailBand Detail)
        {
            // Create two labels and an XRPageBreak object.
            XRLabel label1 = new XRLabel();
            XRLabel label2 = new XRLabel();
            XRPageBreak xrPageBreak0 = new XRPageBreak();

            // Add the controls to the Detail band.
            Detail.Controls.Add(label1);
            Detail.Controls.Add(label2);
            Detail.Controls.Add(xrPageBreak0);

            // Set the labels' text.
            label1.Text = "Label 1";
            label2.Text = "Label 2";

            // Set the location of the controls.

            // The first label is printed on the first page.
            label1.Location = new Point(100, 50);

            // Insert the page break.
            xrPageBreak0.Location = new Point(50, 150);

            // The second label is printed on the second page.
            label2.Location = new Point(100, 250);
        }
        /****************************************************************************************/
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            //DateTime now = this.dateTimePicker1.Value;
            //DateTime date = new DateTime(now.Year, now.Month, 1);
            ////date = date.AddMonths(-1);
            ////this.dateTimePicker1.Value = date;
            //int days = DateTime.DaysInMonth(date.Year, date.Month);
            //date = new DateTime(date.Year, date.Month, days);
            //this.dateTimePicker2.Value = date;
        }
        /****************************************************************************************/
        private void chkMonthly_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMonthly.Checked)
            {
                LoadTabAgents();
            }
            else
            {
                LoadTabAgents();
            }
        }
        /****************************************************************************************/
        private void gridMain3_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "PAYDATE8")
            {
                if (e.RowHandle >= 0)
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                    if (date.Year == 1)
                        e.DisplayText = "";
                }
            }
            else if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            this.Cursor = Cursors.WaitCursor;
            CustomerDetails clientForm = new CustomerDetails(contract);
            clientForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void LoadTabContractsByLocations()
        {
            DataTable ddt = (DataTable)dgv.DataSource;
            DataView tempview = ddt.DefaultView;
            //            tempview.Sort = "loc asc, agentName asc";
            tempview.Sort = "loc asc, agentNumber asc";
            DataTable dx = tempview.ToTable();

            //DataRow[] dRRR = ddt.Select("contractValue>'0'");
            //DataTable dx = ddt.Clone();
            //G1.ConvertToTable(dRRR, dx);
            DataTable dt = ddt.Clone();
            double dbc = 0D;
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                dbc = dx.Rows[i]["dbc"].ObjToDouble();
                //if (dbc > 0D)
                //    continue;
                dt.ImportRow(dx.Rows[i]);
            }

            tempview = dt.DefaultView;
            //            tempview.Sort = "loc asc, agentName asc";
            tempview.Sort = "loc asc, agentName asc";
            dt = tempview.ToTable();
            DataTable lDt = dt.Clone();

            //            lDt.Columns.Add("ibtrust", Type.GetType("System.Double"));
            //          lDt.Columns.Add("sptrust", Type.GetType("System.Double"));
            dt.Columns.Add("total", Type.GetType("System.Double"));

            //string lastLocation = "";
            //string lastAgent = "";
            //string lastAgentName = "";
            string location = "";
            //string agent = "";
            //string lloc = "";
            //double contractValue = 0D;
            //double downPayment = 0D;
            //double payment = 0D;
            double ibtrust = 0D;
            double sptrust = 0D;
            double xxtrust = 0D;
            string xtrust = "";
            //int idx = 0;
            //string ch;
            //string agentNumber = "";
            //string trust = "";
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    agent = dt.Rows[i]["agentName"].ObjToString();
            //    location = dt.Rows[i]["loc"].ObjToString();
            //    agentNumber = dt.Rows[i]["agentNumber"].ObjToString();
            //    if (String.IsNullOrWhiteSpace(lastLocation))
            //        lastLocation = location;
            //    if (String.IsNullOrWhiteSpace(lastAgent))
            //        lastAgent = agentNumber;
            //    if (String.IsNullOrWhiteSpace(lastAgentName))
            //        lastAgentName = agent;
            //    if (location != lastLocation)
            //    {
            //        DataRow drow = lDt.NewRow();
            //        drow["loc"] = lastLocation;
            //        drow["agentNumber"] = lastAgent;
            //        drow["agentName"] = lastAgentName;
            //        drow["downPayment"] = downPayment;
            //        drow["contractValue"] = contractValue;
            //        drow["paymentAmount"] = payment;
            //        drow["totalPayments"] = downPayment + payment;
            //        drow["ibtrust"] = ibtrust;
            //        drow["sptrust"] = sptrust;
            //        drow["total"] = ibtrust + sptrust;
            //        lDt.Rows.Add(drow);
            //        lastLocation = location;
            //        lastAgent = agentNumber;
            //        lastAgentName = agent;
            //        contractValue = 0D;
            //        payment = 0D;
            //        downPayment = 0D;
            //        ibtrust = 0D;
            //        sptrust = 0D;
            //        lloc = "";
            //    }
            //    else if (agentNumber != lastAgent)
            //    {
            //        DataRow drow = lDt.NewRow();
            //        drow["loc"] = lastLocation;
            //        drow["agentNumber"] = lastAgent;
            //        drow["agentName"] = lastAgentName;
            //        lDt.Rows.Add(drow);
            //        lastAgent = agentNumber;
            //        drow["downPayment"] = downPayment;
            //        drow["contractValue"] = contractValue;
            //        drow["paymentAmount"] = payment;
            //        drow["totalPayments"] = downPayment + payment;
            //        drow["ibtrust"] = ibtrust;
            //        drow["sptrust"] = sptrust;
            //        drow["total"] = ibtrust + sptrust;
            //        contractValue = 0D;
            //        payment = 0D;
            //        downPayment = 0D;
            //        ibtrust = 0D;
            //        sptrust = 0D;
            //    }
            //    contractValue += dt.Rows[i]["contractValue"].ObjToDouble();
            //    payment += dt.Rows[i]["paymentAmount"].ObjToDouble();
            //    downPayment += dt.Rows[i]["downPayment"].ObjToDouble();
            //    trust = dt.Rows[i]["trust"].ObjToString();
            //    if (trust.Length > 0)
            //    {
            //        idx = trust.Length - 1;
            //        ch = trust.Substring(idx);
            //        if (ch.ToUpper() == "I")
            //            ibtrust += dt.Rows[i]["contractValue"].ObjToDouble();
            //        else
            //            sptrust += dt.Rows[i]["contractValue"].ObjToDouble();
            //    }
            //    else
            //        sptrust += dt.Rows[i]["contractValue"].ObjToDouble();
            //}
            //DataRow ddr = lDt.NewRow();
            //ddr["loc"] = lastLocation;
            //ddr["agentNumber"] = lastAgent;
            //ddr["agentName"] = lastAgentName;
            //ddr["downPayment"] = downPayment;
            //ddr["contractValue"] = contractValue;
            //ddr["paymentAmount"] = payment;
            //ddr["totalPayments"] = downPayment + payment;
            //ddr["ibtrust"] = ibtrust;
            //ddr["sptrust"] = sptrust;
            //ddr["total"] = ibtrust + sptrust;
            //lDt.Rows.Add(ddr);

            DataTable dd = (DataTable)chkComboLocNames.Properties.DataSource;
            //DataTable dx = G1.get_db_data("Select * from `agents`;");

            //lastLocation = "";
            //bool first = true;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                location = dt.Rows[i]["loc"].ObjToString();
                DataRow[] dr = dd.Select("keycode='" + location + "'");
                if (dr.Length > 0)
                    dt.Rows[i]["Location Name"] = dr[0]["name"].ObjToString();
                dt.Rows[i]["total"] = dt.Rows[i]["ibtrust"].ObjToDouble() + dt.Rows[i]["sptrust"].ObjToDouble();
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                xxtrust = dt.Rows[i]["xxtrust"].ObjToDouble();
                if (xxtrust > 0D)
                    dt.Rows[i]["contractValue"] = 0D;
            }

            //lastLocation = "";
            //for (int i = (lDt.Rows.Count - 1); i >= 0; i--)
            //{
            //    location = lDt.Rows[i]["loc"].ObjToString();
            //    if (String.IsNullOrWhiteSpace(lastLocation))
            //        lastLocation = location;
            //    if (location != lastLocation)
            //    {
            //        DataRow dRow = lDt.NewRow();
            //        lDt.Rows.InsertAt(dRow, (i + 1));
            //        lastLocation = location;
            //    }
            //}

            DailyHistory.CleanupVisibility(gridMain7);
            gridMain7.Columns["ap"].Visible = false;

            G1.NumberDataTable(dt);
            dgv7.DataSource = dt;
        }
        /****************************************************************************************/
        private int footerCount = 0;
        private void gridMain7_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
            }
        }
        /****************************************************************************************/
        private void gridMain7_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
                if (footerCount >= 2)
                {
                    footerCount = 0;
                    e.PS.InsertPageBreak(e.Y);
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (chkFilterNewContracts.Checked)
            {
                ColumnView view = sender as ColumnView;
                //string contract = dt.Rows[row]["contractNumber"].ObjToString();
                //if ( contract == "B18033L")
                //{
                //    e.Visible = true;
                //    e.Handled = true;
                //    return;

                //}
                //                double contractValue = view.GetListSourceRowCellValue(e.ListSourceRow, "contractValue").ObjToDouble();
                double contractValue = dt.Rows[row]["contractValue"].ObjToDouble();
                if (contractValue > 0D)
                {
                    e.Visible = true;
                    e.Handled = true;
                    return;
                }
                string newcontract = dt.Rows[row]["newcontract"].ObjToString();
                double cashAdvance = dt.Rows[row]["cashAdvance"].ObjToDouble();
                if (newcontract == "1")
                    return;
                if (contractValue <= 0D && newcontract != "1" && cashAdvance == 0D)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
                if (contractValue <= 0D && cashAdvance > 0D)
                {
                    double downPayment = dt.Rows[row]["downPayment"].ObjToDouble();
                    if (downPayment <= 0D)
                    {
                        e.Visible = false;
                        e.Handled = true;
                        return;
                    }
                }
                return;
            }
            if (chkDBR.Checked)
            {
                string str = dt.Rows[row]["DBR"].ObjToString();
                if (str.ToUpper() != "DBR")
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
        }
        /****************************************************************************************/
        private void chkFilterNewContracts_CheckedChanged(object sender, EventArgs e)
        {
            dgv.RefreshDataSource();
            gridMain.RefreshData();
            dgv.Refresh();
            this.Refresh();
        }
        /****************************************************************************************/
        private void chkDBR_CheckedChanged(object sender, EventArgs e)
        {
            dgv.RefreshDataSource();
            gridMain.RefreshData();
            dgv.Refresh();
            this.Refresh();
        }
        /****************************************************************************************/
        private void chkNoSummary_CheckedChanged(object sender, EventArgs e)
        {
            LoadTabAgents();
        }
        /****************************************************************************************/
        private void excludeDBRFromTrustsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DevExpress.XtraGrid.GridControl MYdgv = dgv;
            if (dgv7.Visible)
                MYdgv = dgv7;
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)MYdgv.MainView;
            excludeDBRfromtrust(MYdgv, gridMain);
        }
        /****************************************************************************************/
        private void excludeDBRfromtrust(DevExpress.XtraGrid.GridControl dgv, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            string record = dr["record1"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
            {
                if (G1.get_column_number(dt, "xtrust") < 0)
                    dt.Columns.Add("xtrust");
                string xtrust = dt.Rows[row]["xtrust"].ObjToString();
                if (xtrust.ToUpper() == "Y")
                {
                    double ibtrust = 0D;
                    double sptrust = 0D;
                    xtrust = "";
                    string trust = dt.Rows[row]["trust"].ObjToString();
                    if (trust.Length > 0)
                    {
                        int idx = trust.Length - 1;
                        string ch = trust.Substring(idx);
                        if (ch.ToUpper() == "I")
                            ibtrust = dt.Rows[row]["contractValue"].ObjToDouble();
                        else
                            sptrust = dt.Rows[row]["contractValue"].ObjToDouble();
                    }
                    else
                        sptrust = dt.Rows[row]["contractValue"].ObjToDouble();
                    dt.Rows[row]["ibtrust"] = ibtrust;
                    dt.Rows[row]["sptrust"] = sptrust;
                    dt.Rows[row]["xxtrust"] = 0D;
                    dt.Rows[row]["xtrust"] = "";
                }
                else
                {
                    xtrust = "Y";
                    dt.Rows[row]["xtrust"] = "Y";
                    double ibtrust = dt.Rows[row]["ibtrust"].ObjToDouble();
                    double sptrust = dt.Rows[row]["sptrust"].ObjToDouble();
                    double xxtrust = ibtrust + sptrust;
                    dt.Rows[row]["xxtrust"] = xxtrust;
                    dt.Rows[row]["ibtrust"] = 0D;
                    dt.Rows[row]["sptrust"] = 0D;
                }

                G1.update_db_table("contracts", "record", record, new string[] { "xtrust", xtrust });

                dgv.RefreshDataSource();
                gridMain.RefreshData();
                dgv.Refresh();
                this.Refresh();
            }
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            if (e.Column.FieldName.Trim().ToUpper() == "CASHADVANCE")
            {
                string contract = dr["contractNumber"].ObjToString();
                if (!String.IsNullOrWhiteSpace(contract))
                {
                    string record = dr["record1"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(record))
                    {
                        double cash = dr["cashAdvance"].ObjToDouble();
                        double contractValue = DailyHistory.GetContractValue(contract);
                        contractValue = G1.RoundValue(contractValue);
                        dr["contractValue"] = contractValue - cash;
                        G1.update_db_table("contracts", "record", record, new string[] { "cashAdvance", cash.ToString() });
                        int rowHandle = gridMain.FocusedRowHandle;
                        int row = gridMain.GetDataSourceRowIndex(rowHandle);
                        DataTable dt = (DataTable)dgv.DataSource;
                        dt.Rows[row]["cashAdvance"] = cash;
                        dt.Rows[row]["contractValue"] = contractValue - cash;
                        dt.AcceptChanges();
                        RecalculateTrusts(dt);
                        dgv.DataSource = dt;
                        dgv.Refresh();
                    }
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            if (e.IsTotalSummary)
            {
                e.TotalValue = 123.45;
                e.TotalValueReady = true;
            }
        }
        /****************************************************************************************/
        private void Recap(object sender, DevExpress.Data.CustomSummaryExistEventArgs e)
        {
            if (e.IsTotalSummary)
            {
                e.Exists = true;
            }
        }
        /****************************************************************************************/
        private void trustReportsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = null;
            if (dgv.DataSource != null)
                dt = (DataTable)dgv.DataSource;
            if (originalDt != null)
                dt = originalDt;
            DateTime date2 = this.dateTimePicker2.Value;
            DateTime date4 = this.dateTimePicker4.Value;
            if (date4 > date2)
                date2 = date4;

            TrustReports trustForm = new TrustReports(customerDt, dt, date2);
            trustForm.Show();
        }
        /****************************************************************************************/
        private void historicCommissionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HistoricCommissions histForm = new HistoricCommissions();
            histForm.Show();
        }
        /****************************************************************************************/
        private void gridMain8_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0)
            {
                if (e.RowHandle >= 0)
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                    if (date.Year == 1)
                        e.DisplayText = "";
                }
            }
        }
        /****************************************************************************************/
        private void gridMain9_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0)
            {
                if (e.RowHandle >= 0)
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                    if (date.Year == 1)
                        e.DisplayText = "";
                }
            }
        }
        /****************************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;
            SelectColumns sform = new SelectColumns(dgv, "Trust85", "Primary", actualName);
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
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = 'Trust85' order by seq";
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
                procType = "Trust85";
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
        /****************************************************************************************/
        private void btnRunCommissions_Click(object sender, EventArgs e)
        {
            bool split = chkDoSplits.Checked;
            this.Cursor = Cursors.WaitCursor;
            RunCommissions(true, split);
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable holdCommissionsDt = null;
        private void chkConsolidate_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;

            if (!chkConsolidate.Checked)
            {
                holdCommissionsDt = null;
                btnPrintAll.Hide();
                btnChart.Hide();
                btnRunCommissions_Click(null, null);
                return;
            }

            holdCommissionsDt = (DataTable)dgv10.DataSource;
            btnPrintAll.Show();
            btnChart.Show();
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = (DataTable)dgv10.DataSource;
            dt = Commission.ConsolidateCommissions(dt);
            dgv10.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void CombineData(DataTable dt, int oldrow, int row, string field)
        {
            if (G1.get_column_number(dt, field) < 0)
                return;
            string type = "";
            try
            {
                type = dt.Columns[field].DataType.ToString().ToUpper();
                if (type.ToUpper() == "MYSQL.DATA.TYPES.MYSQLDATETIME")
                    return;
                if (type.ToUpper().IndexOf("DATETIME") >= 0)
                    return;
                if (type.ToUpper().IndexOf("INT64") >= 0)
                    return;
                if (type.ToUpper().IndexOf("INT32") >= 0)
                    return;
                if (type.IndexOf("DOUBLE") >= 0 || type.IndexOf("DECIMAL") >= 0)
                {
                    double oldValue = dt.Rows[oldrow][field].ObjToDouble();
                    double newValue = dt.Rows[row][field].ObjToDouble();
                    dt.Rows[oldrow][field] = oldValue + newValue;
                }
                else
                {
                    string oldValue = dt.Rows[oldrow][field].ObjToString();
                    string newValue = dt.Rows[row][field].ObjToString();
                    if (!String.IsNullOrWhiteSpace(oldValue))
                        oldValue += "+";
                    dt.Rows[oldrow][field] = oldValue + newValue;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Combining Data");
            }
        }
        /****************************************************************************************/
        private void btnSaveCommissions_Click(object sender, EventArgs e)
        {
            //if (chkConsolidate.Checked)
            //{
            //    MessageBox.Show("***ERROR*** Commissions must be calculated and split but not consolidated in order to save Commissions!");
            //    return;
            //}
            if (!chkDoSplits.Checked)
            {
                MessageBox.Show("***ERROR*** Commissions must be calculated and split in order to save Commissions!");
                return;
            }
            DateTime endDate = this.dateTimePicker2.Value;
            DateTime now = DateTime.Now;
            TimeSpan ts = now - endDate;
            if (ts.TotalDays > 10)
            {
                if (!G1.ValidateOverridePassword("Enter Password To Override Previously Saved Commissions > "))
                {
                    return;
                }
            }

            this.Cursor = Cursors.WaitCursor;
            string runNumber = EstablishLapseReinstateRecord();
            if (String.IsNullOrWhiteSpace(runNumber))
                return;
            if (1 == 1)
            {
                DataTable dt8 = (DataTable)dgv8.DataSource;
                try
                {
                    MySQL.LapsedMySQL(dt8, "lapsetable", runNumber);
                }
                catch (Exception ex)
                {
                }
                DataTable dt9 = (DataTable)dgv9.DataSource;
                try
                {
                    MySQL.LapsedMySQL(dt9, "reinstatetable", runNumber);
                }
                catch (Exception ex)
                {
                }

                DataTable dtTrust = (DataTable)dgv.DataSource;
                try
                {
                    DataTable subDt = dtTrust.Copy();
                    int col = G1.get_column_number(subDt, "Split DownPayment");
                    for (int i = subDt.Columns.Count - 1; i > col; i--)
                        subDt.Columns.RemoveAt(i);
                    MySQL.TrustMySQL(subDt, "trustdetail", runNumber);
                }
                catch (Exception ex)
                {
                }
            }
            //DateTime startDate = this.dateTimePicker1.Value;
            //DateTime stopDate = this.dateTimePicker2.Value;
            DataTable dt = (DataTable)dgv10.DataSource;
            try
            {
                MySQL.ImportMySQL(dt, "historic_commissions", runNumber);
            }
            catch (Exception ex)
            {
            }
            //dt = (DataTable)dgv.DataSource;
            //DataTable subDt = dt.Copy();
            //int col = G1.get_column_number(subDt, "Split DownPayment");
            //for (int i = subDt.Columns.Count - 1; i > col; i--)
            //    subDt.Columns.RemoveAt(i);
            //MySQL.ImportMySQL(subDt, "trustdetail", runNumber);
            ////Commission.LockCommissions(dt, startDate, stopDate);
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private string EstablishLapseReinstateRecord()
        {
            string record = "";
            DateTime date = this.dateTimePicker1.Value;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = this.dateTimePicker2.Value;
            string date2 = G1.DateTimeToSQLDateTime(date);
            string cmd = "Select * from `lapse_reinstates` where `startDate` >= '" + date1 + "' and `endDate` <= '" + date2 + "'";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                record = G1.create_record("lapse_reinstates", "endDate", "0000-00-00");
                if (G1.BadRecord("lapse_reinstates", record))
                    return "";
                G1.update_db_table("lapse_reinstates", "record", record, new string[] { "startDate", date1, "endDate", date2 });
            }
            else
            {
                record = dx.Rows[0]["record"].ObjToString();
            }
            return record;
        }
        /****************************************************************************************/
        private void gridMain8_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain8.GetFocusedDataRow();
            DateTime date = dr["issueDate8"].ObjToDateTime();
            date = new DateTime(date.Year, date.Month, 1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime date2 = new DateTime(date.Year, date.Month, days);
            string agentName = dr["agentName"].ObjToString();
            this.Cursor = Cursors.WaitCursor;
            Trust85 trustForm = new Trust85(date, date2, agentName);
            trustForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void gridMain9_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain9.GetFocusedDataRow();
            DateTime date = dr["issueDate8"].ObjToDateTime();
            date = new DateTime(date.Year, date.Month, 1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime date2 = new DateTime(date.Year, date.Month, days);
            string agentName = dr["agentName"].ObjToString();
            this.Cursor = Cursors.WaitCursor;
            Trust85 trustForm = new Trust85(date, date2, agentName);
            trustForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void GoToDailyHistory(string contract)
        {
            this.Cursor = Cursors.WaitCursor;
            CustomerDetails clientForm = new CustomerDetails(contract);
            clientForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void gridMain3_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain3.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (chkShowCommissions.Checked)
            {
                GoToDailyHistory(contract);
                return;
            }
            int idx = contract.IndexOf("(L");
            if (idx < 0)
            {
                idx = contract.IndexOf("(R");
                return;
            }
            contract = contract.Substring(idx + 2);
            contract = contract.Replace(")", "");
            if (!G1.validate_date(contract))
                return;

            DateTime date = contract.ObjToDateTime();
            date = new DateTime(date.Year, date.Month, 1);
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime date2 = new DateTime(date.Year, date.Month, days);
            string agentName = dr["agentName"].ObjToString();
            this.Cursor = Cursors.WaitCursor;
            Trust85 trustForm = new Trust85(date, date2, agentName);
            trustForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void cmbSelectCommission_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetupSelectedColumns("TrustCommission", "commissions", dgv10);
        }
        /****************************************************************************************/
        private void btnSelectCommission_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectCommission.Text;
            SelectColumns sform = new SelectColumns(dgv10, "TrustCommission", "commissions", actualName);
            sform.Done += new SelectColumns.d_void_eventdone(sform_DoneCommission);
            sform.Show();
        }
        /***********************************************************************************************/
        void sform_DoneCommission()
        {
            dgv10.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        public static void LoadTrustLocations(DataTable lDt)
        {
            string contract = "";
            string trust = "";
            string loc = "";

            for (int i = 0; i < lDt.Rows.Count; i++)
            {
                contract = lDt.Rows[i]["contractNumber"].ObjToString();
                contract = Trust85.decodeContractNumber(contract, ref trust, ref loc);
                lDt.Rows[i]["loc"] = loc;
            }

            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable dd = G1.get_db_data(cmd);
            string location = "";
            for (int i = 0; i < lDt.Rows.Count; i++)
            {
                location = lDt.Rows[i]["loc"].ObjToString();
                DataRow[] dr = dd.Select("keycode='" + location + "'");
                if (dr.Length > 0)
                    lDt.Rows[i]["Location Name"] = dr[0]["name"].ObjToString();
                else
                    lDt.Rows[i]["Location Name"] = location;
            }
        }
        /***********************************************************************************************/
        private DataTable PullAllData()
        {
            string Y2002 = "";
            if (chk2002.Checked)
                Y2002 = "2002";

            string cmd = "Select * from `trust2013` a JOIN `customers` c ON a.`contractNumber` = c.`contractNumber` ";
            if (chk2002.Checked)
                cmd += " where `Is2002` = '2002' ";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("num");
            dt.Columns.Add("fullname");
            dt.Columns.Add("loc");
            dt.Columns.Add("Location Name");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["fullname"] = dt.Rows[i]["firstName"].ObjToString().Trim() + " " + dt.Rows[i]["lastName"].ObjToString().Trim();

            cmd = "Select * from `trust2013` ";
            if (chk2002.Checked)
                cmd += " where `Is2002` = '2002' ";
            cmd += ";";

            //DataTable dx = G1.get_db_data(cmd); // This is because some of the contracts in trust2013 does not exist in the o
            //dx.Columns.Add("fullname");
            //for (int i = 0; i < dx.Rows.Count; i++)
            //    dx.Rows[i]["fullname"] = dx.Rows[i]["firstName"].ObjToString().Trim() + " " + dx.Rows[i]["lastName"].ObjToString().Trim();

            //string contractNumber = "";
            //DataRow[] dR = null;
            //for (int i = 0; i < dx.Rows.Count; i++)
            //{
            //    contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
            //    dR = dt.Select("contractNumber = '" + contractNumber + "'");
            //    if (dR.Length <= 0)
            //        dt.ImportRow(dx.Rows[i]);
            //}

            LoadTrustLocations(dt);
            return dt;
        }
        /****************************************************************************************/
        private void btnRunDiff_Click(object sender, EventArgs e)
        {
            DateTime workDate2 = this.dateTimePicker2.Value;
            progressBar1.Show();
            label7.Show();

            this.Cursor = Cursors.WaitCursor;
            string contractNumber = "";
            string cmd = "";
            DateTime lastDate = DateTime.Now;
            DateTime payDate8 = DateTime.Now;
            DateTime deceasedDate = DateTime.Now;
            DateTime dueDate8 = DateTime.Now;
            int month = 0;
            int year = 0;
            int day = 0;
            double startBalance = 0D;
            double beginningBalance = 0D;
            double endingBalance = 0D;
            double removals = 0D;
            double value = 0D;
            double difference = 0D;
            double trust85 = 0D;
            double trust100 = 0D;
            double oldTrust85 = 0D;
            string issueDate = "";
            DateTime iDate = DateTime.Now;
            double payment = 0D;
            int numPayments = 0;
            int method = 0;
            DataTable dx = null;
            DataTable dp = null;

            DataTable dt = PullAllData(); //Some of the data in Trust2013 is not in customer file

            dt.Columns.Add("contractValue", Type.GetType("System.Double"));
            dt.Columns.Add("apr", Type.GetType("System.Double"));
            dt.Columns.Add("calcTrust85", Type.GetType("System.Double"));
            dt.Columns.Add("difference", Type.GetType("System.Double"));
            dt.Columns.Add("issueDate");
            dt.Columns.Add("DD");
            dt.Columns.Add("Pmts");
            dt.Columns.Add("method");

            label7.Show();

            label7.Text = "of " + dt.Rows.Count.ToString();
            label7.Refresh();

            progressBar1.Show();
            progressBar1.Minimum = 0;
            progressBar1.Maximum = dt.Rows.Count;

            double Trust85Calc = 0D;
            double Trust85Paid = 0D;
            double Trust85Real = 0D;

            int i = 0;
            double contractValue = 0D;
            double rate = 0D;
            try
            {
                int lastRow = dt.Rows.Count;
                //                lastRow = 50;
                for (i = 0; i < lastRow; i++)
                {
                    label7.Text = (i + 1).ToString() + " of " + dt.Rows.Count.ToString();
                    label7.Refresh();

                    progressBar1.Value = i + 1;
                    progressBar1.Refresh();
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber == "P14152UI")
                    {
                    }
                    beginningBalance = dt.Rows[i]["beginningBalance"].ObjToDouble();
                    endingBalance = dt.Rows[i]["endingBalance"].ObjToDouble();
                    removals = dt.Rows[i]["currentRemovals"].ObjToDouble();

                    cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                    {
                        dt.Rows[i]["method"] = "BAD";
                        dt.Rows[i]["calcTrust85"] = 0D;
                        dt.Rows[i]["difference"] = endingBalance;
                        cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            iDate = dx.Rows[0]["contractDate"].ObjToDateTime();
                            if (iDate.Year > 1850)
                                dt.Rows[i]["issueDate"] = issueDate;
                            deceasedDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                            if (deceasedDate.Year > 1850)
                                dt.Rows[i]["DD"] = deceasedDate.ToString("MM/dd/yyyy");
                        }
                        continue;
                    }

                    contractValue = DailyHistory.GetContractValuePlus(dx.Rows[0]);
                    rate = dx.Rows[0]["apr"].ObjToDouble();

                    dt.Rows[i]["contractValue"] = contractValue;
                    dt.Rows[i]["apr"] = rate;

                    payment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
                    numPayments = dx.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
                    issueDate = dx.Rows[0]["issueDate8"].ObjToString();
                    iDate = DailyHistory.GetIssueDate(dx.Rows[0]["issueDate8"].ObjToDateTime(), contractNumber, dx);
                    issueDate = iDate.ToString("MM/dd/yyyy");
                    lastDate = issueDate.ObjToDateTime();
                    if (issueDate.IndexOf("0000") >= 0)
                    {
                        dt.Rows[i]["calcTrust85"] = 0D;
                        dt.Rows[i]["difference"] = endingBalance;
                        continue;
                    }
                    dt.Rows[i]["issueDate"] = issueDate;
                    deceasedDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                    if (deceasedDate.Year > 1850)
                        dt.Rows[i]["DD"] = deceasedDate.ToString("MM/dd/yyyy");
                    dt.Rows[i]["Pmts"] = numPayments.ToString();


                    //if (1 == 1)
                    //    break;

                    Trust85Calc = 0D;
                    Trust85Paid = 0D;
                    Trust85Real = 0D;

                    method = CalcTrust85Data(contractNumber, workDate2, ref Trust85Calc, ref Trust85Paid, ref Trust85Real);

                    dt.Rows[i]["calcTrust85"] = Trust85Calc;
                    if (endingBalance == 0D && removals > 0D)
                        endingBalance = removals;
                    if (beginningBalance == 0D && endingBalance == 0D)
                        endingBalance = Trust85Paid;
                    difference = endingBalance - Trust85Calc;
                    dt.Rows[i]["difference"] = difference;
                    dt.Rows[i]["method"] = method.ToString();
                    //cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
                    //dx = G1.get_db_data(cmd);
                    //if (dx.Rows.Count <= 0)
                    //{
                    //    dt.Rows[i]["calcTrust85"] = 0D;
                    //    dt.Rows[i]["difference"] = endingBalance;
                    //    continue;
                    //}

                    //DataTable contractDt = dx.Copy();

                    //contractValue = DailyHistory.GetContractValuePlus(dx.Rows[0]);
                    //rate = dx.Rows[0]["apr"].ObjToDouble();

                    //dt.Rows[i]["contractValue"] = contractValue;
                    //dt.Rows[i]["apr"] = rate;

                    //double payment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
                    //int numPayments = dx.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
                    //double totalInterest = dx.Rows[0]["totalInterest"].ObjToString().ObjToDouble();
                    //string dueDate = dx.Rows[0]["dueDate8"].ObjToString();
                    //dueDate8 = dueDate.ObjToDateTime();
                    //string issueDate = dx.Rows[0]["issueDate8"].ObjToString();
                    //DateTime iDate = DailyHistory.GetIssueDate(dx.Rows[0]["issueDate8"].ObjToDateTime(), contractNumber, dx);
                    //issueDate = iDate.ToString("MM/dd/yyyy");
                    //lastDate = issueDate.ObjToDateTime();
                    //if (issueDate.IndexOf("0000") >= 0)
                    //{
                    //    dt.Rows[i]["calcTrust85"] = 0D;
                    //    dt.Rows[i]["difference"] = endingBalance;
                    //    continue;
                    //}
                    //dt.Rows[i]["issueDate"] = issueDate;
                    //deceasedDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                    //if (deceasedDate.Year > 1850)
                    //    dt.Rows[i]["DD"] = deceasedDate.ToString("MM/dd/yyyy");
                    //dt.Rows[i]["Pmts"] = numPayments.ToString();

                    //string apr = dx.Rows[0]["APR"].ObjToString();
                    //double dAPR = apr.ObjToDouble() / 100.0D;

                    //startBalance = DailyHistory.GetFinanceValue(dx.Rows[0]);

                    //cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' order by `paydate8` DESC, `tmstamp` DESC;";
                    //dp = G1.get_db_data(cmd);

                    //if (dueDate8.Year >= 2039)
                    //{
                    //    trust85 = 0D;
                    //    trust100 = 0D;
                    //    oldTrust85 = 0D;
                    //    method = DailyHistory.CalcTrust85Max(dx, dp, ref trust85, ref trust100, ref oldTrust85);
                    //    dt.Rows[i]["calcTrust85"] = trust85;
                    //    if (endingBalance == 0D && removals > 0D)
                    //        endingBalance = removals;
                    //    if (beginningBalance == 0D && endingBalance == 0D)
                    //        endingBalance = oldTrust85;
                    //    difference = endingBalance - trust85;
                    //    dt.Rows[i]["difference"] = difference;
                    //    dt.Rows[i]["method"] = method.ToString();
                    //    continue;
                    //}

                    //DailyHistory.CalculateNewStuff(dp, dAPR, numPayments, startBalance, lastDate);

                    //method = 0;

                    //if (dp.Rows.Count > 0)
                    //{
                    //    trust85 = 0D;
                    //    method = dp.Rows[0]["method"].ObjToInt32();
                    //    for (int j = 0; j < dp.Rows.Count; j++)
                    //    {
                    //        if (dp.Rows[j]["fill"].ObjToString().ToUpper() == "D")
                    //            continue;
                    //        payDate8 = dp.Rows[j]["payDate8"].ObjToDateTime();
                    //        month = payDate8.Month;
                    //        year = payDate8.Year;
                    //        day = DateTime.DaysInMonth(year, month);
                    //        payDate8 = new DateTime(year, month, day);
                    //        if (payDate8 > workDate2)
                    //            continue;

                    //        value = dp.Rows[j]["calculatedTrust85"].ObjToDouble();
                    //        trust85 += value;
                    //        //if (numPayments <= 0)
                    //        //    break;
                    //    }
                    //    dt.Rows[i]["calcTrust85"] = trust85;
                    //    if (endingBalance == 0D && removals > 0D)
                    //        endingBalance = removals;
                    //    if (beginningBalance == 0D && endingBalance == 0D)
                    //        endingBalance = oldTrust85;
                    //    difference = endingBalance - trust85;
                    //    dt.Rows[i]["difference"] = difference;
                    //    dt.Rows[i]["method"] = method.ToString();
                    //}
                    //else
                    //{
                    //    dt.Rows[i]["calcTrust85"] = 0D;
                    //    dt.Rows[i]["difference"] = endingBalance;
                    //    dt.Rows[i]["method"] = method.ToString();
                    //}
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }
            G1.NumberDataTable(dt);
            dgv11.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void chkShowLocations_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowLocations.Checked)
            {
                gridMain11.Columns["Location Name"].GroupIndex = 0;
                gridMain11.Columns["num"].Visible = false;
                chkExpand.Show();
            }
            else
            {
                gridMain11.Columns["Location Name"].GroupIndex = -1;
                gridMain11.Columns["num"].Visible = true;
                chkExpand.Checked = true;
                chkExpand.Hide();
            }
        }
        /****************************************************************************************/
        private void chkExpand_CheckedChanged(object sender, EventArgs e)
        {
            if (chkExpand.Checked)
            {
                gridMain11.OptionsBehavior.AutoExpandAllGroups = true;
                gridMain11.ExpandAllGroups();
                gridMain11.OptionsPrint.ExpandAllGroups = true;
                gridMain11.OptionsPrint.PrintGroupFooter = true;
            }
            else
            {
                gridMain11.OptionsBehavior.AutoExpandAllGroups = false;
                gridMain11.CollapseAllGroups();
                gridMain11.OptionsPrint.ExpandAllGroups = false;
                gridMain11.OptionsPrint.PrintGroupFooter = true;
            }
        }
        /****************************************************************************************/
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            dgv11.RefreshDataSource();
            gridMain11.RefreshData();
            dgv11.Refresh();
            this.Refresh();
        }
        /****************************************************************************************/
        private void gridMain11_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv11.DataSource;
            if (checkBox1.Checked)
            {
                double beginningBalance = dt.Rows[row]["beginningBalance"].ObjToDouble();
                double endingBalance = dt.Rows[row]["endingBalance"].ObjToDouble();
                double calcTrust85 = dt.Rows[row]["calcTrust85"].ObjToDouble();
                if (beginningBalance == 0D && endingBalance == 0D && calcTrust85 == 0D)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
            if (chkOnlyCurrent.Checked)
            {
                double paymentCurrMonth = dt.Rows[row]["paymentCurrMonth"].ObjToDouble();
                double currentRemovals = dt.Rows[row]["currentRemovals"].ObjToDouble();
                if (paymentCurrMonth == 0D && currentRemovals == 0D)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
        }
        /****************************************************************************************/
        private void gridMain11_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
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
        /****************************************************************************************/
        private void gridMain11_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain11.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Contract does not exist in MySQL!");
                return;
            }
            cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Customer does not exist in MySQL!");
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            CustomerDetails clientForm = new CustomerDetails(contract);
            clientForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void chkOnlyCurrent_CheckedChanged(object sender, EventArgs e)
        {
            dgv11.RefreshDataSource();
            gridMain11.RefreshData();
            dgv11.Refresh();
            this.Refresh();
        }
        ///****************************************************************************************/
        //private void recalcTrust85ToolStripMenuItem_ClickX(object sender, EventArgs e)
        //{
        //    DataRow dr = gridMain11.GetFocusedDataRow();
        //    string contract = dr["contractNumber"].ObjToString();
        //    DateTime workDate2 = this.dateTimePicker2.Value;

        //    string contractNumber = contract;
        //    string cmd = "";
        //    DateTime lastDate = DateTime.Now;
        //    DateTime payDate8 = DateTime.Now;
        //    DateTime deceasedDate = DateTime.Now;
        //    DateTime dueDate8 = DateTime.Now;
        //    int month = 0;
        //    int year = 0;
        //    int day = 0;
        //    double startBalance = 0D;
        //    double beginningBalance = 0D;
        //    double endingBalance = 0D;
        //    double removals = 0D;
        //    double value = 0D;
        //    double difference = 0D;
        //    double trust85 = 0D;
        //    double trust100 = 0D;
        //    double oldTrust85 = 0D;
        //    double contractValue = 0D;
        //    double rate = 0D;
        //    int method = 0;
        //    DataTable dx = null;
        //    DataTable dp = null;

        //    beginningBalance = dr["beginningBalance"].ObjToDouble();
        //    endingBalance = dr["endingBalance"].ObjToDouble();
        //    removals = dr["currentRemovals"].ObjToDouble();

        //    cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
        //    dx = G1.get_db_data(cmd);
        //    if (dx.Rows.Count <= 0)
        //    {
        //        dr["calcTrust85"] = 0D;
        //        dr["difference"] = endingBalance;
        //        return;
        //    }

        //    DataTable contractDt = dx.Copy();

        //    contractValue = DailyHistory.GetContractValuePlus(dx.Rows[0]);
        //    rate = dx.Rows[0]["apr"].ObjToDouble();

        //    dr["contractValue"] = contractValue;
        //    dr["apr"] = rate;

        //    double payment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
        //    int numPayments = dx.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
        //    double totalInterest = dx.Rows[0]["totalInterest"].ObjToString().ObjToDouble();
        //    string dueDate = dx.Rows[0]["dueDate8"].ObjToString();
        //    dueDate8 = dueDate.ObjToDateTime();
        //    string issueDate = dx.Rows[0]["issueDate8"].ObjToString();
        //    DateTime iDate = DailyHistory.GetIssueDate(dx.Rows[0]["issueDate8"].ObjToDateTime(), contractNumber, dx);
        //    issueDate = iDate.ToString("MM/dd/yyyy");
        //    lastDate = issueDate.ObjToDateTime();
        //    if (issueDate.IndexOf("0000") >= 0)
        //    {
        //        dr["calcTrust85"] = 0D;
        //        dr["difference"] = endingBalance;
        //        return;
        //    }
        //    dr["issueDate"] = issueDate;
        //    deceasedDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
        //    if (deceasedDate.Year > 1850)
        //        dr["DD"] = deceasedDate.ToString("MM/dd/yyyy");
        //    dr["Pmts"] = numPayments.ToString();

        //    string apr = dx.Rows[0]["APR"].ObjToString();
        //    double dAPR = apr.ObjToDouble() / 100.0D;

        //    startBalance = DailyHistory.GetFinanceValue(dx.Rows[0]);

        //    cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' order by `paydate8` DESC, `tmstamp` DESC;";
        //    dp = G1.get_db_data(cmd);

        //    if (dueDate8.Year >= 2039)
        //    {
        //        trust85 = 0D;
        //        trust100 = 0D;
        //        oldTrust85 = 0D;
        //        method = DailyHistory.CalcTrust85Max(dx, dp, ref trust85, ref trust100, ref oldTrust85);
        //        dr["calcTrust85"] = trust85;
        //        if (endingBalance == 0D && removals > 0D)
        //            endingBalance = removals;
        //        if (beginningBalance == 0D && endingBalance == 0D)
        //            endingBalance = oldTrust85;
        //        difference = endingBalance - trust85;
        //        dr["difference"] = difference;
        //        dr["method"] = method.ToString();
        //        return;
        //    }

        //    DailyHistory.CalculateNewStuff(dp, dAPR, numPayments, startBalance, lastDate);

        //    method = 0;

        //    if (dp.Rows.Count > 0)
        //    {
        //        trust85 = 0D;
        //        method = dp.Rows[0]["method"].ObjToInt32();
        //        for (int j = 0; j < dp.Rows.Count; j++)
        //        {
        //            if (dp.Rows[j]["fill"].ObjToString().ToUpper() == "D")
        //                continue;
        //            payDate8 = dp.Rows[j]["payDate8"].ObjToDateTime();
        //            month = payDate8.Month;
        //            year = payDate8.Year;
        //            day = DateTime.DaysInMonth(year, month);
        //            payDate8 = new DateTime(year, month, day);
        //            if (payDate8 > workDate2)
        //                continue;

        //            value = dp.Rows[j]["calculatedTrust85"].ObjToDouble();
        //            trust85 += value;
        //            //if (numPayments <= 0)
        //            //    break;
        //        }
        //        dr["calcTrust85"] = trust85;
        //        if (endingBalance == 0D && removals > 0D)
        //            endingBalance = removals;
        //        if (beginningBalance == 0D && endingBalance == 0D)
        //            endingBalance = oldTrust85;
        //        difference = endingBalance - trust85;
        //        dr["difference"] = difference;
        //        dr["method"] = method.ToString();
        //    }
        //    else
        //    {
        //        dr["calcTrust85"] = 0D;
        //        dr["difference"] = endingBalance;
        //        dr["method"] = method.ToString();
        //    }
        //}
        /****************************************************************************************/
        public static void CalcTrust85Total(string contractNumber, DateTime workDate2, ref double Trust85Real, ref double Trust85Max, ref double balanceDue)
        {
            Trust85Real = 0D;
            Trust85Max = 0D;
            balanceDue = 0;
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            DataTable contractDt = dx.Copy();

            double payment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
            int numPayments = dx.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
            double totalInterest = dx.Rows[0]["totalInterest"].ObjToString().ObjToDouble();
            string dueDate = dx.Rows[0]["dueDate8"].ObjToString();
            string issueDate = dx.Rows[0]["issueDate8"].ObjToString();
            DateTime iDate = DailyHistory.GetIssueDate(dx.Rows[0]["issueDate8"].ObjToDateTime(), contractNumber, dx);
            issueDate = iDate.ToString("MM/dd/yyyy");
            DateTime lastDate = issueDate.ObjToDateTime();
            string apr = dx.Rows[0]["APR"].ObjToString();
            double dAPR = apr.ObjToDouble() / 100.0D;
            double contractValue = DailyHistory.GetContractValue(dx.Rows[0]);
            Trust85Max = contractValue * 0.85D;
            Trust85Max = G1.RoundValue(Trust85Max);

            cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' order by `paydate8` DESC, `tmstamp` DESC;";
            dx = G1.get_db_data(cmd);

            if (numPayments <= 0 && dx.Rows.Count > 0)
                numPayments = dx.Rows.Count;
            double startBalance = DailyHistory.GetFinanceValue(contractNumber);

            DailyHistory.CalculateNewStuff(dx, dAPR, numPayments, startBalance, lastDate);

            if (dx.Rows.Count > 0)
                balanceDue = dx.Rows[0]["newBalance"].ObjToDouble();
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                iDate = dx.Rows[i]["payDate8"].ObjToDateTime();
                if (iDate >= workDate2)
                    continue;
                Trust85Real += dx.Rows[i]["calculatedTrust85"].ObjToDouble();
                balanceDue = dx.Rows[i]["newBalance"].ObjToDouble();
            }
            return;
        }
        /****************************************************************************************/
        public static void CalcTrustBalance(string contractNumber, DateTime workDate2, ref double balanceDue)
        {
            balanceDue = 0;
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            DataTable contractDt = dx.Copy();

            double payment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
            int numPayments = dx.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
            double totalInterest = dx.Rows[0]["totalInterest"].ObjToString().ObjToDouble();
            string dueDate = dx.Rows[0]["dueDate8"].ObjToString();
            string issueDate = dx.Rows[0]["issueDate8"].ObjToString();
            DateTime iDate = DailyHistory.GetIssueDate(dx.Rows[0]["issueDate8"].ObjToDateTime(), contractNumber, dx);
            issueDate = iDate.ToString("MM/dd/yyyy");
            DateTime lastDate = issueDate.ObjToDateTime();
            string apr = dx.Rows[0]["APR"].ObjToString();
            double dAPR = apr.ObjToDouble() / 100.0D;
            double contractValue = DailyHistory.GetContractValue(dx.Rows[0]);
            double Trust85Max = contractValue * 0.85D;
            Trust85Max = G1.RoundValue(Trust85Max);
            if (contractNumber == "HT16090UI")
            {
            }

            cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' order by `paydate8` DESC, `tmstamp` DESC;";
            dx = G1.get_db_data(cmd);

            if (numPayments <= 0 && dx.Rows.Count > 0)
                numPayments = dx.Rows.Count;
            double startBalance = DailyHistory.GetFinanceValue(contractNumber);

            DailyHistory.CalculateNewStuff(dx, dAPR, numPayments, startBalance, lastDate);

            FilterDeletedPayments(dx);

            if (dx.Rows.Count > 0)
                balanceDue = dx.Rows[0]["newBalance"].ObjToDouble();
            string fill = "";
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                fill = dx.Rows[i]["fill"].ObjToString().ToUpper();
                if (fill == "D")
                    continue;
                iDate = dx.Rows[i]["payDate8"].ObjToDateTime();
                if (iDate <= workDate2)
                    break;
                balanceDue = dx.Rows[i]["newBalance"].ObjToDouble();
            }
            return;
        }
        /****************************************************************************************/
        private void recalcTrust85ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain11.GetFocusedDataRow();
            DateTime workDate = this.dateTimePicker2.Value;
            string contractNumber = dr["contractNumber"].ObjToString();
            double beginningBalance = dr["beginningBalance"].ObjToDouble();
            double endingBalance = dr["endingBalance"].ObjToDouble();
            double removals = dr["currentRemovals"].ObjToDouble();
            double Trust85Calc = 0D;
            double Trust85Paid = 0D;
            double Trust85Real = 0D;
            DateTime workDate2 = this.dateTimePicker2.Value;

            int method = CalcTrust85Data(contractNumber, workDate2, ref Trust85Calc, ref Trust85Paid, ref Trust85Real);

            dr["calcTrust85"] = Trust85Calc;
            if (endingBalance == 0D && removals > 0D)
                endingBalance = removals;
            if (beginningBalance == 0D && endingBalance == 0D)
                endingBalance = Trust85Paid;

            double difference = endingBalance - Trust85Calc;
            dr["difference"] = difference;
            dr["method"] = method.ToString();

        }
        /****************************************************************************************/
        public static int CalcTrust85Data(string contractNumber, DateTime workDate2, ref double Trust85Calc, ref double Trust85Paid, ref double Trust85Real)
        {
            Trust85Calc = 0D;
            Trust85Paid = 0D;
            Trust85Real = 0D;
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return 0;

            DataTable contractDt = dx.Copy();

            double payment = dx.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
            int numPayments = dx.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
            double totalInterest = dx.Rows[0]["totalInterest"].ObjToString().ObjToDouble();
            string dueDate = dx.Rows[0]["dueDate8"].ObjToString();
            string issueDate = dx.Rows[0]["issueDate8"].ObjToString();
            DateTime iDate = DailyHistory.GetIssueDate(dx.Rows[0]["issueDate8"].ObjToDateTime(), contractNumber, dx);
            issueDate = iDate.ToString("MM/dd/yyyy");
            DateTime lastDate = issueDate.ObjToDateTime();
            string apr = dx.Rows[0]["APR"].ObjToString();
            double dAPR = apr.ObjToDouble() / 100.0D;

            cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' order by `paydate8` DESC, `tmstamp` DESC;";
            dx = G1.get_db_data(cmd);

            if (numPayments <= 0 && dx.Rows.Count > 0)
                numPayments = dx.Rows.Count;
            double startBalance = DailyHistory.GetFinanceValue(contractNumber);

            DailyHistory.CalculateNewStuff(dx, dAPR, numPayments, startBalance, lastDate);

            int method = CalcTrust85Header(contractNumber, workDate2, contractDt, dx, ref Trust85Calc, ref Trust85Paid, ref Trust85Real);
            return method;
        }
        /****************************************************************************************/
        public static int CalcTrust85Header(string contractNumber, DateTime workDate2, DataTable contractDt, DataTable paymentsDt, ref double Trust85Calc, ref double Trust85Paid, ref double Trust85Real)
        {
            Trust85Calc = 0D;
            Trust85Paid = 0D;
            Trust85Real = 0D;

            double contractValue = DailyHistory.GetContractValuePlus(contractDt.Rows[0]);
            if (contractValue <= 0D)
                return 0;

            double startBalance = DailyHistory.GetFinanceValue(contractNumber);

            double beginningBalance = 0D;
            double endingBalance = 0D;

            DateTime lastPaidDate = DailyHistory.GetTrustLastPaid(contractNumber, ref beginningBalance, ref endingBalance);
            Trust85Real = endingBalance;


            double trust85P = 0D;
            double trust100P = 0D;
            double financeDays = contractDt.Rows[0]["numberOfPayments"].ObjToDouble();
            double downPayment = contractDt.Rows[0]["downPayment"].ObjToDouble();
            double principal = startBalance + downPayment;

            double payment = contractDt.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();
            double amtOfMonthlyPayt = payment;
            int numPayments = contractDt.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
            string dueDate = contractDt.Rows[0]["dueDate8"].ObjToString();
            string issueDate = contractDt.Rows[0]["issueDate8"].ObjToString();

            DateTime iDate = DailyHistory.GetIssueDate(contractDt.Rows[0]["issueDate8"].ObjToDateTime(), contractNumber, null);
            issueDate = iDate.ToString("MM/dd/yyyy");
            DateTime lastDate = issueDate.ObjToDateTime();
            string apr = contractDt.Rows[0]["APR"].ObjToString();

            double rate = apr.ObjToDouble() / 100.0D;

            int method = ImportDailyDeposits.CalcTrust85(amtOfMonthlyPayt, issueDate, contractValue, downPayment, financeDays, payment, principal, rate, ref trust85P, ref trust100P);

            //DateTime lastTrustDate = DateTime.Now;
            //string cmd = "Select * from `Trust2013` where `contractNumber` = '" + contractNumber + "';";
            //DataTable trustDt = G1.get_db_data(cmd);
            //if ( trustDt.Rows.Count > 0 )
            //{
            //    lastTrustDate = trustDt.Rows[0]["payDate8"].ObjToDateTime();
            //    Trust85Real = trustDt.Rows[0]["endingBalance"].ObjToDouble();
            //}

            double dValue = 0D;
            bool done = false;

            DateTime payDate8 = DateTime.Now;

            for (int i = (paymentsDt.Rows.Count - 1); i >= 0; i--)
            {
                if (paymentsDt.Rows[i]["fill"].ObjToString().ToUpper() == "D")
                    continue;
                payDate8 = paymentsDt.Rows[i]["payDate8"].ObjToDateTime();
                if (payDate8 > lastPaidDate)
                    Trust85Real += paymentsDt.Rows[i]["Trust85P"].ObjToDouble();
                if (payDate8 > workDate2)
                    continue;
                Trust85Paid += paymentsDt.Rows[i]["trust85P"].ObjToDouble();
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
            Trust85Calc = G1.RoundValue(Trust85Calc);
            return method;
        }
        /****************************************************************************************/
        public static double GetTrust85Real(string contractNumber, DateTime workDate2, DataTable paymentDt)
        {
            double Trust85Real = 0D;
            string cmd = "Select * from `Trust2013` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return Trust85Real;
            return Trust85Real;
        }
        /****************************************************************************************/
        private void chkMainDoSplits_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;

            if (chkMainDoSplits.Checked)
                chkDoSplits.Checked = true;
            else
                chkDoSplits.Checked = false;
        }
        /****************************************************************************************/
        private void gridMain10_DoubleClick(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataTable dt8 = (DataTable)dgv8.DataSource;
            DataTable dt9 = (DataTable)dgv9.DataSource;
            DataTable dt10 = (DataTable)dgv10.DataSource;
            DateTime startDate = this.dateTimePicker3.Value;
            DateTime stopDate = this.dateTimePicker4.Value;

            DataRow dr = gridMain10.GetFocusedDataRow();
            string agentName = dr["customer"].ObjToString();
            string agentNumber = dr["agentNumber"].ObjToString();

            string cmbShow = this.cmbShow.Text;

            this.Cursor = Cursors.WaitCursor;
            CommissionDetail commForm = new CommissionDetail(startDate, stopDate, agentNumber, agentName, dt, dt8, dt9, dt10, false, cmbShow);
            commForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***************************************************************************************/
        private bool continuousPrint = false;
        private string fullPath = "";
        public void FireEventAgentTotals(string path)
        {
            tabControl1.SelectedTab = tabAgentTotals;
            chkShowCommissions.Checked = true;
            DataTable dt = (DataTable)dgv3.DataSource;

            gridMain3.Columns["fbi"].Visible = false;
            gridMain3.Columns["fbiCommission"].Visible = false;

            DataView tempview = dt.DefaultView;
            tempview.Sort = "agentName";
            dt = tempview.ToTable();
            dgv3.DataSource = dt;

            gridMain3.RefreshData();
            gridMain3.RefreshEditor(true);


            dgv3.Refresh();
            dgv3.Visible = true;

            fullPath = path;
            continuousPrint = true;

            printPreviewToolStripMenuItem_Click(null, null);

            continuousPrint = false;
            fullPath = "";

            dgv.Refresh();
            dgv.Visible = true;

            tabControl1.SelectedTab = tabCommission;
            dgv10.Refresh();
            dgv10.Visible = true;
        }
        /***************************************************************************************/
        public void FireEventAgentMeetings(string path)
        {
            tabControl1.SelectedTab = tabMeetings;
            DataTable dt = (DataTable)dgv13.DataSource;

            dgv13.Refresh();
            dgv13.Visible = true;

            fullPath = path;
            continuousPrint = true;

            printPreviewToolStripMenuItem_Click(null, null);

            continuousPrint = false;
            fullPath = "";

            dgv.Refresh();
            dgv.Visible = true;

            tabControl1.SelectedTab = tabCommission;
            dgv10.Refresh();
            dgv10.Visible = true;
        }
        /****************************************************************************************/
        private void chkShowCommissions_CheckedChanged(object sender, EventArgs e)
        {
            LoadTabAgents();
        }
        /****************************************************************************************/
        private bool startPrint = false;
        private void gridMain3_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
                if (footerCount >= 2)
                {
                    footerCount = 0;
                    if (chkShowCommissions.Checked)
                    {
                        //                        if ( !chkCollapes.Checked )
                        if (!chkSummarize.Checked && !chkCollapes.Checked)
                            e.PS.InsertPageBreak(e.Y);
                    }
                }
            }
        }
        /****************************************************************************************/
        private void gridMain3_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
                startPrint = true;
            }
            else
            {
                if (chkCollapes.Checked)
                {
                    //if (chkSummarize.Checked )
                    //    e.Cancel = true;
                    //if ( !startPrint )
                    if (e.Level >= 1)
                        e.Cancel = true;
                    startPrint = false;
                }
            }
        }
        /****************************************************************************************/
        private void chkCollapes_CheckedChanged(object sender, EventArgs e)
        {
            if (chkCollapes.Checked)
            {
                gridMain3.CollapseAllGroups();
                gridMain3.OptionsPrint.PrintGroupFooter = true;
            }
            else
                gridMain3.ExpandAllGroups();
        }
        /****************************************************************************************/
        private void btnPrintAll_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataTable dt8 = (DataTable)dgv8.DataSource;
            DataTable dt9 = (DataTable)dgv9.DataSource;
            DataTable dt10 = (DataTable)dgv10.DataSource;
            DateTime startDate = this.dateTimePicker3.Value;
            DateTime stopDate = this.dateTimePicker4.Value;
            string comboWhat = cmbShow.Text.Trim().ToUpper();
            string comboStatus = cmbStatus.Text.Trim().ToUpper();

            DataRow dr = gridMain10.GetFocusedDataRow();
            string agentNumber = dr["agentNumber"].ObjToString();
            bool pwEntered = false;

            DateTime endDate = this.dateTimePicker2.Value;
            DateTime now = DateTime.Now;
            TimeSpan ts = now - endDate;
            if (ts.TotalDays > 10)
            {
                if (!G1.RobbyServer)
                {
                    if (!G1.ValidateOverridePassword("Enter Password To Override Previously Run Commissions > "))
                    {
                        return;
                    }
                }
                pwEntered = true;
            }


            this.Cursor = Cursors.WaitCursor;
            string Printed = "";
            bool isPrinted = false;

            localTrust85 = this;

            DataTable tempDt = dt10.Copy();
            if (holdCommissionsDt.Rows.Count > 0)
                tempDt = holdCommissionsDt.Copy();
            DataView tempview = tempDt.DefaultView;
            tempview.Sort = "lastName asc, firstName asc";
            tempDt = tempview.ToTable();



            using (CommissionDetail commForm = new CommissionDetail(startDate, stopDate, agentNumber, "", dt, dt8, dt9, tempDt, true, comboWhat, comboStatus))
            {
                commForm.ShowDialog();

                Printed = commForm.Printed;
                if (Printed.ToUpper() == "PRINTED")
                    isPrinted = true;
            }

            if (isPrinted)
            {
                if (pwEntered)
                {
                    DialogResult result = MessageBox.Show("Password had to be entered,\nso it must be after 10 days!\nDo you still want to save\nand possibly OVERWRITE Historic Data?", "Save Historic Data Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    if (result == DialogResult.No)
                        return;
                }
                PleaseWait pleaseForm = new PleaseWait("Please Wait!\nSaving All Historic Commission Data!");
                pleaseForm.Show();
                pleaseForm.Refresh();

                DataTable xDt = (DataTable)dgv10.DataSource;
                if (holdCommissionsDt != null)
                {
                    dgv10.DataSource = holdCommissionsDt;
                    btnSaveCommissions_Click(null, null); // Save Commissions
                    dgv10.DataSource = xDt;

                    string what = this.dateTimePicker1.Value.ToString("MM/dd/yyyy") + " to " + this.dateTimePicker2.Value.ToString("MM/dd/yyyy");

                    G1.AddToAudit(LoginForm.username, "Commissions", "Save Commissions", what, "");
                }

                pleaseForm.FireEvent1();
                pleaseForm.Dispose();
                pleaseForm = null;
            }

            localTrust85 = null;

            this.Cursor = Cursors.Default; //ramma zamma
        }
        /****************************************************************************************/
        public static Trust85 localTrust85 = null;
        /****************************************************************************************/
        private void btnMatch_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv3.DataSource;
            CompareCommissions compareForm = new CompareCommissions(dt);
            compareForm.Show();
        }
        /****************************************************************************************/
        private void chkToggleGroups_CheckedChanged(object sender, EventArgs e)
        {
            if (chkToggleGroups.Checked)
            {
                gridMain3.Columns["agentName"].GroupIndex = -1;
                //gridMain3.OptionsBehavior.AutoExpandAllGroups = false;
                //gridMain3.ExpandAllGroups();
                //gridMain3.OptionsPrint.ExpandAllGroups = true;
                gridMain3.OptionsPrint.PrintGroupFooter = true;

            }
            else
            {
                gridMain3.Columns["agentName"].GroupIndex = 0;
                gridMain3.OptionsBehavior.AutoExpandAllGroups = true;
                gridMain3.ExpandAllGroups();
                gridMain3.OptionsPrint.ExpandAllGroups = true;
                gridMain3.OptionsPrint.PrintGroupFooter = true;
            }
        }
        /****************************************************************************************/
        private void cmbLocationTotals_EditValueChanged(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.CheckedComboBoxEdit combo = (DevExpress.XtraEditors.CheckedComboBoxEdit)sender;
            string what = combo.Text;
        }
        /****************************************************************************************/
        private void cmbLocationTotals_TextChanged(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.CheckedComboBoxEdit combo = (DevExpress.XtraEditors.CheckedComboBoxEdit)sender;
            string what = combo.Text;
        }
        /****************************************************************************************/
        private void checkedComboBoxEdit1_Properties_EditValueChanged(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.CheckedComboBoxEdit combo = (DevExpress.XtraEditors.CheckedComboBoxEdit)sender;
            string what = combo.Text;
        }
        /****************************************************************************************/
        private void cmbShow_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (chkConsolidate.Checked)
                return;
            ComboBox combo = (ComboBox)sender;
            string what = combo.Text;
            if (what.ToUpper() == "5%")
            {
                gridMain10.Columns["totalContracts"].Visible = false;
                gridMain10.Columns["totalPayments"].Visible = true;
                gridMain10.Columns["totalCommission"].Visible = true;
                gridMain10.Columns["commission"].Visible = true;
                gridMain10.Columns["contractCommission"].Visible = false;
                gridMain10.Columns["goalCommission"].Visible = false;
                gridMain10.Columns["goal"].Visible = false;
                gridMain10.Columns["Recap"].Visible = false;
                gridMain10.Columns["pastRecap"].Visible = false;
                gridMain10.Columns["pastFailures"].Visible = false;
                gridMain10.Columns["splitGoalCommission"].Visible = false;
                gridMain10.Columns["splitCommission"].Visible = false;
            }
            else if (what.ToUpper() == "1%")
            {
                gridMain10.Columns["totalContracts"].Visible = true;
                gridMain10.Columns["totalPayments"].Visible = false;
                gridMain10.Columns["totalCommission"].Visible = true;
                gridMain10.Columns["commission"].Visible = false;
                gridMain10.Columns["contractCommission"].Visible = true;
                gridMain10.Columns["goalCommission"].Visible = true;
                gridMain10.Columns["goal"].Visible = true;
                gridMain10.Columns["Recap"].Visible = true;
                gridMain10.Columns["pastRecap"].Visible = true;
                gridMain10.Columns["pastFailures"].Visible = true;
                gridMain10.Columns["splitGoalCommission"].Visible = true;
                gridMain10.Columns["splitCommission"].Visible = false;
            }
            else
            {
                gridMain10.Columns["totalContracts"].Visible = true;
                gridMain10.Columns["totalPayments"].Visible = true;
                gridMain10.Columns["totalCommission"].Visible = true;
                gridMain10.Columns["commission"].Visible = true;
                gridMain10.Columns["contractCommission"].Visible = true;
                gridMain10.Columns["goalCommission"].Visible = true;
                gridMain10.Columns["goal"].Visible = true;
                gridMain10.Columns["Recap"].Visible = true;
                gridMain10.Columns["pastRecap"].Visible = true;
                gridMain10.Columns["pastFailures"].Visible = true;
                gridMain10.Columns["splitGoalCommission"].Visible = true;
                gridMain10.Columns["splitCommission"].Visible = false;
            }
            gridMain10.RefreshData();
        }
        /****************************************************************************************/
        private void gridMain10_CustomRowFilter(object sender, RowFilterEventArgs e)
        {
            if (cmbShow.Text.ToUpper() == "ALL")
                return;
            string what = cmbShow.Text.Trim().ToUpper();
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv10.DataSource;
            string type = dt.Rows[row]["type"].ObjToString().Trim().ToUpper();
            if (what == "5%" && type == "GOAL")
            {
                e.Visible = false;
                e.Handled = true;
            }
            else if (what == "1%" && type != "GOAL")
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        public static void LoadTrustAdjustments(DataTable dt, DateTime datePick1, DateTime datePick2)
        {
            string date1 = G1.DateTimeToSQLDateTime(datePick1);
            string date2 = G1.DateTimeToSQLDateTime(datePick2);
            date1 += " 00:00:00";
            date2 += " 23:59:59";
            //string cmd = "Select * from `payments` p JOIN `contracts` c ON p.`contractNumber` = c.`contractNumber` WHERE p.`tmstamp` >='" + date1 + "' AND p.`tmstamp` <= '" + date2 + "' AND p.`edited` = 'TRUSTADJ';";
            string cmd = "Select * from `payments` p JOIN `contracts` c ON p.`contractNumber` = c.`contractNumber` WHERE p.`payDate8` >='" + date1 + "' AND p.`payDate8` <= '" + date2 + "' AND p.`edited` = 'TRUSTADJ';";
            DataTable dx = G1.get_db_data(cmd);

            DataRow[] dR = null;
            DateTime payDate8 = DateTime.Now;
            DateTime oldDate8 = DateTime.Now;
            string contractNumber = "";
            var date3 = G1.DTtoMySQLDT(datePick1);
            bool found = false;
            string record = "";
            string record2 = "";
            string trustRecordCol = "record2";
            if (G1.get_column_number(dt, "record2") < 0)
                trustRecordCol = "record";
            DataTable dtt = dx.Clone();
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                record = dx.Rows[i]["record"].ObjToString();
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                if (contractNumber == "HT21060LI")
                {
                }
                dx.Rows[i]["location"] = "NONE";
                dx.Rows[i]["interestPaid1"] = dx.Rows[i]["interestPaid"].ObjToDouble();
                payDate8 = dx.Rows[i]["payDate8"].ObjToDateTime();
                //payDate8 = dx.Rows[i]["tmstamp"].ObjToDateTime();
                date1 = payDate8.ToString("yyyy-MM-dd");
                cmd = "contractNumber='" + contractNumber + "' AND `edited` = 'TRUSTADJ' AND `fill` <> 'D' ";
                try
                {
                    found = false;
                    dR = dt.Select(cmd);
                    dtt.Rows.Clear();
                    G1.ConvertToTable(dR, dtt);
                    for (int j = 0; j < dR.Length; j++)
                    {
                        record2 = dR[j][trustRecordCol].ObjToString();
                        oldDate8 = dR[j]["payDate8"].ObjToDateTime();
                        if (record == record2)
                        {
                            dR[0]["location"] = "NONE";
                            found = true;
                            break;
                        }
                        //if (oldDate8 == payDate8)
                        //{
                        //    found = true;
                        //    break;
                        //}
                    }
                    if (!found)
                        dt.ImportRow(dx.Rows[i]);
                }
                catch (Exception ex)
                {
                }
            }
        }
        /****************************************************************************************/
        public static void RemoveTrustAdjustments(DataTable dt, DateTime startDate, DateTime stopDate)
        {
            string status = "";
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            DateTime now = DateTime.Now;
            string contractNumber = "";
            double trust85P = 0D;
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                status = dt.Rows[i]["edited"].ObjToString();
                if (status.ToUpper() == "TRUSTADJ")
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    trust85P = dt.Rows[i]["trust85P"].ObjToDouble();
                    now = dt.Rows[i]["tmstamp"].ObjToDateTime();
                    if (now.Year > 100)
                    {
                        if (now < startDate || now > stopDate)
                            dt.Rows.RemoveAt(i);
                    }
                }
            }
        }
        /****************************************************************************************/
        private void agentsPieChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataView tempview = dt.DefaultView;
            tempview.Sort = "agentName asc, agentNumber asc";
            dt = tempview.ToTable();

            DataTable dx = new DataTable();
            dx.Columns.Add("agentNumber");
            dx.Columns.Add("agentName");
            dx.Columns.Add("totalPayments", Type.GetType("System.Double"));
            dx.Columns.Add("commission", Type.GetType("System.Double"));
            dx.Columns.Add("contractValue", Type.GetType("System.Double"));

            string oldAgentNumber = "";
            string oldAgentName = "";

            string agentNumber = "";
            string agentName = "";

            double T_TotalPayments = 0D;
            double T_TotalCommissions = 0D;
            double T_TotalContracts = 0D;

            double totalPayments = 0D;
            double commission = 0D;
            double contractValue = 0D;

            DataRow dRow = null;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                agentName = dt.Rows[i]["agentName"].ObjToString();
                if (String.IsNullOrWhiteSpace(oldAgentName))
                    oldAgentName = agentName;
                if (oldAgentName != agentName)
                {
                    dRow = dx.NewRow();
                    dRow["agentNumber"] = oldAgentNumber;
                    dRow["agentName"] = oldAgentName;
                    dRow["totalPayments"] = T_TotalPayments;
                    dRow["commission"] = T_TotalCommissions;
                    dRow["contractValue"] = T_TotalContracts;
                    dx.Rows.Add(dRow);
                    T_TotalPayments = 0D;
                    T_TotalCommissions = 0D;
                    T_TotalContracts = 0D;
                }
                oldAgentName = agentName;
                oldAgentNumber = dt.Rows[i]["agentNumber"].ObjToString();
                commission = dt.Rows[i]["commission"].ObjToDouble();
                contractValue = dt.Rows[i]["contractValue"].ObjToDouble();
                totalPayments = dt.Rows[i]["totalPayments"].ObjToDouble();

                T_TotalCommissions += commission;
                T_TotalContracts += contractValue;
                T_TotalPayments += totalPayments;
            }
            dRow = dx.NewRow();
            dRow["agentNumber"] = oldAgentNumber;
            dRow["agentName"] = oldAgentName;
            dRow["totalPayments"] = T_TotalPayments;
            dRow["commission"] = T_TotalCommissions;
            dRow["contractValue"] = T_TotalContracts;
            dx.Rows.Add(dRow);

            //            PieChart pieForm = new PieChart("5%", dx, this.dateTimePicker1.Value, this.dateTimePicker2.Value);
            PieChart pieForm = new PieChart("", dx, this.dateTimePicker1.Value, this.dateTimePicker2.Value);
            pieForm.Show();
        }
        /****************************************************************************************/
        private void btnChart_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv10.DataSource;

            DataView tempview = dt.DefaultView;
            tempview.Sort = "name asc, agentCode asc";
            dt = tempview.ToTable();

            DataTable dx = new DataTable();
            dx.Columns.Add("agentNumber");
            dx.Columns.Add("agentName");
            dx.Columns.Add("Contract Commission", Type.GetType("System.Double"));
            dx.Columns.Add("Base Commission", Type.GetType("System.Double"));
            dx.Columns.Add("Total Commission", Type.GetType("System.Double"));

            string oldAgentNumber = "";
            string oldAgentName = "";

            string agentNumber = "";
            string agentName = "";

            double T_TotalBase = 0D;
            double T_TotalCommissions = 0D;
            double T_TotalContracts = 0D;

            double totalPayments = 0D;
            double commission = 0D;
            double contractValue = 0D;

            DataRow dRow = null;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                agentName = dt.Rows[i]["name"].ObjToString();
                if (String.IsNullOrWhiteSpace(oldAgentName))
                    oldAgentName = agentName;
                if (oldAgentName != agentName)
                {
                    dRow = dx.NewRow();
                    dRow["agentNumber"] = oldAgentNumber;
                    dRow["agentName"] = oldAgentName;
                    if (T_TotalBase <= 0D)
                        T_TotalBase = 0D;
                    if (T_TotalCommissions <= 0D)
                        T_TotalCommissions = 0D;
                    if (T_TotalContracts <= 0D)
                        T_TotalContracts = 0D;
                    dRow["Base Commission"] = T_TotalBase;
                    dRow["Total Commission"] = T_TotalCommissions;
                    dRow["Contract Commission"] = T_TotalContracts;
                    dx.Rows.Add(dRow);
                    T_TotalBase = 0D;
                    T_TotalCommissions = 0D;
                    T_TotalContracts = 0D;
                }
                oldAgentName = agentName;
                oldAgentNumber = dt.Rows[i]["agentCode"].ObjToString();
                commission = dt.Rows[i]["commission"].ObjToDouble();
                contractValue = dt.Rows[i]["totalCommission"].ObjToDouble();
                totalPayments = dt.Rows[i]["contractCommission"].ObjToDouble();

                T_TotalCommissions += contractValue;
                T_TotalContracts += totalPayments;
                T_TotalBase += commission;
            }
            dRow = dx.NewRow();
            dRow["agentNumber"] = oldAgentNumber;
            dRow["agentName"] = oldAgentName;
            dRow["Base Commission"] = T_TotalBase;
            dRow["Total Commission"] = T_TotalCommissions;
            dRow["Contract Commission"] = T_TotalContracts;
            dx.Rows.Add(dRow);

            //            PieChart pieForm = new PieChart("1%", dx, this.dateTimePicker1.Value, this.dateTimePicker2.Value);
            PieChart pieForm = new PieChart("", dx, this.dateTimePicker1.Value, this.dateTimePicker2.Value);
            pieForm.Show();
        }
        /****************************************************************************************/
        private void btnCombine_Click(object sender, EventArgs e)
        {
            DataTable dt = null;
            if (dgv.Visible)
                dt = (DataTable)dgv.DataSource;
            else if (dgv3.Visible)
                dt = (DataTable)dgv3.DataSource;
            DataView tempview = dt.DefaultView;
            tempview.Sort = "contractNumber asc";
            dt = tempview.ToTable();
            string contractNumber = "";
            string contract = "";
            double trust85 = 0D;
            double trust100 = 0D;
            double trust85P = 0D;
            double trust100P = 0D;
            double payments = 0D;
            double payment = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    continue;
                for (int j = (i + 1); j < dt.Rows.Count; j++)
                {
                    contract = dt.Rows[j]["contractNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(contractNumber))
                        continue;
                    if (contract != contractNumber)
                        break;
                    payments = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    if (dgv.Visible)
                    {
                        trust85P = dt.Rows[i]["trust85P"].ObjToDouble();
                        trust100P = dt.Rows[i]["trust100P"].ObjToDouble();
                    }
                    payment = dt.Rows[j]["paymentAmount"].ObjToDouble();
                    if (dgv.Visible)
                    {
                        trust85 = dt.Rows[j]["trust85P"].ObjToDouble();
                        trust100 = dt.Rows[j]["trust100P"].ObjToDouble();
                        trust85P += trust85;
                        trust100P += trust100;
                    }
                    payments += payment;
                    if (dgv.Visible)
                    {
                        dt.Rows[i]["trust85P"] = trust85P;
                        dt.Rows[i]["trust100P"] = trust100P;
                    }
                    dt.Rows[i]["paymentAmount"] = payments;
                    dt.Rows[j]["contractNumber"] = "";
                }
            }
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    dt.Rows.RemoveAt(i);
            }
            if (dgv.Visible)
            {
                dgv.DataSource = dt;
                dgv.Refresh();
            }
            else if (dgv3.Visible)
            {
                dgv3.DataSource = dt;
                dgv3.Refresh();
            }
        }
        /****************************************************************************************/
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditHelp helpForm = new EditHelp("Trust85 Report");
            helpForm.Show();
        }
        /****************************************************************************************/
        private void chkLocationTotal_CheckedChanged(object sender, EventArgs e)
        {
            if (chkLocationTotal.Checked)
            {
                gridMain4.Columns["agentName"].GroupIndex = 0;
                gridMain4.OptionsBehavior.AutoExpandAllGroups = true;
                gridMain4.ExpandAllGroups();
                gridMain4.OptionsPrint.ExpandAllGroups = true;
                gridMain4.OptionsPrint.PrintGroupFooter = true;
            }
            else
            {
                gridMain4.Columns["agentName"].GroupIndex = -1;
                gridMain4.OptionsBehavior.AutoExpandAllGroups = false;
                gridMain4.CollapseAllGroups();
                gridMain4.OptionsPrint.ExpandAllGroups = false;
                gridMain4.OptionsPrint.PrintGroupFooter = false;
            }
            gridMain4.RefreshData();
            dgv4.Refresh();
        }
        /****************************************************************************************/
        private void gridMain4_CustomRowFilter(object sender, RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv4.DataSource;
            if (chkLocationTotal.Checked)
            {
                ColumnView view = sender as ColumnView;
                double totalPayments = dt.Rows[row]["totalTrusts"].ObjToDouble();
                if (totalPayments <= 0D)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
        }
        /****************************************************************************************/
        private void chkAgentByLocationTotalLocations_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkReverseAgentsAndLocations.Checked)
            {
                if (chkAgentByLocationTotalLocations.Checked)
                {
                    gridMain2.Columns["Location Name"].Visible = false;
                    gridMain2.Columns["Location Name"].GroupIndex = 0;
                    gridMain2.Columns["agentName"].GroupIndex = 1;
                    gridMain2.ExpandAllGroups();
                }
                else
                {
                    gridMain2.Columns["Location Name"].Visible = true;
                    gridMain2.Columns["Location Name"].GroupIndex = -1;
                    gridMain2.Columns["agentName"].GroupIndex = -1;
                    gridMain2.CollapseAllGroups();
                }
            }
            else
            {
                if (chkAgentByLocationTotalLocations.Checked)
                {
                    gridMain2.Columns["agentName"].Visible = false;
                    gridMain2.Columns["Location Name"].GroupIndex = 1;
                    gridMain2.Columns["agentName"].GroupIndex = 0;
                    gridMain2.ExpandAllGroups();
                }
                else
                {
                    gridMain2.Columns["agentName"].Visible = true;
                    gridMain2.Columns["Location Name"].GroupIndex = -1;
                    gridMain2.Columns["agentName"].GroupIndex = -1;
                    gridMain2.CollapseAllGroups();
                }
            }
        }
        /****************************************************************************************/
        private void chkShowOnlyContractValues_CheckedChanged(object sender, EventArgs e)
        {
            LoadTabLocations();
            if (chkShowOnlyContractValues.Checked)
            {
                gridMain2.Columns["Location Name"].Visible = false;
                gridMain2.Columns["num"].Visible = false;
                gridMain2.ExpandAllGroups();
            }
            if (!chkShowOnlyContractValues.Checked && !chkAgentByLocationTotalLocations.Checked)
            {
                gridMain2.Columns["Location Name"].Visible = true;
                gridMain2.Columns["num"].Visible = true;
                gridMain2.CollapseAllGroups();
            }
        }
        /****************************************************************************************/
        private void gridMain2_CustomRowFilter(object sender, RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv2.DataSource;
            if (chkShowOnlyContractValues.Checked)
            {
                ColumnView view = sender as ColumnView;
                double contractValue = dt.Rows[row]["contractValue"].ObjToDouble();
                if (contractValue > 0D)
                {
                    e.Visible = true;
                    e.Handled = true;
                    return;
                }
                else
                {
                    e.Visible = false;
                    e.Handled = true;
                }
            }
        }
        /****************************************************************************************/
        private void chkReverseAgentsAndLocations_CheckedChanged(object sender, EventArgs e)
        {
            chkAgentByLocationTotalLocations_CheckedChanged(null, null);
        }
        /****************************************************************************************/
        private void gridMain2_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            this.Cursor = Cursors.WaitCursor;
            CustomerDetails clientForm = new CustomerDetails(contract);
            clientForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void Trust85_FormClosing(object sender, FormClosingEventArgs e)
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
        }
        /****************************************************************************************/
        private void gridMain3_CustomRowFilter(object sender, RowFilterEventArgs e)
        { // Trying to filter out data rows and leave footers only - didn't work
            //int row = e.ListSourceRow;
            //if (!summaryPressed)
            //    return;
            //Type t = e.GetType();

            //if ( gridMain3.IsFilterRow ( row))
            //    e.Visible = false;
            //else
            //    e.Visible = true;
            //e.Handled = true;
        }
        /****************************************************************************************/
        private bool oldData = false;
        private void menuReadPreviousData_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            DateTime begin = this.dateTimePicker1.Value;
            DateTime end = this.dateTimePicker2.Value;
            //begin = begin.AddMonths(-1);

            //begin = begin.AddMonths(1);
            int days = DateTime.DaysInMonth(begin.Year, begin.Month);
            DateTime last = new DateTime(begin.Year, begin.Month, days);
            if (last > end)
            {
                this.Cursor = Cursors.Default;
                return;
            }

            string cmd = "Select * from `lapse_reinstates` where `startDate` = '" + begin.ToString("yyyy-MM-dd") + "' AND `endDate` = '" + last.ToString("yyyy-MM-dd") + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                this.Cursor = Cursors.Default;
                return;
            }
            string runNumber = dx.Rows[0]["record"].ObjToString();

            cmd = "Select * from `historic_commissions` where `runNumber` = '" + runNumber + "';";
            dx = G1.get_db_data(cmd);
            dgv10.DataSource = dx;

            cmd = "Select * from `lapsetable` where `runNumber` = '" + runNumber + "';";
            DataTable lapseDt = G1.get_db_data(cmd);
            dgv8.DataSource = lapseDt;

            cmd = "Select * from `reinstatetable` where `runNumber` = '" + runNumber + "';";
            DataTable reinstateDt = G1.get_db_data(cmd);
            dgv9.DataSource = reinstateDt;

            cmd = "Select * from `trustdetail` where `runNumber` = '" + runNumber + "';";
            DataTable trustDt = G1.get_db_data(cmd);
            dgv.DataSource = trustDt;

            if (_agentList == null)
                loadAgents("");

            DataTable dt = (DataTable)dgv.DataSource;
            DateTime startDate = this.dateTimePicker1.Value;
            DateTime stopDate = this.dateTimePicker2.Value;

            oldData = true;

            CalcCommission(dt, startDate, stopDate);

            bool doSplits = false;
            if (chkMainDoSplits.Checked)
                chkDoSplits.Checked = true;
            else
                chkDoSplits.Checked = false;

            if (chkDoSplits.Checked)
                doSplits = true;

            RunCommissions(true, doSplits);

            gridMain10.RefreshData();
            dgv10.Refresh();
            commissionRan = true;

            oldData = false; ;

            loading = true;
            chkDoSplits.Checked = true;
            chkConsolidate.Checked = false;
            btnPrintAll.Hide();
            loading = false;

            tabControl1.SelectedTab = tabPage1;

            dgv.Refresh();

            RunMeetingCommissions();

            if ( G1.RobbyServer )
            {
                btnCalc.Show();
                btnCalc.Refresh();
            }    

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private string pdfFilename = "";
        private ViewPDF pdfForm = null;
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            pdfFilename = "";
            using (System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog())
            {
                ofd.Filter = "PDF Files|*.pdf";
                ofd.Title = "Select a PDF File";
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    pdfFilename = file;
                    using (pdfForm = new ViewPDF("Old Commission Detail", "", file, true, true))
                    {
                        pdfForm.PdfLapses += PdfForm_PdfLapses;
                        pdfForm.ShowDialog();
                    }
                }
            }
        }
        /****************************************************************************************/
        private bool doReprocess = false;
        private void PdfForm_PdfLapses(string strIn)
        {
            if (strIn.ToUpper() != "YES")
                return;

            if (String.IsNullOrWhiteSpace(pdfFilename))
                return;
            //if (pdfForm != null)
            //    pdfForm.Close();
            string data = SMFS.GetText(pdfFilename);

            if (String.IsNullOrWhiteSpace(data))
                return;
            string[] aLine = null;
            string[] Lines = data.Split('\n');

            DateTime fromDate = DateTime.Now;
            DateTime toDate = DateTime.Now;

            if (Lines.Length < 1)
                return;
            string str = Lines[0].Trim();
            aLine = str.Split(' ');
            if (aLine.Length < 3)
                return;
            fromDate = aLine[0].ObjToDateTime();
            toDate = aLine[2].ObjToDateTime();

            this.dateTimePicker1.Value = fromDate;
            this.dateTimePicker2.Value = toDate;
            this.dateTimePicker3.Value = fromDate;
            this.dateTimePicker4.Value = toDate;

            doReprocess = true;
            btnRun_Click(null, null);

            DataTable dt8 = (DataTable)dgv8.DataSource;
            DataTable dt9 = (DataTable)dgv9.DataSource;
            DataTable dt = (DataTable)dgv.DataSource;

            for (int i = 0; i < Lines.Length; i++)
            {

            }
        }
        /****************************************************************************************/
        private void gridMain13_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
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
        private void showDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain10.GetFocusedDataRow();
            string agentNumber = dr["agentNumber"].ObjToString();
            string agentCode = agentNumber;
            if (_agentList == null)
                _agentList = G1.get_db_data("Select * from `agents`;");
            DataTable goalDt = G1.get_db_data("Select * from `goals`;");
            DataRow[] dRows = _agentList.Select("agentCode='" + agentCode + "'");
            if (dRows.Length <= 0)
                return;
            string firstName = dRows[0]["firstName"].ObjToString();
            string lastName = dRows[0]["lastName"].ObjToString();

            dRows = _agentList.Select("firstName='" + firstName + "' AND lastName = '" + lastName + "'");
            if (dRows.Length <= 0)
                return;

            DataTable agentDt = dRows.CopyToDataTable();
            DataTable dt = (DataTable)dgv.DataSource;

            string name = firstName + " " + lastName;
            dRows = dt.Select("agentName='" + name + "'");
            if (dRows.Length <= 0)
                return;
            DataTable rawDt = dRows.CopyToDataTable();
        }
        /****************************************************************************************/
        private void gridMain13_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain13.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            this.Cursor = Cursors.WaitCursor;
            CustomerDetails clientForm = new CustomerDetails(contract);
            clientForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
    }
}