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
using DevExpress.XtraRichEdit.API.Native;
using DevExpress.XtraGrid.Views.Grid;
using MySql.Data.Types;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class BankTDPReport : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private DataTable originalDt = null;
        private DataTable bankDt = null;
        /****************************************************************************************/
        public BankTDPReport()
        {
            InitializeComponent();
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void BankTDPReport_Load(object sender, EventArgs e)
        {
            loading = false;

            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            now = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Value = now;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("tdp", null);
            AddSummaryColumn("trustDeposit", null);
            AddSummaryColumn("insuranceDeposit", null);
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
        private void AddSummaryColumn(string columnName, string format = "")
        {
            if (String.IsNullOrWhiteSpace(format))
                format = "${0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            SetSpyGlass(gridMain);
        }
        /***********************************************************************************************/
        private void SetSpyGlass(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            if (grid.OptionsFind.AlwaysVisible == true)
                grid.OptionsFind.AlwaysVisible = false;
            else
                grid.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            now = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Value = now;
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            now = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Value = now;
        }
        /****************************************************************************************/
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
        private bool CheckForManualPayments(string date1, string date2)
        {
            string cmd = "Select * from `payments` where `tmstamp` >= '" + date1 + "' AND `tmstamp` <= '" + date2 + "' AND `edited` = 'MANUAL' LIMIT 1;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                return true;
            cmd = "Select * from `ipayments` where `tmstamp` >= '" + date1 + "' AND `tmstamp` <= '" + date2 + "' AND `edited` = 'MANUAL' LIMIT 1;";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                return true;
            return false;
        }
        /***********************************************************************************************/
        private void CalculateGoodBadUgly(DataTable dt)
        {
            double total = 0D;
            double value = 0D;
            double badValue = 0D;
            double trustTotal = 0D;
            double insuranceTotal = 0D;
            double trustBad = 0D;
            double insuranceBad = 0D;
            double miscBad = 0D;
            string str = "";
            string cnum = "";
            string code = "";
            string location = "";
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    code = dt.Rows[i]["code"].ObjToString();
                    location = dt.Rows[i]["location"].ObjToString();
                    value = dt.Rows[i]["payment"].ObjToDouble();
                    total += value;
                    if (code != "01")
                    {
                        if (code == "02")
                        {
                            insuranceTotal += value;
                        }
                        else
                        {
                            trustTotal += value;
                        }
                    }
                    else
                    {
                        trustTotal += value;
                    }

                }
            }
            double totalTrust = trustTotal + trustBad;
            double totalInsurance = insuranceTotal + insuranceBad;
            double goodValue = total - badValue;
            gridBand1.Caption = "Total Trust : $" + G1.ReformatMoney(trustTotal + trustBad);
            gridBand4.Caption = "Total Ins   : $" + G1.ReformatMoney(insuranceTotal + insuranceBad);
            gridBand5.Caption = " Total Deposit : $" + G1.ReformatMoney(total);
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            LoadData();
        }
        /****************************************************************************************/
        private void ProcessDates(DataTable dt)
        {
            string filename = "";
            bool doit = false;
            string sDate = "";
            string contractNumber = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    doit = false;
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber == "ZZ0000534")
                    {
                    }
                    filename = dt.Rows[i]["filename"].ObjToString();
                    if (filename.EndsWith(".txt"))
                        doit = true;
                    if (!doit)
                    {
                        dt.Rows[i]["contractDate"] = G1.DTtoMySQLDT("0000-00-00");
                        continue;
                    }
                    filename = filename.Replace(".txt", "");
                    filename = filename.Replace("South_Mississippi_Funeral_Services_", "");
                    filename = filename.Replace("Correction", "");
                    filename = filename.Trim();
                    filename = decodeDate(filename);
                    if (!G1.validate_date(filename))
                    {
                        dt.Rows[i]["contractDate"] = G1.DTtoMySQLDT("0000-00-00");
                        continue;
                    }
                    sDate = filename;
                    if (G1.validate_date(sDate))
                        dt.Rows[i]["contractDate"] = G1.DTtoMySQLDT(sDate);
                    else
                        dt.Rows[i]["contractDate"] = G1.DTtoMySQLDT("0000-00-00");
                }
                catch (Exception ex)
                {
                }
            }
            DataView tempview = dt.DefaultView;
            tempview.Sort = "contractDate";
            dt = tempview.ToTable();
        }
        /****************************************************************************************/
        private string decodeDate(string sDate)
        {
            string date = "";
            string chr = "";
            for (int i = 0; i < sDate.Length; i++)
            {
                chr = sDate.Substring(i, 1);
                if (chr == "-")
                    date += chr;
                else if (Char.IsNumber(sDate, i))
                    date += chr;
            }
            return date;
        }
        /****************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;

            DateTime date = dateTimePicker1.Value;
            DateTime date1 = this.dateTimePicker1.Value;
            DateTime date2 = this.dateTimePicker2.Value;
            string startDate = date1.ToString("yyyy-MM-dd");
            string stopDate = date2.ToString("yyyy-MM-dd");
            string sdate1 = date1.ToString("yyyy-MM-dd") + " 00:00:00";
            string sdate2 = date2.ToString("yyyy-MM-dd") + " 23:59:59";
            string cmd = "Select * from `payments` p JOIN `customers` c ON p.`contractNumber` = c.`contractNumber` where `payDate8` >= '" + sdate1 + "' AND `payDate8` <= '" + sdate2 + "' ";
            //cmd += " AND p.`contractNumber` = 'HT20026L' ";
            cmd += " ORDER BY `payDate8`;";
            DataTable dt = G1.get_db_data(cmd);

            cmd = "Select * from `ipayments` p JOIN `icustomers` c ON p.`contractNumber` = c.`contractNumber` where `payDate8` >= '" + sdate1 + "' AND `payDate8` <= '" + sdate2 + "' ORDER BY `payDate8`;";
            DataTable ddx = G1.get_db_data(cmd);

            for (int i = 0; i < ddx.Rows.Count; i++)
            {
                dt.ImportRow(ddx.Rows[i]);
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = "payDate8";
            dt = tempview.ToTable();

            DataTable dx = new DataTable();
            dx.Columns.Add("num");
            dx.Columns.Add("date");
            dx.Columns.Add("depositNumber");
            dx.Columns.Add("account");
            dx.Columns.Add("tdp", Type.GetType("System.Double"));
            dx.Columns.Add("trustDeposit", Type.GetType("System.Double"));
            dx.Columns.Add("insuranceDeposit", Type.GetType("System.Double"));
            dx.Columns.Add("balance", Type.GetType("System.Double"));
            dx.Columns.Add("accountName");


            DataRow dRow = null;

            DateTime currentDate = DateTime.Now;

            double payment = 0D;
            double downPayment = 0D;
            string str = "";
            string tdp = "";
            string depositNumber = "";
            string contractNumber = "";
            string payer = "";
            string bankInfo = "";
            string accountName = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                currentDate = dt.Rows[i]["payDate8"].ObjToDateTime();
                if (currentDate.Year < 500)
                    continue;
                str = currentDate.ToString("MM/dd/yyyy");
                currentDate = str.ObjToDateTime();

                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();

                depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                tdp = dt.Rows[i]["tdp"].ObjToString();
                if (!String.IsNullOrWhiteSpace(tdp))
                    depositNumber = tdp;

                payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                payment = G1.RoundValue(payment);

                downPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                downPayment = G1.RoundValue(downPayment);

                bankInfo = dt.Rows[i]["bank_account"].ObjToString();
                accountName = GetBankDetails(bankInfo);

                if (DailyHistory.isInsurance(contractNumber))
                {
                    payer = dt.Rows[i]["payer1"].ObjToString();
                    dRow = dx.NewRow();
                    dRow["date"] = currentDate.ToString("MM/dd/yyyy");
                    dRow["depositNumber"] = depositNumber;
                    dRow["account"] = payer;
                    dRow["insuranceDeposit"] = payment;
                    dRow["balance"] = 0D;
                    dRow["accountName"] = accountName;
                    dx.Rows.Add(dRow);
                }
                else
                {
                    dRow = dx.NewRow();
                    dRow["date"] = currentDate.ToString("MM/dd/yyyy");
                    dRow["depositNumber"] = depositNumber;
                    dRow["account"] = contractNumber;
                    if (payment <= 0D && downPayment > 0)
                        dRow["tdp"] = downPayment;
                    else
                        dRow["trustDeposit"] = payment;
                    dRow["balance"] = 0D;
                    dRow["accountName"] = accountName;
                    dx.Rows.Add(dRow);
                }
            }
            G1.NumberDataTable(dx);
            originalDt = dx;
            dgv.DataSource = dx;

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private string GetBankDetails(string bankInfo)
        {
            string cmd = "";
            string accountName = "";
            if (bankDt == null)
            {
                cmd = "Select * from `bank_accounts`;";
                bankDt = G1.get_db_data(cmd);
            }
            if (String.IsNullOrWhiteSpace(bankInfo))
                return accountName;
            if (bankInfo.Trim().ToUpper() == "NONE")
                return accountName;
            string[] Lines = bankInfo.Split('~');
            if (Lines.Length < 3)
                return accountName;
            string glNum = Lines[1];
            string account_no = Lines[2];
            if (String.IsNullOrWhiteSpace(glNum) || String.IsNullOrWhiteSpace(account_no))
                return accountName;
            DataRow[] dR = bankDt.Select("general_ledger_no='" + glNum + "' and account_no = '" + account_no + "'");
            if (dR.Length <= 0)
                return accountName;
            accountName = dR[0]["account_title"].ObjToString();
            accountName = dR[0]["localDescription"].ObjToString();
            return accountName;
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string cnum = dr["account"].ObjToString();
            string cmd = "Select * from `customers` where `contractNumber` = '" + cnum + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0 )
            {
                cmd = "Select * from `icustomers` where `payer` = '" + cnum + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                    cnum = dt.Rows[0]["contractNumber"].ObjToString();
            }
            this.Cursor = Cursors.WaitCursor;
            CustomerDetails clientForm = new CustomerDetails(cnum);
            clientForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /****************************************************************************************/
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

            Printer.setupPrinterMargins(50, 50, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();

            G1.AdjustColumnWidths(gridMain, 0.65D, false);
        }
        /****************************************************************************************/
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

            Printer.setupPrinterMargins(50, 50, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();

            G1.AdjustColumnWidths(gridMain, 0.65D, false);
        }
        /***********************************************************************************************/
        private void printableComponentLink1_BeforeCreateAreas(object sender, EventArgs e)
        {
            footerCount = 0;
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
            string report = "Deposit Report for " + this.dateTimePicker1.Value.ToString("MM/dd/yyyy") + " through " + this.dateTimePicker2.Value.ToString("MM/dd/yyyy");
            Printer.DrawQuad(5, 8, 8, 4, report, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //font = new Font("Ariel", 8, FontStyle.Regular);
            //report = cmbWhat.Text + " / " + cmbWho.Text + " / " + cmbDeposits.Text;
            //Printer.DrawQuad(10, 8, 2, 4, report, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            //            Printer.DrawQuadTicks();
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
                if (footerCount >= 2)
                {
                    footerCount = 0;
                    e.PS.InsertPageBreak(e.Y);
                }
            }
        }
        /****************************************************************************************/
        //private double originalSize = 0D;
        //private Font mainFont = null;
        /****************************************************************************************/
        private void deleteDepositToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (1 == 1)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to delete this Row ?", "Delete Row Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            int[] rows = gridMain.GetSelectedRows();
            int dtRow = 0;
            int firstRow = 0;
            if (rows.Length > 0)
                firstRow = rows[0];
            int row = 0;
            try
            {
                loading = true;
                for (int i = 0; i < rows.Length; i++)
                {
                    row = rows[i];
                    dtRow = gridMain.GetDataSourceRowIndex(row);
                    if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
                    {
                        continue;
                    }
                    dt.Rows.RemoveAt(dtRow);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("*ERROR*** " + ex.Message.ToString());
            }

            loading = false;
            if (firstRow > (dt.Rows.Count - 1))
                firstRow = (dt.Rows.Count - 1);
            dgv.DataSource = dt;
            gridMain.RefreshData();
            dgv.Refresh();

            gridMain.FocusedRowHandle = firstRow;
            gridMain.SelectRow(firstRow);
        }
        /****************************************************************************************/
        private void chkGroupBank_CheckedChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (chkGroupBank.Checked)
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "accountName";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

                gridMain.Columns["accountName"].GroupIndex = 0;
                gridMain.OptionsView.ShowFooter = true;
                this.gridMain.ExpandAllGroups();
            }
            else
            {
//                G1.NumberDataTable(dt);
//                dgv.DataSource = dt;
                gridMain.Columns["accountName"].GroupIndex = -1;
                gridMain.OptionsView.ShowFooter = true;
                gridMain.CollapseAllGroups();

                dt = originalDt;
                if (G1.get_column_number(dt, "Int32_num") < 0)
                    dt.Columns.Add("Int32_num", typeof(int), "num");

                DataView tempview = dt.DefaultView;
                tempview.Sort = "Int32_num";
                dt = tempview.ToTable();
                dt.Columns.Remove("Int32_num");
                dgv.DataSource = dt;
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /****************************************************************************************/
    }
}