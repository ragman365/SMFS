using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using GeneralLib;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using MySql.Data.MySqlClient;
using DevExpress.XtraGrid;
using DevExpress.Utils.Drawing;
using OfficeOpenXml;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class UnityReport : DevExpress.XtraEditors.XtraForm
    {
        DataTable originalDt = null;
        DataTable funDt = null;
        /***********************************************************************************************/
        public UnityReport()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void UnityReport_Load(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            now = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Value = now;
//            now = now.AddMonths(-2);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;

            btnExport.Hide();
            lblSavingColumn.Hide();
            lblSavingTab.Hide();
            txtSavingColumn.Hide();
            txtSavingTab.Hide();

            LoadLocations();

            // SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("trust85", null);
            AddSummaryColumn("trust85", gridMain2);
            AddSummaryColumn("trust85", gridMain3);
            AddSummaryColumn("trust85", gridMain4);
            AddSummaryColumn("trust85", gridMain5);
            AddSummaryColumn("trust85", gridMain6);
            AddSummaryColumn("trust85", gridMain7);
            AddSummaryColumn("trust85", gridMain8);
            AddSummaryColumn("trust85", gridMain9);
            AddSummaryColumn("trust85", gridMain10);
            AddSummaryColumn("trust85", gridMain11);
            AddSummaryColumn("trust85", gridMain12);
            AddSummaryColumn("trust85", gridMain13);
        }
        ///****************************************************************************************/
        //private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "")
        //{
        //    if (gMain == null)
        //        gMain = gridMain;
        //    if (String.IsNullOrWhiteSpace(format))
        //        format = "${0:0,0.00}";
        //    gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
        //    gMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        //}
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
        private void LoadLocations()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);
//            chkComboLocNames.Properties.DataSource = locDt;
            chkComboLocation.Properties.DataSource = locDt;
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DateTime date = dateTimePicker1.Value;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            string date2 = G1.DateTimeToSQLDateTime(date);

            try
            {

                funDt = G1.get_db_data("Select * from `funeralHomes`;");

                string cmd = "Select * from `payments` p JOIN `contracts` x ON p.`contractNumber` = x.`contractNumber` JOIN `customers` c ON p.`contractNumber` = c.`contractNumber` WHERE `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' AND (p.`contractNumber` LIKE '%U' OR p.`contractNumber` LIKE '%UI') ORDER BY p.`contractNumber`;";
                DataTable dt = G1.get_db_data(cmd);

                dt.Columns.Add("num");
                dt.Columns.Add("loc");
                dt.Columns.Add("trust");
                dt.Columns.Add("fullname");
                dt.Columns.Add("bdate");
                dt.Columns.Add("male");
                dt.Columns.Add("female");
                dt.Columns.Add("age");
                dt.Columns.Add("phone");
                dt.Columns.Add("trust85", Type.GetType("System.Double"));
                dt.Columns.Add("ownerName");
                dt.Columns.Add("ownerAddress");
                dt.Columns.Add("ownerPhone");
                dt.Columns.Add("ownerSSN");
                dt.Columns.Add("funeralProvider");
                dt.Columns.Add("beneficiary");

                string contractNumber = "";
                string contract = "";
                string trust = "";
                string loc = "";
                int age = 0;
                string sex = "";
                string areaCode = "";
                string phone = "";
                string ssn = "";
                double trust85 = 0D;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["fullname"] = dt.Rows[i]["firstName"].ObjToString().Trim() + " " + dt.Rows[i]["lastName"].ObjToString().Trim();
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                    dt.Rows[i]["loc"] = loc;
                    dt.Rows[i]["trust"] = trust;
                    date = dt.Rows[i]["birthDate"].ObjToDateTime();
                    dt.Rows[i]["bdate"] = date.ToString("MM/dd/yyyy");
                    age = G1.CalculateAgeCorrect(date, DateTime.Now);
                    dt.Rows[i]["age"] = age.ToString();
                    sex = dt.Rows[i]["sex"].ObjToString().ToUpper();
                    if (!String.IsNullOrWhiteSpace(sex))
                    {
                        if (sex.Substring(0, 1) == "M")
                            dt.Rows[i]["male"] = "M";
                        else if (sex.Substring(0, 1) == "F")
                            dt.Rows[i]["female"] = "F";
                    }
                    areaCode = dt.Rows[i]["areaCode"].ObjToString();
                    phone = dt.Rows[i]["phoneNumber"].ObjToString();
                    if (phone.IndexOf(areaCode) < 0)
                        phone = areaCode + " " + phone;
                    dt.Rows[i]["phone"] = phone;
                    ssn = dt.Rows[i]["ssn"].ObjToString();
                    ssn = ssn.Replace("-", "");
                    if (ssn.Length >= 5)
                        ssn = ssn.Substring(ssn.Length - 4);
                    dt.Rows[i]["ssn"] = ssn;

                    trust85 = dt.Rows[i]["trust85P"].ObjToDouble();
                    DateTime datePaid = dt.Rows[i]["payDate8"].ObjToString().ObjToDateTime();

                    if (datePaid >= DailyHistory.majorDate)
                        trust85 = CalcTrust85(dt.Rows[i]);
                    dt.Rows[i]["trust85"] = trust85;
                }

                CombineContracts(dt);

                LoadFuneralHomes(dt);

                G1.NumberDataTable(dt);
                dgv.DataSource = dt;
                originalDt = dt;

                double eTotal = LoadTab("E", dgv2);
                double ffTotal = LoadTab("FF", dgv3);
                double ctTotal = LoadTab("CT", dgv4);
                double htTotal = LoadTab("HT", dgv5);
                double huTotal = LoadTab("HU", dgv6);
                double tTotal = LoadTab("T", dgv7);
                double wmTotal = LoadTab("WM", dgv8);
                double pTotal = LoadTab("P", dgv9);
                double mTotal = LoadTab("M", dgv10);
                double cTotal = LoadTab("C,L", dgv11);
                double bTotal = LoadTab("B,MC,N,F", dgv12);
                double wfTotal = LoadTab("WF", dgv13);

                DataTable locDt = new DataTable();
                locDt.Columns.Add("location");
                locDt.Columns.Add("trust85", Type.GetType("System.Double"));
                LoadTotalsTab(locDt, "E", eTotal);
                LoadTotalsTab(locDt, "FF", ffTotal);
                LoadTotalsTab(locDt, "CT", ctTotal);
                LoadTotalsTab(locDt, "HT", htTotal);
                LoadTotalsTab(locDt, "HU", huTotal);
                LoadTotalsTab(locDt, "T", tTotal);
                LoadTotalsTab(locDt, "WM", wmTotal);
                LoadTotalsTab(locDt, "P", pTotal);
                LoadTotalsTab(locDt, "M", mTotal);
                LoadTotalsTab(locDt, "C", cTotal);
                LoadTotalsTab(locDt, "B", bTotal);
                LoadTotalsTab(locDt, "WF", wfTotal);

                LoadFHS_Tab();

                DataRow dR = locDt.NewRow();
                locDt.Rows.Add(dR);

                string month = this.dateTimePicker1.Value.ToString("MMMMMMMMMMMMM");

                DataRow dR1 = locDt.NewRow();
                dR1["location"] = month + this.dateTimePicker1.Value.Year.ToString("D4") + " ALLOTTED MONTHLY TOTAL";
                dR1["trust85"] = eTotal + ffTotal + ctTotal + htTotal + huTotal + tTotal + wmTotal + pTotal + mTotal + cTotal + bTotal + wfTotal;
                locDt.Rows.Add(dR1);

                dgv15.DataSource = locDt;

                btnExport.Show();
            }
            catch ( Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void LoadTotalsTab ( DataTable locDt, string loc, double trust85)
        {
            DataRow [] dRows = funDt.Select("keycode='" + loc + "'");
            if (dRows.Length > 0)
            {
                DataRow dR = locDt.NewRow();
                dR["location"] = dRows[0]["name"].ObjToString();
                dR["trust85"] = trust85;
                locDt.Rows.Add(dR);
            }
        }
        /***********************************************************************************************/
        private void LoadFHS_Tab ()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("ownerName");
            dt.Columns.Add("ownerAddress");
            dt.Columns.Add("ownerPhone");
            dt.Columns.Add("ownerSSN");
            dt.Columns.Add("funeralProvider");
            dt.Columns.Add("beneficiary");

            string home = "";
            string keyCode = "";

            string saveHomeName = "";

            for ( int i=0; i<funDt.Rows.Count; i++)
            {
                keyCode = funDt.Rows[i]["keyCode"].ObjToString().ToUpper();
                if (keyCode == "WF-C")
                    continue;
                else if (keyCode == "WF-F")
                    continue;
                else if (keyCode == "WF-R")
                    continue;

                home = funDt.Rows[i]["name"].ObjToString();
                if (keyCode.ToUpper() == "B")
                    saveHomeName = home;
                if (keyCode.ToUpper() == "MC")
                    home = saveHomeName;
                if (keyCode.Length == 1)
                    keyCode += "1";
                DataRow dRow = dt.NewRow();
                dRow["num"] = keyCode;
                dRow["ownerName"] = home;
                dRow["ownerAddress"] = "P O BOX 727, BAY SPRINGS, MS, 39422";
                dRow["ownerPhone"] = "6017643171";
                dRow["ownerSSN"] = funDt.Rows[i]["ownerSSN"].ObjToString();
                dRow["funeralProvider"] = home + " PN 2002";
                dRow["beneficiary"] = home + " PN 2002";
                dt.Rows.Add(dRow);
            }
            dgv14.DataSource = dt;
        }
        /***********************************************************************************************/
        private void LoadFuneralHomes(DataTable dt)
        {
            try
            {
                DataRow[] dRows = null;

                string contract = "";
                string loc = "";
                string home = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contract = dt.Rows[i]["contractNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(contract))
                        continue;
                    loc = dt.Rows[i]["loc"].ObjToString();
                    if (loc.ToUpper() == "MC")
                        loc = "B";
                    dRows = funDt.Select("keycode='" + loc + "'");
                    if (dRows.Length > 0)
                    {
                        home = dRows[0]["name"].ObjToString();
                        dt.Rows[i]["ownerName"] = home;
                        dt.Rows[i]["ownerAddress"] = "P O BOX 727, BAY SPRINGS, MS, 39422";
                        dt.Rows[i]["ownerPhone"] = "6017643171";
                        dt.Rows[i]["ownerSSN"] = dRows[0]["ownerSSN"].ObjToString();
                        dt.Rows[i]["funeralProvider"] = home + " PN 2002";
                        dt.Rows[i]["beneficiary"] = home + " PN 2002";
                    }
                }
            }
             catch ( Exception ex)
            {

            }
        }
        /***********************************************************************************************/
        private void CombineContracts ( DataTable dt)
        {
            double trust85 = 0D;
            string contract = "";
            string contract2 = "";
            DateTime deceasedDate = DateTime.Now;
            DateTime payDate8 = DateTime.Now;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contract = dt.Rows[i]["contractNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(contract))
                    continue;
                payDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if ( deceasedDate.Year > 100)
                {
                    dt.Rows[i]["contractNumber"] = "";
                    continue;
                    //if ( deceasedDate >= this.dateTimePicker1.Value && deceasedDate <= this.dateTimePicker2.Value)
                    //{
                    //    dt.Rows[i]["contractNumber"] = "";
                    //    continue;
                    //}
                }
                trust85 = dt.Rows[i]["trust85"].ObjToDouble();
                for ( int j=(i+1); j<dt.Rows.Count; j++)
                {
                    contract2 = dt.Rows[j]["contractNumber"].ObjToString();
                    if (contract2 != contract)
                        break;
                    trust85 += dt.Rows[j]["trust85"].ObjToDouble();
                    dt.Rows[i]["trust85"] = trust85;
                    dt.Rows[j]["contractNumber"] = "";
                }
            }

            for ( int i=(dt.Rows.Count-1); i>=0; i--)
            {
                contract = dt.Rows[i]["contractNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(contract))
                    dt.Rows.RemoveAt(i);
            }
        }
        /***********************************************************************************************/
        private double CalcTrust85 ( DataRow dRow)
        {
            double trust85 = 0D;
            double trust100 = 0D;
            int method = 0;

            string contract = dRow["contractNumber"].ObjToString();

            double originalDownPayment = DailyHistory.GetOriginalDownPayment(dRow);

            double contractValue = DailyHistory.GetContractValuePlus(dRow);

            DateTime oldIssueDate = dRow["issueDate8"].ObjToDateTime();
            oldIssueDate = DailyHistory.GetIssueDate(oldIssueDate, contract, null);

            double financeMonths = dRow["numberOfPayments"].ObjToDouble();
            double amtOfMonthlyPayt = dRow["amtOfMonthlyPayt"].ObjToDouble();
            double rate = dRow["apr"].ObjToDouble() / 100.0D;
            double downPayment = dRow["downPayment"].ObjToDouble();

            double principal = 0D;
            double payment = 0D;
            double retained = 0D;

            DateTime docp = dRow["payDate8"].ObjToDateTime();

            double paymentAmount = dRow["paymentAmount"].ObjToDouble();
            payment = paymentAmount;
            double debit = dRow["debitAdjustment"].ObjToDouble();
            double credit = dRow["creditAdjustment"].ObjToDouble();
            double interest = dRow["interestPaid"].ObjToDouble();
            double pastInterest = interest;
            paymentAmount += credit - debit + downPayment;
            if (payment == 0D && downPayment > 0D)
                payment = downPayment;

            if (debit > 0)
            {
                principal = debit + interest;
                principal = principal * -1D;
            }

            method = ImportDailyDeposits.CalcTrust85P(docp, amtOfMonthlyPayt, oldIssueDate.ToString("MM/dd/yyyy"), contractValue, originalDownPayment, financeMonths, payment, principal, debit, credit, rate, ref trust85, ref trust100, ref retained);


            //method = ImportDailyDeposits.CalcTrust85(amtOfMonthlyPayt, oldIssueDate.ToString("MM/dd/yyyy"), contractValue, downPayment, financeMonths, payment, principal, rate, ref trust85, ref trust100);
            return trust85;
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            if ( dgv.Visible )
                dr = gridMain.GetFocusedDataRow();
            else if (dgv2.Visible)
                dr = gridMain2.GetFocusedDataRow();


            string contract = dr["contractNumber"].ObjToString();
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
            DataTable ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count <= 0) // Maybe Insurance
            {
                cmd = "Select * from `icustomers` where `payer` = '" + contract + "';";
                ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count <= 0)
                {
                    MessageBox.Show("***ERROR*** Cannot find Contract or Payer!");
                    return;
                }
                contract = ddx.Rows[0]["contractNumber"].ObjToString();
            }
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                DataTable dt = (DataTable)dgv.DataSource;
                bool insurance = false;
                if (contract.ToUpper().IndexOf("ZZ") == 0)
                    insurance = true;
                if (contract.ToUpper().IndexOf("MM") == 0)
                    insurance = true;
                if (contract.ToUpper().IndexOf("OO") == 0)
                    insurance = true;
                if (insurance)
                {
                    cmd = "Select * from `policies` p JOIN `icustomers` d ON p.`contractNumber` = d.`contractNumber` JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
                    cmd += " WHERE p.`contractNumber` = '" + contract + "' ";

                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        Policies policyForm = new Policies(contract);
                        policyForm.Show();
                    }
                    else
                    {
                        CustomerDetails clientForm = new CustomerDetails(contract);
                        clientForm.Show();
                    }
                }
                else
                {
                    CustomerDetails clientForm = new CustomerDetails(contract);
                    clientForm.Show();
                }
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            now = now.AddMonths(-1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            now = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Value = now;
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            now = now.AddMonths(1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            now = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Value = now;
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            if (e.Column.FieldName.Trim().ToUpper() == "IF")
            {
                string record = dr["record"].ObjToString();
                string ifc = dr["IF"].ObjToString().ToUpper();
                dr["IF"] = ifc;
                G1.update_db_table("contracts", "record", record, new string[] { "IF", ifc});
            }
            else if (e.Column.FieldName.Trim().ToUpper() == "CA")
            {
                string record = dr["record"].ObjToString();
                string ca = dr["CA"].ObjToString().ToUpper();
                dr["CA"] = ca;
                G1.update_db_table("contracts", "record", record, new string[] { "CA", ca });
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

            G1.PrintPreview(printableComponentLink1, gridMain);

            //printableComponentLink1.CreateDocument();
            //printableComponentLink1.ShowPreview();

            isPrinting = false;
        }
        /***********************************************************************************************/
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
            if (LoginForm.doLapseReport)
                printableComponentLink1.Print();
            else
                printableComponentLink1.PrintDlg();
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


            font = new Font("Ariel", 10, FontStyle.Bold);
            Printer.DrawQuad(5, 8, 4, 4, "Unity Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            DateTime date = this.dateTimePicker1.Value;
            string workDate = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + (date.Year % 100).ToString("D2");

            date = this.dateTimePicker2.Value;
            string workDate1 = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + (date.Year % 100).ToString("D2");

            string str = "Report : " + workDate1 + " - " + workDate;

            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Bold);
            Printer.DrawQuad(19, 8, 5, 4, str, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private double LoadTab ( string location, DevExpress.XtraGrid.GridControl dgv )
        {
            if (originalDt == null)
                return 0D;
            string names = getLocationQuery(location);
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();

            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);

            DataView tempview = dt.DefaultView;
            tempview.Sort = "contractNumber asc";
            dt = tempview.ToTable();

            double total = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
                total += dt.Rows[i]["trust85"].ObjToDouble();
            DataRow dRow = dt.NewRow();
            dRow["trust85"] = total;
            dt.Rows.Add(dRow);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            return total;
        }
        /***********************************************************************************************/
        private void chkComboLocation_EditValueChanged(object sender, EventArgs e)
        {
            if (originalDt == null)
                return;
            string names = getLocationQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            DataView tempview = dt.DefaultView;
            tempview.Sort = "contractNumber asc";
            dt = tempview.ToTable();
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /*******************************************************************************************/
        private string getLocationQuery( string location = "" )
        {
            string procLoc = "";
            string[] locIDs = this.chkComboLocation.EditValue.ToString().Split('|');
            if (!String.IsNullOrWhiteSpace(location))
                locIDs = location.Split(',');
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
        /***********************************************************************************************/
        private void btnExport_Click(object sender, EventArgs e)
        {
            if ( 1 == 1)
            {
                Tyburn1();
                return;
            }
            //using (var package = new ExcelPackage())
            //{
            //    var workSheet1 = package.Workbook.Worksheets.Add("Brookhaven");
            //    var workSheet2 = package.Workbook.Worksheets.Add("Magee");
            //    workSheet2.Name = "The second sheet";
            //    workSheet2.Cells[1, 1] = "Something completely different";
            //}

            //    DataTable myDt = ExcelWriter.ReadExcelFile("C:/rag/2nd Qtr Data 2019X.xls");

            FileStream stream = new FileStream("c:/rag/demo.xls", FileMode.OpenOrCreate);
            ExcelWriter writer = new ExcelWriter(stream);
            writer.BeginWrite();
            writer.WriteCell(10, 10, "Somewhere in Excel");
            writer.WriteCell(0, 0, "ExcelWriter Demo");
            writer.WriteCell(1, 0, "int");
            writer.WriteCell(1, 1, 10);
            writer.WriteCell(2, 0, "double");
            writer.WriteCell(2, 1, 1.5);
            writer.WriteCell(3, 0, "empty");
            writer.WriteCell(3, 1);
            writer.EndWrite();
            stream.Close();
        }
        /***********************************************************************************************/
        private void LoadUpExcelTab ( DataTable dt, Excel.Worksheet oSheet, string name, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain )
        {
            oSheet.Name = name;
            txtSavingTab.Text = oSheet.Name;
            txtSavingTab.Refresh();

            string caption = "";
            string data = "";
            int index = 0;

            DataTable sortDt = new DataTable();
            sortDt.Columns.Add("columns", Type.GetType("System.Int32"));
            sortDt.Columns.Add("col", Type.GetType("System.Int32"));
            for (int col = 0; col < gridMain.Columns.Count; col++)
            {
                if (!gridMain.Columns[col].Visible)
                    continue;
                index = gridMain.Columns[col].ColIndex.ObjToInt32();
                if (index < 0)
                    continue;
                DataRow dRow = sortDt.NewRow();
                dRow["columns"] = index;
                dRow["col"] = col;
                sortDt.Rows.Add(dRow);
            }
            DataView tempview = sortDt.DefaultView;
            tempview.Sort = "columns asc";
            sortDt = tempview.ToTable();

            int myCol = 0;

            for (int col = 0; col < sortDt.Rows.Count; col++)
            {
                try
                {
                    myCol = sortDt.Rows[col]["col"].ObjToInt32();
                    if (!gridMain.Columns[myCol].Visible)
                        continue;
                    caption = gridMain.Columns[myCol].Caption;
                    txtSavingColumn.Text = caption;
                    txtSavingColumn.Refresh();
                    name = gridMain.Columns[myCol].FieldName;
//                    oSheet.Cells[col + 1, 1] = caption;
                    oSheet.Cells[1, col + 1] = caption;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        data = dt.Rows[j][name].ObjToString();
                        if (!String.IsNullOrWhiteSpace(data))
                            oSheet.Cells[col + 1][j + 2] = data;
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        private object missing = Type.Missing;
        /***********************************************************************************************/
        private void Tyburn1()
        {
            DialogResult result = MessageBox.Show("Do you REALLY want to SAVE this data to an Excel File?", "Save Unity to Excel Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            lblSavingTab.Show();
            lblSavingColumn.Show();
            txtSavingTab.Show();
            txtSavingColumn.Show();

            DateTime startTime = DateTime.Now;

            Excel.Application oXL = new Excel.Application();
            oXL.Visible = false;
            Excel.Workbook oWB = oXL.Workbooks.Add(missing);

            try
            {
                Excel.Worksheet oSheet = oWB.ActiveSheet as Excel.Worksheet;
                DataTable dt = (DataTable)dgv.DataSource;
                LoadUpExcelTab(dt, oSheet, "Master", gridMain);

                Excel.Worksheet oSheet14 = oWB.Sheets.Add(missing, missing, 1, missing) as Excel.Worksheet;
                dt = (DataTable)dgv14.DataSource;
                LoadUpExcelTab(dt, oSheet14, "FHS", gridMain14);

                Excel.Worksheet oSheet13 = oWB.Sheets.Add(missing, missing, 1, missing) as Excel.Worksheet;
                dt = (DataTable)dgv13.DataSource;
                LoadUpExcelTab(dt, oSheet13, "WF", gridMain13);

                Excel.Worksheet oSheet12 = oWB.Sheets.Add(missing, missing, 1, missing) as Excel.Worksheet;
                dt = (DataTable)dgv12.DataSource;
                LoadUpExcelTab(dt, oSheet12, "BSRF", gridMain12);

                Excel.Worksheet oSheet11 = oWB.Sheets.Add(missing, missing, 1, missing) as Excel.Worksheet;
                dt = (DataTable)dgv11.DataSource;
                LoadUpExcelTab(dt, oSheet11, "JCC", gridMain11);

                Excel.Worksheet oSheet10 = oWB.Sheets.Add(missing, missing, 1, missing) as Excel.Worksheet;
                dt = (DataTable)dgv10.DataSource;
                LoadUpExcelTab(dt, oSheet10, "CCI", gridMain10);

                Excel.Worksheet oSheet9 = oWB.Sheets.Add(missing, missing, 1, missing) as Excel.Worksheet;
                dt = (DataTable)dgv9.DataSource;
                LoadUpExcelTab(dt, oSheet9, "BN", gridMain9);

                Excel.Worksheet oSheet8 = oWB.Sheets.Add(missing, missing, 1, missing) as Excel.Worksheet;
                dt = (DataTable)dgv8.DataSource;
                LoadUpExcelTab(dt, oSheet8, "WM", gridMain8);

                Excel.Worksheet oSheet7 = oWB.Sheets.Add(missing, missing, 1, missing) as Excel.Worksheet;
                dt = (DataTable)dgv7.DataSource;
                LoadUpExcelTab(dt, oSheet7, "CAPPS", gridMain7);

                Excel.Worksheet oSheet6 = oWB.Sheets.Add(missing, missing, 1, missing) as Excel.Worksheet;
                dt = (DataTable)dgv6.DataSource;
                LoadUpExcelTab(dt, oSheet6, "HH", gridMain6);

                Excel.Worksheet oSheet5 = oWB.Sheets.Add(missing, missing, 1, missing) as Excel.Worksheet;
                dt = (DataTable)dgv5.DataSource;
                LoadUpExcelTab(dt, oSheet5, "HT", gridMain5);

                Excel.Worksheet oSheet4 = oWB.Sheets.Add(missing, missing, 1, missing) as Excel.Worksheet;
                dt = (DataTable)dgv4.DataSource;
                LoadUpExcelTab(dt, oSheet4, "CT", gridMain4);

                Excel.Worksheet oSheet3 = oWB.Sheets.Add(missing, missing, 1, missing) as Excel.Worksheet;
                dt = (DataTable)dgv3.DataSource;
                LoadUpExcelTab(dt, oSheet3, "FF", gridMain3);

                Excel.Worksheet oSheet2 = oWB.Sheets.Add(missing, missing, 1, missing) as Excel.Worksheet;
                dt = (DataTable)dgv2.DataSource;
                LoadUpExcelTab(dt, oSheet2, "EV", gridMain2);

                Excel.Worksheet oSheet15 = oWB.Sheets.Add(missing, missing, 1, missing) as Excel.Worksheet;
                dt = (DataTable)dgv15.DataSource;
                LoadUpExcelTab(dt, oSheet15, "TOTALS", gridMain15);
            }
            catch ( Exception ex)
            {
            }

//            string fileName = "C:\\rag\\demo2.xlsx";
            try
            {
                using (SaveFileDialog ofdImage = new SaveFileDialog())
                {
                    ofdImage.Filter = "Excel files (*.xlsx)|*.xlsx";

                    if (ofdImage.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string fileName = ofdImage.FileName;

                        if (!String.IsNullOrWhiteSpace(fileName))
                        {
                            oWB.SaveAs(fileName, Excel.XlFileFormat.xlOpenXMLWorkbook,
                                missing, missing, missing, missing,
                                Excel.XlSaveAsAccessMode.xlNoChange,
                                missing, missing, missing, missing, missing);
                        }
                    }
                }
            }
            catch ( Exception ex)
            {
            }
            oWB.Close(missing, missing, missing);
            oXL.UserControl = true;
            oXL.Quit();

            DateTime stopTime = DateTime.Now;
            TimeSpan ts = stopTime - startTime;

            int hours = ts.Hours;
            int minutes = ts.Minutes;
            int seconds = ts.Seconds;

            MessageBox.Show("***INFO*** Total Processing Time = " + hours.ToString("D2") + ":" + minutes.ToString("D2") + ":" + seconds.ToString("D2") + "!!");
        }
        /***********************************************************************************************/
    }
}