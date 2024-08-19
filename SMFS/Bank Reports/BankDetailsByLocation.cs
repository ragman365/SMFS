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
    public partial class BankDetailsByLocation : DevExpress.XtraEditors.XtraForm
    {
        private DataTable groupContracts = null;
        private DataTable agentsDt = null;
        private bool runAgents = false;
        private DataTable paymentDetail = null;
        private string bankDetails = "";
        private double beginningBalance = 0D;
        private DataTable monthlyBankDt = null;
        private DataTable workDt = null;
        private string workTitle = "";
        private DataTable oldDt = null;
        private DateTime workDate = DateTime.Now;
        private bool loading = true;
        private string workReport = "";
        private string mainReport = "";
        private bool workSummary = false;
        /****************************************************************************************/
        private DataTable comboDt = null;
        /****************************************************************************************/
        public BankDetailsByLocation( DataTable dt, string title, DataTable payDt, DataTable old, DateTime date, string report, bool isSummary )
        {
            InitializeComponent();
            workDt = dt;
            workSummary = isSummary;

            workTitle = title + " by Location";
            this.Text = workTitle;

            paymentDetail = payDt;
            oldDt = old;
            workDate = date;
            workReport = report;
            mainReport = report;
        }
        /****************************************************************************************/
        private void BankDetailsByLocation_Load(object sender, EventArgs e)
        {
            chkGroup.Hide();
            dgv2.Hide();
            dgv2.Dock = DockStyle.Fill;
            btnExitLocation.Hide ();

            if ( workSummary )
            {
                string cmd = "Select * from `ach_combinations` ORDER by 'order';";
                comboDt = G1.get_db_data(cmd);
            }


            dgv.Dock = DockStyle.Fill;

            AddSummaryColumn("Payment", gridMain2);
            AddSummaryColumn("Debit", gridMain2);


            loadLocatons();

            dgv.DataSource = workDt;

            cmbType.Hide();
            lblType.Hide();

            loading = false;
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
        private void loadLocatons()
        {
            string columnName = "";
            for (int i = 0; i < workDt.Columns.Count; i++)
            {
                columnName = workDt.Columns[i].ColumnName.Trim();
                if (columnName.ToUpper() == "NUM")
                    continue;
                if (columnName.ToUpper() == "DATE")
                    continue;
                if (columnName.ToUpper() == "TOTAL")
                    continue;
                if (columnName.ToUpper() == "TRANSFERS")
                    continue;
                if (columnName.ToUpper() == "DAILY TOTAL")
                    continue;
                cmbLocation.Items.Add(columnName);
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
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(10, 10, 80, 10);

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
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(10, 10, 80, 10);

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


            DateTime date = workDate;
            string workDate1 = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            Printer.DrawQuad(20, 8, 5, 4, "Month Closing - " + workDate1, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            //DataRow dr = gridMain.GetFocusedDataRow();
            //int rowHandle = gridMain.FocusedRowHandle;
            //int row = gridMain.GetDataSourceRowIndex(rowHandle);

            //DateTime date = dr["date"].ObjToDateTime();
            //string bankDebit = dr["bankDebit"].ObjToString();
            //string bank_account = dr["bank_account"].ObjToString();
            //if (!String.IsNullOrWhiteSpace(bankDebit))
            //{
            //    string cmd = "Select * from `bank_details` WHERE `record` = '" + bankDebit + "';";
            //    DataTable bankDt = G1.get_db_data(cmd);
            //    if (bankDt.Rows.Count > 0)
            //    {
            //        if (G1.get_column_number(bankDt, "bank_account") < 0)
            //            bankDt.Columns.Add("bank_account");
            //        bankDt.Rows[0]["bank_account"] = bank_account;
            //        bankDt.Columns.Add("depositNumber");
            //        bankDt.Rows[0]["depositNumber"] = bankDt.Rows[0]["description"].ObjToString();
            //        BankEditDebit debitForm = new BankEditDebit(bankDt);
            //        debitForm.Text = "Document Debit for " + date.ToString("MM/dd/yyyy");
            //        debitForm.TopMost = true;
            //        debitForm.ManualDone += DebitForm_ManualDone;
            //        debitForm.ShowDialog();
            //        return;
            //    }
            //}

            //string what = cmbLockbox.Text;

            //if ( paymentDetail != null )
            //{
            //    try
            //    {
            //        if (G1.get_column_number(paymentDetail, "search") >= 0)
            //            paymentDetail.Columns.Remove("search");

            //        paymentDetail.Columns.Add("search");

            //        for (int i = 0; i < paymentDetail.Rows.Count; i++)
            //            paymentDetail.Rows[i]["search"] = paymentDetail.Rows[i]["payDate8"].ObjToDateTime().ToString("yyyyMMdd");

            //        string str = date.ToString("yyyyMMdd");

            //        string cmd = "search='" + str + "'";

            //        DataRow[] dRows = paymentDetail.Select( cmd );
            //        if (dRows.Length > 0)
            //        {
            //            DataTable dt = dRows.CopyToDataTable();
            //            ViewDataTable viewForm = new ViewDataTable(dt, "contractNumber,firstName, lastName,paymentAmount,depositNumber,fill1,location,debitAdjustment,downPayment");
            //            viewForm.Text = "Showing All Deposits for " + date.ToString("MM/dd/yyyy");
            //            viewForm.ShowDialog();
            //        }
            //    }
            //    catch ( Exception ex)
            //    {
            //    }
            //}

            //WeeklyClose weekForm = new WeeklyClose(date, date, what, "Lock Box Deposits");
            //weekForm.Show();
        }
        /****************************************************************************************/
        private void DebitForm_ManualDone(DataTable dd)
        {
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayTextxx(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() != "NUM")
            {
                string str = e.DisplayText;
                str = str.Replace(",", "");
                double dValue = str.ObjToDouble();
                if (dValue == 0D)
                {
                    e.DisplayText = "";
                    if (e.Column.FieldName.ToUpper() == "DAILY TOTAL")
                        e.DisplayText = "   -";
                }
                else
                {
                    str = G1.ReformatMoney(dValue);
                    if (str == "0")
                    {
                        str = "";
                        if (e.Column.FieldName.ToUpper() == "DAILY TOTAL")
                            str = "   -";
                    }
                    e.DisplayText = str;
                }
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
                    string str = date.ToString("MM/dd/yy");
                    e.DisplayText = str;
                    e.DisplayText = date.ToString("MM/dd/yy");
                    if (date.Year < 30)
                        e.DisplayText = "";
                }
            }
            else
            {
                if (e.Column.FieldName.ToUpper() != "NUM")
                {
                    string str = e.DisplayText;
                    str = str.Replace(",", "");
                    double dValue = str.ObjToDouble();
                    if (dValue == 0D)
                    {
                        e.DisplayText = "";
                        if (e.Column.FieldName.ToUpper() == "DAILY TOTAL")
                            e.DisplayText = "   -";
                    }
                    else
                    {
                        str = G1.ReformatMoney(dValue);
                        if (str == "0")
                        {
                            str = "";
                            if (e.Column.FieldName.ToUpper() == "DAILY TOTAL")
                                str = "   -";
                        }
                        e.DisplayText = str;
                    }
                }
            }
        }
        /****************************************************************************************/
        private string oldWhat = "";
        /****************************************************************************************/
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
        private double originalSize = 0D;
        private Font mainFont = null;
        private Font newFont = null;
        private void ScaleCells()
        {
            if (originalSize == 0D)
            {
                //                originalSize = gridMain.Columns["address1"].AppearanceCell.FontSizeDelta.ObjToDouble();
                originalSize = gridMain.Columns["Total"].AppearanceCell.Font.Size;
                mainFont = gridMain.Columns["Total"].AppearanceCell.Font;
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

            location = AchBankDeposits.CleanupLocation(location);

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
                            location = AchBankDeposits.CleanupLocation(location);
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
        private void cmbLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            string location = cmbLocation.Text.Trim();

            chkGroup.Show();
            chkGroup.Refresh();

            string workLocation = location;

            string whatReport = "";
            if (workLocation.IndexOf(" Trust") > 0)
                workReport = "Trust";
            else if (workLocation.IndexOf(" Ins") > 0)
                workReport = "Ins";
            else if (workLocation.IndexOf(" Funeral") > 0)
                workReport = "Funeral";

            if (mainReport == "Cover Report")
                workReport = "Funeral";

            workLocation = workLocation.Replace("Trust", "");
            workLocation = workLocation.Replace("Ins", "");
            workLocation = workLocation.Replace("Funeral", "");
            workLocation = workLocation.Trim();

            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("Date");
            dt.Columns.Add("aDate");
            dt.Columns.Add("What");
            dt.Columns.Add("Contract");
            dt.Columns.Add("Payment", Type.GetType("System.Double"));
            dt.Columns.Add("Debit", Type.GetType("System.Double"));


            int col = 0;
            int row = 0;
            double payment = 0D;
            double debit = 0D;
            double downPayment = 0D;
            double dValue = 0D;
            DateTime date = DateTime.Now;
            string contractNumber = "";
            string aDate = "";
            string what = "";
            DataRow dRow = null;
            int whatCol = G1.get_column_number(paymentDetail, "what");

            bool gotInsurance = false;

            for (int i = 0; i < paymentDetail.Rows.Count; i++)
            {
                try
                {
                    date = paymentDetail.Rows[i]["payDate8"].ObjToDateTime();

                    contractNumber = paymentDetail.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber.IndexOf("ZZ") == 0)
                    {
                        if (workReport == "Trust")
                            continue;
                        location = paymentDetail.Rows[i]["location"].ObjToString();
                        location = DetermineLocation(contractNumber, location, "Ins");
                        location = AchBankDeposits.CleanupLocation(location);
                        gotInsurance = true;
                        if (mainReport == "Cover Report")
                            continue;
                        if (comboDt != null)
                        {
                            if (!CheckComboLocation(workLocation, location, "Ins"))
                                continue;
                        }
                        else
                        {
                            if (location != workLocation)
                                continue;
                        }
                        payment = paymentDetail.Rows[i]["paymentAmount"].ObjToDouble();
                        debit = paymentDetail.Rows[i]["debitAdjustment"].ObjToDouble();

                        if (payment != 0D || debit != 0D)
                        {
                            dRow = dt.NewRow();
                            dRow["Date"] = date.ToString("MM/dd/yyyy");
                            aDate = date.ToString("yyyyMMdd");
                            dRow["aDate"] = aDate;
                            dRow["Contract"] = contractNumber;
                            if (payment > 0D)
                                dRow["Payment"] = payment;
                            else if (debit > 0D)
                                dRow["Debit"] = debit;
                            dRow["What"] = "Ins";
                            dt.Rows.Add(dRow);
                        }
                    }
                    else
                    {
                        if (workReport == "Ins")
                            continue;
                        location = paymentDetail.Rows[i]["location"].ObjToString();
                        if (gotInsurance)
                        {
                            if (mainReport == "ACH Detail Report")
                                break;
                            location = DetermineLocation(contractNumber, location, "Funeral");
                            what = "Funeral";
                        }
                        else
                        {
                            if (mainReport == "Cover Report")
                                continue;
                            location = DetermineLocation(contractNumber, location, "Trust");
                            what = "Trust";
                        }
                        if ( comboDt != null )
                        {
                            if (!CheckComboLocation(workLocation, location, "Trust"))
                                continue;
                        }
                        else
                        {
                            if (location != workLocation)
                                continue;
                        }
                        if (String.IsNullOrWhiteSpace(workReport) && whatCol > 0)
                            what = paymentDetail.Rows[i][whatCol].ObjToString();
                        payment = paymentDetail.Rows[i]["paymentAmount"].ObjToDouble();
                        debit = paymentDetail.Rows[i]["debitAdjustment"].ObjToDouble();
                        downPayment = paymentDetail.Rows[i]["downPayment"].ObjToDouble();
                        if (payment != 0D || debit != 0D)
                        {
                            dRow = dt.NewRow();
                            dRow["Date"] = date.ToString("MM/dd/yyyy");
                            aDate = date.ToString("yyyyMMdd");
                            dRow["aDate"] = aDate;
                            dRow["Contract"] = contractNumber;
                            if (payment > 0D)
                                dRow["Payment"] = payment;
                            else if (debit > 0D)
                                dRow["Debit"] = debit;
                            dRow["What"] = what;
                            dt.Rows.Add(dRow);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = "aDate asc";
            dt = tempview.ToTable();

            col = G1.get_column_number(dt, "aDate");
            dt.Columns.RemoveAt(col);

            dgv.Hide();

            G1.NumberDataTable(dt);

            dgv2.DataSource = dt;
            dgv2.Show();
            chkGroup.Show();
            if (chkGroup.Checked)
                gridMain2.ExpandAllGroups();

            if (mainReport != "ACH Detail Report")
            {
                cmbType.Show();
                cmbType.Refresh();
                lblType.Visible = true;
                lblType.Refresh();
            }

            btnExitLocation.Show();
            btnExitLocation.Refresh();


            //ViewDataTable viewForm = new ViewDataTable(dt, "date,Contract,Payment,Debit");
            //viewForm.Text = "Details for Location (" + workLocation + ")";
            //viewForm.ShowDialog();
        }
        /****************************************************************************************/
        private bool CheckComboLocation ( string workLocation, string location, string type )
        {
            if (comboDt == null)
                return false;
            bool found = false;
            string loc = workLocation + " " + type;
            DataRow[] dRows = comboDt.Select("heading='" + loc + "'");
            if (dRows.Length <= 0)
            {
                dRows = comboDt.Select("location='" + loc + "'");
                if ( dRows.Length <= 0 )
                    return false;
            }
            string newLoc = "";
            for ( int i=0; i<dRows.Length; i++)
            {
                newLoc = dRows[i]["location"].ObjToString();
                newLoc = newLoc.Replace(type, "");
                newLoc = newLoc.Trim();
                if ( newLoc == location )
                {
                    found = true;
                    break;
                }
            }
            return found;
        }
        /****************************************************************************************/
        private void btnExitLocation_Click(object sender, EventArgs e)
        {
            dgv2.Hide();
            chkGroup.Hide();
            btnExitLocation.Hide();

            cmbType.Hide();
            cmbType.Refresh();
            lblType.Visible = false;
            lblType.Refresh();

            cmbLocation.Text = "";

            dgv.Show();
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void chkGroup_CheckedChanged(object sender, EventArgs e)
        {
            if (!dgv2.Visible)
                return;

            CheckBox cBox = (CheckBox)sender;
            if (cBox.Checked)
            {
                gridMain2.Columns[1].GroupIndex = 0;

                //new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, "Payment", "Payment", "${0:0,0.00}");
                gridMain2.ExpandAllGroups();
            }
            else
                gridMain2.Columns[1].GroupIndex = -1;

            gridMain2.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void gridMain2_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            string contract = dr["Contract"].ObjToString();
            string what = dr["What"].ObjToString();
            this.Cursor = Cursors.WaitCursor;
            if (what == "Trust")
            {
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
            }
            else if ( what == "Ins")
            {
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
            }
            else
            {
                EditCust editForm = new EditCust ( contract );
                editForm.Show();
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            gridMain2.RefreshData();
            gridMain2.RefreshEditor(true);

            dgv2.Refresh();
        }
        /****************************************************************************************/
        private void gridMain2_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            string type = cmbType.Text;
            if (String.IsNullOrWhiteSpace(type))
                return;
            if (type.ToUpper() == "ALL")
                return;
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv2.DataSource;
            string what = dt.Rows[row]["what"].ObjToString();
            if (what.ToUpper() != type.ToUpper())
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /****************************************************************************************/
    }
}