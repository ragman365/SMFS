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
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class BankPayments : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        DataTable originalDt = null;
        private bool loading = false;
        private string workTitle = "";
        public static DataTable balanceDt = null;
        /***********************************************************************************************/
        public BankPayments( string title = "")
        {
            InitializeComponent();
            workTitle = title;
        }
        /***********************************************************************************************/
        private void BankPayments_Load(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);

            G1.SetupVisibleColumns(gridMain, this.columnsToolStripMenuItem, nmenu_Click);
            SetupTotalsSummary();
            LoadData();
            //gridMain.Columns["downPayment"].Visible = false;

            if ( !String.IsNullOrWhiteSpace ( workTitle ))
                this.Text = workTitle;
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
        /***********************************************************************************************/
        private void loadLocatons()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);
            chkComboLocNames.Properties.DataSource = locDt;
            chkComboLocation.Properties.DataSource = locDt;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("paymentAmount", gridMain);
            AddSummaryColumn("debitAdjustment", gridMain);
            AddSummaryColumn("creditAdjustment", gridMain);
            AddSummaryColumn("downPayment", gridMain);
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

            Printer.setupPrinterMargins(50, 100, 130, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            G1.PrintPreview(printableComponentLink1, gridMain);

            //printableComponentLink1.CreateDocument();
            //printableComponentLink1.ShowPreviewDialog();

            AutoRunPaidOutReport();
        }
        /***********************************************************************************************/
        private void AutoRunPaidOutReport()
        {
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to Run the Paid-Out Report Now ?", "Run Paid-Out Report Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            PaymentsReport paymentForm = new PaymentsReport( "Paid Up Contracts Report", "Trust Paid Off Contracts (2.0)");
            paymentForm.Show();
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

            Printer.setupPrinterMargins(50, 100, 130, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

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

            font = new Font("Ariel", 10, FontStyle.Bold);
            string title = "Trust Bank Payments Report";
            Printer.DrawQuad(6, 7, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            DateTime date = this.dateTimePicker2.Value;
            string workDate = date.ToString("MM/dd/yyyy");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            title = "Month Ending: ";
            string lock1 = this.dateTimePicker1.Value.ToString("MM/dd/yyyy");
            string lock2 = this.dateTimePicker2.Value.ToString("MM/dd/yyyy");
            Printer.DrawQuad(20, 3, 5, 4, "Stop " + lock2, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(20, 1, 5, 4, "Start " + lock1, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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
                if (footerCount >= 2)
                {
                    footerCount = 0;
//                    e.PS.InsertPageBreak(e.Y);
                }
            }
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
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
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            DateTime date = dateTimePicker1.Value;
            DateTime beginningDate = date;
            DateTime oldImportDate = DailyHistory.majorDate;

            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            string date2 = G1.DateTimeToSQLDateTime(date);

            DateTime now = DateTime.Now;

            DateTime paidout = new DateTime(2039, 12, 31);

            string paidDate = "`payDate8` >= 'XYZZY1' and `payDate8` <= 'XYZZY2' ";

            string cmd = "Select p.*,d.*,a.`firstName`,a.`lastName` from `payments` p LEFT JOIN `contracts` d on p.`contractNumber` = d.`contractNumber` LEFT JOIN `customers` a ON p.`contractNumber` = a.`contractNumber` ";
            cmd += " WHERE ";
            cmd += paidDate;
            string saveDate = cmd;
            cmd += " AND `edited` <> 'TRUSTADJ' ";
            cmd += ";";

            string saveCmd = cmd;
            cmd = cmd.Replace("XYZZY1", date1);
            cmd = cmd.Replace("XYZZY2", date2);

            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("paymentType");
            dt.Columns.Add("bankDetail");

            DailyHistory.RemoveDeletedPayments(dt);

            DateTime payDate8 = DateTime.Now;

            double debit = 0D;
            double credit = 0D;

            dt.Columns.Add("num");
            dt.Columns.Add("customer");
            if (G1.get_column_number(dt, "retained") < 0)
                dt.Columns.Add("retained", Type.GetType("System.Double"));
            dt.Columns.Add("monthsPaid", Type.GetType("System.Double"));
            dt.Columns.Add("loc");
            dt.Columns.Add("Location Name");

            //LoadOtherCombos(dt);

            DataTable funDt = G1.get_db_data("Select * from `funeralhomes`;");

            LoadFunerals(dt, date1, date2);

            double payment = 0D;
            double downPayment = 0D;
            double trust100P = 0D;
            double trust85P = 0D;
            double interest = 0D;
            debit = 0D;
            credit = 0D;
            double retained = 0D;
            double months = 0D;
            double expected = 0D;
            string fname = "";
            string lname = "";
            string name = "";
            string contractNumber = "";
            string miniContract = "";
            string trust = "";
            string loc = "";
            string depositNumber = "";
            string depositLocation = "";
            bool manual = true;
            string edited = "";
            double saveRetained = 0D;
            double downpayment = 0D;
            bool calculateTrust100 = false;

            double amtOfMonthlyPayt = 0D;
            double contractValue = 0D;
            double originalDownPayment = 0D;
            double financeMonths = 0D;
            double amtPaid = 0D;
            double rate = 0D;
            double trust85 = 0D;
            double trust100 = 0D;
            double principal = 0D;
            payDate8 = DateTime.Now;
            DateTime oldIssueDate = DateTime.Now;
            int method = 0;
            string lockTrust85 = "";

            DataTable dm = null;

            string[] Lines = null;
            string bankAccount = "";
            DataRow[] dRows = null;
            string fill1 = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                bankAccount = dt.Rows[i]["bank_account"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( bankAccount))
                {
                    dt.Rows[i]["bankDetail"] = bankAccount;
                    Lines = bankAccount.Split('~');
                    if (Lines.Length >= 3)
                        dt.Rows[i]["bank_account"] = Lines[2].Trim();
                }
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                downpayment = dt.Rows[i]["downPayment"].ObjToDouble();
                downpayment = G1.RoundValue(downpayment);

                payDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();
                edited = dt.Rows[i]["edited"].ObjToString();

                fname = dt.Rows[i]["firstName"].ObjToString();
                lname = dt.Rows[i]["lastName"].ObjToString();
                name = fname + " " + lname;
                dt.Rows[i]["customer"] = name;
                payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();

                trust85 = dt.Rows[i]["trust85P"].ObjToDouble();
                trust100 = dt.Rows[i]["trust100P"].ObjToDouble();
                lockTrust85 = dt.Rows[i]["lockTrust85"].ObjToString().ToUpper();

                payment = G1.RoundValue(payment);
                debit = G1.RoundValue(debit);
                credit = G1.RoundValue(credit);

                miniContract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                if (DailyHistory.isInsurance(contractNumber))
                {
                    cmd = "Select * from `cust_extended` where `contractNumber` = '" + contractNumber + "';";
                    dm = G1.get_db_data(cmd);
                    if (dm.Rows.Count > 0)
                        loc = dm.Rows[0]["serviceLoc"].ObjToString();
                }
                dt.Rows[i]["loc"] = loc;
                if (funDt.Rows.Count > 0 && !String.IsNullOrWhiteSpace(loc))
                {
                    DataRow[] dr = funDt.Select("keycode='" + loc + "'");
                    if (dr.Length > 0)
                        dt.Rows[i]["Location Name"] = dr[0]["name"].ObjToString();
                }
                depositLocation = dt.Rows[i]["location"].ObjToString().ToUpper();
                dt.Rows[i]["location"] = depositLocation;
                manual = false;
                edited = dt.Rows[i]["edited"].ObjToString();
                if (edited.Trim().ToUpper() == "MANUAL" || edited.Trim().ToUpper() == "TRUSTADJ" || edited.Trim().ToUpper() == "CEMETERY" )
                    manual = true;
                depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( depositNumber) && !manual )
                {
                    depositLocation = depositNumber.Substring(0, 1).ToUpper();
                    if (depositLocation == "T")
                    {
                        dt.Rows[i]["location"] = "LK";
                        fill1 = dt.Rows[i]["fill1"].ObjToString().ToUpper();
                        if (fill1 == "TFBX")
                            dt.Rows[i]["location"] = "TFBX";
                    }
                    else if (depositLocation == "A")
                        dt.Rows[i]["location"] = "ACH";
                    else if (depositLocation == "C")
                        dt.Rows[i]["location"] = "CC";
                    if (chkCombineHO.Checked)
                    {
                        loc = dt.Rows[i]["location"].ObjToString();
                        if ( loc.Trim().ToUpper() == "HOCC" )
                            dt.Rows[i]["location"] = "HO";
                    }
                    if (manual)
                        dt.Rows[i]["depositNumber"] = dt.Rows[i]["userId"].ObjToString();
                }
                if (chkCombineHO.Checked)
                {
                    loc = dt.Rows[i]["location"].ObjToString();
                    if (loc.Trim().ToUpper().IndexOf ("HO") >= 0 )
                        dt.Rows[i]["location"] = "HO";
                }

                try
                {
                    payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    expected = dt.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                    months = 0D;
                    if (expected > 0D)
                        months = payment / expected;
                    dt.Rows[i]["monthsPaid"] = Math.Truncate(months);

                }
                catch
                {

                }
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = "payDate8 asc";
            dt = tempview.ToTable();

            LoadPaymentTypes(dt);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            dgv.RefreshDataSource();
            gridMain.RefreshData();
            dgv.Refresh();
            this.Refresh();

            originalDt = dt;
            balanceDt = dt;
            this.Cursor = Cursors.Default;
        }
        /*******************************************************************************************/
        private void LoadFunerals ( DataTable dt, string date1, string date2 )
        {
            DataRow dR = null;
            DataRow[] dRows = null;
            DataTable dm = null;

            string contractNumber = "";

            DataTable bankDt = G1.get_db_data("Select * from `bank_accounts`;");

            string cmd = "Select p.*,d.*,a.`firstName`,a.`lastName` from `cust_payments` p LEFT JOIN `contracts` d on p.`contractNumber` = d.`contractNumber` LEFT JOIN `customers` a ON p.`contractNumber` = a.`contractNumber` ";

            cmd += " WHERE p.`status` = 'accept' AND p.`dateModified`>= '" + date1 + "' AND `dateModified` <= '" + date2 + "' AND `bankAccount` <> '' ";

            DataTable dx = G1.get_db_data(cmd);
            string bankAccount = "";
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                bankAccount = dx.Rows[i]["bankAccount"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( bankAccount))
                {
                    dRows = bankDt.Select("`account_no`='" + bankAccount + "'");
                    if (dRows.Length > 0)
                    {
                        bankAccount = dRows[0]["location"] + "~" + dRows[0]["general_ledger_no"].ObjToString() + "~" + dRows[0]["account_no"].ObjToString();
                    }
                }
                dR = dt.NewRow();
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                dR["contractNumber"] = contractNumber;
                if ( DailyHistory.isInsurance ( contractNumber ) )
                {
                    cmd = "Select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
                    dm = G1.get_db_data(cmd);
                    if ( dm.Rows.Count > 0 )
                    {
                        dx.Rows[i]["firstName"] = dm.Rows[0]["firstName"].ObjToString();
                        dx.Rows[i]["lastName"] = dm.Rows[0]["lastName"].ObjToString();
                    }
                }
                dR["payDate8"] = G1.DTtoMySQLDT(dx.Rows[i]["dateModified"].ObjToDateTime());
                dR["paymentAmount"] = dx.Rows[i]["payment"].ObjToDouble();
                dR["bank_account"] = bankAccount;
                dR["firstName1"] = dx.Rows[i]["firstName"].ObjToString();
                dR["lastName1"] = dx.Rows[i]["lastName"].ObjToString();
                dR["firstName"] = dx.Rows[i]["firstName"].ObjToString();
                dR["lastName"] = dx.Rows[i]["lastName"].ObjToString();
                dR["location"] = "FUNERAL";
                dt.Rows.Add(dR);
            }
        }
        /*******************************************************************************************/
        private void LoadPaymentTypes ( DataTable dt)
        {
            string paymentType = "";
            double debit = 0D;
            double credit = 0D;
            string trustAdj = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                dt.Rows[i]["paymentType"] = "Payment";
                trustAdj = dt.Rows[i]["edited"].ObjToString();
                if (trustAdj.Trim().ToUpper() == "TRUSTADJ")
                    dt.Rows[i]["paymentType"] = "Trust Adj";
                else
                {
                    debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    if (debit != 0D || credit != 0D)
                        dt.Rows[i]["paymentType"] = "Debit/Credit";
                }
            }
        }
        /*******************************************************************************************/
        private void ProcessManualPayments ( DataTable dt, DataTable dx )
        {
            DataRow [] dR = null;
            string contractNumber = "";
            DateTime payDate8 = DateTime.Now;
            DateTime payDate1 = DateTime.Now;
            double payment = 0D;
            double pay1 = 0D;
            string record1 = "";
            string record2 = "";
            string edited1 = "";
            string edited2 = "";
            string deposit1 = "";
            string deposit2 = "";

            for ( int i=0; i<dx.Rows.Count; i++)
            {
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                if ( contractNumber == "CT19006LI")
                {

                }
                payDate8 = dx.Rows[i]["payDate8"].ObjToDateTime();
                payment = dx.Rows[i]["paymentAmount"].ObjToDouble();
                record1 = dx.Rows[i]["record"].ObjToString();
                edited1 = dx.Rows[i]["edited"].ObjToString();
                deposit1 = dx.Rows[i]["depositNumber"].ObjToString();

                dR = dt.Select("contractNumber='" + contractNumber + "'");
                for ( int k=0; k<dR.Length; k++)
                {
                    record2 = dR[k]["record"].ObjToString();
                    if (record1 != record2)
                    {
                        edited2 = dR[k]["edited"].ObjToString();
                        if (edited1 == edited2)
                        {
                            deposit2 = dR[k]["depositNumber"].ObjToString();
                            if (deposit1 == deposit2)
                            {
                                pay1 = dR[k]["paymentAmount"].ObjToDouble();
                                payDate1 = dR[k]["payDate8"].ObjToDateTime();
                                if (pay1 == payment && payDate1 == payDate8)
                                    dx.Rows[i]["contractNumber"] = "";
                            }
                        }
                    }
                    else
                        dx.Rows[i]["contractNumber"] = "";
                }
            }
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( contractNumber))
                    dt.ImportRow(dx.Rows[i]);
            }

        }
        /*******************************************************************************************/
        private void SplitLocation ( DataTable dt )
        {
            string user = "";
            string location = "";
            string str = "";
            for ( int i=(dt.Rows.Count-1); i>= 0; i--)
            {
                str = dt.Rows[i]["location"].ObjToString();
                if (str.ToUpper() == "DWNPA")
                {
                    dt.Rows.RemoveAt(i);
                    continue;
                }
                //if (str.ToUpper() == "ACH")
                //{
                //    dt.Rows.RemoveAt(i);
                //    continue;
                //}
                if ( str.Length >= 2)
                {
                    user = str.Substring(2);
                    location = str.Substring(0, 2);
                    dt.Rows[i]["location"] = location;
                    dt.Rows[i]["userId"] = user;
                }
            }
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
            string contract = "";
            string trust = "";
            string loc = "";
            string c = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contract = dt.Rows[i]["contractNumber"].ObjToString();
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
            }
        }
        /***********************************************************************************************/
        private void chkComboLocation_EditValueChanged(object sender, EventArgs e)
        {
            string names = getLocationQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void chkComboLocNames_EditValueChanged(object sender, EventArgs e)
        {
            string names = getLocationNameQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
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
                }
            }
            return procLoc.Length > 0 ? " `loc` IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            DateTime date = this.dateTimePicker1.Value;
//            GetWeeklyDate(date);
        }
        /***********************************************************************************************/
        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            DateTime date = this.dateTimePicker2.Value;
//            GetWeeklyDate(date);
        }
        /***********************************************************************************************/
        private void GetWeeklyDate(DateTime date)
        {
            loading = true;
            DateTime idate = date;
            if (idate.DayOfWeek == DayOfWeek.Friday)
            {
                this.dateTimePicker1.Value = date;
                this.dateTimePicker2.Value = date.AddDays(6);
                return;
            }
            for (;;)
            {
                idate = idate.AddDays(-1);
                if (idate.DayOfWeek == DayOfWeek.Friday)
                {
                    this.dateTimePicker1.Value = idate;
                    this.dateTimePicker2.Value = idate.AddDays(6);
                    break;
                }
            }
            loading = false;
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                DataTable dt = (DataTable)dgv.DataSource;
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void chkSort_CheckedChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (chkSort.Checked)
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "location asc, payDate8 asc";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

                gridMain.Columns["location"].GroupIndex = 0;
                gridMain.Columns["customer"].Visible = false;
                gridMain.Columns["lastName"].Visible = true;
                gridMain.Columns["firstName"].Visible = true;
                gridMain.OptionsView.ShowFooter = true;
                this.gridMain.ExpandAllGroups();
            }
            else
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "userId asc, payDate8 asc";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

                gridMain.Columns["location"].GroupIndex = -1;
                gridMain.Columns["customer"].Visible = true;
                gridMain.Columns["lastName"].Visible = false;
                gridMain.Columns["firstName"].Visible = false;
                gridMain.OptionsView.ShowFooter = true;
                gridMain.CollapseAllGroups();
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (!chkIncludeDownPayments.Checked)
            {
                double downPayment = dt.Rows[row]["downPayment"].ObjToDouble();
                if ( downPayment > 0D )
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
        }
        /***********************************************************************************************/
        private void chkIncludeDownPayments_CheckedChanged(object sender, EventArgs e)
        {
            if (chkIncludeDownPayments.Checked)
            {
                gridMain.Columns["downPayment"].Visible = true;
                gridMain.RefreshData();
                dgv.Refresh();
            }
            else
            {
                gridMain.Columns["downPayment"].Visible = false;
                gridMain.RefreshData();
                dgv.Refresh();
            }
        }
        /***********************************************************************************************/
        private void chkDeposits_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDeposits.Checked)
            {
                gridMain.Columns["location"].GroupIndex = 0;
                gridMain.Columns["depositNumber"].GroupIndex = 1;
                gridMain.OptionsBehavior.AutoExpandAllGroups = true;
                gridMain.ExpandAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = true;
                gridMain.OptionsPrint.PrintGroupFooter = true;
            }
            else
            {
                gridMain.Columns["location"].GroupIndex = -1;
                gridMain.Columns["depositNumber"].GroupIndex = -1;
                gridMain.OptionsBehavior.AutoExpandAllGroups = false;
                gridMain.CollapseAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = false;
                gridMain.OptionsPrint.PrintGroupFooter = true;
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void chkExpand_CheckedChanged(object sender, EventArgs e)
        {
            if (chkExpand.Checked)
            {
                gridMain.OptionsBehavior.AutoExpandAllGroups = true;
                gridMain.ExpandAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = true;
                gridMain.OptionsPrint.PrintGroupFooter = true;
                gridMain.OptionsView.GroupFooterShowMode = DevExpress.XtraGrid.Views.Grid.GroupFooterShowMode.VisibleAlways;
            }
            else
            {
                gridMain.OptionsBehavior.AutoExpandAllGroups = false;
                gridMain.CollapseAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = false;
                gridMain.OptionsPrint.PrintGroupFooter = true;
                if ( chkSort.Checked)
                    gridMain.OptionsView.GroupFooterShowMode = DevExpress.XtraGrid.Views.Grid.GroupFooterShowMode.VisibleAlways;
            }
        }
        /***********************************************************************************************/
        private void chkPaymentType_CheckedChanged(object sender, EventArgs e)
        {
            if (chkPaymentType.Checked)
            {
                gridMain.Columns["location"].GroupIndex = -1;
                gridMain.Columns["depositNumber"].GroupIndex = -1;
                gridMain.Columns["paymentType"].GroupIndex = 0;

                gridMain.OptionsBehavior.AutoExpandAllGroups = true;
                gridMain.ExpandAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = true;
                gridMain.OptionsPrint.PrintGroupFooter = true;
            }
            else
            {
                gridMain.Columns["location"].GroupIndex = -1;
                gridMain.Columns["depositNumber"].GroupIndex = -1;
                gridMain.Columns["paymentType"].GroupIndex = -1;
                gridMain.OptionsBehavior.AutoExpandAllGroups = false;
                gridMain.CollapseAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = false;
                gridMain.OptionsPrint.PrintGroupFooter = true;
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void cmbShow_SelectedIndexChanged(object sender, EventArgs e)
        {
            string showWhat = cmbShow.Text.Trim().ToUpper();
            if (showWhat == "ALL")
                dgv.DataSource = originalDt;
            else if (showWhat == "LK")
            {
                string names = "`location`='LK'";
                DataRow[] dRows = originalDt.Select(names);
                DataTable dt = originalDt.Clone();
                for (int i = 0; i < dRows.Length; i++)
                    dt.ImportRow(dRows[i]);
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

            }
            else if (showWhat == "ACH")
            {
                string names = "`location`='ACH'";
                DataRow[] dRows = originalDt.Select(names);
                DataTable dt = originalDt.Clone();
                for (int i = 0; i < dRows.Length; i++)
                    dt.ImportRow(dRows[i]);
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

            }
            else if (showWhat == "MANUAL")
            {
                string names = "`edited`='MANUAL'";
                DataRow[] dRows = originalDt.Select(names);
                DataTable dt = originalDt.Clone();
                for (int i = 0; i < dRows.Length; i++)
                    dt.ImportRow(dRows[i]);
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

            }
            else if (showWhat == "FUNERAL")
            {
                string names = "`location`='Funeral'";
                DataRow[] dRows = originalDt.Select(names);
                DataTable dt = originalDt.Clone();
                for (int i = 0; i < dRows.Length; i++)
                    dt.ImportRow(dRows[i]);
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void chkGroupBank_CheckedChanged(object sender, EventArgs e)
        {
            if (chkGroupBank.Checked)
            {
                gridMain.Columns["location"].GroupIndex = -1;
                gridMain.Columns["depositNumber"].GroupIndex = -1;
                gridMain.Columns["paymentType"].GroupIndex = -1;
                gridMain.Columns["bankDetail"].GroupIndex = 0;

                gridMain.OptionsBehavior.AutoExpandAllGroups = true;
                gridMain.ExpandAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = true;
                gridMain.OptionsPrint.PrintGroupFooter = true;
            }
            else
            {
                gridMain.Columns["location"].GroupIndex = -1;
                gridMain.Columns["depositNumber"].GroupIndex = -1;
                gridMain.Columns["paymentType"].GroupIndex = -1;
                gridMain.Columns["bankDetail"].GroupIndex = -1;
                gridMain.OptionsBehavior.AutoExpandAllGroups = false;
                gridMain.CollapseAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = false;
                gridMain.OptionsPrint.PrintGroupFooter = true;
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
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
        /***********************************************************************************************/
    }
}