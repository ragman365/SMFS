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
using DevExpress.XtraGrid.Views.Base;
using DevExpress.Xpo.Helpers;
using System.IO;
using ExcelLibrary.BinaryFileFormat;
using System.Security.Cryptography;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using MySql.Data.MySqlClient;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class FindMismatches : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private DataTable originalDt = null;
        private bool loading = false;
        /***********************************************************************************************/
        public FindMismatches()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void FindMismatches_Load(object sender, EventArgs e)
        {
            btnMismatch.Hide();

            gridMain.Columns["mismatch"].Visible = false;
            gridMain.Columns["balance"].Visible = false;
            gridMain.Columns["totalMonths"].Visible = false;
            gridMain.Columns["firstPayDate"].Visible = false;
            gridMain.Columns["newDueDate"].Visible = false;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                G1.SpyGlass(gridMain);
        }
        /***********************************************************************************************/
        private void SetSpyGlass(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            if (grid.OptionsFind.AlwaysVisible == true)
                grid.OptionsFind.AlwaysVisible = false;
            else
                grid.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
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
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                string cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    string cnum = contract.TrimStart('0');
                    cnum = cnum.Replace(" ", "");

                    cmd = "Select * from `icustomers` where `payer` = '" + cnum + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                        contract = dx.Rows[0]["contractNumber"].ObjToString();
                }
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        private bool isPrinting = false;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isPrinting = true;
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

            Printer.setupPrinterMargins(50, 100, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();
            isPrinting = false;
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

            font = new Font("Ariel", 12);
            string text = this.Text;
            Printer.DrawQuad(5, 7, 5, 4, text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Bold);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isPrinting = true;
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

            Printer.setupPrinterMargins(50, 100, 80, 50);

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
            isPrinting = false;
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.RowHandle < 0)
                return;
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            string workContract = this.txtContract.Text.Trim();

            string cmd = "Select * from `customers` p JOIN `contracts` d ON p.`contractNumber` = d.`contractNumber` ";
            cmd += " WHERE ";
            cmd += " p.`deceasedDate` < '1000-01-01' AND `dueDate8` <> '2039-12-31' ";
            if (!String.IsNullOrWhiteSpace(workContract))
                cmd += " AND d.`contractNumber` = '" + workContract + "' ";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("num");
            dt.Columns.Add("newDueDate");
            dt.Columns.Add("status");
            dt.Columns.Add("mismatch");
            if (G1.get_column_number(dt, "balance") < 0)
                dt.Columns.Add("balance", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "totalMonths") < 0)
                dt.Columns.Add("totalMonths", Type.GetType("System.Double"));


            DateTime lapseDate = DateTime.Now;
            string lapsed = "";
            string status = "";

            DateTime badDate = new DateTime(1000, 1, 1);

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                status = "";
                lapsed = dt.Rows[i]["lapsed"].ObjToString().ToUpper();
                lapseDate = dt.Rows[i]["lapseDate8"].ObjToDateTime();
                if (lapsed == "Y" || lapseDate > badDate)
                    status = "Lapsed";
                dt.Rows[i]["status"] = status;
            }

            G1.NumberDataTable(dt);

            dgv.DataSource = dt;

            originalDt = dt;

            loading = true;


            loading = false;

            btnMismatch.Show();
            btnMismatch.Refresh();

            gridMain.Columns["mismatch"].Visible = false;
            gridMain.Columns["balance"].Visible = false;
            gridMain.Columns["totalMonths"].Visible = false;
            gridMain.Columns["firstPayDate"].Visible = false;
            gridMain.Columns["newDueDate"].Visible = false;

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void radioLapses_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;

            //radioMismatches.Checked = false;
            //radioAll.Checked = false;
            //radioActive.Checked = false;
            //radioLapses.Checked = true;

            loading = false;

            DataTable dt = null;

            DataRow[] dRows = originalDt.Select("status='Lapsed'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            else
                dt = originalDt.Clone();

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void radioMismatches_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;

            //radioLapses.Checked = false;
            //radioAll.Checked = false;
            //radioActive.Checked = false;
            //radioMismatches.Checked = true;

            loading = false;

            DataTable dt = null;

            DataRow[] dRows = originalDt.Select("mismatch='Mismatch'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            else
                dt = originalDt.Clone();

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void radioActive_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;

            //radioLapses.Checked = false;
            //radioMismatches.Checked = false;
            //radioAll.Checked = false;
            //radioActive.Checked = true;

            loading = false;

            DataTable dt = null;

            DataRow[] dRows = originalDt.Select("status<>'Lapsed'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            else
                dt = originalDt.Clone();

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void radioAll_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;

            loading = true;

            //radioMismatches.Checked = false;
            //radioLapses.Checked = false;
            //radioActive.Checked = false;
            //radioAll.Checked = true;

            loading = false;

            dgv.DataSource = originalDt;
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void btnMismatch_Click(object sender, EventArgs e)
        { // Run The Mismatch

            gridMain.Columns["mismatch"].Visible = true;
            gridMain.Columns["balance"].Visible = true;
            gridMain.Columns["totalMonths"].Visible = true;
            gridMain.Columns["firstPayDate"].Visible = true;
            gridMain.Columns["newDueDate"].Visible = true;

            barImport.Visible = true;
            barImport.Show();
            this.Cursor = Cursors.WaitCursor;

            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;

            int[] rows = gridMain.GetSelectedRows();
            int lastRow = dt.Rows.Count;
            if ( rows.Length > 1 )
                lastRow = rows.Length;

            DateTime oldDueDate = DateTime.Now;
            string contract = "";
            string cmd = "";
            DataTable dx = null;
            DateTime lastDate = DateTime.Now;
            DateTime firstPayDate = DateTime.Now;
            DateTime dueDate = DateTime.Now;
            double dAPR = 0D;
            double startBalance = 0D;
            int numPayments = 0;
            double payment = 0D;
            double monthlyPayment = 0D;
            double newBalance = 0D;
            string edited = "";
            TimeSpan ts;
            DateTime iDate = DateTime.Now;
            string issueDate = "";
            string apr = "";
            DateTime docp = DateTime.Now;
            double debit = 0D;
            double credit = 0D;
            DateTime nextDueDate = DateTime.Now;
            double creditBalance = 0D;
            double months = 0D;
            double oldBalance = 0D;
            DateTime currentDueDate = DateTime.Now;
            double balance = 0D;
            double originalDownPayment = 0D;
            DateTime contractDueDate8 = DateTime.Now;
            DateTime lastDueDate8 = DateTime.Now;
            DateTime payDate8 = DateTime.Now;
            double dPayments = 0D;
            string lapsed = "";
            DateTime xDate = DateTime.Now;
            decimal totalMonths = 0;
            double downPayment = 0D;
            DateTime testDueDate = DateTime.Now;
            int row = 0;

            string fill = "";

            int dueRow = 0;
            DateTime date = DateTime.Now;
            DateTime saveLastDate = DateTime.Now;
            double dMonths = 0D;
            double interest = 0D;

            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            for (int i = 0; i < lastRow; i++)
            {
                Application.DoEvents();

                barImport.Value = i;
                barImport.Refresh();
                try
                {
                    row = i;
                    if (rows.Length > 1)
                    {
                        row = rows[i];
                        row = gridMain.GetDataSourceRowIndex(row);
                    }
                    date = dt.Rows[row]["issueDate8"].ObjToDateTime();
                    dt.Rows[row]["mismatch"] = "";
                    lapsed = "";

                    oldDueDate = dt.Rows[row]["dueDate8"].ObjToDateTime();
                    contractDueDate8 = oldDueDate;

                    dueDate = new DateTime(1850, 1, 1);
                    firstPayDate = new DateTime(1850, 1, 1);
                    saveLastDate = lastDate;

                    dueDate = dt.Rows[row]["firstPayDate"].ObjToDateTime();
                    firstPayDate = dueDate;
                    if (dueDate.Year < 1900)
                    {
                        dueDate = DailyHistory.GetIssueDate(dueDate, contract, null);
                        if (dueDate.Year < 1900)
                            continue;
                    }
                    lastDueDate8 = dueDate;


                    contract = dt.Rows[row]["contractNumber"].ObjToString();
                    if ( contract.ToUpper() == "E17018UI")
                    {

                    }

                    startBalance = DailyHistory.GetFinanceValue(dt.Rows[row]);
                    numPayments = dt.Rows[row]["numberOfPayments"].ObjToString().ObjToInt32();
                    monthlyPayment = dt.Rows[row]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();

                    iDate = DailyHistory.GetIssueDate(dt.Rows[row]["issueDate8"].ObjToDateTime(), contract, null);
                    issueDate = iDate.ToString("MM/dd/yyyy");
                    lastDate = issueDate.ObjToDateTime();
                    apr = dt.Rows[row]["APR"].ObjToString();
                    dAPR = apr.ObjToDouble() / 100.0D;

                    dx = LoadMainData2(contract, "", monthlyPayment);
                    if (dx.Rows.Count <= 0)
                        continue;

                    dueRow = dx.Rows.Count - 1;
                    if (dueRow >= 1)
                    {
                        iDate = dx.Rows[dueRow]["dueDate8"].ObjToDateTime();
                        downPayment = dx.Rows[dueRow]["downPayment"].ObjToDouble();
                        if (downPayment > 0D)
                        {
                            try
                            {
                                iDate = dx.Rows[dueRow - 1]["dueDate8"].ObjToDateTime();
                            }
                            catch ( Exception ex)
                            {
                            }
                        }
                        iDate = issueDate.ObjToDateTime();
                        firstPayDate = DailyHistory.CheckFirstPayDate(firstPayDate, iDate);
                        dt.Rows[row]["firstPayDate"] = G1.DTtoMySQLDT(firstPayDate);
                        if (firstPayDate.Year > 1900)
                            iDate = firstPayDate;
                        totalMonths = 0;
                        dMonths = 0D;
                        for ( int j=dueRow; j>=0; j--)
                        {
                            fill = dx.Rows[j]["fill"].ObjToString().ToUpper();
                            if (fill == "D")
                                continue;
                            credit = dx.Rows[j]["creditAdjustment"].ObjToDouble();
                            interest = dx.Rows[j]["interestPaid"].ObjToDouble();
                            //if ( credit > 0D && interest == 0D ) // Fix for Credit/Debit Issue (DOLP) // Removed for M23002LI on 10/15/2024
                            //{
                            //    continue;
                            //}
                            totalMonths += dx.Rows[j]["NumPayments"].ObjToDecimal();
                            dMonths += dx.Rows[j]["NumPayments"].ObjToDouble();
                            //totalMonths = (decimal) G1.RoundValue((double) totalMonths);
                        }
                        //totalMonths = (decimal) G1.RoundValue((double) totalMonths);
                        totalMonths = (decimal)Math.Truncate((decimal) dMonths);
                        if ( dMonths > 0)
                        {
                            dt.Rows[row]["totalMonths"] = dMonths;
                            dMonths += 0.005D;
                            //totalMonths = Math.Truncate(totalMonths);
                            dMonths = Math.Truncate(dMonths);
                            numPayments = Convert.ToInt32(dMonths);
                            if (numPayments != 0)
                                iDate = iDate.AddMonths((int)numPayments);
                        }
                        if (1 != 1)
                        {
                            dt.Rows[row]["newDueDate"] = iDate.ToString("MM/dd/yyyy"); // Ramma Zamma
                            if (iDate != oldDueDate)
                                dt.Rows[row]["mismatch"] = "Mismatch";
                        }
                        else
                        {
                            testDueDate = VerifyDueDate(contract);

                            dt.Rows[row]["newDueDate"] = testDueDate.ToString("MM/dd/yyyy");
                            if (testDueDate != oldDueDate)
                                dt.Rows[row]["mismatch"] = "Mismatch";
                        }

                        if (iDate < DailyHistory.majorDate)
                        {
                            dt.Rows[row]["mismatch"] = "N/A";
                        }
                        //else
                        //{
                        //    dt.Rows[row]["newDueDate"] = testDueDate.ToString("MM/dd/yyyy");
                        //    if (testDueDate != oldDueDate)
                        //        dt.Rows[row]["mismatch"] = "Mismatch";
                        //}

                        cmd = "Select * from `payments` where `contractNumber` = '" + contract + "' order by `paydate8` DESC, `tmstamp` DESC, `record` DESC;";
                        dx = G1.get_db_data(cmd);
                        dx.Columns.Add("numPayments", Type.GetType("System.Double"));

                        DailyHistory.CalculateNewStuff(dx, dAPR, numPayments, startBalance, lastDate);
                        if (dx.Rows.Count <= 0)
                            continue;

                        balance = dx.Rows[0]["balance"].ObjToDouble();
                        dt.Rows[row]["balance"] = balance;
                        continue;
                    }
                    else
                    {
                        cmd = "Select * from `payments` where `contractNumber` = '" + contract + "' order by `paydate8` DESC, `tmstamp` DESC, `record` DESC;";
                        dx = G1.get_db_data(cmd);
                        dx.Columns.Add("numPayments", Type.GetType("System.Double"));

                        DailyHistory.CalculateNewStuff(dx, dAPR, numPayments, startBalance, lastDate);
                        if (dx.Rows.Count <= 0)
                            continue;

                        //testDueDate = VerifyDueDate(contract);

                        //dt.Rows[row]["newDueDate"] = testDueDate.ToString("MM/dd/yyyy");
                        //if (testDueDate != oldDueDate)
                        //    dt.Rows[row]["mismatch"] = "Mismatch";

                        balance = dx.Rows[0]["balance"].ObjToDouble();
                        dt.Rows[row]["balance"] = balance;
                        iDate = dx.Rows[0]["dueDate8"].ObjToDateTime();
                        dt.Rows[row]["newDueDate"] = iDate.ToString("MM/dd/yyyy");
                        dt.Rows[row]["mismatch"] = "N/A";
                        continue;
                    }

                    payDate8 = dx.Rows[0]["payDate8"].ObjToDateTime();
                    debit = dx.Rows[0]["debitAdjustment"].ObjToDouble();

                    iDate = dx.Rows[0]["dueDate8"].ObjToDateTime();
                    if (iDate.Year < 1900)
                    {
                        iDate = dx.Rows[0]["dueDate8"].ObjToDateTime();
                        lapsed = dt.Rows[row]["status"].ObjToString();
                    }
                    numPayments = dx.Rows[0]["NumPayments"].ObjToInt32();
                    dPayments = dx.Rows[0]["NumPayments"].ObjToDouble();
                    dPayments = G1.RoundValue(dPayments);

                    numPayments = Convert.ToInt32(dPayments);
                    if (debit != 0D)
                        numPayments = 0;

                    if ( numPayments != 0 )
                        iDate = iDate.AddMonths((int)numPayments);
                    //dt.Rows[row]["mismatch"] = "";
                    //dt.Rows[row]["balance"] = balance;
                    //dt.Rows[row]["newDueDate"] = iDate.ToString("MM/dd/yyyy");
                    //if (docp <= DailyHistory.majorDate)
                    //    iDate = oldDueDate;

                    //if (iDate != oldDueDate)
                    //    dt.Rows[row]["mismatch"] = "Mismatch";

                    iDate = DailyHistory.GetIssueDate(dt.Rows[row]["issueDate8"].ObjToDateTime(), contract, null);
                    issueDate = iDate.ToString("MM/dd/yyyy");
                    lastDate = issueDate.ObjToDateTime();
                    apr = dt.Rows[row]["APR"].ObjToString();
                    dAPR = apr.ObjToDouble() / 100.0D;
                    //docp = dt.Rows[row]["lastDatePaid8"].ObjToDateTime();
                    //if ( docp.Year < 1000 )
                    //{
                    //    cmd = "Select * from `payments` where `contractNumber` = '" + contract + "' order by `paydate8` DESC, `tmstamp` DESC, `record` DESC LIMIT 1;";
                    //    dx = G1.get_db_data(cmd);
                    //    if ( dx.Rows.Count > 0 )
                    //    {
                    //        docp = dx.Rows[0]["payDate8"].ObjToDateTime();
                    //    }
                    //}

                    cmd = "Select * from `payments` where `contractNumber` = '" + contract + "' order by `paydate8` DESC, `tmstamp` DESC, `record` DESC;";
                    dx = G1.get_db_data(cmd);
                    dx.Columns.Add("numPayments", Type.GetType("System.Double"));
                    //dx.Columns.Add("balance", Type.GetType("System.Double"));

                    //originalDownPayment = DailyHistory.GetDownPayment(contract);

                    //try
                    //{
                    //    DailyHistory.CalcMainDueDates(contract, dx, originalDownPayment, contractDueDate8, firstPayDate, monthlyPayment, lastDueDate8, saveLastDate);
                    //}
                    //catch ( Exception ex)
                    //{
                    //}

                    DailyHistory.CalculateNewStuff(dx, dAPR, numPayments, startBalance, lastDate);
                    if (dx.Rows.Count <= 0)
                        continue;

                    //payment = monthlyPayment;
                    //xDate = DailyHistory.getNextDueDate(dx, payment, ref newBalance);


                    //payment = monthlyPayment;
                    //DailyHistory.ReCalculateDueDate(contract, docp, monthlyPayment, payment, 0D, 0D, ref nextDueDate, ref creditBalance, ref months, ref oldBalance, ref currentDueDate);


                    //DailyHistory.CalculateDueNow(contract, ref balance);

                    //iDate = dx.Rows[0]["currentDueDate8"].ObjToDateTime();
                    balance = dx.Rows[0]["balance"].ObjToDouble();
                    dt.Rows[row]["balance"] = balance;
                    //if (xDate == iDate)
                    //{
                    //    dt.Rows[row]["newDueDate"] = iDate.ToString("MM/dd/yyyy");
                    //}
                    //if (payDate8 < DailyHistory.majorDate)
                    //{
                    //    dt.Rows[row]["mismatch"] = "N/A";
                    //}
                    //txtContract.Text = contract;
                    //txtContract.Refresh();
                    if (1 == 1)
                        continue;
                    if ( iDate.Year < 1000 )
                    {
                        firstPayDate = dx.Rows[0]["nextDueDate"].ObjToDateTime();
                        ts = oldDueDate - firstPayDate;
                        iDate = firstPayDate;
                    }
                    dt.Rows[row]["mismatch"] = "";
                    dt.Rows[row]["balance"] = balance;
                    dt.Rows[row]["newDueDate"] = iDate.ToString("MM/dd/yyyy");
                    if (docp <= DailyHistory.majorDate)
                        iDate = oldDueDate;
                    if (iDate != oldDueDate)
                        dt.Rows[row]["mismatch"] = "Mismatch";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Customer i=" + i.ToString() + " " + ex.Message.ToString());
                }
            }
            barImport.Value = lastRow;
            barImport.Refresh();
            dgv.DataSource = dt;
            dgv.Refresh();
            this.Refresh();
            this.Cursor = Cursors.Default;

            loading = false;
        }
        /****************************************************************************************/
        public static DateTime VerifyDueDate(string workContract)
        {
            DateTime dueDate = DateTime.Now;
            string cmd = "Select * from `customers` p JOIN `contracts` d ON p.`contractNumber` = d.`contractNumber` ";
            cmd += " WHERE ";
            cmd += " p.`deceasedDate` < '1000-01-01' AND `dueDate8` <> '2039-12-31' ";
            cmd += " AND d.`contractNumber` = '" + workContract + "' ";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return dueDate;

            DateTime lastDate = DateTime.Now;

            dueDate = dt.Rows[0]["dueDate8"].ObjToDateTime();

            DateTime date = dt.Rows[0]["issueDate8"].ObjToDateTime();
            string lapsed = "";

            DateTime oldDueDate = dt.Rows[0]["dueDate8"].ObjToDateTime();
            DateTime contractDueDate8 = oldDueDate;

            dueDate = new DateTime(1850, 1, 1);
            DateTime firstPayDate = new DateTime(1850, 1, 1);
            DateTime saveLastDate = lastDate;

            dueDate = dt.Rows[0]["firstPayDate"].ObjToDateTime();
            firstPayDate = dueDate;
            if (dueDate.Year < 1900)
            {
                dueDate = DailyHistory.GetIssueDate(dueDate, workContract, null);
                if (dueDate.Year < 1900)
                    return dueDate;
            }
            DateTime lastDueDate8 = dueDate;


            double startBalance = DailyHistory.GetFinanceValue(dt.Rows[0]);
            int numPayments = dt.Rows[0]["numberOfPayments"].ObjToString().ObjToInt32();
            double monthlyPayment = dt.Rows[0]["amtOfMonthlyPayt"].ObjToString().ObjToDouble();

            DateTime iDate = DailyHistory.GetIssueDate(dt.Rows[0]["issueDate8"].ObjToDateTime(), workContract, null);
            string issueDate = iDate.ToString("MM/dd/yyyy");
            lastDate = issueDate.ObjToDateTime();
            string apr = dt.Rows[0]["APR"].ObjToString();
            double dAPR = apr.ObjToDouble() / 100.0D;

            DataTable dx = LoadMainData2(workContract, "", monthlyPayment);
            if (dx.Rows.Count <= 0)
                return dueDate;

            int dueRow = dx.Rows.Count - 1;
            if (dueRow >= 1)
            {
                iDate = dx.Rows[dueRow]["dueDate8"].ObjToDateTime();
                double downPayment = dx.Rows[dueRow]["downPayment"].ObjToDouble();
                if (downPayment > 0D)
                {
                    try
                    {
                        iDate = dx.Rows[dueRow - 1]["dueDate8"].ObjToDateTime();
                    }
                    catch (Exception ex)
                    {
                    }
                }
                iDate = issueDate.ObjToDateTime();
                firstPayDate = DailyHistory.CheckFirstPayDate(firstPayDate, iDate);
                dt.Rows[0]["firstPayDate"] = G1.DTtoMySQLDT(firstPayDate);
                if (firstPayDate.Year > 1900)
                    iDate = firstPayDate;
                decimal totalMonths = 0;
                double dMonths = 0D;
                double credit = 0D;
                double interest = 0D;
                string fill = "";
                for (int j = dueRow; j >= 0; j--)
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
                //totalMonths = (decimal) G1.RoundValue((double) totalMonths);
                totalMonths = (decimal)Math.Truncate((decimal)dMonths);
                if (dMonths > 0)
                {
                    //dt.Rows[0]["totalMonths"] = dMonths;

                    //dMonths += 0.0005D;

                    //totalMonths = Math.Truncate(totalMonths);
                    dMonths = Math.Truncate(dMonths);
                    numPayments = Convert.ToInt32(dMonths);
                    if (numPayments != 0)
                        iDate = iDate.AddMonths((int)numPayments);
                }
                //dt.Rows[0]["newDueDate"] = iDate.ToString("MM/dd/yyyy");
                dueDate = iDate;
            }
            return dueDate;
        }
        /****************************************************************************************/
        public static DataTable LoadMainData2(string workContract, string workPayer, double ExpectedPayment)
        {
            string paymentsFile = "payments";
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
                        string contract = ddx.Rows[i]["contractNumber"].ObjToString();
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

            //Double startBalance = DailyHistory.GetFinanceValue(workContract);

            try
            {
                double startBalance = 0D;

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

                DateTime insPayDate8 = DateTime.Now;
                DateTime insDueDate8 = DateTime.Now;

                DateTime pDate = DateTime.Now;
                DateTime dDate = DateTime.Now;
                for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                {
                    //payment = dt.Rows[i]["paymentAmount"].ObjToString().ObjToDouble();
                    payment = DailyHistory.getPayment(dt, i);

                    debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                    //if ( credit > 0D && interest == 0D ) // Adjust for Credit/Interest issue (DOLP) // Remove for M23002LI on 10/15/2024
                    //{
                    //    dt.Rows[i]["numMonthPaid"] = 0D;
                    //    continue;
                    //}
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
                            NumPayments = NumPayments * -1D;
                        if (!String.IsNullOrWhiteSpace(workPayer))
                        {
                            pDate = dt.Rows[i]["payDate8"].ObjToDateTime();
                            dDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                            if (pDate > DailyHistory.killSecNatDate)
                                dt.Rows[i]["numMonthPaid"] = 0D;
                            double months = DailyHistory.CheckMonthsForInsuranceNew(workContract, workPayer, ExpectedPayment, payment, pDate, dDate);
                            NumPayments = months;
                            //nextDueDate = dueDate.ObjToDateTime();
                            //int imonths = (int)months;
                            //nextDueDate = nextDueDate.AddMonths(imonths);
                        }
                    }
                    dt.Rows[i]["NumPayments"] = NumPayments;
                    if (insurance)
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
                string location = "";
                string userId = "";
                int imonths = 0;
                DateTime lastDueDate = DateTime.Now;
                DateTime nextDueDate = DateTime.Now;
                if (DailyHistory.isInsurance(workContract) && dt.Rows.Count > 1)
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
                        lastDueDate = insDueDate8;
                        nextDueDate = lastDueDate.AddMonths(imonths);
                    }
                    //lblCDD.Text = "CDD " + nextDueDate.ToString("MM/dd/yyyy");
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
                    payment = DailyHistory.getPayment(dt, i);

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

                double downPay = DailyHistory.GetDownPayment(workContract);

                //DailyHistory.GetTotals(dt, downPay);
                if (DailyHistory.isInsurance(workContract))
                {
                    double months = 0D;
                    imonths = 0;
                    DateTime dueDate8 = DateTime.Now;
                    DateTime datePaid = DateTime.Now;
                    bool first = true;

                    for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                    {
                        datePaid = dt.Rows[i]["payDate8"].ObjToDateTime();
                        if (datePaid < DailyHistory.killSecNatDate)
                        {
                            dueDate8 = dt.Rows[i]["dueDate8"].ObjToDateTime();
                            months = dt.Rows[i]["NumPayments"].ObjToDouble();
                            continue;
                        }
                        if (first)
                        {
                            first = false;
                            imonths = Convert.ToInt32((months));

                            dueDate8 = dueDate8.AddMonths(imonths);
                            dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(dueDate8);
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
                if (DailyHistory.isInsurance(workContract))
                    DailyHistory.LoadExpectedPremiums(dt, workPayer);
            }
            catch ( Exception ex)
            {
            }
            return dt;
        }
        /***********************************************************************************************/
        private void chkAll_CheckedChanged(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void chkActive_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;

            loading = true;

            chkLapsed.Checked = false;
            //chkAll.Checked = false;

            loading = false;

            DataTable dt = originalDt;

            CheckBox box = (CheckBox)sender;

            DataRow[] dRows = null;
            if (box.Checked)
            {
                dRows = dt.Select("status<>'Lapsed'");
                if (dRows.Length > 0)
                    dt = dRows.CopyToDataTable();
                else
                    dt = originalDt.Clone();
            }
            else
            {
                dt = originalDt;
            }

            if ( chkMismatch.Checked )
            {
                dRows = dt.Select("mismatch='Mismatch'");
                if (dRows.Length > 0)
                    dt = dRows.CopyToDataTable();
                else
                    dt = originalDt.Clone();
            }

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void chkLapsed_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;

            loading = true;

            chkActive.Checked = false;
            //chkAll.Checked = false;

            loading = false;

            DataTable dt = originalDt;

            CheckBox box = (CheckBox)sender;

            DataRow[] dRows = null;
            if (box.Checked)
            {
                dRows = dt.Select("status='Lapsed'");
                if (dRows.Length > 0)
                    dt = dRows.CopyToDataTable();
                else
                    dt = originalDt.Clone();
            }
            else
            {
                dt = originalDt;
            }

            if (chkMismatch.Checked)
            {
                dRows = dt.Select("mismatch='Mismatch'");
                if (dRows.Length > 0)
                    dt = dRows.CopyToDataTable();
                else
                    dt = originalDt.Clone();
            }


            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void chkMismatch_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;

            loading = false;

            DataTable dt = null;

            DataRow[] dRows = null;

            CheckBox box = (CheckBox)sender;

            if (box.Checked)
            {
                dRows = originalDt.Select("mismatch='Mismatch'");
                if (dRows.Length > 0)
                    dt = dRows.CopyToDataTable();
                else
                    dt = originalDt.Clone();
            }
            else
            {
                dt = originalDt;
            }

            if ( chkActive.Checked )
            {
                dRows = dt.Select("status<>'Lapsed'");
                if (dRows.Length > 0)
                    dt = dRows.CopyToDataTable();
                else
                    dt = originalDt.Clone();
            }
            else if ( chkLapsed.Checked )
            {
                dRows = dt.Select("status='Lapsed'");
                if (dRows.Length > 0)
                    dt = dRows.CopyToDataTable();
                else
                    dt = originalDt.Clone();
            }

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void fixDueDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contractNumber = dr["contractNumber"].ObjToString();
            DateTime newDueDate = dr["newDueDate"].ObjToDateTime();
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count > 0 )
            {
                string record = dx.Rows[0]["record"].ObjToString();
                G1.update_db_table("contracts", "record", record, new string[] { "dueDate8", newDueDate.ToString("yyyyMMdd")});
            }
        }
        /***********************************************************************************************/
    }
}