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
    public partial class LockBoxDeposits : DevExpress.XtraEditors.XtraForm
    {
        private DataTable groupContracts = null;
        private DataTable agentsDt = null;
        private bool runAgents = false;
        private DataTable paymentDetail = null;
        private string bankDetails = "";
        private double beginningBalance = 0D;
        private string workReport = "";
        private bool foundLocalPreference = false;
        private bool loading = true;
        /****************************************************************************************/
        public LockBoxDeposits( string report = "")
        {
            InitializeComponent();
            SetupTotalsSummary();
            workReport = report;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("TDA", null);
            AddSummaryColumn("IDA", null);
            AddSummaryColumn("NDA", null);
            AddSummaryColumn("NOX", null);
            AddSummaryColumn("returns", null);
            AddSummaryColumn("transfers", null);
            AddSummaryColumn("misc", null);
            gridMain.Columns["balance"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
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
        private void LockBoxDeposits_Load(object sender, EventArgs e)
        {
            btnSave.Hide();

            txtBeginningBalance.Hide();
            label1.Hide();

            cmbLockbox.Hide();
            cmbLockbox.Text = workReport;

            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);

            int days = DateTime.DaysInMonth(now.Year, now.Month);
            DateTime stop = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker1.Value = stop;

            this.Cursor = Cursors.WaitCursor;

            //GetTheData();

            string saveName = "LockBox Deposits " + workReport + " Primary";
            string skinName = "";

            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            if (!String.IsNullOrWhiteSpace(skinName))
            {
                if (skinName != "DevExpress Style")
                    skinForm_SkinSelected("Skin : " + skinName);
            }

            loadGroupCombo(cmbSelectColumns, "LockBox Deposits " + workReport, "Primary");

            loading = false;

            ScaleCells();

            this.Cursor = Cursors.Default;
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
        private void CleanupScreen ()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt != null)
            {
                dt.Rows.Clear();
                dgv.DataSource = dt;
            }
            btnSave.Hide();
            btnSave.Refresh();
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

            //GetTheData();

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

            //GetTheData();

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
            dx.Columns.Add("DDA", Type.GetType("System.Double"));
            dx.Columns.Add("TDA", Type.GetType("System.Double"));
            dx.Columns.Add("IDA", Type.GetType("System.Double"));
            dx.Columns.Add("NDA", Type.GetType("System.Double"));
            dx.Columns.Add("NOX", Type.GetType("System.Double"));
            dx.Columns.Add("misc", Type.GetType("System.Double"));
            dx.Columns.Add("returns", Type.GetType("System.Double"));
            dx.Columns.Add("transfers", Type.GetType("System.Double"));
            dx.Columns.Add("dailyTotals", Type.GetType("System.Double"));
            dx.Columns.Add("balance", Type.GetType("System.Double"));
            dx.Columns.Add("comment");
            dx.Columns.Add("bank_account");
            dx.Columns.Add("manual");
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
        private DataTable LoadData( DateTime startDate, DateTime stopDate, string bankDetails )
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
            if ( G1.get_column_number ( dt, "dailyTotals") < 0 )
                dt.Columns.Add("dailyTotals", Type.GetType("System.Double"));
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
            //    location = location.Substring(idx+1);
            //    DataRow[] dRows = groupContracts.Select("loc='" + location.Trim() + "'");
            //    if (dRows.Length > 0)
            //        info.GroupText += " " + dRows[0]["contracts"].ObjToString();
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

            Printer.setupPrinterMargins(10, 5, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            G1.PrintPreview(printableComponentLink1, gridMain);
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

            Printer.setupPrinterMargins(10, 5, 80, 50);

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
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            int rowHandle = e.RowHandle;
            if (!gridMain.IsDataRow(rowHandle))
            {
                DevExpress.XtraPrinting.BrickGraphics brick = (DevExpress.XtraPrinting.BrickGraphics)e.BrickGraphics;
                int i = 1;
                //GridGroupRowInfo info = e as GridGroupRowInfo;
                //string location = info.GroupText;
                //int idx = location.LastIndexOf(']');
                //if (idx > 0)
                //{
                //    location = location.Substring(idx + 1);
                //    DataRow[] dRows = groupContracts.Select("loc='" + location.Trim() + "'");
                //    if (dRows.Length > 0)
                //        info.GroupText += " " + dRows[0]["contracts"].ObjToString();
                //}
            }
        }
        /****************************************************************************************/
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            string manual = dt.Rows[row]["manual"].ObjToString();
            if (manual == "y")
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            if (this.gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private void btnGetDeposits_Click(object sender, EventArgs e)
        {
            GetTheData();
            btnSave.Show();
        }
        /****************************************************************************************/
        private DataTable oldDt = null;
        private void GetTheData()
        {
            this.Cursor = Cursors.WaitCursor;
            DateTime stopDate = this.dateTimePicker1.Value;
            DateTime startDate = new DateTime(stopDate.Year, stopDate.Month, 1);

            string whatBox = cmbLockbox.Text.Trim();

            bankDetails = GetBankAccount(whatBox);

            DateTime date = DateTime.Now;
            DataRow[] dRows = null;

            DataTable dt = LoadMain(startDate, stopDate);


            G1.NumberDataTable(dt);
            dgv.DataSource = dt;


            string date1 = G1.DateTimeToSQLDateTime(startDate);
            string date2 = G1.DateTimeToSQLDateTime(stopDate);
            DateTime aDate = DateTime.Now;

            DataTable dx = (DataTable)dgv.DataSource;

            if (paymentDetail != null)
            {
                paymentDetail.Rows.Clear();
                paymentDetail.Dispose();
            }

            paymentDetail = null;

            double endingBalance = 0D;
            string accountTitle = "";
            beginningBalance = getBeginningBalance(bankDetails, startDate, ref endingBalance, ref accountTitle);
            string str = G1.ReformatMoney(beginningBalance);
            txtBeginningBalance.Text = str;

            //zeroOutColumn(dx, "TDA");
            //zeroOutColumn(dx, "IDA");
            //zeroOutColumn(dx, "returns");

            DataRow dR = dx.NewRow();
            dR["date"] = G1.DTtoMySQLDT(startDate);
            dR["comment"] = "Balance Forward";
            dR["balance"] = beginningBalance;
            //dR["bank_account"] = bankDetails;
            //dR["accountTitle"] = whatBox;
            dx.Rows.InsertAt(dR, 0);

            dx = getTrustPayments(dx, startDate, stopDate);
            dx = getInsurancePayments(dx, startDate, stopDate);

            dx = loadBankDebits(dx, startDate, stopDate, "");

            oldDt = LoadData(startDate, stopDate, bankDetails);
            if (oldDt.Rows.Count > 0)
            {
                for (int i = 0; i < oldDt.Rows.Count; i++)
                {
                    date = oldDt.Rows[i]["date"].ObjToDateTime();
                    dRows = dt.Select("aDate='" + date.ToString("yyyyMMdd") + "'");
                    if (dRows.Length > 0)
                    {
                        CombineData ( dRows[0], oldDt.Rows[i]);
                        AddManualData(dx, oldDt.Rows[i]);
                    }
                }
            }

            LoadDOW(dx);

            double balance = RecalcTotals(dx);
            balance = G1.RoundValue(balance);

            if (endingBalance != balance)
                btnSave.Show();
            else
                btnSave.Hide();
            btnSave.Refresh();

            // dx = SortDownTable(dx); This is not necessary here because data is summarized per day.

            G1.NumberDataTable(dx);
            dgv.DataSource = dx;

            str = bankDetails.Replace("~", " / ");

            this.Text = "Lockbox Deposit Report (" + whatBox + ")";
            gridBand5.Caption = "Lockbox Deposits for " + str + " " + accountTitle;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable SortDownTable(DataTable dt)
        {
            string depositNumber = "";
            DateTime date = DateTime.Now;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(depositNumber))
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
        private void CombineData(DataRow fRow, DataRow lRow)
        {
            double dda = 0D;
            double tda = 0D;
            double ida = 0D;
            double nda = 0D;
            double nox = 0D;
            double misc = 0D;
            double returns = 0D;
            double transfers = 0D;
            string comment = "";
            string extra = "";
            try
            {
                dda += fRow["DDA"].ObjToDouble();
                tda += fRow["TDA"].ObjToDouble();
                ida += fRow["IDA"].ObjToDouble();
                nda += fRow["NDA"].ObjToDouble();
                nox += fRow["NOX"].ObjToDouble();
                misc += fRow["misc"].ObjToDouble();
                returns += fRow["returns"].ObjToDouble();
                transfers += fRow["transfers"].ObjToDouble();
                comment = fRow["comment"].ObjToString();

                dda += lRow["DDA"].ObjToDouble();
                tda += lRow["TDA"].ObjToDouble();
                ida += lRow["IDA"].ObjToDouble();
                nda += lRow["NDA"].ObjToDouble();
                nox += lRow["NOX"].ObjToDouble();
                misc += lRow["misc"].ObjToDouble();
                returns += lRow["returns"].ObjToDouble();
                transfers += lRow["transfers"].ObjToDouble();
                extra = lRow["comment"].ObjToString();
                if (!String.IsNullOrWhiteSpace(comment))
                    comment += "\n";
                comment += extra;

                fRow["DDA"] = dda;
                fRow["TDA"] = tda;
                fRow["IDA"] = ida;
                fRow["NDA"] = nda;
                fRow["NOX"] = nox;
                fRow["misc"] = misc;
                fRow["returns"] = returns;
                fRow["transfers"] = transfers;
                fRow["comment"] = comment;
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void AddManualData(DataTable dx, DataRow lRow)
        {
            double dda = 0D;
            double tda = 0D;
            double ida = 0D;
            double nda = 0D;
            double nox = 0D;
            double misc = 0D;
            double returns = 0D;
            double transfers = 0D;
            string comment = "";
            string extra = "";

            try
            {
                DataRow fRow = dx.NewRow();

                dda += lRow["DDA"].ObjToDouble();
                tda += lRow["TDA"].ObjToDouble();
                ida += lRow["IDA"].ObjToDouble();
                nda += lRow["NDA"].ObjToDouble();
                nox += lRow["NOX"].ObjToDouble();
                misc += lRow["misc"].ObjToDouble();
                returns += lRow["returns"].ObjToDouble();
                transfers += lRow["transfers"].ObjToDouble();
                extra = lRow["comment"].ObjToString();
                comment += extra;

                DateTime date = lRow["date"].ObjToDateTime();
                fRow["date"] = G1.DTtoMySQLDT(date);
                fRow["aDate"] = "";
                fRow["DDA"] = dda;
                fRow["TDA"] = tda;
                fRow["IDA"] = ida;
                fRow["NDA"] = nda;
                fRow["NOX"] = nox;
                fRow["misc"] = misc;
                fRow["returns"] = returns;
                fRow["transfers"] = transfers;
                fRow["comment"] = comment;
                fRow["manual"] = "y";

                dx.Rows.Add(fRow);
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private DataTable loadBankDebits(DataTable dx, DateTime startDate, DateTime stopDate, string accountNumber)
        {
            string date1 = G1.DateTimeToSQLDateTime(startDate);
            string date2 = G1.DateTimeToSQLDateTime(stopDate);

            if (G1.get_column_number(dx, "bankDebit") < 0)
                dx.Columns.Add("bankDebit");
            if (G1.get_column_number(dx, "misc") < 0)
                dx.Columns.Add("misc", Type.GetType("System.Double"));

            try
            {
                string whatBox = cmbLockbox.Text.Trim();

                string bankDetails = GetBankAccount ( whatBox );
                if (String.IsNullOrWhiteSpace(bankDetails))
                    return dx;

                string[] Lines = bankDetails.Split('~');
                if (Lines.Length < 3)
                    return dx;

                string description = Lines[0];
                string bankAccount = Lines[2];
                //if (String.IsNullOrWhiteSpace(accountNumber))
                //    accountNumber = bankAccount;

                string cmd = "Select * from `bank_details` p WHERE `date` >= '" + date1 + "' and `date` <= '" + date2 + "' AND `bankAccount` = '" + bankAccount + "' AND `debit` > '0.00' ORDER BY `date` asc;";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                    return dx;

                string assignTo = "";
                double debit = 0D;
                description = "";
                string debitDepNum = "";
                double dValue = 0D;

                DataRow dRow = null;
                DataRow[] dRows = null;
                DateTime date = DateTime.Now;
                string aDate = "";

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    debit = dt.Rows[i]["debit"].ObjToDouble();
                    assignTo = dt.Rows[i]["assignTo"].ObjToString().ToUpper();
                    description = dt.Rows[i]["description"].ObjToString();
                    debitDepNum = dt.Rows[i]["debitDepNum"].ObjToString();

                    debit = debit * -1D;

                    if (!String.IsNullOrWhiteSpace(assignTo))
                    {
                        aDate = date.ToString("yyyyMMdd");
                        dRows = dx.Select("aDate='" + aDate + "'");
                        if (dRows.Length > 0)
                        {
                            CombineDetail(dRows[0], debit, assignTo, description);
                            dRows[0]["bankDebit"] = dt.Rows[i]["record"].ObjToString();
                            continue;
                        }
                    }

                    dRow = dx.NewRow();
                    dRow["date"] = G1.DTtoMySQLDT(date);
                    dRow["adate"] = date.ToString("yyyyMMdd");
                    dRow["bankDebit"] = dt.Rows[i]["record"].ObjToString();
                    dValue = Math.Abs(debit);
                    dRow["comment"] = description + "~" + debitDepNum + " " + G1.ReformatMoney(dValue);
                    if (!String.IsNullOrWhiteSpace(accountNumber))
                    {
                        if (debitDepNum.Length >= 2)
                            dRow["accountTitle"] = debitDepNum.Substring(0, 2);
                    }
                    //dRow["depositNumber"] = debitDepNum;
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
                    else if (assignTo == "RETURN")
                        dRow["returns"] = Math.Abs(debit);
                    else if (assignTo == "MISCELLANEOUS")
                        dRow["misc"] = Math.Abs(debit);
                    dx.Rows.Add(dRow);
                }
            }
            catch (Exception ex)
            {
            }
            return dx;
        }
        /****************************************************************************************/
        private void CombineDetail(DataRow fRow, double debit, string assignTo, string comment = "")
        {
            double dda = 0D;
            double tda = 0D;
            double ida = 0D;
            double nda = 0D;
            double returns = 0D;
            double transfers = 0D;
            double misc = 0D;
            string detail = "";
            try
            {
                dda += fRow["DDA"].ObjToDouble();
                tda += fRow["TDA"].ObjToDouble();
                ida += fRow["IDA"].ObjToDouble();
                nda += fRow["NDA"].ObjToDouble();
                returns += fRow["returns"].ObjToDouble();
                transfers += fRow["transfers"].ObjToDouble();
                misc += fRow["misc"].ObjToDouble();
                detail = fRow["comment"].ObjToString();

                if (assignTo == "TRUST DOWN PAYMENT")
                    dda += debit;
                else if (assignTo == "TRUST DEPOSIT")
                    tda += debit;
                else if (assignTo == "INSURANCE DEPOSIT")
                    ida += debit;
                else if (assignTo == "FUNERAL DEPOSIT")
                    nda += debit;
                else if (assignTo == "TRANSFER")
                    transfers += Math.Abs(debit);
                else if (assignTo == "RETURN")
                    returns += Math.Abs(debit);
                else if (assignTo == "MISCELLANEOUS")
                    misc += Math.Abs(debit);

                fRow["DDA"] = dda;
                fRow["TDA"] = tda;
                fRow["IDA"] = ida;
                fRow["NDA"] = nda;
                fRow["returns"] = returns;
                fRow["transfers"] = transfers;
                fRow["misc"] = misc;
                if (!String.IsNullOrWhiteSpace(comment))
                {
                    if (!String.IsNullOrWhiteSpace(detail))
                        detail += "\n" + comment;
                    else
                        detail = comment;
                    fRow["comment"] = detail;
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***************************************************************************************/
        private double RecalcTotals ( DataTable dt )
        {
            double originalEndingBalance = 0D;
            double tda = 0D;
            double ida = 0D;
            double nda = 0D;
            double nox = 0D;
            double returns = 0D;
            double transfers = 0D;
            double misc = 0D;
            double dailyTotals = 0D;
            double balance = beginningBalance;

            string box = cmbLockbox.Text.ToUpper();
            bool gotNox = false;

            DateTime lastDate = DateTime.MinValue;
            DateTime date = DateTime.Now;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["date"].ObjToDateTime();
                if (lastDate == DateTime.MinValue)
                    lastDate = date;
                if (date != lastDate)
                {
                    dt.Rows[i-1]["dailyTotals"] = dailyTotals;
                    dailyTotals = 0D;
                }

                dt.Rows[i]["dailyTotals"] = 0D;

                tda = dt.Rows[i]["TDA"].ObjToDouble();
                ida = dt.Rows[i]["ida"].ObjToDouble();
                nda = dt.Rows[i]["nda"].ObjToDouble();
                nox = dt.Rows[i]["NOX"].ObjToDouble();
                if (nox != 0D)
                    gotNox = true;
                returns = dt.Rows[i]["returns"].ObjToDouble();
                transfers = dt.Rows[i]["transfers"].ObjToDouble();
                misc = dt.Rows[i]["misc"].ObjToDouble();

                balance = balance + tda + ida + nda + nox - returns - transfers - misc;
                balance = G1.RoundValue(balance);
                dt.Rows[i]["balance"] = balance;

                dailyTotals = tda + ida + nda;
                dailyTotals = G1.RoundValue(dailyTotals);

                if ( i == (dt.Rows.Count - 1))
                {
                    dt.Rows[i]["dailyTotals"] = dailyTotals;
                    dailyTotals = 0D;
                }
            }

            if (box == "TFBX" )
                gridMain.Columns["NDA"].Visible = false;
            else
                gridMain.Columns["NDA"].Visible = true;

            return balance;
        }
        /***************************************************************************************/
        private void LoadDOW ( DataTable dt )
        {
            for ( int i=0; i<dt.Rows.Count; i++)
            {
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
        DataTable getTrustPayments ( DataTable dx, DateTime startDate, DateTime stopDate )
        {
            string date1 = G1.DateTimeToSQLDateTime(startDate);
            string date2 = G1.DateTimeToSQLDateTime(stopDate);

            try
            {
                string box = cmbLockbox.Text.ToUpper();

                string bankDetails = GetBankAccount(box);
                if (String.IsNullOrWhiteSpace(bankDetails))
                    return dx;

                string cmd = "Select * from `payments` where `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' AND `bank_account` = '" + bankDetails + "' ORDER BY `payDate8` asc;";
                DataTable dt = G1.get_db_data(cmd);
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
                string comment = "";
                int numChars = 4;

                string what = cmbLockbox.Text;

                DataRow[] dRows = null;

                string contractNumber = "";

                TimeSpan ts;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber == "L22060LI")
                    {
                    }
                    if (contractNumber == "L22061LI")
                    {
                    }
                    date = dt.Rows[i]["payDate8"].ObjToDateTime();
                    paymentAmount = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                    payment = paymentAmount;
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    location = dt.Rows[i]["location"].ObjToString();
                    edited = dt.Rows[i]["edited"].ObjToString().ToUpper();
                    if (edited == "TRUSTADJ" || edited == "CEMETERY")
                        continue;
                    if ( edited == "MANUAL" )
                    {
                        //if (debit <= 0D)
                        //    continue;
                    }

                    if (String.IsNullOrWhiteSpace(depositNumber))
                        continue;

                    firstChar = depositNumber.ToUpper().Substring(0, 1);
                    if (box == "ACH")
                    {
                        if (debit > 0D)
                        {
                            if (contractNumber.ToUpper().StartsWith("ZZ"))
                                continue;
                        }
                        else
                        {
                            if (firstChar != "A")
                                continue;
                        }
                    }
                    else
                    {
                        if (firstChar == "T")
                        {
                            if (box != "TFBX" && fill1.ToUpper() == "TFBX")
                                continue;
                        }
                        else
                        {
                            if (debit > 0D)
                            {
                                if (contractNumber.ToUpper().StartsWith("ZZ"))
                                    continue;
                            }
                        }
                    }

                    G1.copy_dt_row(dt, i, paymentDetail, paymentDetail.Rows.Count);

                    totals += payment;

                    dRows = dx.Select("aDate='" + date.ToString("yyyyMMdd") + "'");
                    if ( dRows.Length > 0 )
                    {
                        dailyAmount = dRows[0]["TDA"].ObjToDouble();
                        dailyAmount += payment;
                        dRows[0]["TDA"] = dailyAmount;

                        if ( debit > 0D)
                        {
                            dailyAmount = dRows[0]["returns"].ObjToDouble();
                            dailyAmount += debit;
                            dRows[0]["returns"] = dailyAmount;
                            comment = dRows[0]["comment"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(comment))
                                comment += "\n";
                            comment += contractNumber + " " + G1.ReformatMoney(debit);
                            dRows[0]["comment"] = comment;
                        }
                    }
                    else
                    {
                    }
                }
                dx.AcceptChanges();
            }
            catch ( Exception ex)
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
        DataTable getInsurancePayments(DataTable dx, DateTime startDate, DateTime stopDate )
        {
            string date1 = G1.DateTimeToSQLDateTime(startDate);
            string date2 = G1.DateTimeToSQLDateTime(stopDate);

            try
            {
                string box = cmbLockbox.Text.ToUpper();
                string bankDetails = GetBankAccount(box);
                if (String.IsNullOrWhiteSpace(bankDetails))
                    return dx;

                string cmd = "Select * from `ipayments` where `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' AND `bank_account` = '" + bankDetails + "' ORDER BY `payDate8` asc;";
                DataTable dt = G1.get_db_data(cmd);
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
                int day = 0;
                string depositNumber = "";
                string edited = "";
                string c = "";
                DateTime testDate = new DateTime(2019, 7, 31);

                DataRow[] dRows = null;

                string fill1 = "";
                string firstChar = "";
                int numChars = 4;

                string what = cmbLockbox.Text;
                string contractNumber = "";

                TimeSpan ts;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    date = dt.Rows[i]["payDate8"].ObjToDateTime();
                    if ( date.ToString("MM/dd/yyyy") == "03/25/2022")
                    {
                    }
                    paymentAmount = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                    edited = dt.Rows[i]["edited"].ObjToString().ToUpper();

                    if ( edited == "TRUSTADJ" || edited == "CEMETERY")
                        continue;

                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(depositNumber))
                        continue;

                    firstChar = depositNumber.ToUpper().Substring(0, 1);
                    if (box == "ACH")
                    {
                        if (debit > 0D)
                        {
                            if (contractNumber.ToUpper().StartsWith("ZZ"))
                                continue;
                        }
                        else
                        {
                            if (firstChar != "A")
                                continue;
                        }
                    }
                    else
                    {
                        if (firstChar == "T")
                        {
                            if (box != "TFBX" && fill1.ToUpper() == "TFBX")
                                continue;
                        }
                        else
                        {
                            if (debit > 0D)
                            {
                                if (contractNumber.ToUpper().StartsWith("ZZ"))
                                    continue;
                            }
                        }
                    }

                    G1.copy_dt_row(dt, i, paymentDetail, paymentDetail.Rows.Count);

                    payment = paymentAmount;
                    totals += payment;

                    dRows = dx.Select("aDate='" + date.ToString("yyyyMMdd") + "'");
                    if (dRows.Length > 0)
                    {
                        dailyAmount = dRows[0]["IDA"].ObjToDouble();
                        dailyAmount += payment;
                        dRows[0]["IDA"] = dailyAmount;

                        if (debit > 0D)
                        {
                            dailyAmount = dRows[0]["returns"].ObjToDouble();
                            dailyAmount += debit;
                            dRows[0]["returns"] = dailyAmount;
                        }
                    }
                }
                dx.AcceptChanges();
            }
            catch ( Exception ex)
            {
            }
            return dx;
        }
        /****************************************************************************************/
        private bool CheckForSave ()
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
        private void SaveData ()
        {
            this.Cursor = Cursors.WaitCursor;
            DateTime stopDate = this.dateTimePicker1.Value;
            DateTime startDate = new DateTime(stopDate.Year, stopDate.Month, 1);
            string date1 = G1.DateTimeToSQLDateTime(startDate);
            string date2 = G1.DateTimeToSQLDateTime(stopDate);

            DateTime saveDate = DateTime.Now;
            string date = "";

            string cmd = "DELETE FROM `lockboxdeposits` where `date` >= '" + date1 + "' AND `date` <= '" + date2 + "' AND `manual` <> 'y' AND `bank_account` = '" + bankDetails + "';";
            G1.get_db_data(cmd);

            DataTable dt = (DataTable)dgv.DataSource;

            double tda = 0D;
            double ida = 0D;
            double nda = 0D;
            double nox = 0D;
            double returns = 0D;
            double transfer = 0D;
            double misc = 0D;
            double balance = 0D;
            string comment = "";
            string aDate = "";
            string record = "";
            string manual = "";

            double credits = 0D;
            double debits = 0D;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                saveDate = dt.Rows[i]["date"].ObjToDateTime();
                date = saveDate.ToString("MM/dd/yyyy");
                tda = dt.Rows[i]["TDA"].ObjToDouble();
                ida = dt.Rows[i]["IDA"].ObjToDouble();
                nda = dt.Rows[i]["NDA"].ObjToDouble();
                nox = dt.Rows[i]["NOX"].ObjToDouble();
                returns = dt.Rows[i]["returns"].ObjToDouble();
                transfer = dt.Rows[i]["transfers"].ObjToDouble();
                misc = dt.Rows[i]["misc"].ObjToDouble();
                balance = dt.Rows[i]["balance"].ObjToDouble();
                comment = dt.Rows[i]["comment"].ObjToString();
                if (comment.ToUpper() == "BALANCE FORWARD")
                    continue;
                aDate = dt.Rows[i]["adate"].ObjToString();

                tda = G1.RoundValue(tda);
                ida = G1.RoundValue(ida);
                nda = G1.RoundValue(nda);
                nox = G1.RoundValue(nox);
                returns = G1.RoundValue(returns);
                transfer = G1.RoundValue(transfer);
                misc = G1.RoundValue(misc);
                balance = G1.RoundValue(balance);

                manual = dt.Rows[i]["manual"].ObjToString();

                credits += tda + ida + nda + nox;
                debits += returns + transfer + misc;

                if (manual == "Y")
                {
                    manual = "y";
                    record = G1.create_record("lockboxdeposits", "comment", "-1");
                    if (G1.BadRecord("lockboxdeposits", record))
                        break;
                    G1.update_db_table("lockboxdeposits", "record", record, new string[] { "date", date, "adate", aDate, "comment", comment, "TDA", tda.ToString(), "IDA", ida.ToString(), "NDA", nda.ToString(), "NOX", nox.ToString(), "returns", returns.ToString(), "transfers", transfer.ToString(), "misc", misc.ToString(), "balance", balance.ToString(), "bank_account", bankDetails, "manual", manual });
                }
            }

            UpdateBankTotals(bankDetails, this.dateTimePicker1.Value, credits, debits, beginningBalance, balance);

            this.Cursor = Cursors.Default;
            btnSave.Hide();
        }
        /****************************************************************************************/
        private void UpdateBankTotals ( string bankAccount, DateTime monthDate, double credits, double debits, double beginningBalance, double endingbalance )
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

            G1.update_db_table("bank_totals", "record", record, new string[] { "bank_account", bankAccount, "date", searchDate.ToString("yyyy-MM-dd"), "adate", "", "credits", credits.ToString(), "debits", debits.ToString(), "beginningBalance", beginningBalance.ToString(), "endingBalance", endingbalance.ToString() });

            cmd = "Select * from `bank_totals` where `bank_account` = '" + bankAccount + "' AND `date` > '" + searchDate.ToString("yyyy-MM-dd") + "';";
            dt = G1.get_db_data(cmd);
            for ( int i=0; i<dt.Rows.Count; i++)
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

            DataTable dt = (DataTable)dgv.DataSource;
            RecalcTotals(dt);

            dgv.DataSource = dt;
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
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
                    BankEditDebit debitForm = new BankEditDebit(bankDt);
                    debitForm.Text = "Document Debit for " + date.ToString("MM/dd/yyyy");
                    debitForm.TopMost = true;
                    debitForm.ManualDone += DebitForm_ManualDone;
                    debitForm.ShowDialog();
                    return;
                }
            }

            string what = workReport;

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
            string bank = dd.Rows[0]["bankAccount"].ObjToString();
            string debitDepNum = dd.Rows[0]["debitDepNum"].ObjToString();
            string depositNumber = dd.Rows[0]["depositNumber"].ObjToString();
            string assignTo = dd.Rows[0]["assignTo"].ObjToString().ToUpper();

            string found = "";
            string bankRecord = "";

            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                bankRecord = dt.Rows[i]["bankDebit"].ObjToString();
                if (bankRecord == record)
                {
                    G1.update_db_table("bank_details", "record", record, new string[] { "assignTo", assignTo, "description", depositNumber, "debitDepNum", debitDepNum });
                    //dt.Rows[i]["depositNumber"] = depositNumber;
                    //dt.Rows[i]["assignTo"] = assignTo;
                    //dt.Rows[i]["debitDepNum"] = debitDepNum;
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
                    string aDate = date.ToString("yyyyMMdd");
                    DataRow[] dRows = dt.Select("aDate='" + aDate + "'");
                    if (dRows.Length > 0)
                    {
                        double dda = 0D;
                        double tda = 0D;
                        double ida = 0D;
                        double nda = 0D;
                        double returns = 0D;
                        double transfers = 0D;
                        double misc = 0D;

                        DataTable bankDt = dRows.CopyToDataTable();
                        for (int j = 0; j < bankDt.Rows.Count; j++)
                        {
                            dda += bankDt.Rows[j]["DDA"].ObjToDouble();
                            tda += bankDt.Rows[j]["TDA"].ObjToDouble();
                            ida += bankDt.Rows[j]["IDA"].ObjToDouble();
                            nda += bankDt.Rows[j]["NDA"].ObjToDouble();
                            returns += bankDt.Rows[j]["returns"].ObjToDouble();
                            transfers += bankDt.Rows[j]["transfers"].ObjToDouble();
                        }
                        dRows = dt.Select("bankDebit='" + bankRecord + "'");
                        if (dRows.Length > 0)
                            dt.Rows.Remove(dRows[0]);
                        dRows = dt.Select("aDate='" + aDate + "'");
                        if (dRows.Length > 0)
                        {
                            dRows[0]["DDA"] = dda;
                            dRows[0]["TDA"] = tda;
                            dRows[0]["IDA"] = ida;
                            dRows[0]["NDA"] = nda;
                            dRows[0]["returns"] = returns;
                            dRows[0]["transfers"] = transfers;
                        }
                    }
                    double balance = RecalcTotals(dt);
                    dgv.DataSource = dt;
                    dgv.RefreshDataSource();
                    gridMain.RefreshEditor(true);
                    break;
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_DoubleClickX(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);

            DateTime date = dr["date"].ObjToDateTime();

            string what = cmbLockbox.Text;

            if ( paymentDetail != null )
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

                    DataRow[] dRows = paymentDetail.Select( cmd );
                    if (dRows.Length > 0)
                    {
                        DataTable dt = dRows.CopyToDataTable();
                        ViewDataTable viewForm = new ViewDataTable(dt, "contractNumber,firstName, lastName,paymentAmount,depositNumber,fill1");
                        viewForm.ShowDialog();
                    }
                }
                catch ( Exception ex)
                {
                }
            }

            //WeeklyClose weekForm = new WeeklyClose(date, date, what, "Lock Box Deposits");
            //weekForm.Show();
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
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (row < 0 || row > (dt.Rows.Count - 1))
                return;
            if (rowHandle == (dt.Rows.Count - 1))
                return; // Already at the last row
            DataRow dRow = dt.NewRow();
            dRow["date"] = G1.DTtoMySQLDT(date);
            dRow["TDA"] = 0.0D;
            dRow["IDA"] = 0.0D;
            dRow["NDA"] = 0.0D;
            dRow["returns"] = 0.0D;
            dRow["transfers"] = 0.0D;
            dRow["dailyTotals"] = 0.00D;
            dRow["comment"] = "Enter Comment Here";
            dRow["manual"] = "Y";
            dRow["dow"] = G1.DayOfWeekText(date);
            dt.Rows.InsertAt(dRow, row);
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
            else
                {
                bool doit = false;
                if (e.Column.FieldName.ToUpper().IndexOf("TDA") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("IDA") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("NDA") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("TRANSFERS") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("RETURNS") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("MISC") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("DAILYTOTALS") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                if ( doit )
                {
                    string str = e.DisplayText;
                    str = str.Replace(",", "");
                    double dValue = str.ObjToDouble();
                    if (dValue == 0D)
                        e.DisplayText = "";
                }
            }
        }
        /****************************************************************************************/
        private double getBeginningBalance(string bankAccount, DateTime monthDate, ref double endingBalance, ref string bankDescription )
        {
            double beginningBalance = 0D;
            endingBalance = 0D;
            bankDescription = "";

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
                asOf = monthDate;

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
            e.TotalValue = str;
        }
        /****************************************************************************************/
        private void cmbLockbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (!CheckForSave())
                return;

            GetTheData();
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

            GetTheData();
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

                    string comment = dt.Rows[row]["comment"].ObjToString();
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
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            DateTime date = dr["date"].ObjToDateTime();
            string aDate = dr["aDate"].ObjToString();
            if (!String.IsNullOrWhiteSpace(aDate))
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (row < 0 || row > (dt.Rows.Count - 1))
                return;
            dt.Rows.RemoveAt(row);

            double balance = RecalcTotals(dt);

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
        private void ScaleCells()
        {
            if (originalSize == 0D)
            {
                //                originalSize = gridMain.Columns["address1"].AppearanceCell.FontSizeDelta.ObjToDouble();
                originalSize = gridMain.Columns["comment"].AppearanceCell.Font.Size;
                mainFont = gridMain.Columns["comment"].AppearanceCell.Font;
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
        private string oldWhat = "";
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            DataRow dr = gridMain.GetFocusedDataRow();
            string manual = dr["manual"].ObjToString().ToUpper();
            if (manual != "Y")
            {
                e.Valid = false;
                return;
            }
            if (view.FocusedColumn.FieldName.ToUpper() == "TRANSFERS")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                //DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                string bankAccount = dr["bank_account"].ObjToString().ToUpper();
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
        /****************************************************************************************/
        private void gridMain_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                string cancelled = View.GetRowCellDisplayText(e.RowHandle, View.Columns["comment"]);
                if (!String.IsNullOrWhiteSpace(cancelled))
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
        /****************************************************************************************/
        private void btnSelectColumns_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;
            SelectColumns sform = new SelectColumns(dgv, "LockBox Deposits " + workReport, "Primary", actualName);
            sform.Done += new SelectColumns.d_void_eventdone(sform_Done);
            sform.Show();
        }
        /***********************************************************************************************/
        void sform_Done(DataTable dt)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = workReport + " Primary";
            string saveName = "LockBox Deposits " + workReport + " " + name;
            string skinName = "";
            SetupSelectedColumns("LockBox Deposits " + workReport, name, dgv);
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
                procType = "LockBox Deposits " + workReport;
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
            string saveName = "Lockbox Deposits " + workReport + " " + name;
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
                string saveName = "LockBox Deposits " + workReport + " " + name;
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
            string procName = "LockBox Deposits " + workReport;
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
                SetupSelectedColumns("Lockbox Deposits " + workReport, comboName, dgv);
                string name = "Lockbox Deposits " + workReport + " " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                SetupTotalsSummary();
                gridMain.OptionsView.ShowFooter = true;
            }
            else
            {
                SetupSelectedColumns("LockBox Deposits " + workReport, "Primary", dgv);
                string name = "LockBox Deposits" + workReport + " Primary";
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