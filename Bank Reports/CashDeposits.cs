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
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using MyXtraGrid;
using DevExpress.XtraCharts;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class CashDeposits : DevExpress.XtraEditors.XtraForm
    {
        private DataTable groupContracts = null;
        private DataTable agentsDt = null;
        private bool runAgents = false;
        private DataTable paymentDetail = null;
        private DataTable depositDt = null;
        private string bankDetails = "";
        private double beginningBalance = 0D;
        private DataTable monthlyBankDt = null;
        private string workReport = "";
        private bool foundLocalPreference = false;
        private bool loading = true;
        /****************************************************************************************/
        public CashDeposits()
        {
            InitializeComponent();
            SetupTotalsSummary();
            workReport = "Cash Local";
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("TDA", null);
            AddSummaryColumn("IDA", null);
            AddSummaryColumn("NDA", null);
            AddSummaryColumn("DDA", null);
            AddSummaryColumn("misc", null);
            AddSummaryColumn("returns", null);
            AddSummaryColumn("transfers", null);
            //gridMain.Columns["balance"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName)
        {
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            //gridMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
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
        /****************************************************************************************/
        private void CashDeposits_Load(object sender, EventArgs e)
        {
            btnSave.Hide();
            btnDraftReport.Hide();
            btnLocalTransfer.Hide();

            cmbLockbox.Hide();
            txtBeginningBalance.Hide();
            label1.Hide();

            //gridMain.Columns["returns"].Visible = false;

            loadLocatons();

            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);

            int days = DateTime.DaysInMonth(now.Year, now.Month);
            DateTime stop = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker1.Value = stop;

            this.Cursor = Cursors.WaitCursor;

            //btnGetDeposits_Click(null, null);

            string cmd = "Select * from `bank_accounts`;";
            monthlyBankDt = G1.get_db_data(cmd);

            barImport.Hide();
            labelMaximum.Hide();

            string saveName = "Local Cash Deposits " + workReport + " Primary";
            string skinName = "";

            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            if (!String.IsNullOrWhiteSpace(skinName))
            {
                if (skinName != "DevExpress Style")
                    skinForm_SkinSelected("Skin : " + skinName);
            }

            loadGroupCombo(cmbSelectColumns, "Local Cash Deposits " + workReport, "Primary");

            ScaleCells();

            loading = false;

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void loadLocatons()
        {
            string cmd = "Select * from `bank_accounts` order by `record`;";
            DataTable locDt = G1.get_db_data(cmd);

            DataTable newLocDt = locDt.Clone();

            string account_title = "";

            for (int i = 0; i < locDt.Rows.Count; i++)
            {
                account_title = locDt.Rows[i]["account_title"].ObjToString();
                if (account_title == "Cash - The First - Lockbox")
                    continue;
                if (account_title == "Cash - The First - Remote")
                    continue;
                if (account_title == "Cash - The First Credit Card")
                    continue;

                if (locDt.Rows[i]["show_dropdown"].ObjToString() == "1")
                    G1.copy_dt_row(locDt, i, newLocDt, newLocDt.Rows.Count);
                else
                {
                    if (account_title == "Cash - Trustmark Fisher Insurance")
                        G1.copy_dt_row(locDt, i, newLocDt, newLocDt.Rows.Count);
                }
            }

            DataView tempview = newLocDt.DefaultView;
            tempview.Sort = "localDescription";
            newLocDt = tempview.ToTable();

            chkComboLocation.Properties.DataSource = newLocDt;
        }
        /***********************************************************************************************/
        private void loadLocatonsx()
        {
            string cmd = "Select * from `funeralReports` ORDER by `order`;";
            DataTable locDt = G1.get_db_data(cmd);

            DataTable newLocDt = locDt.Clone();

            for (int i = 0; i < locDt.Rows.Count; i++)
            {
                G1.copy_dt_row(locDt, i, newLocDt, newLocDt.Rows.Count);
            }
            chkComboLocation.Properties.DataSource = newLocDt;
        }
        /****************************************************************************************/
        private void AddToLocationCombo(DataTable locationDt, string text)
        {
            DataRow ddrx = locationDt.NewRow();
            ddrx["options"] = text;
            locationDt.Rows.Add(ddrx);
        }
        /****************************************************************************************/
        private void checkedComboBoxEdit1_Properties_EditValueChanged(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
        private void CleanupScreen()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt != null)
            {
                dt.Rows.Clear();
                dgv.DataSource = dt;
            }
            btnSave.Hide();
            btnSave.Refresh();

            btnDraftReport.Hide();
            btnDraftReport.Refresh();

            btnLocalTransfer.Hide();
            btnLocalTransfer.Refresh();

            barImport.Hide();
            barImport.Refresh();

            labelMaximum.Hide();
            labelMaximum.Refresh();
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            if (!CheckForSave())
                return;
            this.Cursor = Cursors.WaitCursor;
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, 1);
            //this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker1.Value = new DateTime(now.Year, now.Month, days);

            DateTime startDate = now;
            DateTime stopDate = this.dateTimePicker1.Value;

            CleanupScreen();

            //btnGetDeposits_Click(null, null);

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            if (!CheckForSave())
                return;
            this.Cursor = Cursors.WaitCursor;
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            //this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker1.Value = new DateTime(now.Year, now.Month, days);

            DateTime startDate = now;
            DateTime stopDate = this.dateTimePicker1.Value;

            CleanupScreen();

            //btnGetDeposits_Click(null, null);

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable LoadMain(DateTime startDate, DateTime stopDate)
        {
            DateTime start = startDate;
            string date1 = G1.DateTimeToSQLDateTime(start);
            DateTime stop = stopDate;
            string date2 = G1.DateTimeToSQLDateTime(stop);
            DateTime aDate = DateTime.Now;

            DataTable dx = new DataTable();
            dx.Columns.Add("num");
            dx.Columns.Add("record");
            dx.Columns.Add("date");
            dx.Columns.Add("dow");
            dx.Columns.Add("adate");
            dx.Columns.Add("TDA", Type.GetType("System.Double"));
            dx.Columns.Add("IDA", Type.GetType("System.Double"));
            dx.Columns.Add("NDA", Type.GetType("System.Double"));
            dx.Columns.Add("DDA", Type.GetType("System.Double"));
            dx.Columns.Add("misc", Type.GetType("System.Double"));
            dx.Columns.Add("returns", Type.GetType("System.Double"));
            dx.Columns.Add("transfers", Type.GetType("System.Double"));
            dx.Columns.Add("dailyTotals", Type.GetType("System.Double"));
            dx.Columns.Add("balance", Type.GetType("System.Double"));
            dx.Columns.Add("comment");
            dx.Columns.Add("manual");
            dx.Columns.Add("bank_account");
            dx.Columns.Add("depositNumber");
            dx.Columns.Add("serviceId");
            dx.Columns.Add("location");

            TimeSpan ts = stop - start;

            for (int i = 0; i <= ts.Days; i++)
            {
                if (start.AddDays(i) > stop)
                    break;
                DataRow dR = dx.NewRow();
                aDate = start.AddDays(i);
                dR["date"] = aDate.ToString("MM/dd/yyyy");
                dR["adate"] = aDate.ToString("yyyyMMdd");
                dR["dow"] = G1.DayOfWeekText(aDate);
                dx.Rows.Add(dR);
            }

            return dx;
        }
        /****************************************************************************************/
        private DataTable LoadData(DateTime startDate, DateTime stopDate, string bankDetails)
        {
            DateTime start = startDate;
            string date1 = G1.DateTimeToSQLDateTime(start);
            DateTime stop = stopDate;
            string date2 = G1.DateTimeToSQLDateTime(stop);
            DateTime aDate = DateTime.Now;

            string cmd = "Select * from `lockboxdeposits` where `date` >= '" + date1 + "' and `date` <= '" + date2 + "' AND `bank_account` = '" + bankDetails + "' ORDER BY `date` asc;";
            DataTable dt = G1.get_db_data(cmd);
            if (G1.get_column_number(dt, "num") < 0)
                dt.Columns.Add("num");
            if (G1.get_column_number(dt, "dow") < 0)
                dt.Columns.Add("dow");
            if (G1.get_column_number(dt, "dailyTotals") < 0)
                dt.Columns.Add("dailyTotals", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "what") < 0)
                dt.Columns.Add("what");
            if (dt.Rows.Count <= 0)
                return dt;
            return dt;
        }
        /****************************************************************************************/
        private void LoadUpGroupRows(DataTable dt)
        {
            string location = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                location = dt.Rows[i]["Location Name"].ObjToString();
                DataRow[] dRows = groupContracts.Select("loc='" + location.Trim() + "'");
                if (dRows.Length > 0)
                    dt.Rows[i]["Location Name"] = location + " [ Contracts: " + dRows[0]["contracts"].ObjToString() + " ]";
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawGroupRow(object sender, DevExpress.XtraGrid.Views.Base.RowObjectCustomDrawEventArgs e)
        {
            //GridGroupRowInfo info = e.Info as GridGroupRowInfo;
            //string location = info.GroupText;
            //int idx = location.LastIndexOf(']');
            //if (idx > 0)
            //{
            //    location = location.Substring(idx + 1);
            //    location = location.Replace("Cash ", "");
            //    DataRow[] dRows = monthlyBankDt.Select("localDescription='" + location.Trim() + "'");
            //    if (dRows.Length > 0)
            //    {
            //        double beginningBalance = dRows[0]["beginningBalance"].ObjToDouble();
            //        string str = G1.ReformatMoney(beginningBalance);
            //        info.GroupText += " Beginning Balance $" + str;
            //    }
            //}
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

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = false;

            Printer.setupPrinterMargins(10, 10, 80, 50);

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

            DataTable ddd = (DataTable)dgv.DataSource;

            printableComponentLink1.ShowPreview();
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
            printableComponentLink1.Landscape = false;

            Printer.setupPrinterMargins(10, 10, 80, 50);

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

            //            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Regular);
            string title = this.Text;
            Printer.DrawQuad(6, 7, 4, 3, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            DateTime date = this.dateTimePicker1.Value;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            Printer.DrawQuad(20, 8, 5, 4, "Month Closing - " + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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
                    e.PS.InsertPageBreak(e.Y);
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            if (runAgents)
            {
                int row = e.ListSourceRow;
                if (row >= 0)
                {
                    //if (gridMain.IsDataRow(row))
                    //{
                    //    e.Visible = false;
                    //    e.Handled = true;
                    //    return;
                    //}
                }
            }
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            G1.SpyGlass(gridMain);
            //if (this.gridMain.OptionsFind.AlwaysVisible == true)
            //    gridMain.OptionsFind.AlwaysVisible = false;
            //else
            //    gridMain.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private DataTable oldDt = null;
        private void btnGetDeposits_Click(object sender, EventArgs e)
        {
            btnSave.Hide();
            btnSave.Refresh();

            btnDraftReport.Hide();
            btnDraftReport.Refresh();

            btnLocalTransfer.Hide();
            btnLocalTransfer.Refresh();

            DateTime stopDate = this.dateTimePicker1.Value;
            DateTime startDate = new DateTime(stopDate.Year, stopDate.Month, 1);

            DataTable locationDt = (DataTable)chkComboLocation.Properties.DataSource;

            string locationList = chkComboLocation.Text.Trim();
            if (String.IsNullOrWhiteSpace(locationList))
                return;

            if ( depositDt != null )
            {
                depositDt.Rows.Clear();
                depositDt.Dispose();
            }
            depositDt = new DataTable();
            depositDt.Columns.Add("bankAccount");
            depositDt.Columns.Add("TDA");
            depositDt.Columns.Add("IDA");
            depositDt.Columns.Add("NDA");
            depositDt.Columns.Add("DDA");

            string[] Lines = locationList.Split('|');

            if (Lines.Length > 0)
            {
                labelMaximum.Text = Lines.Length.ToString();
                barImport.Minimum = 0;
                barImport.Maximum = Lines.Length;
                barImport.Value = 0;
                barImport.Show();
                barImport.Refresh();
                labelMaximum.Show();
                labelMaximum.Refresh();
            }

            string accountTitle = "";
            string accountNumber = "";
            string localDescription = "";
            string glNumber = "";
            string bankName = "";
            string fullBankDetails = "";
            string cmd = "";
            DataTable bankDt = null;
            DataTable payDt = null;
            string str = "";
            int row = 0;

            DataRow[] dRows = null;
            DataRow[] bRows = null;
            DataRow dR = null;

            this.Cursor = Cursors.WaitCursor;

            DataTable mainDt = null;

            paymentDetail = null;

            for (int i = 0; i < Lines.Length; i++)
            {
                barImport.Value = (i + 1);
                barImport.Refresh();
                localDescription = Lines[i].Trim();
                if (String.IsNullOrWhiteSpace(localDescription))
                    continue;

                dRows = locationDt.Select("localDescription='" + localDescription + "'");
                if (dRows.Length <= 0)
                    continue;

                accountNumber = dRows[0]["account_no"].ObjToString();
                glNumber = dRows[0]["general_ledger_no"].ObjToString();
                bankName = dRows[0]["location"].ObjToString();

                fullBankDetails = bankName + "~" + glNumber + "~" + accountNumber;
                //fullBankDetails = localDescription + "~" + glNumber + "~" + accountNumber;


                string whatBox = "Cash " + localDescription;

                // bankDetails = whatBox;

                DataTable dt = LoadData(startDate, stopDate, fullBankDetails);
                if (G1.get_column_number(dt, "accountTitle") < 0)
                    dt.Columns.Add("accountTitle");

                oldDt = dt.Copy();

                DataTable dx = dt.Clone();

                if (mainDt == null)
                    mainDt = dx.Clone();

                string date1 = G1.DateTimeToSQLDateTime(startDate);
                string date2 = G1.DateTimeToSQLDateTime(stopDate);
                DateTime aDate = DateTime.Now;

                double endingBalance = 0D;
                accountTitle = "";
                beginningBalance = getBeginningBalance(fullBankDetails, startDate, ref endingBalance, ref accountTitle);

                bRows = monthlyBankDt.Select("account_title='" + accountTitle + "'");
                if (bRows.Length > 0)
                    bRows[0]["beginningBalance"] = beginningBalance;

                //zeroOutColumn(dx, "TDA");
                //zeroOutColumn(dx, "IDA");
                //zeroOutColumn(dx, "NDA");
                //zeroOutColumn(dx, "DDA");
                //zeroOutColumn(dx, "returns");

                if (G1.get_column_number(dx, "accountTitle") < 0)
                    dx.Columns.Add("accountTitle");

                dR = dx.NewRow();
                dR["date"] = G1.DTtoMySQLDT(startDate);
                dR["depositNumber"] = "";
                dR["serviceId"] = "";
                dR["balance"] = beginningBalance;
                dR["bank_account"] = fullBankDetails;
                dR["accountTitle"] = whatBox;
                dR["comment"] = "Balance Forward";
                dx.Rows.Add(dR);

                getTrustPayments(dx, startDate, stopDate, fullBankDetails, whatBox);
                getInsurancePayments(dx, startDate, stopDate, fullBankDetails, whatBox);

                cmd = "Select * from `cust_payment_details` c JOIN `fcust_extended` f ON c.`contractNumber` = f.`contractNumber` WHERE c.`dateReceived` >= '" + date1 + "' and c.`dateReceived` <= '" + date2 + "' AND `localDescription` = '" + localDescription + "' AND `bankAccount` = '" + accountNumber + "' order by `dateReceived`, c.`depositNumber`;";
                payDt = G1.get_db_data(cmd);
                if (payDt.Rows.Count > 0)
                {
                    getFuneralDetailPayments(dx, startDate, stopDate, payDt, fullBankDetails, whatBox);
                }

                loadTrustDownPayments(dx, startDate, stopDate, payDt, fullBankDetails, localDescription);

                dx = loadBankDebits(dx, startDate, stopDate, bankDetails, accountNumber, whatBox, fullBankDetails );


                CompareTables(dx, dt, whatBox);

                LoadDOW(dx);

                DataView tempview = dx.DefaultView;
                tempview.Sort = "date";
                dx = tempview.ToTable();

                MatchDepositNumbers(dx);

                dx = SortDownTable(dx);

                RecalcDailyTotals(dx, beginningBalance);

                if (G1.get_column_number(mainDt, "accountTitle") < 0)
                    mainDt.Columns.Add("accountTitle");

                for (int j = 0; j < dx.Rows.Count; j++)
                    G1.copy_dt_row(dx, j, mainDt, mainDt.Rows.Count);
            }

            if (mainDt != null)
            {
                if (mainDt.Rows.Count > 0)
                {
                    DataView tempview = mainDt.DefaultView;
                    tempview.Sort = "accountTitle,date";
                    mainDt = tempview.ToTable();
                }
            }

            gridMain.Columns["accountTitle"].GroupIndex = 0;

            btnSave.Show();
            btnSave.Refresh();

            btnDraftReport.Show();
            btnDraftReport.Refresh();

            btnLocalTransfer.Show();
            btnLocalTransfer.Refresh();


            // mainDt = SortDownTable(mainDt);

            G1.NumberDataTable(mainDt);
            dgv.DataSource = mainDt;

            gridMain.OptionsView.ShowFooter = true;
            this.gridMain.ExpandAllGroups();
            gridMain.RefreshEditor(true);

            str = bankDetails.Replace("~", " / ");

            this.Text = "Cash Local Deposits Report";
            gridBand5.Caption = "Funeral Detail Deposits for " + str + " " + accountTitle;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable SortDownTable(DataTable dt)
        {
            string depositNumber = "";
            DateTime date = DateTime.Now;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                if ( String.IsNullOrWhiteSpace ( depositNumber))
                {
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    depositNumber = "Z" + date.ToString("yyyyMMdd");
                    dt.Rows[i]["depositNumber"] = depositNumber;
                }
            }
            DataView tempview = dt.DefaultView;
            tempview.Sort = "date asc, depositNumber asc";
            dt = tempview.ToTable();
            return dt;
        }
        /****************************************************************************************/
        private DataTable MatchDepositNumbers(DataTable dt)
        {
            string bankAccount = "";
            string oldBankAccount = "";

            string depositNumber = "";
            string oldDepositNumber = "";

            int lastIndx = -1;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                bankAccount = dt.Rows[i]["bank_account"].ObjToString();
                depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(depositNumber))
                    continue;
                if (lastIndx == -1)
                    lastIndx = i;
                if (String.IsNullOrWhiteSpace(oldBankAccount))
                    oldBankAccount = bankAccount;
                if (String.IsNullOrWhiteSpace(oldDepositNumber))
                    oldDepositNumber = depositNumber;
                if (oldDepositNumber == depositNumber && oldBankAccount == bankAccount)
                {
                    if (depositNumber.ToUpper().IndexOf("TD") < 0)
                    {
                        if (i > lastIndx)
                            CombineIndexes(dt, lastIndx, i);
                    }
                    continue;
                }
                lastIndx = i;
                oldBankAccount = bankAccount;
                oldDepositNumber = depositNumber;
            }

            double tda = 0D;
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                tda = dt.Rows[i]["TDA"].ObjToDouble();
                if (tda == -1D)
                    dt.Rows.RemoveAt(i);
            }
            return dt;
        }
        /****************************************************************************************/
        private void CombineIndexes(DataTable dt, int lastIndx, int i)
        {
            double tda = 0D;
            double ida = 0D;
            double nda = 0D;
            double dda = 0D;
            double returns = 0D;
            double dValue = 0D;

            bool gotNDA = false;

            //zeroOutColumn(dx, "TDA");
            //zeroOutColumn(dx, "IDA");
            //zeroOutColumn(dx, "NDA");
            //zeroOutColumn(dx, "DDA");
            //zeroOutColumn(dx, "returns");

            try
            {
                tda = dt.Rows[lastIndx]["TDA"].ObjToDouble();
                dValue = dt.Rows[i]["TDA"].ObjToDouble();
                tda += dValue;
                dt.Rows[lastIndx]["TDA"] = tda;
                dt.Rows[i]["TDA"] = -1D;

                ida = dt.Rows[lastIndx]["IDA"].ObjToDouble();
                dValue = dt.Rows[i]["IDA"].ObjToDouble();
                ida += dValue;
                dt.Rows[lastIndx]["IDA"] = ida;
                dt.Rows[i]["IDA"] = -1D;

                nda = dt.Rows[lastIndx]["NDA"].ObjToDouble();
                dValue = dt.Rows[i]["NDA"].ObjToDouble();
                nda += dValue;
                dt.Rows[lastIndx]["NDA"] = nda;
                if (nda > 0D)
                    gotNDA = true;
                dt.Rows[i]["NDA"] = -1D;

                dda = dt.Rows[lastIndx]["DDA"].ObjToDouble();
                dValue = dt.Rows[i]["DDA"].ObjToDouble();
                dda += dValue;
                dt.Rows[lastIndx]["DDA"] = dda;
                dt.Rows[i]["DDA"] = -1D;

                returns = dt.Rows[lastIndx]["returns"].ObjToDouble();
                dValue = dt.Rows[i]["returns"].ObjToDouble();
                returns += dValue;
                dt.Rows[lastIndx]["returns"] = returns;
                dt.Rows[i]["returns"] = -1D;

                string serviceId = dt.Rows[lastIndx]["serviceId"].ObjToString();
                serviceId = serviceId.Replace("Count=", "");
                if (!G1.validate_numeric(serviceId))
                    serviceId = "1";
                int count = 0;
                try
                {
                    count = Convert.ToInt32(serviceId);
                    count++;
                    if (!gotNDA)
                        dt.Rows[lastIndx]["serviceId"] = "Count=" + count.ToString();
                }
                catch (Exception ex)
                {
                }
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void CompareTables(DataTable dx, DataTable dt, string accountTitle)
        {
            string oldDepositNumber = "";
            string newDepositNumber = "";

            DateTime oldDate = DateTime.Now;
            DateTime newDate = DateTime.Now;

            int row = 0;

            string manual = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                manual = dt.Rows[i]["manual"].ObjToString();
                if (manual.ToUpper() == "Y")
                {
                    G1.copy_dt_row(dt, i, dx, dx.Rows.Count);
                    row = dx.Rows.Count - 1;
                    dx.Rows[row]["accountTitle"] = accountTitle;
                }
            }
        }
        /****************************************************************************************/
        DataTable loadTrustDownPayments(DataTable dx, DateTime startDate, DateTime stopDate, DataTable xDt, string bankDetails, string whatBox)
        {
            string date1 = G1.DateTimeToSQLDateTime(startDate);
            string date2 = G1.DateTimeToSQLDateTime(stopDate);
            string depositNumber = "";
            string contractNumber = "";
            DateTime date = DateTime.Now;
            double payment = 0D;
            double lossRecovery = 0D;
            DataRow dR = null;
            string serviceId = "";
            string location = "";
            string str = "";
            string fName = "";
            string lName = "";
            DataTable payDt = null;
            string matchBankAccount = "Cash " + whatBox;
            string[] Lines = bankDetails.Split('~');
            string bankAccount = "";
            if (Lines.Length >= 3)
                bankAccount = Lines[2];
            DataRow[] dRows = null;
            string cmd = "";
            try
            {
                if (paymentDetail == null)
                    paymentDetail = xDt.Clone();

                cmd = "Select * from `downpayments`  WHERE `date` >= '" + date1 + "' and `date` <= '" + date2 + "' AND ( `paymentType` = 'Cash' OR `paymentType` = 'Check-Local' ) AND `localDescription` = '" + whatBox + "' order by `date`, `depositNumber`;";
                DataTable dt = G1.get_db_data(cmd);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    fName = dt.Rows[i]["firstName"].ObjToString();
                    lName = dt.Rows[i]["lastName"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(fName))
                        fName = fName.Substring(0, 1);

                    //dt.Rows[i]["bankAccount"] = matchBankAccount;

                    payment = dt.Rows[i]["downPayment"].ObjToDouble();
                    lossRecovery = dt.Rows[i]["lossRecoveryFee"].ObjToDouble();
                    str = G1.ReformatMoney(payment);
                    str = str.Replace(",", "");
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    cmd = "Select * from `payments` where `depositNumber` = '" + depositNumber + "' AND `downPayment` = '" + str + "' ";
                    if (!String.IsNullOrWhiteSpace(bankAccount))
                        cmd += " AND `bank_account` LIKE '%" + bankAccount + "'";
                    cmd += ";";
                    payDt = G1.get_db_data(cmd);
                    if (payDt.Rows.Count <= 0)
                    {
                        if (payment > 0D)
                        {
                            if ( lossRecovery == 0D )
                            {
                                dRows = dt.Select("depositNumber='" + depositNumber + "'");
                                if (dRows.Length > 0)
                                    lossRecovery = dRows[0]["lossRecoveryFee"].ObjToDouble();
                            }
                            dR = dx.NewRow();

                            dR["accountTitle"] = matchBankAccount;

                            dR["date"] = G1.DTtoMySQLDT(dt.Rows[i]["date"].ObjToDateTime());
                            dR["DDA"] = payment + lossRecovery;

                            dR["aDate"] = date.ToString("yyyyMMdd");
                            dR["bank_account"] = bankDetails;
                            //if (String.IsNullOrWhiteSpace(serviceId))
                            serviceId = fName + " " + lName;
                            dR["serviceId"] = serviceId;
                            dR["depositNumber"] = depositNumber;
                            dx.Rows.Add(dR);
                        }
                        continue;
                    }
                    else
                    {
                        if (lossRecovery == 0D)
                        {
                            dRows = dt.Select("depositNumber='" + depositNumber + "' AND lossRecoveryFee > '0'");
                            if (dRows.Length > 0)
                                lossRecovery = dRows[0]["lossRecoveryFee"].ObjToDouble();
                        }
                    }
                    payDt.Rows[0]["payDate8"] = dt.Rows[i]["date"];
                    payDt.Rows[0]["downPayment"] = payment + lossRecovery;

                    G1.copy_dt_row(payDt, 0, paymentDetail, paymentDetail.Rows.Count);

                    serviceId = "";

                    contractNumber = payDt.Rows[0]["contractNumber"].ObjToString();
                    cmd = "Select * from `fcust_extended` where `contractNumber` = '" + contractNumber + "';";
                    payDt = G1.get_db_data(cmd);
                    if (payDt.Rows.Count > 0)
                    {
                        serviceId = payDt.Rows[0]["serviceId"].ObjToString();
                        location = payDt.Rows[0]["serviceLoc"].ObjToString();
                    }
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    //bankDetails = dt.Rows[i]["bankAccount"].ObjToString();

                    dR = dx.NewRow();

                    dR["accountTitle"] = matchBankAccount;

                    dR["date"] = G1.DTtoMySQLDT(dt.Rows[i]["date"].ObjToDateTime());
                    dR["DDA"] = payment + lossRecovery;

                    dR["aDate"] = date.ToString("yyyyMMdd");
                    dR["bank_account"] = bankDetails;
                    if (String.IsNullOrWhiteSpace(serviceId))
                        serviceId = fName + " " + lName;
                    dR["serviceId"] = serviceId;
                    dR["depositNumber"] = depositNumber;
                    dx.Rows.Add(dR);
                }
            }
            catch (Exception ex)
            {
            }
            return dx;
        }
        /****************************************************************************************/
        private DataTable loadBankDebits(DataTable dx, DateTime startDate, DateTime stopDate, string bankDetails, string bankAccount, string accountTitle, string fullBankDetails )
        {
            string date1 = G1.DateTimeToSQLDateTime(startDate);
            string date2 = G1.DateTimeToSQLDateTime(stopDate);

            if (G1.get_column_number(dx, "bankDebit") < 0)
                dx.Columns.Add("bankDebit");
            if (G1.get_column_number(dx, "misc") < 0)
                dx.Columns.Add("misc", Type.GetType("System.Double"));

            try
            {
                string cmd = "Select * from `bank_details` p WHERE `date` >= '" + date1 + "' and `date` <= '" + date2 + "' AND `bankAccount` = '" + bankAccount + "' AND `debit` > '0.00' ORDER BY `date` asc;";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                    return dx;

                string assignTo = "";
                double debit = 0D;
                string description = "";
                string debitDepNum = "";
                double dvalue = 0D;

                DataRow dRow = null;
                DateTime date = DateTime.Now;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    debit = dt.Rows[i]["debit"].ObjToDouble();
                    assignTo = dt.Rows[i]["assignTo"].ObjToString().ToUpper();
                    description = dt.Rows[i]["description"].ObjToString();
                    debitDepNum = dt.Rows[i]["debitDepNum"].ObjToString();

                    if ( !String.IsNullOrWhiteSpace ( assignTo ))
                    {
                    }

                    dvalue = debit;
                    debit = debit * -1D;

                    dRow = dx.NewRow();
                    dRow["date"] = G1.DTtoMySQLDT(date);
                    dRow["adate"] = date.ToString("yyyyMMdd");
                    dRow["bankDebit"] = dt.Rows[i]["record"].ObjToString();
                    dRow["comment"] = description + "~" + debitDepNum + "~" + G1.ReformatMoney(dvalue);

                    dRow["bank_account"] = fullBankDetails;
                    //dRow["comment"] = description;
                    //dRow["depositNumber"] = debitDepNum;
                    dRow["accountTitle"] = accountTitle;

                    if (assignTo == "TRUST DOWN PAYMENT")
                        dRow["DDA"] = debit;
                    else if (assignTo == "TRUST DEPOSIT")
                        dRow["TDA"] = debit;
                    else if (assignTo == "INSURANCE DEPOSIT")
                        dRow["IDA"] = debit;
                    else if (assignTo == "FUNERAL DEPOSIT")
                        dRow["NDA"] = debit;
                    else if (assignTo == "TRANSFER")
                        dRow["transfers"] = Math.Abs(debit);
                    else if (assignTo == "MISCELLANEOUS")
                        dRow["misc"] = debit;
                    else if (assignTo == "RETURN")
                        dRow["returns"] = debit;
                    dx.Rows.Add(dRow);
                }
            }
            catch (Exception ex)
            {
            }
            return dx;
        }
        /****************************************************************************************/
        DataTable getFuneralDetailPayments(DataTable dx, DateTime startDate, DateTime stopDate, DataTable dt, string bankDetails, string whatBox)
        {
            string date1 = G1.DateTimeToSQLDateTime(startDate);
            string date2 = G1.DateTimeToSQLDateTime(stopDate);

            try
            {
                if (paymentDetail == null)
                    paymentDetail = dt.Clone();

                DateTime oldDate = DateTime.Now;
                DateTime date = DateTime.Now;
                double dailyAmount = 0D;
                double paymentAmount = 0D;
                double credit = 0D;
                double debit = 0D;
                double downPayment = 0D;
                double payment = 0D;
                double totals = 0D;
                bool first = true;
                string depositNumber = "";
                string location = "";
                string edited = "";
                string serviceId = "";
                string comment = "";
                int day = 0;
                DateTime testDate = new DateTime(2019, 7, 31);
                string c = "";
                string fill1 = "";
                string firstChar = "";
                int numChars = 4;

                //string what = workReport.ToUpper();

                DataRow[] dRows = null;
                DataRow dR = null;

                string contractNumber = "";
                string status = "";

                TimeSpan ts;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    status = dt.Rows[i]["status"].ObjToString().ToUpper();
                    if (status != "RECEIVED" && status != "DEPOSITED" && status != "DEBIT")
                        continue;
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    date = dt.Rows[i]["dateReceived"].ObjToDateTime();
                    paymentAmount = dt.Rows[i]["amtActuallyReceived"].ObjToDouble();
                    if (paymentAmount <= 0D)
                        paymentAmount = dt.Rows[i]["paid"].ObjToDouble();
                    payment = paymentAmount;
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    serviceId = dt.Rows[i]["serviceId"].ObjToString();
                    if (serviceId == "BN22004")
                    {
                    }
                    if (depositNumber.ToUpper().IndexOf("TD") == 0 || depositNumber.ToUpper().IndexOf("CCTD") == 0 || depositNumber.ToUpper().IndexOf ( "T") == 0 )
                        continue;

                    //if (String.IsNullOrWhiteSpace(depositNumber))
                    //    continue;

                    //firstChar = depositNumber.ToUpper().Substring(0, 1);

                    //G1.copy_dt_row(dt, i, paymentDetail, paymentDetail.Rows.Count);

                    firstChar = depositNumber.ToUpper().Substring(0, 1);

                    dR = paymentDetail.NewRow();
                    dR["contractNumber"] = dt.Rows[i]["contractNumber"].ObjToString();
                    dR["payDate8"] = G1.DTtoMySQLDT(date);
                    dR["paymentAmount"] = paymentAmount;
                    dR["what"] = "Funeral";
                    dR["depositNumber"] = depositNumber;
                    dR["location"] = depositNumber;
                    if (depositNumber.Length >= 2)
                    {
                        location = depositNumber.Substring(0, 2);
                        dR["location"] = location;
                    }
                    paymentDetail.Rows.Add(dR);


                    totals += payment;

                    dR = dx.NewRow();

                    location = dt.Rows[i]["serviceLoc"].ObjToString();
                    dR["accountTitle"] = whatBox;

                    dR["date"] = G1.DTtoMySQLDT(dt.Rows[i]["dateReceived"].ObjToDateTime());
                    //dR["comment"] = contractNumber + " " + depositNumber;

                    if (status == "DEBIT")
                    {
                        dR["Returns"] = payment;
                        dR["comment"] = serviceId + " " + depositNumber + " " + G1.ReformatMoney(payment);
                    }
                    else
                    {
                        dR["NDA"] = payment;
                    }

                    dR["aDate"] = date.ToString("yyyyMMdd");
                    dR["bank_account"] = bankDetails;
                    dR["serviceId"] = serviceId;
                    dR["depositNumber"] = depositNumber;
                    dR["what"] = "Funeral";
                    dx.Rows.Add(dR);
                }
                dx.AcceptChanges();
            }
            catch (Exception ex)
            {
            }
            return dx;
        }
        /***************************************************************************************/
        private double RecalcDailyTotals(DataTable dt, double beginningBalance)
        {
            double originalEndingBalance = 0D;
            double tda = 0D;
            double ida = 0D;
            double nda = 0D;
            double dda = 0D;
            double misc = 0D;
            double returns = 0D;
            double transfers = 0D;
            double dailyTotals = 0D;
            double balance = beginningBalance;

            string accountTitle = "";
            string lastTitle = "";

            if (G1.get_column_number(dt, "dailyTotals") < 0)
                dt.Columns.Add("dailyTotals", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "printAccountTitle") < 0)
                dt.Columns.Add("printAccountTitle");

            string box = "Cash Deposits";
            bool gotNox = false;

            DateTime lastDate = DateTime.MinValue;
            DateTime date = DateTime.Now;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                accountTitle = dt.Rows[i]["accountTitle"].ObjToString();
                if (String.IsNullOrWhiteSpace(lastTitle))
                    lastTitle = accountTitle;
                dt.Rows[i]["printAccountTitle"] = accountTitle + " Beginning Balance = " + beginningBalance.ToString();
                date = dt.Rows[i]["date"].ObjToDateTime();
                if (lastDate == DateTime.MinValue)
                    lastDate = date;
                if (date != lastDate || lastTitle != accountTitle)
                {
                    dt.Rows[i - 1]["dailyTotals"] = dailyTotals;
                    dailyTotals = 0D;
                    lastDate = date;

                    lastTitle = accountTitle;
                }

                dt.Rows[i]["dailyTotals"] = 0D;

                tda = dt.Rows[i]["TDA"].ObjToDouble();
                ida = dt.Rows[i]["ida"].ObjToDouble();
                nda = dt.Rows[i]["nda"].ObjToDouble();
                if (nda != 0D)
                    gotNox = true;
                dda = dt.Rows[i]["dda"].ObjToDouble();
                misc = dt.Rows[i]["misc"].ObjToDouble();
                misc = Math.Abs(misc);
                returns = dt.Rows[i]["returns"].ObjToDouble();
                transfers = dt.Rows[i]["transfers"].ObjToDouble();

                balance = balance + tda + ida + nda + dda - misc - returns - transfers;
                balance = G1.RoundValue(balance);
                dt.Rows[i]["balance"] = balance;

                dailyTotals += tda + ida + nda + dda - returns;
                dailyTotals = G1.RoundValue(dailyTotals);

                if (i == (dt.Rows.Count - 1))
                {
                    dt.Rows[i]["dailyTotals"] = dailyTotals;
                    dailyTotals = 0D;
                }
            }

            return balance;
        }
        /***************************************************************************************/
        private double RecalcDailyTotals(DataTable dt, double beginningBalance, string bankDetails)
        {
            double originalEndingBalance = 0D;
            double tda = 0D;
            double ida = 0D;
            double nda = 0D;
            double dda = 0D;
            double misc = 0D;
            double returns = 0D;
            double transfers = 0D;
            double dailyTotals = 0D;
            double balance = beginningBalance;

            string accountTitle = "";
            string lastTitle = "";

            if (G1.get_column_number(dt, "dailyTotals") < 0)
                dt.Columns.Add("dailyTotals", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "printAccountTitle") < 0)
                dt.Columns.Add("printAccountTitle");

            string box = "Cash Deposits";
            bool gotNox = false;

            DateTime lastDate = DateTime.MinValue;
            DateTime date = DateTime.Now;

            DataRow[] dRows = dt.Select("bank_account='" + bankDetails + "'");
            if (dRows.Length <= 0)
                return balance;

            DataTable ddd = dRows.CopyToDataTable();

            for (int i = 0; i < dRows.Length; i++)
            {
                accountTitle = dRows[i]["accountTitle"].ObjToString();
                if (String.IsNullOrWhiteSpace(lastTitle))
                    lastTitle = accountTitle;
                dRows[i]["printAccountTitle"] = accountTitle + " Beginning Balance = " + beginningBalance.ToString();
                date = dRows[i]["date"].ObjToDateTime();
                if (lastDate == DateTime.MinValue)
                    lastDate = date;
                if (date != lastDate || lastTitle != accountTitle)
                {
                    dRows[i - 1]["dailyTotals"] = dailyTotals;
                    dailyTotals = 0D;
                    lastDate = date;

                    lastTitle = accountTitle;
                }

                dRows[i]["dailyTotals"] = 0D;

                tda = dRows[i]["TDA"].ObjToDouble();
                ida = dRows[i]["ida"].ObjToDouble();
                nda = dRows[i]["nda"].ObjToDouble();
                if (nda != 0D)
                    gotNox = true;
                dda = dRows[i]["dda"].ObjToDouble();
                misc = dRows[i]["misc"].ObjToDouble();
                returns = dRows[i]["returns"].ObjToDouble();
                transfers = dRows[i]["transfers"].ObjToDouble();

                balance = balance + tda + ida + nda + dda - misc - returns - transfers;
                balance = G1.RoundValue(balance);
                dRows[i]["balance"] = balance;

                dailyTotals += tda + ida + nda + dda - returns;
                dailyTotals = G1.RoundValue(dailyTotals);

                if (i == (dRows.Length - 1))
                {
                    dRows[i]["dailyTotals"] = dailyTotals;
                    dailyTotals = 0D;
                }
            }

            return balance;
        }
        /***************************************************************************************/
        private void LoadDOW(DataTable dt)
        {
            if (G1.get_column_number(dt, "dow") < 0)
                dt.Columns.Add("dow");

            string str = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["dow"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    dt.Rows[i]["dow"] = G1.DayOfWeekText(dt.Rows[i]["date"].ObjToDateTime());
            }
        }
        /***************************************************************************************/
        private string GetBankAccount(string what)
        {
            string location = "";
            string bank_gl = "";
            string bankAccount = "";
            string bankDetails = "";
            string cmd = "";
            if (what == "LKBX")
                cmd = "Select * from `bank_accounts` where `lkbx_ach` = '1';";
            else if (what == "TFBX")
                cmd = "Select * from `bank_accounts` where `tfbx` = '1';";
            else if (what == "ACH")
                cmd = "Select * from `bank_accounts` where `ach` = '1';";

            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                location = dx.Rows[0]["location"].ObjToString();
                bank_gl = dx.Rows[0]["general_ledger_no"].ObjToString();
                bankAccount = dx.Rows[0]["account_no"].ObjToString();
                bankDetails = location + "~" + bank_gl + "~" + bankAccount;
            }
            return bankDetails;
        }
        /****************************************************************************************/
        DataTable getTrustPayments(DataTable dx, DateTime startDate, DateTime stopDate, string bankDetails, string accountTitle)
        {
            string date1 = G1.DateTimeToSQLDateTime(startDate);
            string date2 = G1.DateTimeToSQLDateTime(stopDate);

            try
            {
                string cmd = "Select * from `payments` p LEFT JOIN `customers` f ON p.`contractNumber` = f.`contractNumber` WHERE `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' AND `bank_account` = '" + bankDetails + "' ORDER BY `payDate8` asc, `depositNumber`;";
                DataTable dt = G1.get_db_data(cmd);
                dt.Columns.Add("what");
                if (dt.Rows.Count <= 0)
                {
                    if (paymentDetail == null)
                        paymentDetail = dt.Clone();
                    return dx;
                }

                if (paymentDetail == null)
                    paymentDetail = dt.Clone();

                DailyHistory.RemoveDeletedPayments(dt);

                DateTime oldDate = DateTime.Now;
                DateTime date = DateTime.Now;
                double dailyAmount = 0D;
                double paymentAmount = 0D;
                double credit = 0D;
                double debit = 0D;
                double downPayment = 0D;
                double payment = 0D;
                double totals = 0D;
                bool first = true;
                string depositNumber = "";
                string serviceId = "";
                string location = "";
                string edited = "";
                int day = 0;
                DateTime testDate = new DateTime(2019, 7, 31);
                string c = "";
                string fill1 = "";
                string firstChar = "";
                int numChars = 4;
                string fName = "";
                string lName = "";

                string status = "";
                DataRow dR = null;

                string what = cmbLockbox.Text;

                DataRow[] dRows = null;

                string contractNumber = "";
                string str = "";

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();

                    if (String.IsNullOrWhiteSpace(depositNumber))
                        continue;

                    if (depositNumber.ToUpper().IndexOf("TD") == 0 || depositNumber.ToUpper().IndexOf("CCTD") == 0)
                        continue;
                    dt.Rows[i]["what"] = "Trust";

                    firstChar = depositNumber.ToUpper().Substring(0, 1);
                    if (firstChar.ToUpper() == "A")
                    {
                        if (depositNumber.Length >= 9)
                        {
                            str = depositNumber.Substring(1);
                            if (G1.validate_date(str))
                                continue;
                        }
                    }

                    location = dt.Rows[i]["location"].ObjToString();
                    //if (location.ToUpper() == "CC" || location.ToUpper() == "CCDC" || location.ToUpper() == "HOCC")
                    if (location.ToUpper() == "CC" || location.ToUpper() == "CCDC")
                        continue;

                    date = dt.Rows[i]["payDate8"].ObjToDateTime();
                    paymentAmount = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                    payment = paymentAmount;
                    serviceId = dt.Rows[i]["serviceId"].ObjToString();
                    edited = dt.Rows[i]["edited"].ObjToString().ToUpper();
                    if (edited == "TRUSTADJ" || edited == "CEMETERY")
                        continue;

                    fName = dt.Rows[i]["firstName"].ObjToString();
                    lName = dt.Rows[i]["lastName"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(fName))
                        fName = fName.Substring(0, 1);

                    dR = dx.NewRow();

                    dR["accountTitle"] = accountTitle;
                    dR["what"] = "Trust";

                    dR["date"] = G1.DTtoMySQLDT(dt.Rows[i]["payDate8"].ObjToDateTime());
                    //dR["comment"] = contractNumber + " " + depositNumber;

                    if (debit > 0D)
                        dR["Returns"] = debit;
                    else if (downPayment > 0D)
                        dR["DDA"] = downPayment;
                    else
                        dR["TDA"] = payment;
                    dR["depositNumber"] = depositNumber;
                    //if (String.IsNullOrWhiteSpace(serviceId))
                    //    serviceId = fName + " " + lName;
                    //dR["serviceId"] = serviceId;
                    dR["aDate"] = date.ToString("yyyyMMdd");
                    dR["bank_account"] = bankDetails;
                    dx.Rows.Add(dR);

                    G1.copy_dt_row(dt, i, paymentDetail, paymentDetail.Rows.Count);

                    totals += payment;
                }
                dx.AcceptChanges();
            }
            catch (Exception ex)
            {
            }
            return dx;
        }
        /****************************************************************************************/
        DataTable getInsurancePayments(DataTable dx, DateTime startDate, DateTime stopDate, string bankDetails, string accountTitle)
        {
            string date1 = G1.DateTimeToSQLDateTime(startDate);
            string date2 = G1.DateTimeToSQLDateTime(stopDate);

            try
            {
                string cmd = "Select * from `ipayments` p LEFT JOIN `icustomers` f ON p.`contractNumber` = f.`contractNumber` WHERE `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' AND `bank_account` = '" + bankDetails + "' ORDER BY `payDate8` asc, `depositNumber`;";
                //string cmd = "Select * from `ipayments` where `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' AND `bank_account` = '" + bankDetails + "' ORDER BY `payDate8` asc;";
                DataTable dt = G1.get_db_data(cmd);
                dt.Columns.Add("what");
                if (dt.Rows.Count <= 0)
                    return dx;

                if (paymentDetail == null)
                    paymentDetail = dt.Clone();

                DailyHistory.RemoveDeletedPayments(dt);

                DateTime oldDate = DateTime.Now;
                DateTime date = DateTime.Now;
                double dailyAmount = 0D;
                double paymentAmount = 0D;
                double credit = 0D;
                double debit = 0D;
                double downPayment = 0D;
                double payment = 0D;
                double totals = 0D;
                bool first = true;
                string depositNumber = "";
                string location = "";
                string edited = "";
                int day = 0;
                DateTime testDate = new DateTime(2019, 7, 31);
                string c = "";
                string fill1 = "";
                string firstChar = "";
                int numChars = 4;
                string fName = "";
                string lName = "";
                string serviceId = "";

                string status = "";
                DataRow dR = null;

                string what = cmbLockbox.Text;

                DataRow[] dRows = null;

                string contractNumber = "";

                TimeSpan ts;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    date = dt.Rows[i]["payDate8"].ObjToDateTime();
                    paymentAmount = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                    payment = paymentAmount;
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    serviceId = dt.Rows[i]["serviceId"].ObjToString();
                    edited = dt.Rows[i]["edited"].ObjToString().ToUpper();
                    if (edited == "TRUSTADJ" || edited == "CEMETERY")
                        continue;

                    location = dt.Rows[i]["location"].ObjToString();
                    //if (location.ToUpper() == "CC" || location.ToUpper() == "CCDC" || location.ToUpper() == "HOCC" )
                    if (location.ToUpper() == "CC" || location.ToUpper() == "CCDC")
                        continue;

                    fName = dt.Rows[i]["firstName"].ObjToString();
                    lName = dt.Rows[i]["lastName"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(fName))
                        fName = fName.Substring(0, 1);

                    dR = dx.NewRow();

                    dR["accountTitle"] = accountTitle;

                    dR["date"] = G1.DTtoMySQLDT(dt.Rows[i]["payDate8"].ObjToDateTime());
                    //dR["comment"] = contractNumber + " " + depositNumber;
                    dR["comment"] = "";

                    if (debit > 0D)
                    {
                        dR["Returns"] = debit;
                    }
                    else
                        dR["IDA"] = payment;

                    dR["aDate"] = date.ToString("yyyyMMdd");
                    dR["bank_account"] = bankDetails;
                    dR["depositNumber"] = depositNumber;
                    dR["what"] = "Ins";
                    //if (String.IsNullOrWhiteSpace(serviceId))
                    //    serviceId = fName + " " + lName;
                    //dR["serviceId"] = serviceId;
                    dx.Rows.Add(dR);

                    dt.Rows[i]["what"] = "Ins";

                    G1.copy_dt_row(dt, i, paymentDetail, paymentDetail.Rows.Count);

                    totals += payment;
                }
                dx.AcceptChanges();
            }
            catch (Exception ex)
            {
            }
            return dx;
        }
        /****************************************************************************************/
        private void zeroOutColumn(DataTable dx, string column)
        {
            if (G1.get_column_number(dx, column) < 0)
                return;
            for (int i = 0; i < dx.Rows.Count; i++)
                dx.Rows[i][column] = 0D;
        }
        /****************************************************************************************/
        private bool CheckForSave()
        {
            if (!btnSave.Visible)
                return true;
            DialogResult result = MessageBox.Show("***Question***\nInformation has been modified!\nWould you like to save your changes?", "Modified Data Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
                return false;
            if (result == DialogResult.No)
                return true;
            SaveData();
            return true;
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveData();
        }
        /****************************************************************************************/
        private void SaveData()
        {
            this.Cursor = Cursors.WaitCursor;
            DateTime stopDate = this.dateTimePicker1.Value;
            DateTime startDate = new DateTime(stopDate.Year, stopDate.Month, 1);
            string date1 = G1.DateTimeToSQLDateTime(startDate);
            string date2 = G1.DateTimeToSQLDateTime(stopDate);

            DateTime saveDate = DateTime.Now;
            string date = "";

            DataTable dx = (DataTable)dgv.DataSource;
            DataTable dt = dx.Copy();

            DataView tempview = dt.DefaultView;
            tempview.Sort = "accountTitle,date";
            dt = tempview.ToTable();

            double tda = 0D;
            double ida = 0D;
            double nda = 0D;
            double dda = 0D;
            double misc = 0D;
            double returns = 0D;
            double transfer = 0D;
            double balance = 0D;
            string comment = "";
            string aDate = "";
            string record = "";
            string manual = "";
            string bankAccount = "";
            string oldBankAccount = "";
            double endingBalance = 0D;

            string depositNumber = "";
            string serviceId = "";

            double credits = 0D;
            double debits = 0D;
            string accountTitle = "";
            string d_tda = "";
            string d_ida = "";
            string d_nda = "";
            string d_dda = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                bankAccount = dt.Rows[i]["bank_account"].ObjToString();
                if (String.IsNullOrWhiteSpace(oldBankAccount))
                {
                    oldBankAccount = bankAccount;
                    string cmd = "DELETE FROM `lockboxdeposits` where `date` >= '" + date1 + "' AND `date` <= '" + date2 + "' AND `bank_account` = '" + bankAccount + "';";
                    G1.get_db_data(cmd);
                }
                if (oldBankAccount != bankAccount)
                {
                    if (!String.IsNullOrWhiteSpace(oldBankAccount))
                    {
                        beginningBalance = getBeginningBalance(oldBankAccount, startDate, ref endingBalance, ref accountTitle);
                        UpdateBankTotals(oldBankAccount, this.dateTimePicker1.Value, credits, debits, beginningBalance, balance, d_tda, d_ida, d_nda, d_dda);
                    }
                    oldBankAccount = bankAccount;
                    d_tda = "";
                    d_ida = "";
                    d_nda = "";
                    d_dda = "";

                    string cmd = "DELETE FROM `lockboxdeposits` where `date` >= '" + date1 + "' AND `date` <= '" + date2 + "' AND `bank_account` = '" + bankAccount + "';";
                    G1.get_db_data(cmd);
                }

                saveDate = dt.Rows[i]["date"].ObjToDateTime();
                date = saveDate.ToString("MM/dd/yyyy");
                tda = dt.Rows[i]["TDA"].ObjToDouble();
                ida = dt.Rows[i]["IDA"].ObjToDouble();
                nda = dt.Rows[i]["NDA"].ObjToDouble();
                dda = dt.Rows[i]["DDA"].ObjToDouble();
                misc = dt.Rows[i]["misc"].ObjToDouble();
                returns = dt.Rows[i]["returns"].ObjToDouble();
                transfer = dt.Rows[i]["transfers"].ObjToDouble();
                balance = dt.Rows[i]["balance"].ObjToDouble();
                comment = dt.Rows[i]["comment"].ObjToString();
                if (comment.Trim().ToUpper() == "BALANCE FORWARD")
                    continue;
                aDate = dt.Rows[i]["adate"].ObjToString();
                accountTitle = dt.Rows[i]["accountTitle"].ObjToString();
                manual = dt.Rows[i]["manual"].ObjToString();

                tda = G1.RoundValue(tda);
                ida = G1.RoundValue(ida);
                nda = G1.RoundValue(nda);
                dda = G1.RoundValue(dda);
                misc = G1.RoundValue(misc);
                returns = G1.RoundValue(returns);
                transfer = G1.RoundValue(transfer);
                balance = G1.RoundValue(balance);

                credits += tda + ida + nda + dda;
                debits += misc + returns + transfer;

                depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( depositNumber ))
                {
                    if (tda != 0D)
                        d_tda = depositNumber;
                    else if (ida != 0D)
                        d_ida = depositNumber;
                    else if (nda != 0D)
                        d_nda = depositNumber;
                    else if (dda != 0D)
                        d_dda = depositNumber;
                }

                if (manual.ToUpper() == "Y")
                {
                    record = G1.create_record("lockboxdeposits", "comment", "-1");
                    if (G1.BadRecord("lockboxdeposits", record))
                        break;
                    G1.update_db_table("lockboxdeposits", "record", record, new string[] { "date", date, "adate", aDate, "comment", comment, "TDA", tda.ToString(), "IDA", ida.ToString(), "NDA", nda.ToString(), "dda", dda.ToString(), "misc", misc.ToString(), "returns", returns.ToString(), "transfers", transfer.ToString(), "balance", balance.ToString(), "manual", manual, "bank_account", bankAccount });
                    if (manual.ToUpper() == "Y")
                    {
                        depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                        serviceId = dt.Rows[i]["serviceId"].ObjToString();
                        G1.update_db_table("lockboxdeposits", "record", record, new string[] { "depositNumber", depositNumber, "serviceId", serviceId });
                    }
                }

                if (i == (dt.Rows.Count - 1))
                {
                    if (!String.IsNullOrWhiteSpace(bankAccount))
                    {
                        beginningBalance = getBeginningBalance(bankAccount, startDate, ref endingBalance, ref accountTitle);
                        UpdateBankTotals(bankAccount, this.dateTimePicker1.Value, credits, debits, beginningBalance, balance, d_tda, d_ida, d_nda, d_dda);
                    }
                    oldBankAccount = bankAccount;
                }
            }

            this.Cursor = Cursors.Default;
            btnSave.Hide();
        }
        /****************************************************************************************/
        private void UpdateBankTotals(string bankAccount, DateTime monthDate, double credits, double debits, double beginningBalance, double endingbalance, string tda, string ida, string nda, string dda )
        {
            int days = DateTime.DaysInMonth(monthDate.Year, monthDate.Month);

            DateTime searchDate = new DateTime(monthDate.Year, monthDate.Month, days);

            string cmd = "Select * from `bank_totals` where `bank_account` = '" + bankAccount + "' AND `date` = '" + searchDate.ToString("yyyy-MM-dd") + "';";
            DataTable dt = G1.get_db_data(cmd);
            string record = "";
            if (dt.Rows.Count <= 0)
                record = G1.create_record("bank_totals", "adate", "-1");
            else
                record = dt.Rows[0]["record"].ObjToString();

            G1.update_db_table("bank_totals", "record", record, new string[] { "bank_account", bankAccount, "date", searchDate.ToString("yyyy-MM-dd"), "adate", "", "credits", credits.ToString(), "debits", debits.ToString(), "beginningBalance", beginningBalance.ToString(), "endingBalance", endingbalance.ToString(), "tda", tda, "ida", ida, "nda", nda, "dda", dda });

            cmd = "Select * from `bank_totals` where `bank_account` = '" + bankAccount + "' AND `date` > '" + searchDate.ToString("yyyy-MM-dd") + "';";
            dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                debits = dt.Rows[i]["debits"].ObjToDouble();
                credits = dt.Rows[i]["credits"].ObjToDouble();
                beginningBalance = endingbalance;
                beginningBalance = G1.RoundValue(beginningBalance);
                endingbalance = beginningBalance + credits - debits;
                endingbalance = G1.RoundValue(endingbalance);

                G1.update_db_table("bank_totals", "record", record, new string[] { "beginningBalance", beginningBalance.ToString(), "endingBalance", endingbalance.ToString() });
            }
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            btnSave.Show();

            if (e == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            DataTable dt = (DataTable)dgv.DataSource;

            string bankAccount = dr["bank_account"].ObjToString();

            bool doTotal = false;
            string column = e.Column.FieldName.Trim().ToUpper();
            if (column == "TDA" || column == "IDA" || column == "DDA" || column == "PDA"  || column == "NDA" || column == "MISC" || column == "RETURNS" || column == "TRANSFERS" )
               doTotal = true;

            if (e.Column.FieldName.Trim().ToUpper() == "DATE")
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "accountTitle,date";
                dt = tempview.ToTable();
                dgv.DataSource = dt;

                RecalcBankAccount(dt, bankAccount);
                dgv.Refresh();
                return;
            }
            else if (e.Column.FieldName.Trim().ToUpper() == "TRANSFERS")
            {
                RecalcBankAccount(dt, bankAccount);
                dgv.Refresh();
                return;
            }
            else if (e.Column.FieldName.Trim().ToUpper() == "RETURNS")
            {
                RecalcBankAccount(dt, bankAccount);
                dgv.Refresh();
                return;
            }
            else if (e.Column.FieldName.Trim().ToUpper() == "MISC")
            {
                RecalcBankAccount(dt, bankAccount);
                dgv.Refresh();
                return;
            }
            else if (e.Column.FieldName.Trim().ToUpper() == "DEPOSITNUMBER")
            {
                string serviceId = dt.Rows[row]["serviceId"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( serviceId ) && oldColumn == "DEPOSITNUMBER" )
                {
                    string depositNumber = dt.Rows[row]["depositNumber"].ObjToString();
                    string cmd = "Select * from `fcust_extended` WHERE `serviceId` = '" + serviceId + "';";
                    DataTable exDt = G1.get_db_data(cmd);
                    if ( exDt.Rows.Count > 0 )
                    {
                        string contractNumber = exDt.Rows[0]["contractNumber"].ObjToString();
                        cmd = "Select * from `cust_payment_details` where `contractNumber` = '" + contractNumber + "' AND `depositNumber` = '" + oldWhat + "';";
                        exDt = G1.get_db_data(cmd);
                        if ( exDt.Rows.Count == 1 )
                        {
                            string record = exDt.Rows[0]["record"].ObjToString();
                            //G1.update_db_table("cust_payment_details", "record", record, new string[] { "depositNumber", depositNumber });
                        }
                    }
                }
            }

            if (doTotal)
            {
                //RecalcBalance(dt, row);
                RecalcBankAccount(dt, bankAccount);
                dgv.Refresh();
                return;
            }

            //dgv.DataSource = dt;
            //dgv.Refresh();
        }
        /****************************************************************************************/
        private void RecalcBalance ( DataTable dt, int startRow )
        {
            double tda = 0D;
            double ida = 0D;
            double nda = 0D;
            double dda = 0D;
            double misc = 0D;
            double returns = 0D;
            double transfers = 0D;
            double dailyTotals = 0D;
            string comment = "";
            try
            {
                double balance = dt.Rows[startRow - 1]["balance"].ObjToDouble();

                for (int i = startRow; i < dt.Rows.Count; i++)
                {
                    comment = dt.Rows[i]["comment"].ObjToString().ToUpper();
                    if (comment == "BALANCE FORWARD")
                        break;

                    tda = dt.Rows[i]["TDA"].ObjToDouble();
                    ida = dt.Rows[i]["IDA"].ObjToDouble();
                    nda = dt.Rows[i]["NDA"].ObjToDouble();
                    dda = dt.Rows[i]["DDA"].ObjToDouble();
                    misc = dt.Rows[i]["misc"].ObjToDouble();
                    returns = dt.Rows[i]["returns"].ObjToDouble();
                    transfers = dt.Rows[i]["transfers"].ObjToDouble();

                    balance = balance + tda + ida + nda + dda - misc - returns - transfers;
                    balance = G1.RoundValue(balance);
                    dt.Rows[i]["balance"] = balance;

                    dailyTotals = dt.Rows[i]["dailyTotals"].ObjToDouble();
                    if ( dailyTotals != 0D)
                    {
                        dailyTotals += tda + ida + nda - misc - returns - transfers;
                        dailyTotals = G1.RoundValue(dailyTotals);
                        dt.Rows[i]["dailyTotals"] = dailyTotals;
                    }
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataTable ddd = (DataTable)dgv.DataSource;

            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);

            DateTime date = dr["date"].ObjToDateTime();
            string bankDebit = dr["bankDebit"].ObjToString();
            if (!String.IsNullOrWhiteSpace(bankDebit))
            {
                string cmd = "Select * from `bank_details` WHERE `record` = '" + bankDebit + "';";
                DataTable bankDt = G1.get_db_data(cmd);
                if (bankDt.Rows.Count > 0)
                {
                    bankDt.Columns.Add("depositNumber");
                    bankDt.Rows[0]["depositNumber"] = bankDt.Rows[0]["description"].ObjToString();
                    bankDt.Columns.Add("bank_account");
                    bankDt.Rows[0]["bank_account"] = dr["bank_account"].ObjToString();

                    BankEditDebit debitForm = new BankEditDebit(bankDt, true );
                    debitForm.Text = "Document Debit for " + date.ToString("MM/dd/yyyy");
                    debitForm.TopMost = true;
                    debitForm.ManualDone += DebitForm_ManualDone;
                    debitForm.ShowDialog();
                    return;
                }
            }

            //string what = workReport;

            if (paymentDetail != null)
            {
                try
                {
                    if (G1.get_column_number(paymentDetail, "search") >= 0)
                        paymentDetail.Columns.Remove("search");

                    paymentDetail.Columns.Add("search");

                    for (int i = 0; i < paymentDetail.Rows.Count; i++)
                        paymentDetail.Rows[i]["search"] = paymentDetail.Rows[i]["payDate8"].ObjToDateTime().ToString("yyyyMMdd");

                    string str = date.ToString("yyyyMMdd");

                    string cmd = "search='" + str + "'";

                    DataRow[] dRows = paymentDetail.Select(cmd);

                    if (oldDt != null)
                    {
                        if (oldDt.Rows.Count > 0)
                        {
                            DateTime oldDate = DateTime.Now;
                            for (int j = 0; j < oldDt.Rows.Count; j++)
                            {
                                oldDate = oldDt.Rows[j]["date"].ObjToDateTime();
                                oldDt.Rows[j]["aDate"] = oldDate.ToString("yyyyMMdd");
                            }
                        }
                    }

                    bool doManual = false;
                    str = date.ToString("yyyyMMdd");
                    cmd = "adate='" + str + "'";

                    DataRow[] tRows = null;
                    if (oldDt != null)
                    {
                        tRows = oldDt.Select(cmd);
                        if (tRows.Length > 0)
                            doManual = true;
                    }

                    string selection = "";
                    bool doDeposits = false;
                    if (dRows.Length > 0)
                        doDeposits = true;
                    if (doManual && dRows.Length > 0)
                    {
                        doManual = false;
                        doDeposits = false;
                        string lines = "Manual Edits\nDeposit Details";
                        using (SelectFromList listForm = new SelectFromList(lines, false))
                        {
                            listForm.Text = "Choose only one of these options!";
                            listForm.ShowDialog();
                            selection = SelectFromList.theseSelections;
                            if (String.IsNullOrWhiteSpace(selection))
                                return;
                            if (selection == "Manual Edits")
                                doManual = true;
                            else
                                doDeposits = true;
                        }
                    }
                    if (doManual)
                    {
                        DataTable dt = tRows.CopyToDataTable();
                        //ViewDataTable viewForm = new ViewDataTable(dt, "adate,misc, returns,transfers,comment");
                        //viewForm.Text = this.Text + " for Manual Edits for " + date.ToString("MM/dd/yyyy");
                        //viewForm.ShowDialog();
                        EditManualBank viewForm = new EditManualBank(dt);
                        viewForm.ManualDone += ViewForm_ManualDone;
                        viewForm.Text = this.Text + " for Manual Edits for " + date.ToString("MM/dd/yyyy");
                        viewForm.ShowDialog();
                        return;
                    }

                    if (doDeposits)
                    {
                        DataTable dt = dRows.CopyToDataTable();
                        ViewDataTable viewForm = new ViewDataTable(dt, "contractNumber,firstName, lastName,paymentAmount,debitAdjustment,depositNumber,fill1");
                        viewForm.Text = this.Text + " for " + date.ToString("MM/dd/yyyy");
                        viewForm.ShowDialog();
                        return;
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        /****************************************************************************************/
        private void ViewForm_ManualDone(DataTable dd, DateTime workDate, DataTable originalDt)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            ResetEditRow(dd, workDate, originalDt, "comment");

            ResetEditRow(dd, workDate, originalDt, "returns");
            ResetEditRow(dd, workDate, originalDt, "TDA");
            ResetEditRow(dd, workDate, originalDt, "IDA");
            //            ResetEditRow(dd, workDate, originalDt, "PDA");
            ResetEditRow(dd, workDate, originalDt, "NDA");
            ResetEditRow(dd, workDate, originalDt, "DDA");
            ResetEditRow(dd, workDate, originalDt, "misc");
            ResetEditRow(dd, workDate, originalDt, "transfers");

            DataRow[] ddRows = dd.Select("mod='D'");
            if (ddRows.Length <= 0)
                return;

            string record = "";
            string oldRecord = "";

            for (int i = 0; i < ddRows.Length; i++)
            {
                record = ddRows[i]["record"].ObjToString();
                if (!String.IsNullOrWhiteSpace(record))
                {
                    for (int j = 0; j < oldDt.Rows.Count; j++)
                    {
                        oldRecord = oldDt.Rows[j]["record"].ObjToString();
                        if (record == oldRecord)
                        {
                            oldDt.Rows.RemoveAt(j);
                            break;
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
        private void ResetEditRow(DataTable dd, DateTime workDate, DataTable originalDt, string fieldName)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            string record = "";
            string oldRecord = "";

            try
            {
                bool gotDelete = false;
                DataRow[] ddRows = dd.Select("mod='D'");
                if (ddRows.Length > 0)
                    gotDelete = true;

                DataRow[] dRows = dt.Select("adate=" + workDate.ToString("yyyyMMdd"));
                string dataType = dd.Columns[fieldName].DataType.ObjToString();
                if (dRows.Length > 0)
                {
                    if (dataType.ToUpper() == "SYSTEM.STRING")
                    {
                        ResetEditCommentRow(dd, workDate, originalDt, fieldName);
                        gridMain.RefreshEditor(true);
                        return;
                    }
                    DataTable editDt = dRows.CopyToDataTable();
                    double oldReturn = 0D;
                    double newReturn = 0D;
                    double returns = 0D;
                    if (originalDt != null)
                    {
                        for (int i = 0; i < originalDt.Rows.Count; i++)
                            oldReturn += originalDt.Rows[i][fieldName].ObjToDouble();
                    }

                    for (int i = 0; i < dd.Rows.Count; i++)
                    {
                        if (dd.Rows[i]["mod"].ObjToString() != "D")
                        {
                            record = dd.Rows[i]["record"].ObjToString();
                            newReturn += dd.Rows[i][fieldName].ObjToDouble();
                        }
                    }

                    returns = editDt.Rows[0][fieldName].ObjToDouble();
                    returns = returns - oldReturn + newReturn;
                    dRows[0][fieldName] = returns;

                    string str = workDate.ToString("yyyyMMdd");
                    string cmd = "adate='" + str + "'";

                    DataRow[] tRows = oldDt.Select(cmd);
                    if (tRows.Length > 0)
                    {
                        for (int i = 0; i < tRows.Length; i++)
                        {
                            record = tRows[i]["record"].ObjToString();
                            for (int j = 0; j < dd.Rows.Count; j++)
                            {
                                oldRecord = dd.Rows[j]["record"].ObjToString();
                                if (record == oldRecord)
                                {
                                    tRows[i][fieldName] = dd.Rows[j][fieldName].ObjToDouble();
                                    dd.Rows[j][fieldName] = 0D;
                                    break;
                                }
                            }
                        }
                        //if (gotDelete)
                        //{
                        //    for ( int i=0; i<ddRows.Length; i++ )
                        //    {
                        //        record = ddRows[i]["record"].ObjToString();
                        //        if ( !String.IsNullOrWhiteSpace ( record ))
                        //        {
                        //            for ( int j=0; j<oldDt.Rows.Count; j++)
                        //            {
                        //                oldRecord = oldDt.Rows[j]["record"].ObjToString();
                        //                if ( record == oldRecord )
                        //                {
                        //                    oldDt.Rows.RemoveAt(j);
                        //                    break;
                        //                }
                        //            }
                        //        }
                        //    }
                        //}
                    }
                    gridMain.RefreshEditor(true);
                }
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void ResetEditCommentRow(DataTable dd, DateTime workDate, DataTable originalDt, string fieldName)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            DataRow[] dRows = dt.Select("adate=" + workDate.ToString("yyyyMMdd"));
            string dataType = dd.Columns[fieldName].DataType.ObjToString();
            if (dRows.Length > 0)
            {
                DataTable editDt = dRows.CopyToDataTable();

                string comment = "";
                string newComment = "";
                if (originalDt == null)
                {
                    newComment = dRows[0][fieldName].ObjToString();
                    newComment = newComment.TrimEnd('\n');
                    newComment = newComment.TrimEnd('\r');
                    //if (newComment.IndexOf('\n') >= 0)
                    //    newComment = newComment.Replace("\n", "");
                    //if (newComment.IndexOf('\r') >= 0)
                    //    newComment = newComment.Replace("\r", "");
                }

                for (int i = 0; i < dd.Rows.Count; i++)
                {
                    if (dd.Rows[i]["mod"].ObjToString() == "D")
                        continue;
                    comment = dd.Rows[i][fieldName].ObjToString();
                    comment = comment.TrimEnd('\n');
                    comment = comment.TrimEnd('\r');
                    //if (comment.IndexOf('\n') >= 0)
                    //    comment = comment.Replace("\n", "");
                    //if (comment.IndexOf('\r') >= 0)
                    //    comment = comment.Replace("\r", "");
                    if (!String.IsNullOrWhiteSpace(newComment))
                        newComment += "\n";
                    newComment += comment;
                }
                dRows[0][fieldName] = newComment;

                string str = workDate.ToString("yyyyMMdd");
                string cmd = "adate='" + str + "'";

                string record = "";
                string oldRecord = "";

                DataRow[] tRows = oldDt.Select(cmd);
                if (tRows.Length > 0)
                {
                    for (int i = 0; i < tRows.Length; i++)
                    {
                        record = tRows[i]["record"].ObjToString();
                        for (int j = 0; j < dd.Rows.Count; j++)
                        {
                            oldRecord = dd.Rows[j]["record"].ObjToString();
                            if (record == oldRecord)
                            {
                                if (dd.Rows[j]["mod"].ObjToString() == "D")
                                    tRows[i][fieldName] = "";
                                else
                                {
                                    comment = dd.Rows[j][fieldName].ObjToString();
                                    tRows[i][fieldName] = comment;
                                    dd.Rows[j][fieldName] = "";
                                }
                                break;
                            }
                        }
                    }
                }
                gridMain.RefreshEditor(true);
            }
        }
        /****************************************************************************************/
        private void DebitForm_ManualDone(DataTable dd)
        {
            if (dd == null)
                return;
            if (dd.Rows.Count <= 0)
                return;

            string record = dd.Rows[0]["record"].ObjToString();
            double debit = dd.Rows[0]["debit"].ObjToDouble();
            DateTime date = dd.Rows[0]["date"].ObjToDateTime();
            string bankAccount = dd.Rows[0]["bankAccount"].ObjToString();
            string bank_account = dd.Rows[0]["bank_account"].ObjToString();
            string debitDepNum = dd.Rows[0]["debitDepNum"].ObjToString();
            string depositNumber = dd.Rows[0]["depositNumber"].ObjToString();
            string assignTo = dd.Rows[0]["assignTo"].ObjToString().ToUpper();

            string found = "";
            string bankRecord = "";

            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    bankRecord = dt.Rows[i]["bankDebit"].ObjToString();
                    if (bankRecord == record)
                    {
                        G1.update_db_table("bank_details", "record", record, new string[] { "assignTo", assignTo, "description", depositNumber, "debitDepNum", debitDepNum });
                        //dt.Rows[i]["depositNumber"] = depositNumber;
                        //dt.Rows[i]["assignTo"] = assignTo;
                        //dt.Rows[i]["debitDepNum"] = debitDepNum;
                        dt.Rows[i]["DDA"] = 0D;
                        dt.Rows[i]["TDA"] = 0D;
                        dt.Rows[i]["IDA"] = 0D;
                        dt.Rows[i]["NDA"] = 0D;
                        dt.Rows[i]["misc"] = 0D;
                        dt.Rows[i]["returns"] = 0D;
                        dt.Rows[i]["transfers"] = 0D;
                        dt.Rows[i]["comment"] = depositNumber;

                        if (assignTo == "TRUST DOWN PAYMENT")
                            dt.Rows[i]["DDA"] = debit;
                        else if (assignTo == "TRUST DEPOSIT")
                            dt.Rows[i]["TDA"] = debit;
                        else if (assignTo == "INSURANCE DEPOSIT")
                            dt.Rows[i]["IDA"] = debit;
                        else if (assignTo == "FUNERAL DEPOSIT")
                            dt.Rows[i]["NDA"] = debit;
                        else if (assignTo == "TRANSFER")
                            dt.Rows[i]["transfers"] = Math.Abs(debit);
                        else if (assignTo == "RETURN")
                            dt.Rows[i]["returns"] = debit;
                        else if (assignTo == "MISCELLANEOUS")
                            dt.Rows[i]["misc"] = debit;

                        //string aDate = date.ToString("yyyyMMdd");
                        //DataRow[] dRows = dt.Select("aDate='" + aDate + "'");
                        //if (dRows.Length > 0)
                        //{
                        //    double dda = 0D;
                        //    double tda = 0D;
                        //    double ida = 0D;
                        //    double nda = 0D;
                        //    double returns = 0D;
                        //    double transfers = 0D;
                        //    double misc = 0D;

                        //    DataTable bankDt = dRows.CopyToDataTable();
                        //    for (int j = 0; j < bankDt.Rows.Count; j++)
                        //    {
                        //        dda += bankDt.Rows[j]["DDA"].ObjToDouble();
                        //        tda += bankDt.Rows[j]["TDA"].ObjToDouble();
                        //        ida += bankDt.Rows[j]["IDA"].ObjToDouble();
                        //        nda += bankDt.Rows[j]["NDA"].ObjToDouble();
                        //        returns += bankDt.Rows[j]["returns"].ObjToDouble();
                        //        transfers += bankDt.Rows[j]["transfers"].ObjToDouble();
                        //    }
                        //    dRows = dt.Select("bankDebit='" + bankRecord + "'");
                        //    if (dRows.Length > 0)
                        //        dt.Rows.Remove(dRows[0]);
                        //    dRows = dt.Select("aDate='" + aDate + "'");
                        //    if (dRows.Length > 0)
                        //    {
                        //        dRows[0]["DDA"] = dda;
                        //        dRows[0]["TDA"] = tda;
                        //        dRows[0]["IDA"] = ida;
                        //        dRows[0]["NDA"] = nda;
                        //        dRows[0]["returns"] = returns;
                        //        dRows[0]["transfers"] = transfers;
                        //    }
                        //}
                        RecalcBankAccount(dt, bank_account);
                        //double balance = RecalcTotals(dt);
                        dgv.DataSource = dt;
                        dgv.RefreshDataSource();
                        gridMain.RefreshEditor(true);
                        break;
                    }
                }
                catch ( Exception ex )
                {
                }
            }
        }
        /****************************************************************************************/
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker1.Value = date;
        }
        /****************************************************************************************/
        private void pictureAdd_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            DateTime date = dr["date"].ObjToDateTime();
            string accountTitle = dr["accountTitle"].ObjToString();
            string bankAccount = dr["bank_account"].ObjToString();
            double balance = dr["balance"].ObjToDouble();
            string comment = dr["comment"].ObjToString().ToUpper();

            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (row < 0 || row > (dt.Rows.Count - 1))
                return;
            bankAccount = dt.Rows[row]["bank_account"].ObjToString();
            string[] Lines = bankAccount.Split('~');
            if ( Lines.Length >= 3 )
            {
                bankAccount = Lines[2];
                string cmd = "Select * from `bank_accounts` WHERE `account_no` = '" + bankAccount + "';";
                DataTable ddx = G1.get_db_data(cmd);
                if ( ddx.Rows.Count > 0 )
                {
                    bankAccount = ddx.Rows[0]["location"].ObjToString() + "~" + ddx.Rows[0]["general_ledger_no"].ObjToString() + "~" + ddx.Rows[0]["account_no"].ObjToString();
                }
            }
            //if (rowHandle == (dt.Rows.Count - 1))
            //    return; // Already at the last row
            DataRow dRow = dt.NewRow();
            dRow["date"] = G1.DTtoMySQLDT(date);
            dRow["TDA"] = 0.0D;
            dRow["IDA"] = 0.0D;
            dRow["NDA"] = 0.0D;
            dRow["DDA"] = 0.0D;
            dRow["misc"] = 0.0D;
            dRow["returns"] = 0.0D;
            dRow["transfers"] = 0.0D;
            dRow["dailyTotals"] = 0.00D;
            dRow["dow"] = G1.DayOfWeekText(date);
            dRow["accountTitle"] = accountTitle;
            dRow["bank_account"] = bankAccount;
            dRow["balance"] = balance;
            dRow["manual"] = "Y";
            dRow["comment"] = "Enter Comment Here";
            if (comment.ToUpper() == "BALANCE FORWARD")
            {
                if (row == (dt.Rows.Count - 1))
                    dt.Rows.Add(dRow);
                else
                    dt.Rows.InsertAt(dRow, row + 1);
            }
            else
                dt.Rows.InsertAt(dRow, row);

            //DataView tempview = dt.DefaultView;
            //tempview.Sort = "accountTitle,date";
            //dt = tempview.ToTable();

            RecalcBankAccount(dt, bankAccount);

            G1.NumberDataTable(dt);
            dt.AcceptChanges();
            dgv.DataSource = dt;

            gridMain.ClearSelection();
            gridMain.RefreshData();
            gridMain.FocusedRowHandle = rowHandle + 1;
            gridMain.SelectRow(rowHandle + 1);
            dgv.Refresh();

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
            else if (e.Column.FieldName.ToUpper().IndexOf("DEPOSITNUMBER") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                string str = e.DisplayText.Trim().ToUpper();
                if (str.IndexOf("Z") == 0)
                    e.DisplayText = "";
            }
            else
            {
                bool doit = false;
                if (e.Column.FieldName.ToUpper().IndexOf("TDA") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("IDA") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("NDA") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("DDA") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("MISC") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("RETURNS") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("TRANSFERS") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("DAILYTOTALS") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                if ( doit )
                {
                    string str = e.DisplayText;
                    str = str.Replace(",", "");
                    str = str.Replace("$", "");
                    double dValue = str.ObjToDouble();
                    if (dValue == 0D)
                        e.DisplayText = "";

                    DataTable dt = (DataTable)dgv.DataSource;
                    if (e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    {
                        int row = e.ListSourceRowIndex;
                        str = dt.Rows[row]["comment"].ObjToString();
                        if (str.ToUpper().IndexOf("BALANCE FORWARD") >= 0)
                        {
                            if (e.Column.FieldName.ToUpper().IndexOf("TDA") >= 0)
                                e.DisplayText = getLastDepositNumber(dt, row, "TDA");
                            else if (e.Column.FieldName.ToUpper().IndexOf("IDA") >= 0)
                                e.DisplayText = getLastDepositNumber(dt, row, "IDA");
                            else if (e.Column.FieldName.ToUpper().IndexOf("NDA") >= 0)
                                e.DisplayText = getLastDepositNumber(dt, row, "NDA");
                            else if (e.Column.FieldName.ToUpper().IndexOf("DDA") >= 0)
                                e.DisplayText = getLastDepositNumber(dt, row, "DDA");
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
        private string getLastDepositNumber ( DataTable dt, int row, string column )
        {
            if (depositDt == null)
                return "";
            string bankAccount = dt.Rows[row]["bank_account"].ObjToString();
            if (String.IsNullOrWhiteSpace(bankAccount))
                return "";
            DataRow[] dRows = depositDt.Select("bankAccount='" + bankAccount + "'");
            if (dRows.Length <= 0)
                return "";
            string lastDepositNumber = "";
            try
            {
                lastDepositNumber = dRows[0][column].ObjToString();
            }
            catch ( Exception ex )
            {
            }
            return lastDepositNumber;
        }
        /****************************************************************************************/
        private double getBeginningBalance(string bankAccount, DateTime monthDate, ref double endingBalance, ref string bankDescription )
        {
            double beginningBalance = 0D;
            endingBalance = 0D;
            bankDescription = "";

            DataRow dRow = null;

            int days = DateTime.DaysInMonth(monthDate.Year, monthDate.Month);

            DateTime searchDate = new DateTime(monthDate.Year, monthDate.Month, days);

            string[] Lines = bankAccount.Split('~');

            if (Lines.Length < 3)
                return beginningBalance;

            string location = Lines[0];
            string generalLedger = Lines[1];
            string account = Lines[2];

            string cmd = "Select * from `bank_accounts` where `location` = '" + location + "' AND `general_ledger_no` = '" + generalLedger + "' AND `account_no` = '" + account + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return beginningBalance;

            beginningBalance = dt.Rows[0]["beginningBalance"].ObjToDouble();
            DateTime asOf = dt.Rows[0]["asOfDate"].ObjToDateTime();
            bankDescription = dt.Rows[0]["account_title"].ObjToString();

            if (asOf.Year < 10)
            {
                asOf = monthDate.AddMonths(-1);
                days = DateTime.DaysInMonth(asOf.Year, asOf.Month);
                asOf = new DateTime(asOf.Year, asOf.Month, days);
            }

            cmd = "Select * from `bank_totals` where `bank_account` = '" + bankAccount + "' AND `date` = '" + searchDate.ToString("yyyy-MM-dd") + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                endingBalance = dt.Rows[0]["endingBalance"].ObjToDouble();

            for (; ; )
            {
                searchDate = searchDate.AddMonths(-1);
                days = DateTime.DaysInMonth(searchDate.Year, searchDate.Month);

                searchDate = new DateTime(searchDate.Year, searchDate.Month, days);
                if (searchDate < asOf)
                    break;
                cmd = "Select * from `bank_totals` where `bank_account` = '" + bankAccount + "' AND `date` = '" + searchDate.ToString("yyyy-MM-dd") + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    beginningBalance = dt.Rows[0]["endingBalance"].ObjToDouble();
                    dRow = depositDt.NewRow();
                    dRow["bankAccount"] = bankAccount;
                    dRow["TDA"] = dt.Rows[0]["TDA"].ObjToString();
                    dRow["IDA"] = dt.Rows[0]["IDA"].ObjToString();
                    dRow["NDA"] = dt.Rows[0]["NDA"].ObjToString();
                    dRow["DDA"] = dt.Rows[0]["DDA"].ObjToString();
                    depositDt.Rows.Add(dRow);
                    break;
                }
            }
            return beginningBalance;
        }
        /****************************************************************************************/
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            double balance = 0D;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count > 0)
            {
                int lastRow = dt.Rows.Count - 1;
                balance = dt.Rows[lastRow]["balance"].ObjToDouble();
            }
            string str = G1.ReformatMoney(balance);
            str = str.Replace("$", "");
            e.TotalValue = str;
        }
        /****************************************************************************************/
        private void cmbLockbox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        /****************************************************************************************/
        private void dateTimePicker1_CloseUp(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker1.Value = date;

            if (!CheckForSave())
                return;

            btnGetDeposits_Click(null, null);
        }
        /****************************************************************************************/
        private void gridMain_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {

            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                string column = e.Column.FieldName.ToUpper();
                if (column == "NUM")
                {
                    DataTable dt = (DataTable)dgv.DataSource;
                    int row = gridMain.GetDataSourceRowIndex(e.RowHandle);

                    string comment= dt.Rows[row]["comment"].ObjToString();
                    if (comment.Trim().ToUpper() != "BALANCE FORWARD")
                    {
                        string adate = dt.Rows[row]["adate"].ObjToString();
                        if (String.IsNullOrWhiteSpace(adate))
                        {
                            e.Appearance.BackColor = Color.Red;
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
        private string oldWhat = "";
        private string oldColumn = "";
        /****************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            DataRow dr = gridMain.GetFocusedDataRow();
            string manual = dr["manual"].ObjToString().ToUpper();
            if (manual != "Y")
            {
                if (view.FocusedColumn.FieldName.ToUpper() == "DEPOSITNUMBER")
                {
                    oldColumn = "DEPOSITNUMBER";
                    DataTable dt = (DataTable)dgv.DataSource;
                    int rowhandle = gridMain.FocusedRowHandle;
                    int row = gridMain.GetDataSourceRowIndex(rowhandle);
                    oldWhat = dt.Rows[row]["DEPOSITNUMBER"].ObjToString();
                }
                else
                    e.Valid = false;
                return;
            }
            if (view.FocusedColumn.FieldName.ToUpper() == "DATE")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                //string manual = dr["manual"].ObjToString().ToUpper();
                //if (manual != "Y")
                //{
                //    e.Valid = false;
                //    return;
                //}
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row]["date"] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
            }
            else if (view.FocusedColumn.FieldName.ToUpper() == "TRANSFERS" )
            {
                DataTable dt = (DataTable)dgv.DataSource;
                //DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                string bankAccount = dr["bank_account"].ObjToString().ToUpper();
            }
            else if (view.FocusedColumn.FieldName.ToUpper() == "RETURNS")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                //DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                string bankAccount = dr["bank_account"].ObjToString().ToUpper();
            }
            else if (view.FocusedColumn.FieldName.ToUpper() == "MISC")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                //DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                string bankAccount = dr["bank_account"].ObjToString().ToUpper();
            }
            else if (view.FocusedColumn.FieldName.ToUpper() == "DEPOSITNUMBER")
            {
                oldColumn = "DEPOSITNUMBER";

                DataTable dt = (DataTable)dgv.DataSource;
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = dt.Rows[row]["DEPOSITNUMBER"].ObjToString();
            }

            string ba = dr["bank_account"].ObjToString();
            if (!String.IsNullOrWhiteSpace(ba))
            {
                DataTable dx = (DataTable)dgv.DataSource;
                //RecalcBankAccount(dx, ba );
                //RecalcDailyTotals(dx, beginningBalance);
                //dgv.DataSource = dx;
                //dgv.Refresh();
            }
        }
        /***************************************************************************************/
        private void RecalcBankAccount(DataTable dt, string bankAccount)
        {
            //DataTable dt = (DataTable)dgv.DataSource;

            DataRow[] dRows = dt.Select("bank_account LIKE '%" + bankAccount + "'", "date asc");
            if (dRows.Length <= 0)
                return;

            DataTable ddt = dRows.CopyToDataTable();

            double endingBalance = 0D;
            string accountTitle = "";

            DateTime stopDate = this.dateTimePicker1.Value;
            DateTime startDate = new DateTime(stopDate.Year, stopDate.Month, 1);

            beginningBalance = getBeginningBalance(bankAccount, startDate, ref endingBalance, ref accountTitle);

            double tda = 0D;
            double ida = 0D;
            double nda = 0D;
            double dda = 0D;
            double misc = 0D;
            double returns = 0D;
            double transfers = 0D;
            double dailyTotals = 0D;
            double balance = beginningBalance;
            string bank = "";
            string comment = "";
            bool start = false;

            DateTime lastDate = DateTime.MinValue;
            DateTime date = DateTime.Now;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                bank = dt.Rows[i]["bank_account"].ObjToString();
                if (!start)
                {
                    if (bank != bankAccount)
                        continue;
                    start = true;
                }
                if (start)
                {
                    if (bank != bankAccount)
                    {
                        dt.Rows[i - 1]["dailyTotals"] = dailyTotals;
                        dailyTotals = 0D;
                        break;
                    }
                }
                date = dt.Rows[i]["date"].ObjToDateTime();
                if (lastDate == DateTime.MinValue)
                    lastDate = date;
                if (date != lastDate)
                {
                    dt.Rows[i - 1]["dailyTotals"] = dailyTotals;
                    dailyTotals = 0D;
                    lastDate = date;
                }
                comment = dt.Rows[i]["comment"].ObjToString().ToUpper();
                if (comment == "BALANCE FORWARD")
                    continue;

                dt.Rows[i]["dailyTotals"] = 0D;

                tda = dt.Rows[i]["TDA"].ObjToDouble();
                ida = dt.Rows[i]["ida"].ObjToDouble();
                nda = dt.Rows[i]["nda"].ObjToDouble();
                dda = dt.Rows[i]["dda"].ObjToDouble();
                misc = dt.Rows[i]["misc"].ObjToDouble();
                returns = dt.Rows[i]["returns"].ObjToDouble();
                transfers = dt.Rows[i]["transfers"].ObjToDouble();

                balance = balance + tda + ida + nda + dda - misc - returns - transfers;
                balance = G1.RoundValue(balance);
                dt.Rows[i]["balance"] = balance;

                dailyTotals += tda + ida + nda - returns;
                dailyTotals = G1.RoundValue(dailyTotals);

                if (i == (dt.Rows.Count - 1))
                {
                    dt.Rows[i]["dailyTotals"] = dailyTotals;
                    dailyTotals = 0D;
                }
            }

            ddt = dRows.CopyToDataTable();

            dgv.RefreshDataSource();

            gridMain.OptionsView.ShowFooter = true;
            this.gridMain.ExpandAllGroups();
            gridMain.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            string bankDetails = dr["bank_account"].ObjToString();
            DateTime date = dr["date"].ObjToDateTime();
            string aDate = dr["aDate"].ObjToString();
            if (!String.IsNullOrWhiteSpace(aDate))
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (row < 0 || row > (dt.Rows.Count - 1))
                return;

            DateTime stopDate = this.dateTimePicker1.Value;
            DateTime startDate = new DateTime(stopDate.Year, stopDate.Month, 1);

            double endingBalance = 0D;
            string accountTitle = "";
            beginningBalance = getBeginningBalance(bankDetails, startDate, ref endingBalance, ref accountTitle);

            dt.Rows.RemoveAt(row);

            RecalcBankAccount(dt, bankDetails);

            //double balance = RecalcDailyTotals(dt, beginningBalance, bankDetails );

            G1.NumberDataTable(dt);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            dgv.Refresh();

            btnSave.Show();
        }
        /****************************************************************************************/
        private double originalSize = 0D;
        private Font mainFont = null;
        private Font newFont = null;
        private Font HeaderFont = null;

        private void ScaleCells()
        {
            if (originalSize == 0D)
            {
                //                originalSize = gridMain.Columns["address1"].AppearanceCell.FontSizeDelta.ObjToDouble();
                originalSize = gridMain.Columns["comment"].AppearanceCell.Font.Size;
                mainFont = gridMain.Columns["comment"].AppearanceCell.Font;
                HeaderFont = gridMain.Appearance.HeaderPanel.Font;
            }
            double scale = txtScale.Text.ObjToDouble();
            double size = scale / 100D * originalSize;
            Font font = new Font(mainFont.Name, (float)size);
            gridMain.Appearance.GroupFooter.Font = font;
            gridMain.AppearancePrint.FooterPanel.Font = font;
            gridMain.AppearancePrint.HeaderPanel.Font = font;


            gridMain.Appearance.GroupFooter.Font = font;
            gridMain.Appearance.FooterPanel.Font = font;
            gridMain.AppearancePrint.FooterPanel.Font = font;
            gridMain.AppearancePrint.GroupFooter.Font = font;

            font = new Font(HeaderFont.Name, (float)size, FontStyle.Regular);
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                gridMain.Columns[i].AppearanceCell.Font = font;
                gridMain.Columns[i].AppearanceHeader.Font = font;
            }

            newFont = font;
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
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
        private void btnDraftReport_Click(object sender, EventArgs e)
        {
            if (paymentDetail == null)
                return;
            if (paymentDetail.Rows.Count <= 0)
                return;

            this.Cursor = Cursors.WaitCursor;

            DataTable dt = (DataTable)dgv.DataSource;
            DataTable reportDt = (DataTable)chkComboLocation.Properties.DataSource;
            string location = "";
            string oldloc = "";
            string contractNumber = "";
            string payer = "";
            string cmd = "";
            string aDate = "";
            DateTime date = DateTime.Now;
            DataTable dx = null;
            DataTable rDt = new DataTable();
            rDt.Columns.Add("Num");
            rDt.Columns.Add("Date");
            rDt.Columns.Add("aDate");
            DateTime startDate = paymentDetail.Rows[0]["payDate8"].ObjToDateTime();
            startDate = new DateTime(startDate.Year, startDate.Month, 1);
            int days = DateTime.DaysInMonth(startDate.Year, startDate.Month);
            DataRow dRow = null;
            DataRow[] dRows = null;
            for (int i = 0; i < days; i++)
            {
                dRow = rDt.NewRow();
                dRow["aDate"] = startDate.AddDays(i).ToString("yyyyMMdd");
                dRow["Date"] = startDate.AddDays(i).ToString("MM/dd/yy");
                rDt.Rows.Add(dRow);
            }

            int col = 0;
            int row = 0;
            double payment = 0D;
            double debit = 0D;
            double downPayment = 0D;
            double dValue = 0D;

            bool gotInsurance = false;
            string what = "";

            for (int i = 0; i < paymentDetail.Rows.Count; i++)
            {
                try
                {
                    date = paymentDetail.Rows[i]["payDate8"].ObjToDateTime();
                    aDate = date.ToString("yyyyMMdd");
                    dRows = rDt.Select("aDate='" + aDate + "'");
                    if (dRows.Length <= 0)
                        continue;

                    contractNumber = paymentDetail.Rows[i]["contractNumber"].ObjToString().Trim();
                    if (contractNumber == "ZZ0001564")
                    {
                    }
                    if (contractNumber.IndexOf("ZZ") == 0)
                    {
                        location = paymentDetail.Rows[i]["location"].ObjToString();
                        //if (G1.validate_numeric(location))
                        //{
                        //    cmd = "Select * from `icontracts` c JOIN `icustomers` b ON c.`contractNumber` = b.`contractNumber` WHERE c.`contractNumber` = '" + contractNumber + "';";
                        //    dx = G1.get_db_data(cmd);
                        //    if (dx.Rows.Count > 0)
                        //    {
                        //        payer = dx.Rows[0]["payer"].ObjToString();
                        //        location = ImportDailyDeposits.FindLastPaymentLocation(payer, ref oldloc);
                        //    }
                        //}
                        location = DetermineLocation(contractNumber, location, "Ins");
                        col = CheckForColumn(rDt, location, "Ins");
                        if (col <= 0)
                            continue;
                        dValue = dRows[0][col].ObjToDouble();
                        payment = paymentDetail.Rows[i]["paymentAmount"].ObjToDouble();
                        debit = paymentDetail.Rows[i]["debitAdjustment"].ObjToDouble();
                        //if (payment == 0D && debit != 0D)
                        //    payment = Math.Abs(debit) * -1D;
                        dValue += payment;
                        dRows[0][col] = dValue;
                        gotInsurance = true;
                    }
                    else
                    {
                        //if (gotInsurance)
                        //    break;
                        location = paymentDetail.Rows[i]["location"].ObjToString();
                        what = paymentDetail.Rows[i]["what"].ObjToString();
                        location = DetermineLocation(contractNumber, location, what );
                        col = CheckForColumn(rDt, location, what );
                        if (col <= 0)
                            continue;
                        dValue = dRows[0][col].ObjToDouble();
                        payment = paymentDetail.Rows[i]["paymentAmount"].ObjToDouble();
                        debit = paymentDetail.Rows[i]["debitAdjustment"].ObjToDouble();
                        downPayment = paymentDetail.Rows[i]["downPayment"].ObjToDouble();
                        //if (payment == 0D && debit != 0D)
                        //    payment = Math.Abs(debit) * -1D;
                        //else if (payment == 0D && downPayment != 0D)
                        //    payment = downPayment;
                        dValue += payment;
                        dRows[0][col] = dValue;
                    }
                }
                catch (Exception ex)
                {
                }
            }

            int lastRow = rDt.Rows.Count;

            dRow = rDt.NewRow();
            rDt.Rows.Add(dRow);

            double total = 0D;
            double columnTotals = 0D;
            try
            {
                dRow = rDt.NewRow();
                for (int i = 3; i < rDt.Columns.Count; i++)
                {
                    total = 0D;
                    for (int j = 0; j < lastRow; j++)
                    {
                        payment = rDt.Rows[j][i].ObjToDouble();
                        total += payment;
                    }
                    dRow[i] = total;
                    columnTotals += total;
                }
                rDt.Rows.Add(dRow);
            }
            catch (Exception ex)
            {
            }

            int lastCol = rDt.Columns.Count;
            double rowTotals = 0D;
            double transfers = 0D;
            double dailyTotal = 0D;

            try
            {
                rDt.Columns.Add("Total", Type.GetType("System.Double"));
                rDt.Columns.Add("Transfers", Type.GetType("System.Double"));
                rDt.Columns.Add("Daily Total", Type.GetType("System.Double"));
                col = G1.get_column_number(rDt, "Total");
                for (int i = 0; i < lastRow; i++)
                {
                    total = 0D;
                    for (int j = 3; j < lastCol; j++)
                    {
                        payment = rDt.Rows[i][j].ObjToDouble();
                        total += payment;
                    }
                    rDt.Rows[i][col] = total;
                    rowTotals += total;

                    rDt.Rows[i]["Num"] = (i + 1).ToString();
                    //transfers = dt.Rows[i]["transfers"].ObjToDouble();
                    //rDt.Rows[i]["Transfers"] = transfers;

                    dailyTotal = total - transfers;
                    dailyTotal = G1.RoundValue(dailyTotal);
                    rDt.Rows[i]["Daily Total"] = dailyTotal;
                }
            }
            catch (Exception ex)
            {
            }
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    transfers = dt.Rows[i]["transfers"].ObjToDouble();
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    aDate = date.ToString("yyyyMMdd");
                    dRows = rDt.Select("aDate='" + aDate + "'");
                    if (dRows.Length <= 0)
                        continue;
                    dailyTotal = dRows[0]["Daily Total"].ObjToDouble();
                    dailyTotal = dailyTotal - transfers;
                    dRows[0]["Daily Total"] = dailyTotal;
                }
            }
            catch ( Exception ex)
            {
            }

            try
            {
                columnTotals = G1.RoundValue(columnTotals);
                rowTotals = G1.RoundValue(rowTotals);
                if (columnTotals != rowTotals)
                {
                }

                lastRow = rDt.Rows.Count - 1;
                rDt.Rows[lastRow]["Total"] = rowTotals;
            }
            catch (Exception ex)
            {
            }

            col = G1.get_column_number(rDt, "aDate");
            rDt.Columns.RemoveAt(col);

            BankDetailsByLocation bankForm = new BankDetailsByLocation(rDt, this.Text, paymentDetail, oldDt, this.dateTimePicker1.Value, "Cash Deposits", false );
            bankForm.Show();

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable funDt = null;
        /****************************************************************************************/
        private string DetermineLocation(string contractNumber, string location, string type)
        {
            string cmd = "";
            string payer = "";
            DataTable dx = null;
            string oldloc = "";
            string wordLocation = "";

            string[] Lines = null;

            location = CleanupLocation(location);

            DataRow[] dRows = null;

            if (funDt == null)
                funDt = G1.get_db_data("Select * from `funeralHomes`;");

            try
            {
                if (contractNumber.IndexOf("ZZ") == 0)
                {
                    if (G1.validate_numeric(location))
                    {
                        cmd = "Select * from `icontracts` c JOIN `icustomers` b ON c.`contractNumber` = b.`contractNumber` WHERE c.`contractNumber` = '" + contractNumber + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            payer = dx.Rows[0]["payer"].ObjToString();
                            location = ImportDailyDeposits.FindLastPaymentLocation(payer, ref oldloc);
                            location = CleanupLocation(location);
                        }
                    }
                }
                else if (type == "Trust")
                {
                    if (G1.validate_numeric(location))
                    {
                        dRows = funDt.Select("SDICode='" + location + "'");
                        if (dRows.Length > 0)
                            location = dRows[0]["LocationCode"].ObjToString();
                    }
                    else
                    {
                        Lines = location.Split(' ');
                        for (int i = 0; i < Lines.Length; i++)
                        {
                            wordLocation = Lines[i].Trim();
                            if (wordLocation == "Colonial")
                                continue;
                            if (wordLocation == "Colonial-")
                                continue;
                            if (wordLocation == "Funeral")
                                continue;
                            dRows = funDt.Select("LocationCode LIKE '%" + wordLocation + "'");
                            if (dRows.Length > 0)
                            {
                                location = dRows[0]["LocationCode"].ObjToString();
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (G1.validate_numeric(location))
                    {
                        dRows = funDt.Select("SDICode='" + location + "'");
                        if (dRows.Length > 0)
                            location = dRows[0]["LocationCode"].ObjToString();
                    }
                    else
                    {
                        Lines = location.Split(' ');
                        for (int i = 0; i < Lines.Length; i++)
                        {
                            wordLocation = Lines[i].Trim();
                            if (wordLocation == "Colonial")
                                continue;
                            if (wordLocation == "Colonial-")
                                continue;
                            if (wordLocation == "Funeral")
                                continue;
                            dRows = funDt.Select("LocationCode LIKE '%" + wordLocation + "'");
                            if (dRows.Length > 0)
                            {
                                location = dRows[0]["LocationCode"].ObjToString();
                                break;
                            }
                            else
                            {
                                dRows = funDt.Select("atneedcode = '" + wordLocation + "'");
                                if (dRows.Length > 0)
                                {
                                    location = dRows[0]["LocationCode"].ObjToString();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return location;
        }
        /****************************************************************************************/
        private int CheckForColumn(DataTable rDt, string location, string type = "")
        {
            int col = -1;
            DataRow[] dRows = null;
            try
            {
                location = CleanupLocation(location);
                if (String.IsNullOrWhiteSpace(location))
                    location = "BLANK";
                if (!String.IsNullOrWhiteSpace(type))
                    location += " " + type;

                col = G1.get_column_number(rDt, location);
                if (col > 0)
                    return col;
                rDt.Columns.Add(location, Type.GetType("System.Double"));
                col = G1.get_column_number(rDt, location);
            }
            catch (Exception ex)
            {
            }
            return col;
        }
        /****************************************************************************************/
        public static string CleanupLocation(string location)
        {
            location = location.Replace("Colonial-", "");
            location = location.Replace("Colonial", "");
            location = location.Replace("Chapel", "");
            location = location.Replace("Funeral", "");
            location = location.Replace("Home", "");
            location = location.Trim();
            return location;
        }
        /****************************************************************************************/
        private void btnLocalTransfer_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataTable locationDt = (DataTable)chkComboLocation.Properties.DataSource;


            this.Cursor = Cursors.WaitCursor;
            TransferReport transForm = new TransferReport(dt, this.dateTimePicker1.Value, locationDt );
            transForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        /****************************************************************************************/
        private void btnSelectColumns_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;
            SelectColumns sform = new SelectColumns(dgv, "Local Cash Deposits " + workReport, "Primary", actualName);
            sform.Done += new SelectColumns.d_void_eventdone(sform_Done);
            sform.Show();
        }
        /***********************************************************************************************/
        void sform_Done(DataTable dt)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = workReport + " Primary";
            string saveName = "Local Cash Deposits " + workReport + " " + name;
            string skinName = "";
            SetupSelectedColumns("Local Cash Deposits " + workReport, name, dgv);
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
        private void SetupSelectedColumns(string procType, string group, DevExpress.XtraGrid.GridControl dgv)
        {
            if (String.IsNullOrWhiteSpace(group))
                return;
            if (String.IsNullOrWhiteSpace(procType))
                procType = "Local Cash Deposits " + workReport;
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
        private void lockScreenDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "Local Cash Deposits " + workReport + " " + name;
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
                string name = comboName;
                if (String.IsNullOrWhiteSpace(name))
                    name = "Primary";
                string saveName = "Local Cash Deposits " + workReport + " " + name;
                G1.RemoveLocalPreferences(LoginForm.username, saveName);
                foundLocalPreference = false;
            }

            //G1.RemoveLocalPreferences(LoginForm.username, "DailyHistoryLayout");
            foundLocalPreference = false;
        }
        /***********************************************************************************************/
        void skinForm_SkinSelected(string s)
        {
            if (s.ToUpper().IndexOf("SKIN : ") >= 0)
            {
                string skin = s.Replace("Skin : ", "");
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
                string color = s.Replace("Color : ", "");
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
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string s);
        public event d_void_eventdone_string SkinChange;
        protected void OnSkinChange(string done)
        {
            if (SkinChange != null)
                SkinChange.Invoke(done);
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns()
        {
            string group = cmbSelectColumns.Text.Trim().ToUpper();
            if (group.Trim().Length == 0)
                return;
            string procName = "Local Cash Deposits " + workReport;
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + procName + "' order by seq";
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
        private void loadGroupCombo(System.Windows.Forms.ComboBox cmb, string key, string module)
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
        private void cmbSelectColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            string comboName = cmbSelectColumns.Text;
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                SetupSelectedColumns("Local Cash Deposits " + workReport, comboName, dgv);
                string name = "Local Cash Deposits " + workReport + " " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                SetupTotalsSummary();
                gridMain.OptionsView.ShowFooter = true;
            }
            else
            {
                SetupSelectedColumns("Local Cash Deposits " + workReport, "Primary", dgv);
                string name = "Local Cash Deposits" + workReport + " Primary";
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                gridMain.OptionsView.ShowFooter = true;
                SetupTotalsSummary();
            }
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