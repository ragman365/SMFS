using DevExpress.Utils;
using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using GeneralLib;
using MyXtraGrid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class DailyPayments : DevExpress.XtraEditors.XtraForm
    {
        private DataTable groupContracts = null;
        private DataTable paymentDetail = null;
        private string bankDetails = "";
        private double beginningBalance = 0D;
        private string workReport = "";
        /****************************************************************************************/
        public DailyPayments()
        {
            InitializeComponent();
            SetupTotalsSummary();
            workReport = "";

        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("amount", gridMain);
            AddSummaryColumn("debit", gridMain);
            AddSummaryColumn("inSystem", gridMain);
            AddSummaryColumn("diff", gridMain);
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
        private void DailyPayments_Load(object sender, EventArgs e)
        {
            //PleaseWait pleaseForm = null;
            //pleaseForm = new PleaseWait("Please Wait!\nLoading Information");
            //pleaseForm.Show();
            //pleaseForm.Refresh();

            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);

            int days = DateTime.DaysInMonth(now.Year, now.Month);
            DateTime stop = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker1.Value = stop;

            this.Cursor = Cursors.WaitCursor;

            ScaleCells();

            this.Cursor = Cursors.Default;

            //pleaseForm.FireEvent1();
            //pleaseForm.Dispose();
            //pleaseForm = null;
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddDays(1);
            this.dateTimePicker1.Value = now;
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddDays(-1);
            this.dateTimePicker1.Value = now;
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
            printableComponentLink1.Landscape = true;

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
            string title = "Daily Payments (All Sources) for " + this.dateTimePicker1.Value.ToString("MM/dd/yyyy");
            Printer.DrawQuad(5, 7, 5, 3, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
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
            if (workReport != "Funeral Detail Report")
                return;
            if (e.HasFooter)
            {
                footerCount++;
            }
        }
        /****************************************************************************************/
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (workReport != "Funeral Detail Report")
                return;
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
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            if (this.gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            //int days = DateTime.DaysInMonth(date.Year, date.Month);
            //date = new DateTime(date.Year, date.Month, days);
            //this.dateTimePicker1.Value = date;
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
                originalSize = gridMain.Columns["bankDetails"].AppearanceCell.Font.Size;
                mainFont = gridMain.Columns["bankDetails"].AppearanceCell.Font;
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
        private void btnGetDeposits_Click(object sender, EventArgs e)
        {
            LoadData();
        }
        /***********************************************************************************************/
        private void LoadData ()
        {
            string cmd = "Select * from `bank_details` WHERE `bankAccount` = 'Xyzzy55';";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("bankDetails");
            dt.Columns.Add("found");
            dt.Columns.Add("ID");
            dt.Columns.Add("sDate");
            dt.Columns.Add("inSystem", Type.GetType("System.Double"));
            dt.Columns.Add("diff", Type.GetType("System.Double"));
            dt.Columns.Add("depositNumber");


            string account = "";
            string saveAccount = account;
            string bankAccount = account;

            string account2 = bankAccount;
            string account3 = bankAccount;
            string account4 = bankAccount;

            cmd = "Select * from `bank_accounts`;";
            DataTable bankDt = G1.get_db_data(cmd);
            //if (bankDt.Rows.Count > 0)
            //{
            //    account = bankDt.Rows[0]["location"].ObjToString() + "~" + bankDt.Rows[0]["general_ledger_no"].ObjToString() + "~" + bankDt.Rows[0]["account_no"].ObjToString();

            //    account2 = bankDt.Rows[0]["account_title"].ObjToString() + "~" + bankDt.Rows[0]["general_ledger_no"].ObjToString() + "~" + bankDt.Rows[0]["account_no"].ObjToString();
            //    account3 = bankDt.Rows[0]["localDescription"].ObjToString() + "~" + bankDt.Rows[0]["general_ledger_no"].ObjToString() + "~" + bankDt.Rows[0]["account_no"].ObjToString();
            //    //dt.Rows[i]["bankDetails"] = dRows[0]["account_title"].ObjToString() + " " + account;

            //}


            double systemAmount = 0D;
            double amount = 0D;
            double diff = 0D;

            string contractNumber = "";

            this.Cursor = Cursors.WaitCursor;

            DateTime date = this.dateTimePicker1.Value;
            string date1 = date.ToString("yyyy-MM-dd");

            string[] Lines = null;
            DataRow[] dRows = null;
            DataRow dRow = null;
            string bankDetails = "";
            string dateStr = "";

            try
            {
                cmd = "Select * from `ipayments` p LEFT JOIN `icustomers` f ON p.`contractNumber` = f.`contractNumber` WHERE `payDate8` = '" + date1 + "' ORDER BY `payDate8` asc;";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    Lines = null;

                    for (int i = 0; i < dx.Rows.Count; i++)
                    {
                        date = dx.Rows[i]["payDate8"].ObjToDateTime();
                        dateStr = date.ToString("yyyyMMdd");

                        amount = dx.Rows[i]["paymentAmount"].ObjToDouble();
                        if (amount <= 0D)
                            continue;
                        bankDetails = dx.Rows[i]["bank_account"].ObjToString();
                        bankAccount = bankDetails;
                        Lines = bankDetails.Split('~');
                        if (Lines.Length >= 3)
                            bankDetails = Lines[2].Trim();

                        dRow = dt.NewRow();
                        //dRow["date"] = G1.DTtoMySQLDT(searchDate);
                        dRow["date"] = G1.DTtoMySQLDT(date);
                        dRow["amount"] = amount;
                        dRow["bankAccount"] = bankDetails;
                        dRow["bankDetails"] = dx.Rows[i]["bank_account"].ObjToString();
                        dRow["ID"] = dx.Rows[i]["payer1"].ObjToString();
                        dRow["depositNumber"] = dx.Rows[i]["depositNumber"].ObjToString();
                        dRow["found"] = "Insurance";
                        dRow["sDate"] = date.ToString("yyyyMMdd");
                        dt.Rows.Add(dRow);
                    }
                }

                double downPayment = 0D;

                cmd = "Select * from `payments` p LEFT JOIN `customers` f ON p.`contractNumber` = f.`contractNumber` WHERE `payDate8` = '" + date1 + "' ORDER BY `payDate8` asc;";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    for (int i = 0; i < dx.Rows.Count; i++)
                    {
                        date = dx.Rows[i]["payDate8"].ObjToDateTime();
                        dateStr = date.ToString("yyyyMMdd");

                        amount = DailyHistory.getPayment(dx, i);
                        downPayment = DailyHistory.getDownPayment(dx, i);

                        if (amount <= 0D && downPayment <= 0D)
                            continue;

                        if (downPayment > 0D)
                            amount = downPayment;
                        bankDetails = dx.Rows[i]["bank_account"].ObjToString();
                        bankAccount = bankDetails;
                        Lines = bankDetails.Split('~');
                        if (Lines.Length >= 3)
                            bankDetails = Lines[2].Trim();


                        dRow = dt.NewRow();
                        //dRow["date"] = G1.DTtoMySQLDT(searchDate);
                        dRow["date"] = G1.DTtoMySQLDT(date);
                        dRow["amount"] = amount;
                        dRow["bankAccount"] = bankDetails;
                        dRow["bankDetails"] = dx.Rows[i]["bank_account"].ObjToString();
                        contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                        dRow["ID"] = dx.Rows[i]["contractNumber"].ObjToString();
                        dRow["depositNumber"] = dx.Rows[i]["depositNumber"].ObjToString();
                        dRow["found"] = "Trust";
                        if (downPayment > 0D)
                            dRow["found"] = "Down Payment";
                        dRow["sDate"] = date.ToString("yyyyMMdd");
                        dt.Rows.Add(dRow);
                    }
                }

                string depositNumber = "";
                cmd = "Select * from `cust_payment_details` c JOIN `fcust_extended` f ON c.`contractNumber` = f.`contractNumber` WHERE `dateReceived` = '" + date1 + "' AND `status` = 'DEPOSITED' ;";
                dx = G1.get_db_data(cmd);
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    date = dx.Rows[i]["dateReceived"].ObjToDateTime();
                    dateStr = date.ToString("yyyyMMdd");
                    amount = dx.Rows[i]["amtActuallyReceived"].ObjToDouble();
                    if (amount <= 0D)
                        amount = dx.Rows[i]["paid"].ObjToDouble();
                    bankDetails = dx.Rows[i]["bankAccount"].ObjToString();
                    bankAccount = bankDetails;
                    depositNumber = dx.Rows[i]["depositNumber"].ObjToString();
                    if (depositNumber.ToUpper().IndexOf("TD") == 0 || depositNumber.ToUpper().IndexOf("CCTD") == 0 )
                    {
                        dRows = dt.Select("depositNumber='" + depositNumber + "' AND found = 'Down Payment' AND sDate = '" + dateStr + "'");
                        if (dRows.Length > 0)
                            continue;
                    }

                    dRow = dt.NewRow();
                    dRow["date"] = G1.DTtoMySQLDT(date);
                    dRow["amount"] = amount;
                    dRow["bankAccount"] = bankDetails;
                    dRows = bankDt.Select("account_no='" + bankAccount + "'");

                    //    account = bankDt.Rows[0]["location"].ObjToString() + "~" + bankDt.Rows[0]["general_ledger_no"].ObjToString() + "~" + bankDt.Rows[0]["account_no"].ObjToString();

                    //    account2 = bankDt.Rows[0]["account_title"].ObjToString() + "~" + bankDt.Rows[0]["general_ledger_no"].ObjToString() + "~" + bankDt.Rows[0]["account_no"].ObjToString();
                    //    account3 = bankDt.Rows[0]["localDescription"].ObjToString() + "~" + bankDt.Rows[0]["general_ledger_no"].ObjToString() + "~" + bankDt.Rows[0]["account_no"].ObjToString();

                    if ( dRows.Length > 0 )
                        dRow["bankDetails"] = dRows[0]["localDescription"].ObjToString() + "~" + dRows[0]["general_ledger_no"].ObjToString() + "~" + dRows[0]["account_no"].ObjToString();
                    dRow["ID"] = dx.Rows[i]["serviceId"].ObjToString();
                    dRow["depositNumber"] = dx.Rows[i]["depositNumber"].ObjToString();
                    dRow["found"] = "Funeral";
                    dRow["sDate"] = date.ToString("yyyyMMdd");
                    dt.Rows.Add(dRow);
                }
            }
            catch ( Exception ex)
            {
            }

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.Refresh();

            this.Cursor = Cursors.Default;
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
        private void chkGroupFound_CheckedChanged(object sender, EventArgs e)
        {
            if (chkGroupFound.Checked)
            {
                gridMain.Columns["found"].GroupIndex = 0;
                gridMain.Columns["found"].Visible = false;
                gridMain.RefreshEditor(true);
                gridMain.ExpandAllGroups();
                if (chkGroupBank.Checked)
                    chkGroupBank.Checked = false;
            }
            else
            {
                gridMain.Columns["found"].GroupIndex = -1;
                gridMain.Columns["found"].Visible = true;
                gridMain.RefreshEditor(true);
            }
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void chkGroupBank_CheckedChanged(object sender, EventArgs e)
        {
            if (chkGroupBank.Checked )
            {
                gridMain.Columns["bankAccount"].GroupIndex = 0;
                gridMain.Columns["bankAccount"].Visible = false;
                gridMain.RefreshEditor(true);
                gridMain.ExpandAllGroups();
                if (chkGroupFound.Checked)
                    chkGroupFound.Checked = false;
            }
            else
            {
                gridMain.Columns["bankAccount"].GroupIndex = -1;
                gridMain.Columns["bankAccount"].Visible = true;
                gridMain.RefreshEditor(true);
            }
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();
            string found = dr["found"].ObjToString().ToUpper();
            string what = dr["ID"].ObjToString();
            string depositNumber = dr["depositNumber"].ObjToString();
            //string lastName = dr["lastName"].ObjToString();
            //string firstName = dr["firstName"].ObjToString();
            DateTime localDate = dr["date"].ObjToDateTime();
            if (found == "INSURANCE")
            {
                string cmd = "Select * from `payers` where `payer` = '" + what + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    what = dx.Rows[0]["contractNumber"].ObjToString();
                    this.Cursor = Cursors.WaitCursor;
                    if (!String.IsNullOrWhiteSpace(what))
                    {
                        CustomerDetails detailsForm = new CustomerDetails(what);
                        detailsForm.TopMost = true;
                        detailsForm.Show();
                    }
                    this.Cursor = Cursors.Default;
                }
            }
            else if (found == "TRUST" || found == "DOWN PAYMENT" )
            {
                this.Cursor = Cursors.WaitCursor;
                if (!String.IsNullOrWhiteSpace(what))
                {
                    CustomerDetails detailsForm = new CustomerDetails(what);
                    detailsForm.TopMost = true;
                    detailsForm.Show();
                }
                this.Cursor = Cursors.Default;
            }
            else if (found == "FUNERAL")
            {
                if (!String.IsNullOrWhiteSpace(what))
                {
                    string cmd = "Select * from `fcust_extended` WHERE `serviceId` = '" + what + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        string contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                        FunPayments editFunPayments = new FunPayments(this, contractNumber, "", false, false);
                        editFunPayments.TopMost = true;
                        editFunPayments.Show();
                        this.Cursor = Cursors.Default;
                    }
                }
            }
            //else if (found == "DOWN PAYMENT")
            //{
            //    if (!String.IsNullOrWhiteSpace(what))
            //    {
            //        string cmd = "Select * from `downpayments` where `date` = '" + localDate.ToString("yyyy-MM-dd") + "' AND `depositNumber` = '" + what + "' LIMIT 10;";
            //        DataTable dx = G1.get_db_data(cmd);
            //        if (dx.Rows.Count > 0)
            //        {
            //            this.Cursor = Cursors.WaitCursor;
            //            DownPayments dpForm = new DownPayments(dx);
            //            dpForm.TopMost = true;
            //            dpForm.Show();
            //            this.Cursor = Cursors.Default;
            //        }
            //        else
            //        {
            //            MessageBox.Show("*** ERROR *** Cannot find any Down Payments for :\n" + what + "!!!", "Down Payment Lookup Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //        }
            //    }
            //}
        }
        /****************************************************************************************/
    }
}