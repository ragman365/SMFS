using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using GeneralLib;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;


using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;


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
using System.Drawing.Drawing2D;
using DevExpress.XtraPrintingLinks;
using DevExpress.XtraGrid.Views.BandedGrid;
using ExcelLibrary.BinaryFileFormat;
using DevExpress.XtraBars.ViewInfo;
using System.Text;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;

/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class ContractActivity : DevExpress.XtraEditors.XtraForm
    {
        private DataTable groupContracts = null;
        private bool runAgents = false;
        private DataTable originalDt = null;
        /****************************************************************************************/
        private bool doSimple = false;
        private bool doLocDetail = false;
        private bool foundLocalPreference = false;
        /****************************************************************************************/
        public ContractActivity()
        {
            InitializeComponent();
            if (G1.oldCopy)
                menuStrip1.BackColor = Color.LightBlue;

            SetupTotalsSummary();
            SetupDisplay();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("currentPrice", null);
            AddSummaryColumn("preneedDiscount", null);
            AddSummaryColumn("custPrice", null);
            AddSummaryColumn("netAdjust", null);
            AddSummaryColumn("netPrice", null);
            AddSummaryColumn("classa", null);
            AddSummaryColumn("compDiscount", null);
            AddSummaryColumn("trustDiscount", null);
            AddSummaryColumn("trustGrowth", null);
            AddSummaryColumn("trustAmountReceived", null);
            AddSummaryColumn("insuranceDiscount", null);
            AddSummaryColumn("insuranceGrowth", null);
            AddSummaryColumn("insurancePayments", null);
            AddSummaryColumn("cashReceived", null);
            AddSummaryColumn("totalAllPayments", null);
            AddSummaryColumn("refund", null);
            AddSummaryColumn("balanceDue", null);
            AddSummaryColumn("dbr", null);
            AddSummaryColumn("cliffBalance", null);

            //AddSummaryColumn("balanceDue", null);


            gridMain.Columns["loc"].Visible = false;
            gridMain.Columns["Location Name"].Visible = false;
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
        private void ContractActivity_Load(object sender, EventArgs e)
        {
            chkInclude.Hide();
            chkExcludeBlankLine.Hide();

            barImport.Hide();

            gridMain.Columns["Location Name"].Visible = false;
            gridMain.Columns["contractNumber"].Visible = false;

            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            DateTime stop = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Value = stop;

            this.dateTimePicker3.Value = this.dateTimePicker1.Value;
            this.dateTimePicker4.Value = this.dateTimePicker2.Value;

            gridMain.Columns["num"].Visible = true;
//            gridMain.Columns["loc"].Visible = false;
            gridMain.Columns["Location Name"].Visible = false;
            gridMain.Columns["contractNumber"].Visible = false;

            gridMain.OptionsView.ShowFooter = false;
            gridMain.OptionsView.ShowBands = false;

            gridMain.OptionsPrint.PrintBandHeader = false;
            gridMain.OptionsPrint.PrintFooter = false;

            //getLocations();

            loadLocatons();

            string saveName = "Contract Activity Primary";
            string skinName = "";

            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            loadGroupCombo(cmbSelectColumns, "Contract Activity", "Primary");
            cmbSelectColumns.Text = "Primary";

            //gridMain.Appearance.Row.Font = new Font("Tahoma", 9F);
            //gridMain.AppearancePrint.Row.Font = new Font("Tahoma", 9F);
        }
        /***********************************************************************************************/
        private void loadLocatons()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);

            DataTable newLocDt = locDt.Clone();

            string assignedLocations = "";

            cmd = "Select * from `users` where `username` = '" + LoginForm.username + "';";
            DataTable userDt = G1.get_db_data(cmd);
            if (userDt.Rows.Count > 0)
                assignedLocations = userDt.Rows[0]["assignedLocations"].ObjToString();

            string locationCode = "";
            string keyCode = "";
            string[] Lines = null;
            string locations = "";
            string location = "";

            for (int i = locDt.Rows.Count - 1; i >= 0; i--)
            {
                keyCode = locDt.Rows[i]["keycode"].ObjToString();
                if (keyCode.IndexOf("-") > 0)
                    locDt.Rows.RemoveAt(i);
            }
            for (int i = 0; i < locDt.Rows.Count; i++)
            {
                locationCode = locDt.Rows[i]["locationCode"].ObjToString();
                if (String.IsNullOrWhiteSpace(locationCode))
                    continue;
                Lines = assignedLocations.Split('~');
                for (int j = 0; j < Lines.Length; j++)
                {
                    location = Lines[j].Trim();
                    if (String.IsNullOrWhiteSpace(location))
                        continue;
                    if (location.ToUpper() == locationCode.ToUpper())
                    {
                        location = locDt.Rows[i]["atNeedCode"].ObjToString();
                        locations += location + "|";
                        newLocDt.ImportRow(locDt.Rows[i]);
                    }
                }
            }
            if (!LoginForm.administrator)
                locDt = newLocDt;

            DataView tempview = locDt.DefaultView;
            tempview.Sort = "atneedcode";
            locDt = tempview.ToTable();


            chkComboLocation.Properties.DataSource = locDt;

            //locations = locations.TrimEnd('|');
            //chkComboLocation.EditValue = locations;
            //chkComboLocation.Text = locations;
        }
        /***********************************************************************************************/
        private DataTable _LocationList;
        private void getLocations()
        {
            //string cmd = "SELECT `LocationCode` FROM `inventory` GROUP BY `LocationCode` ASC;";
            string cmd = "Select * from `funeralhomes` order by `atneedcode`;";
            _LocationList = G1.get_db_data(cmd);

            string str = "";

            for (int i = _LocationList.Rows.Count - 1; i >= 0; i--)
            {
                str = _LocationList.Rows[i]["atneedcode"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    _LocationList.Rows.RemoveAt(i);
            }

            chkComboLocation.Properties.DataSource = _LocationList;
        }
        /*******************************************************************************************/
        private string getLocationNameQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboLocation.EditValue.ToString().Split('|');
            DataRow[] dRows = null;
            DataTable dt = (DataTable) chkComboLocation.Properties.DataSource;
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    dRows = dt.Select("LocationCode='" + locIDs[i].Trim() + "'");
                    if (dRows.Length > 0)
                    {
                        procLoc += "'" + dRows[0]["atneedcode"].ObjToString().Trim() + "'";
                        //procLoc += "'" + locIDs[i].Trim() + "'";
                    }
                }
            }
            return procLoc.Length > 0 ? " `serviceLoc` IN (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
        }
        /****************************************************************************************/
        private void SetupDisplay ()
        {
            string display = cmbDisplay.Text;
            if (display.ToUpper() == "NET CASH")
            {
                gridMain.Columns["netAdjust"].Visible = true;
                gridMain.Columns["netPrice"].Visible = true;
                gridMain.Columns["compDiscount"].Visible = false;
                gridMain.Columns["trustDiscount"].Visible = false;
                gridMain.Columns["insuranceDiscount"].Visible = false;
                gridMain.Columns["trustGrowth"].Visible = false;
                gridMain.Columns["insuranceGrowth"].Visible = false;
            }
            else
            {
                gridMain.Columns["netAdjust"].Visible = false;
                gridMain.Columns["netPrice"].Visible = false;
                gridMain.Columns["compDiscount"].Visible = true;
                gridMain.Columns["trustDiscount"].Visible = true;
                gridMain.Columns["insuranceDiscount"].Visible = true;
                gridMain.Columns["trustGrowth"].Visible = true;
                gridMain.Columns["insuranceGrowth"].Visible = true;
            }

            gridMain.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            runData ();

            ScaleCells();

            SetupDisplay();

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private bool CheckForContract( string contractNumber, ref DateTime gsDate )
        {
            gsDate = DateTime.MinValue;
            string cmd = "Select * from `lapse_list` where `contractNumber` = '" + contractNumber + "' AND `detail` = 'Goods and Services' ORDER BY `noticeDate` DESC;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;
            gsDate = dt.Rows[0]["noticeDate"].ObjToDateTime();
            return true;
        }
        /****************************************************************************************/
        private DataTable GetDataSet ( DateTime startDate, DateTime stopDate )
        {
            DateTime start = startDate;
            string date1 = G1.DateTimeToSQLDateTime(start);
            DateTime stop = stopDate;
            string date2 = G1.DateTimeToSQLDateTime(stop);
            string contractNumber = "";
            string loc = "";
            string contract = "";
            string trust = "";
            double contractValue = 0D;
            double downPayment = 0D;
            double payments = 0D;
            int idx = 0;
            string ch = "";
            string serviceId = "";
            string search = cmbSearch.Text.ToUpper();

            string cmd = "Select * from `fcust_extended` c JOIN `fcustomers` a on c.`contractNumber` = a.`contractNumber` where ";
            if (search == "DECEASED DATE")
                cmd += " a.`deceasedDate` >= '" + date1 + "' AND a.`deceasedDate` <= '" + date2 + "' ";
            else if (search == "CREATE DATE")
                cmd += " c.`caseCreatedDate` >= '" + date1 + "' AND c.`caseCreatedDate` <= '" + date2 + "' ";
            else
                cmd += " c.`serviceDate` >= '" + date1 + "' AND c.`serviceDate` <= '" + date2 + "' ";

            string names = getLocationNameQuery();
            if (!String.IsNullOrWhiteSpace(names))
                cmd += " AND " + names + " ";

            contract = txtContract.Text.Trim();
            if (!string.IsNullOrWhiteSpace(contract))
            {
                cmd = "Select * from `fcust_extended` c JOIN `fcustomers` a on c.`contractNumber` = a.`contractNumber` where ";
                cmd += " c.`serviceId` = '" + contract + "' ";
            }

            //cmd += " AND `serviceLoc` <> 'NM' ";
            cmd += ";";

            int year = start.Year;

            string yy = (year % 100).ToString("D2");

            cmd = "Select * from `fcustomers` p JOIN `fcust_extended` f ON p.`contractNumber` = f.`contractNumber` where p.`deceasedDate` <= '" + date2 + "' ";
            cmd += " AND p.`serviceId` LIKE '__" + yy + "%' ";
            //cmd += " AND f.`serviceDate` > '1000-01-01' ";

            if (!String.IsNullOrWhiteSpace(names))
                cmd += " AND " + names + " ";

            cmd += " AND f.`OpenCloseFuneral` <> 'Y' ";

            contract = txtContract.Text.Trim();
            if (!string.IsNullOrWhiteSpace(contract))
            {
                cmd = "Select * from `fcustomers` p JOIN `fcust_extended` f ON p.`contractNumber` = f.`contractNumber` where p.`deceasedDate` <= '" + date2 + "' ";
                cmd += " AND f.`serviceId` = '" + contract + "' ";
            }
            cmd += " ORDER BY p.`deceasedDate`;";

            DataTable dt = G1.get_db_data(cmd);

            Trust85.FindContract(dt, "WF18208LI");


            if (String.IsNullOrWhiteSpace(contract))
                dt = SalesTaxReport.ProcessTheData(dt, start, stop);

            return dt;
        }
        /****************************************************************************************/
        private DataTable LoadDataFast(DateTime startDate, DateTime stopDate, DataTable mainDt, bool ytd)
        {
            DateTime start = startDate;
            string date1 = G1.DateTimeToSQLDateTime(start);
            DateTime stop = stopDate;
            string date2 = G1.DateTimeToSQLDateTime(stop);
            string contractNumber = "";
            string loc = "";
            string contract = "";
            string trust = "";
            double contractValue = 0D;
            double downPayment = 0D;
            double payments = 0D;
            int idx = 0;
            string ch = "";
            string serviceId = "";
            string search = cmbSearch.Text.ToUpper();

            //string cmd = "Select * from `fcust_extended` c JOIN `fcustomers` a on c.`contractNumber` = a.`contractNumber` where ";
            //if (search == "DECEASED DATE")
            //    cmd += " a.`deceasedDate` >= '" + date1 + "' AND a.`deceasedDate` <= '" + date2 + "' ";
            //else if (search == "CREATE DATE")
            //    cmd += " c.`caseCreatedDate` >= '" + date1 + "' AND c.`caseCreatedDate` <= '" + date2 + "' ";
            //else
            //    cmd += " c.`serviceDate` >= '" + date1 + "' AND c.`serviceDate` <= '" + date2 + "' ";

            //string names = getLocationNameQuery();
            //if (!String.IsNullOrWhiteSpace(names))
            //    cmd += " AND " + names + " ";

            //contract = txtContract.Text.Trim();
            //if (!string.IsNullOrWhiteSpace(contract))
            //{
            //    cmd = "Select * from `fcust_extended` c JOIN `fcustomers` a on c.`contractNumber` = a.`contractNumber` where ";
            //    cmd += " c.`serviceId` = '" + contract + "' ";
            //}

            ////cmd += " AND `serviceLoc` <> 'NM' ";
            //cmd += ";";

            //int year = start.Year;

            //string yy = (year % 100).ToString("D2");

            //cmd = "Select * from `fcustomers` p JOIN `fcust_extended` f ON p.`contractNumber` = f.`contractNumber` where p.`deceasedDate` <= '" + date2 + "' ";
            //cmd += " AND p.`serviceId` LIKE '__" + yy + "%' ";

            //if (!String.IsNullOrWhiteSpace(names))
            //    cmd += " AND " + names + " ";

            //cmd += " AND f.`OpenCloseFuneral` <> 'Y' ";

            //contract = txtContract.Text.Trim();
            //if (!string.IsNullOrWhiteSpace(contract))
            //{
            //    cmd = "Select * from `fcustomers` p JOIN `fcust_extended` f ON p.`contractNumber` = f.`contractNumber` where p.`deceasedDate` <= '" + date2 + "' ";
            //    cmd += " AND f.`serviceId` = '" + contract + "' ";
            //}
            //cmd += " ORDER BY p.`deceasedDate`;";

            //DataTable dt = G1.get_db_data(cmd);

            //if ( String.IsNullOrWhiteSpace ( contract ))
            //    dt = SalesTaxReport.ProcessTheData(dt, start, stop);

            DataTable dt = null;
            int year1 = startDate.Year;
            int year2 = stopDate.Year;
            if (year1 == year2)
            {
                dt = GetDataSet(startDate, stopDate);
                Trust85.FindContract(dt, "WF18208LI");
            }
            else
            {
                DateTime testDate = new DateTime(startDate.Year, 12, 31);
                dt = GetDataSet(startDate, testDate);

                testDate = new DateTime(stopDate.Year, 1, 1);
                DataTable dx = GetDataSet(testDate, stopDate);
                string cNum = "";
                DataRow[] ddRows = null;
                for (int i = (dx.Rows.Count - 1); i >= 0; i--)
                {
                    cNum = dx.Rows[i]["contractNumber"].ObjToString();
                    ddRows = dt.Select("contractNumber='" + cNum + "'");
                    if (ddRows.Length > 0)
                        dx.Rows.RemoveAt(i);
                }
                dt.Merge(dx);
            }

            dt.Columns.Add("name");
            dt.Columns.Add("burial", Type.GetType("System.Double"));
            dt.Columns.Add("cremation", Type.GetType("System.Double"));
            dt.Columns.Add("other", Type.GetType("System.Double"));
            dt.Columns.Add("total", Type.GetType("System.Double"));
            dt.Columns.Add("payments", Type.GetType("System.Double"));
            dt.Columns.Add("netAdjust", Type.GetType("System.Double"));
            dt.Columns.Add("netPrice", Type.GetType("System.Double"));
            dt.Columns.Add("diff");
            dt.Columns.Add("cliffBalance", Type.GetType("System.Double"));

            //dt.Columns.Add("trustPayments", Type.GetType("System.Double"));
            dt.Columns.Add("trustDiscount", Type.GetType("System.Double"));
            //dt.Columns.Add("trustGrowth", Type.GetType("System.Double"));

            dt.Columns.Add("insuranceAmountReceived", Type.GetType("System.Double"));
            //dt.Columns.Add("insurancePayments", Type.GetType("System.Double"));
            dt.Columns.Add("insuranceDiscount", Type.GetType("System.Double"));
            //dt.Columns.Add("insuranceGrowth", Type.GetType("System.Double"));

            dt.Columns.Add("totalAllPayments", Type.GetType("System.Double"));

            //dt.Columns.Add("cashReceived", Type.GetType("System.Double"));
            dt.Columns.Add("loc");
            dt.Columns.Add("Location Name");
            dt.Columns.Add("GOOD");
            dt.Columns.Add("gsdate");
            dt.Columns.Add("issueD");
            dt.Columns.Add("policyNumber");
            dt.Columns.Add("policyAmount");
            dt.Columns.Add("payerNumber");

            bool isGood = false;
            DateTime gsDate = DateTime.Now;

            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable dd = G1.get_db_data(cmd);

            DataTable payDt = null;
            DataRow[] dr = null;
            string deceasedDate = "";
            DateTime ddate = DateTime.Now;
            string funeralClass = "";
            double dValue = 0D;
            string serviceLoc = "";

            double currentPrice = 0D;
            double discount = 0D;
            double classA = 0D;
            double totalReceived = 0D;
            double balanceDue = 0D;
            double oldBalanceDue = 0D;
            double custPrice = 0D;
            double diff = 0D;

            double trustPayments = 0D;
            double trustPaymentsReceived = 0D;
            double trustGrowth = 0D;
            double dbr = 0D;
            double insurancePayments = 0D;
            double insurancePaymentsReceived = 0;
            double insuranceGrowth = 0D;
            double cashReceived = 0D;
            double grossReceived = 0D;
            double totalAllPayments = 0D;
            double totalIgnore = 0D;
            double compDiscounts = 0D;
            double refund = 0D;
            double growth = 0D;
            double loss = 0D;

            double totalServices = 0D;
            double totalMerchandise = 0D;
            double totalCashAdvance = 0D;

            double trustDiscount = 0D;
            double trustFiled = 0D;
            double insuranceDiscount = 0D;
            double preneedDiscount = 0D;
            double packageDiscount = 0D;
            double thirdDiscount = 0D;

            double contractTotal = 0D;
            double totalCost = 0D;
            double preDiscount = 0D;
            double salesTax = 0D;

            double compD = 0D;
            double compD2 = 0D;

            double netAdjust = 0D;
            double netPrice = 0D;

            double trustFiledRemaining = 0D;

            bool doGrowth = false;
            string issueDate = "";
            string policyNumber = "";
            string payerNumber = "";
            string policyAmount = "";

            DataRow[] dRows = null;

            barImport.Show();
            barImport.Maximum = dt.Rows.Count;
            barImport.Minimum = 0;
            barImport.Value = 0;
            barImport.Refresh();

            //Trust85.FindContract(dt, "WF18208LI");

            bool doEvents = true;
            Form form = G1.IsFormOpen("EditCust");
            if (form != null)
            {
                doEvents = false;
                //return;
            }


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    if ( doEvents )
                        Application.DoEvents();

                    barImport.Value = i + 1;
                    barImport.Refresh();

                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (DailyHistory.gotCemetery(contractNumber))
                        continue;
                    if (contractNumber == "SX22217")
                    {
                    }
                    serviceId = dt.Rows[i]["serviceId"].ObjToString();
                    if (serviceId.ToUpper() == "WR23023")
                    {
                    }
                    if (serviceId.ToUpper() == "FO22003")
                    {
                    }
                    if (DailyHistory.gotCemetery(serviceId))
                        continue;
                    isGood = CheckForContract(contractNumber, ref gsDate);

                    if (isGood || !isGood )
                    {
                        compD = 0D;
                        trustDiscount = 0D;

                        dt.Rows[i]["GOOD"] = "Y";
                        dt.Rows[i]["gsdate"] = gsDate.ToString("yyyy-MM-dd");

                        currentPrice = dt.Rows[i]["currentPrice"].ObjToDouble();
                        salesTax = dt.Rows[i]["taxAmount"].ObjToDouble();
                        currentPrice += salesTax;
                        discount = dt.Rows[i]["preneedDiscount"].ObjToDouble();
                        discount = Math.Abs(discount);

                        packageDiscount = dt.Rows[i]["packageDiscount"].ObjToDouble();
                        packageDiscount = Math.Abs(packageDiscount);

                        discount += packageDiscount;

                        dt.Rows[i]["preneedDiscount"] = discount;

                        //currentPrice = currentPrice + discount;
                        dt.Rows[i]["currentPrice"] = currentPrice;

                        classA = dt.Rows[i]["classa"].ObjToDouble();

                        if (classA > 0D)
                        {
                            GetClassaDetail(contractNumber, serviceId, ref issueDate, ref policyNumber, ref policyAmount, ref payerNumber);
                            dt.Rows[i]["issueD"] = issueDate;
                            dt.Rows[i]["policyNumber"] = policyNumber;
                            dt.Rows[i]["payerNumber"] = payerNumber;
                            dt.Rows[i]["policyAmount"] = policyAmount;
                        }

                        //discount += classA;
                        totalReceived = dt.Rows[i]["trustAmountReceived"].ObjToDouble();
                        totalReceived = Math.Abs(totalReceived);

                        oldBalanceDue = dt.Rows[i]["balanceDue"].ObjToDouble();

                        //balanceDue = currentPrice - discount - totalReceived;
                        //dt.Rows[i]["balanceDue"] = balanceDue;

                        dValue = dt.Rows[i]["preneedDiscount"].ObjToDouble();
                        dValue = Math.Abs(dValue);
                        //dt.Rows[i]["preneedDiscount"] = discount;

                        payments = getPayments(dt, i);

                        growth = dt.Rows[i]["amountGrowth"].ObjToDouble();
                        loss = dt.Rows[i]["amountDiscount"].ObjToDouble();

                        totalAllPayments = 0D;
                        contractTotal = currentPrice;
                        totalCost = dt.Rows[i]["custPrice"].ObjToDouble();

                        //calculateTotalServices(contractNumber, ref contractTotal, ref totalCost, ref preDiscount);

                        trustPayments = dt.Rows[i]["trustPayments"].ObjToDouble();
                        trustPaymentsReceived = dt.Rows[i]["trustPaymentsreceived"].ObjToDouble();
                        trustFiledRemaining = dt.Rows[i]["trustFiledRemaining"].ObjToDouble();
                        trustGrowth = dt.Rows[i]["trustGrowth"].ObjToDouble();
                        trustFiled = dt.Rows[i]["trustAmountFiled"].ObjToDouble();
                        insurancePayments = dt.Rows[i]["insurancePayments"].ObjToDouble();
                        insurancePaymentsReceived = dt.Rows[i]["insurancePaymentsreceived"].ObjToDouble();
                        insuranceGrowth = dt.Rows[i]["insuranceGrowth"].ObjToDouble();

                        cashReceived = dt.Rows[i]["cashReceived"].ObjToDouble();
                        //if (growth > 0D)
                        //{
                        //    cashReceived += growth - payments;
                        //}
                        grossReceived = dt.Rows[i]["grossAmountReceived"].ObjToDouble();
                        //cashReceived = dt.Rows[i]["grossAmountReceived"].ObjToDouble();

                        //cashReceived = grossReceived;

                        compDiscounts = dt.Rows[i]["compDiscount"].ObjToDouble();

                        //payments = calculateTotalPayments(contractNumber, ref trustPayments, ref trustPaymentsReceived, ref insurancePayments, ref insurancePaymentsReceived, ref cashReceived, ref compDiscounts, ref classA );

                        //dt.Rows[i]["classa"] = 0D;

                        //classA += compDiscounts;
                        //if (compDiscounts > 0D)
                        //dt.Rows[i]["classa"] = compDiscounts + classA;

                        dbr = dt.Rows[i]["dbr"].ObjToDouble();

                        refund = dt.Rows[i]["refund"].ObjToDouble();
                        refund = Math.Abs(refund);
                        dt.Rows[i]["refund"] = refund;
                        //refund = 00D;

                        //totalAllPayments = trustPaymentsReceived + insurancePaymentsReceived + cashReceived + refund;
                        totalAllPayments = trustPaymentsReceived + insurancePaymentsReceived + cashReceived + dbr + classA;
                        totalAllPayments = G1.RoundValue(totalAllPayments);
                        dt.Rows[i]["totalAllPayments"] = totalAllPayments;

                        dValue = trustPaymentsReceived;
                        dValue = Math.Abs(dValue);
                        dt.Rows[i]["trustAmountReceived"] = dValue;

                        dValue = trustPayments;
                        dValue = Math.Abs(dValue);
                        dt.Rows[i]["trustPayments"] = dValue;

                        //trustDiscount = trustPayments - trustPaymentsReceived - dbr; // OLD

                        //trustDiscount = trustPayments - trustPaymentsReceived;
                        //dValue = trustDiscount;
                        //dValue = Math.Abs(dValue);
                        //dt.Rows[i]["trustDiscount"] = dValue;
                        if (trustPaymentsReceived > trustPayments )
                        {
                            //compD = trustPayments - trustPaymentsReceived;
                            //compD = G1.RoundValue(compD);

                            //dt.Rows[i]["classa"] = dt.Rows[i]["classa"].ObjToDouble() + compD;
                        }
                        else
                        {
                            trustDiscount = trustPayments - trustPaymentsReceived - dbr; // Not used
                            trustDiscount = trustFiled - trustPaymentsReceived - dbr; // Not used
                            trustDiscount = trustPayments - trustFiled; // Not used, had to add trustFiledRemaining to get proper trust discount
                            trustDiscount = trustPayments - trustPaymentsReceived - trustFiledRemaining - dbr;
                            trustDiscount = trustPayments - trustPaymentsReceived - dbr;
                            dValue = trustDiscount;
                            dValue = Math.Abs(dValue);
                            dt.Rows[i]["trustDiscount"] = dValue;
                            if (trustPayments > 0D && trustPaymentsReceived > 0D)
                                trustDiscount = trustPayments - trustPaymentsReceived - dbr;
                            else
                            {
                                if ( dValue != 0D )
                                {
                                }
                                trustDiscount = 0D;
                                dt.Rows[i]["trustDiscount"] = trustDiscount;
                            }
                        }

                        dValue = cashReceived;
                        dValue = Math.Abs(dValue);
                        dt.Rows[i]["cashReceived"] = dValue;

                        dValue = insurancePaymentsReceived;
                        dValue = Math.Abs(dValue);
                        dt.Rows[i]["insuranceAmountReceived"] = dValue;

                        dValue = insurancePaymentsReceived;
                        dValue = Math.Abs(dValue);
                        dt.Rows[i]["insurancePayments"] = dValue;

                        insuranceDiscount = insurancePayments - insurancePaymentsReceived;
                        insuranceDiscount += insuranceGrowth;
                        dValue = insuranceDiscount;
                        dValue = Math.Abs(dValue);
                        dt.Rows[i]["insuranceDiscount"] = dValue;

                        custPrice = dt.Rows[i]["custPrice"].ObjToDouble();
                        preneedDiscount = dt.Rows[i]["preneedDiscount"].ObjToDouble();
                        preneedDiscount = Math.Abs(preneedDiscount);

                        thirdDiscount = dt.Rows[i]["thirdDiscount"].ObjToDouble();
                        thirdDiscount = G1.RoundValue(thirdDiscount);

                        //dt.Rows[i]["preneedDiscount"] = preneedDiscount;

                        totalServices = dt.Rows[i]["currentServices"].ObjToDouble();
                        totalMerchandise = dt.Rows[i]["currentMerchandise"].ObjToDouble();
                        totalCashAdvance = dt.Rows[i]["cashAdvance"].ObjToDouble();

                        custPrice = dt.Rows[i]["currentServices"].ObjToDouble() + dt.Rows[i]["currentMerchandise"].ObjToDouble() + dt.Rows[i]["cashAdvance"].ObjToDouble();
                        dt.Rows[i]["currentPrice"] = custPrice;
                        dt.Rows[i]["custPrice"] = custPrice - preneedDiscount;

                        dt.Rows[i]["currentPrice"] = contractTotal;
                        dt.Rows[i]["custPrice"] = totalCost;
                        custPrice = totalCost;
                        //dt.Rows[i]["preneedDiscount"] = Math.Abs (preDiscount);

                        dt.Rows[i]["compDiscount"] = compDiscounts;

                        if (thirdDiscount != 0D)
                        {
                            preneedDiscount = dt.Rows[i]["preneedDiscount"].ObjToDouble();
                            preneedDiscount += thirdDiscount;
                            dt.Rows[i]["preneedDiscount"] = preneedDiscount;
                            custPrice -= thirdDiscount;
                            dt.Rows[i]["custPrice"] = custPrice;
                        }

                        balanceDue = custPrice - classA - trustPaymentsReceived - cashReceived - insurancePaymentsReceived - trustDiscount - insuranceDiscount - compDiscounts - dbr - compD;

                        balanceDue = custPrice - compD - compDiscounts - classA - trustDiscount + trustGrowth - trustPaymentsReceived - dbr - insuranceDiscount + insuranceGrowth - insurancePaymentsReceived - cashReceived;

                        netAdjust = trustGrowth + insuranceGrowth - compD - compDiscounts - trustDiscount - insuranceDiscount;
                        netPrice = custPrice + netAdjust;
                        dt.Rows[i]["netAdjust"] = netAdjust;
                        dt.Rows[i]["netPrice"] = netPrice;

                        //balanceDue = currentPrice - classA - trustPaymentsReceived - cashReceived - insurancePaymentsReceived - trustDiscount - insuranceDiscount - preneedDiscount;
                        //balanceDue -= refund;
                        balanceDue += refund;
                        balanceDue = G1.RoundValue(balanceDue);
                        //if (doGrowth)
                        //{
                        //    if (growth > 0D)
                        //    {
                        //        compDiscounts = dt.Rows[i]["classa"].ObjToDouble();
                        //        if (compDiscounts > growth)
                        //        {
                        //            compDiscounts -= growth;
                        //            dt.Rows[i]["classa"] = compDiscounts;
                        //            balanceDue += growth;
                        //        }
                        //    }
                        //}

                        //balanceDue = balanceDue + insuranceGrowth;

                        if (balanceDue + growth == 0D)
                        {
                            cmd = "Select * from `cust_payments` WHERE `contractNumber` = '" + contractNumber + "' AND `type` = 'Insurance Direct';";
                            payDt = G1.get_db_data(cmd);
                            if (payDt.Rows.Count > 0)
                            {
                                //preneedDiscount = dt.Rows[i]["preneedDiscount"].ObjToDouble();
                                //preneedDiscount = preneedDiscount - growth;
                                //preneedDiscount = Math.Abs(preneedDiscount);
                                //dt.Rows[i]["preneedDiscount"] = preneedDiscount;
                                compD = dt.Rows[i]["compDiscount"].ObjToDouble();
                                compD = compD - growth;
                                //dt.Rows[i]["compDiscount"] = compD;
                                balanceDue = 0D;
                            }
                        }

                        dt.Rows[i]["balanceDue"] = balanceDue;


                        //payments = payments - classA;
                        //dt.Rows[i]["payments"] = payments;

                        //balanceDue = currentPrice - discount - payments;
                        //dt.Rows[i]["balanceDue"] = balanceDue;
                    }
                    else
                        dt.Rows[i]["GOOD"] = "N";

                    serviceLoc = dt.Rows[i]["serviceLoc"].ObjToString();

                    contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                    if (loc == "HC")
                    {
                        dt.Rows[i]["contractNumber"] = "";
                        continue;
                    }
                    loc = dt.Rows[i]["serviceLoc"].ObjToString();

                    funeralClass = dt.Rows[i]["funeral_classification"].ObjToString();
                    if (funeralClass.ToUpper().IndexOf("BURIAL") >= 0)
                        dt.Rows[i]["burial"] = dt.Rows[i]["custPrice"].ObjToDouble();
                    else if (funeralClass.ToUpper().IndexOf("CREMATION") >= 0)
                        dt.Rows[i]["cremation"] = dt.Rows[i]["custPrice"].ObjToDouble();
                    else
                        dt.Rows[i]["other"] = dt.Rows[i]["custPrice"].ObjToDouble();

                    dt.Rows[i]["total"] = dt.Rows[i]["custPrice"].ObjToDouble();

                    dr = dd.Select("atneedcode='" + loc + "'");
                    if (dr.Length > 0)
                        dt.Rows[i]["Location Name"] = dr[0]["LocationCode"].ObjToString();
                    else
                        dt.Rows[i]["Location Name"] = loc;

                    dRows = dd.Select("merchandiseCode='" + serviceLoc + "'");
                    if (dRows.Length > 0)
                    {
                        string lName = dRows[0]["LocationCode"].ObjToString();
                        dt.Rows[i]["Location Name"] = dRows[0]["LocationCode"].ObjToString();
                        if (chkExcludeMerch.Checked)
                            dt.Rows[i]["GOOD"] = "BAD";
                    }
                    dt.Rows[i]["loc"] = loc;

                    dt.Rows[i]["name"] = dt.Rows[i]["firstName"] + " " + dt.Rows[i]["lastName"].ObjToString();

                    currentPrice = dt.Rows[i]["currentPrice"].ObjToDouble();
                    preDiscount = dt.Rows[i]["preneedDiscount"].ObjToDouble();
                    custPrice = dt.Rows[i]["custPrice"].ObjToDouble();
                    trustDiscount = dt.Rows[i]["trustDiscount"].ObjToDouble();
                    insuranceDiscount = dt.Rows[i]["insuranceDiscount"].ObjToDouble();
                    classA = dt.Rows[i]["classa"].ObjToDouble();
                    totalReceived = dt.Rows[i]["totalAllPayments"].ObjToDouble();
                    diff = currentPrice - preDiscount - compDiscounts - totalReceived - trustDiscount - insuranceDiscount - classA;
                    diff = G1.RoundValue(diff);
                    //if (diff != balanceDue )
                    //    dt.Rows[i]["diff"] = "Y";

                    if (oldBalanceDue != balanceDue)
                        dt.Rows[i]["diff"] = "Y";
                }
                catch (Exception ex)
                {
                }
            }

            if (dt.Rows.Count > 1)
            {
                barImport.Value = barImport.Maximum - 1;
                barImport.Refresh();
            }

            dRows = dt.Select("GOOD<>'BAD'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            else
                dt.Rows.Clear();

            DataView tempview = dt.DefaultView;
            tempview.Sort = "servicelOC";
            dt = tempview.ToTable();

            //for ( int i=0; i<dt.Rows.Count; i++)
            //{
            //    preneedDiscount = dt.Rows[i]["preneedDiscount"].ObjToDouble();
            //    preneedDiscount = Math.Abs(preneedDiscount);
            //    dt.Rows[i]["preneedDiscount"] = preneedDiscount;
            //}

            return dt;
        }
        //****************************************************************************************/
        private bool GetClassaDetail ( string contractNumber, string serviceId, ref string issueDate, ref string policyNumber, ref string policyAmount, ref string payerNumber )
        {
            issueDate = "";
            policyNumber = "";
            payerNumber = "";
            policyAmount = "";
            if (String.IsNullOrWhiteSpace(contractNumber))
                return false;
            DataTable dt = null;
            string payer = "";
            string policy = "";
            string date = "";
            string type = "";
            string reference = "";
            string trust_policy = "";
            double money = 0D;
            string report = "";
            string deleteFlag = "";
            string service = "";
            string[] Lines = null;
            DateTime deceasedDate = DateTime.Now;
            bool rv = false;
            DateTime limitDate = new DateTime(2020, 1, 1);
            DataTable policyDt = new DataTable();
            policyDt.Columns.Add("issueDate");
            policyDt.Columns.Add("policyNumber");
            policyDt.Columns.Add("policyAmount");
            policyDt.Columns.Add("payerNumber");

            DataRow[] dRows = null;
            DataRow dRow = null;

            string cmd = "Select * from `cust_payments` WHERE `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);

            for (int i = 0; i < dx.Rows.Count; i++)
            {
                type = dx.Rows[i]["type"].ObjToString().ToUpper();
                if ( type == "CLASS A")
                {
                    trust_policy = dx.Rows[i]["trust_policy"].ObjToString();
                    reference = dx.Rows[i]["referenceNumber"].ObjToString();
                    Lines = trust_policy.Split('/');
                    if ( Lines.Length >= 2 )
                    {
                        payer = Lines[0].Trim();
                        cmd = "Select * from `policies` WHERE `policyNumber` = '" + reference + "';";
                        dt = G1.get_db_data(cmd);
                        if ( dt.Rows.Count > 0 )
                        {
                            for (int j = 0; j < dt.Rows.Count; j++)
                            {
                                report = dt.Rows[j]["report"].ObjToString();
                                if (String.IsNullOrWhiteSpace(report))
                                    continue;
                                deleteFlag = dt.Rows[j]["deleteFlag"].ObjToString();
                                if (!String.IsNullOrWhiteSpace(deleteFlag))
                                    continue;
                                deceasedDate = dt.Rows[j]["deceasedDate"].ObjToDateTime();
                                if (deceasedDate < limitDate)
                                    continue;
                                service = dt.Rows[j]["serviceId"].ObjToString();
                                if ( !String.IsNullOrWhiteSpace ( service ))
                                {
                                    if (service != serviceId)
                                        continue;
                                }
                                date = dt.Rows[j]["issueDate8"].ObjToDateTime().ToString("MM/dd/yyyy");
                                money = dt.Rows[j]["liability"].ObjToDouble();

                                dRows = policyDt.Select("issueDate='" + date + "' AND policyNumber='" + reference + "' AND payerNumber='" + payer + "'");
                                if ( dRows.Length == 0 )
                                {
                                    dRow = policyDt.NewRow();
                                    dRow["issueDate"] = date;
                                    dRow["policyNumber"] = reference;
                                    dRow["payerNumber"] = payer;
                                    dRow["policyAmount"] = G1.ReformatMoney(money);
                                    policyDt.Rows.Add(dRow);
                                }

                                //issueDate += date + "\n";
                                //policyNumber += reference + "\n";
                                //payerNumber += payer + "\n";
                            }
                            rv = true;
                        }
                    }
                }
            }

            for (int j = 0; j < policyDt.Rows.Count; j++)
            {
                date = policyDt.Rows[j]["issueDate"].ObjToString();
                reference = policyDt.Rows[j]["policyNumber"].ObjToString();
                payer = policyDt.Rows[j]["payerNumber"].ObjToString();
                policy = policyDt.Rows[j]["policyAmount"].ObjToString();

                issueDate += date + "\n";
                policyNumber += reference + "\n";
                payerNumber += payer + "\n";
                policyAmount += policy + "\n";
            }

            issueDate = issueDate.TrimEnd('\n');
            policyNumber = policyNumber.TrimEnd('\n');
            payerNumber = payerNumber.TrimEnd('\n');
            policyAmount = policyAmount.TrimEnd('\n');
            return rv;
        }
        ///****************************************************************************************/
        //private DataTable LoadData(DateTime startDate, DateTime stopDate, DataTable mainDt, bool ytd)
        //{
        //    DateTime start = startDate;
        //    string date1 = G1.DateTimeToSQLDateTime(start);
        //    DateTime stop = stopDate;
        //    string date2 = G1.DateTimeToSQLDateTime(stop);
        //    string contractNumber = "";
        //    string loc = "";
        //    string contract = "";
        //    string trust = "";
        //    double contractValue = 0D;
        //    double downPayment = 0D;
        //    double payments = 0D;
        //    int idx = 0;
        //    string ch = "";
        //    string serviceId = "";
        //    string search = cmbSearch.Text.ToUpper();

        //    string cmd = "Select * from `fcust_extended` c JOIN `fcustomers` a on c.`contractNumber` = a.`contractNumber` where ";
        //    if (search == "DECEASED DATE")
        //        cmd += " a.`deceasedDate` >= '" + date1 + "' AND a.`deceasedDate` <= '" + date2 + "' ";
        //    else if (search == "CREATE DATE")
        //        cmd += " c.`caseCreatedDate` >= '" + date1 + "' AND c.`caseCreatedDate` <= '" + date2 + "' ";
        //    else
        //        cmd += " c.`serviceDate` >= '" + date1 + "' AND c.`serviceDate` <= '" + date2 + "' ";

        //    string names = getLocationNameQuery();
        //    if (!String.IsNullOrWhiteSpace(names))
        //        cmd += " AND " + names + " ";

        //    contract = txtContract.Text.Trim();
        //    if (!string.IsNullOrWhiteSpace(contract))
        //    {
        //        cmd = "Select * from `fcust_extended` c JOIN `fcustomers` a on c.`contractNumber` = a.`contractNumber` where ";
        //        cmd += " c.`serviceId` = '" + contract + "' ";
        //    }

        //    //cmd += " AND `serviceLoc` <> 'NM' ";
        //    cmd += ";";
        //    DataTable dt = G1.get_db_data(cmd);

        //    dt.Columns.Add("name");
        //    dt.Columns.Add("burial", Type.GetType("System.Double"));
        //    dt.Columns.Add("cremation", Type.GetType("System.Double"));
        //    dt.Columns.Add("other", Type.GetType("System.Double"));
        //    dt.Columns.Add("total", Type.GetType("System.Double"));
        //    dt.Columns.Add("payments", Type.GetType("System.Double"));

        //    //dt.Columns.Add("trustPayments", Type.GetType("System.Double"));
        //    dt.Columns.Add("trustDiscount", Type.GetType("System.Double"));

        //    dt.Columns.Add("insuranceAmountReceived", Type.GetType("System.Double"));
        //    //dt.Columns.Add("insurancePayments", Type.GetType("System.Double"));
        //    dt.Columns.Add("insuranceDiscount", Type.GetType("System.Double"));

        //    dt.Columns.Add("totalAllPayments", Type.GetType("System.Double"));

        //    //dt.Columns.Add("cashReceived", Type.GetType("System.Double"));
        //    dt.Columns.Add("loc");
        //    dt.Columns.Add("Location Name");
        //    dt.Columns.Add("GOOD");
        //    dt.Columns.Add("gsdate");

        //    bool isGood = false;
        //    DateTime gsDate = DateTime.Now;

        //    cmd = "Select * from `funeralhomes` order by `keycode`;";
        //    DataTable dd = G1.get_db_data(cmd);

        //    DataRow[] dr = null;
        //    string deceasedDate = "";
        //    DateTime ddate = DateTime.Now;
        //    string funeralClass = "";
        //    double dValue = 0D;
        //    string serviceLoc = "";

        //    double currentPrice = 0D;
        //    double discount = 0D;
        //    double classA = 0D;
        //    double totalReceived = 0D;
        //    double balanceDue = 0D;
        //    double custPrice = 0D;

        //    double trustPayments = 0D;
        //    double trustPaymentsReceived = 0D;
        //    double insurancePayments = 0D;
        //    double insurancePaymentsReceived = 0;
        //    double cashReceived = 0D;
        //    double totalAllPayments = 0D;
        //    double totalIgnore = 0D;
        //    double compDiscounts = 0D;

        //    double totalServices = 0D;
        //    double totalMerchandise = 0D;
        //    double totalCashAdvance = 0D;

        //    double trustDiscount = 0D;
        //    double insuranceDiscount = 0D;
        //    double preneedDiscount = 0D;

        //    double contractTotal = 0D;
        //    double totalCost = 0D;
        //    double preDiscount = 0D;

        //    DataRow[] dRows = null;

        //    barImport.Show();
        //    barImport.Maximum = dt.Rows.Count;
        //    barImport.Minimum = 0;
        //    barImport.Value = 0;
        //    barImport.Refresh();

        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        try
        //        {
        //            Application.DoEvents();

        //            barImport.Value = i + 1;
        //            barImport.Refresh();

        //            contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
        //            if (contractNumber == "B18019L")
        //            {
        //            }
        //            serviceId = dt.Rows[i]["serviceId"].ObjToString();
        //            if (serviceId.ToUpper() == "BS22002")
        //            {
        //            }
        //            isGood = CheckForContract(contractNumber, ref gsDate);

        //            if (isGood || !isGood)
        //            {
        //                dt.Rows[i]["GOOD"] = "Y";
        //                dt.Rows[i]["gsdate"] = gsDate.ToString("yyyy-MM-dd");

        //                currentPrice = dt.Rows[i]["currentPrice"].ObjToDouble();
        //                discount = dt.Rows[i]["preneedDiscount"].ObjToDouble();
        //                discount = Math.Abs(discount);

        //                dt.Rows[i]["preneedDiscount"] = discount;

        //                currentPrice = currentPrice + discount;
        //                dt.Rows[i]["currentPrice"] = currentPrice;

        //                classA = dt.Rows[i]["classa"].ObjToDouble();
        //                discount += classA;
        //                totalReceived = dt.Rows[i]["trustAmountReceived"].ObjToDouble();
        //                totalReceived = Math.Abs(totalReceived);

        //                balanceDue = currentPrice - discount - totalReceived;
        //                //dt.Rows[i]["balanceDue"] = balanceDue;

        //                dValue = dt.Rows[i]["preneedDiscount"].ObjToDouble();
        //                dValue = Math.Abs(dValue);
        //                //dt.Rows[i]["preneedDiscount"] = discount;

        //                payments = getPayments(dt, i);

        //                totalAllPayments = 0D;

        //                calculateTotalServices(contractNumber, ref contractTotal, ref totalCost, ref preDiscount);

        //                payments = calculateTotalPayments(contractNumber, ref trustPayments, ref trustPaymentsReceived, ref insurancePayments, ref insurancePaymentsReceived, ref cashReceived, ref compDiscounts, ref classA);

        //                //dt.Rows[i]["classa"] = 0D;

        //                //classA += compDiscounts;
        //                //if (compDiscounts > 0D)
        //                dt.Rows[i]["classa"] = compDiscounts + classA;

        //                totalAllPayments = trustPaymentsReceived + insurancePaymentsReceived + cashReceived;
        //                dt.Rows[i]["totalAllPayments"] = totalAllPayments;

        //                dValue = trustPaymentsReceived;
        //                dValue = Math.Abs(dValue);
        //                dt.Rows[i]["trustAmountReceived"] = dValue;

        //                dValue = trustPayments;
        //                dValue = Math.Abs(dValue);
        //                dt.Rows[i]["trustPayments"] = dValue;

        //                trustDiscount = trustPayments - trustPaymentsReceived;
        //                dValue = trustDiscount;
        //                dValue = Math.Abs(dValue);
        //                dt.Rows[i]["trustDiscount"] = dValue;

        //                dValue = cashReceived;
        //                dValue = Math.Abs(dValue);
        //                dt.Rows[i]["cashReceived"] = dValue;

        //                dValue = insurancePaymentsReceived;
        //                dValue = Math.Abs(dValue);
        //                dt.Rows[i]["insuranceAmountReceived"] = dValue;

        //                dValue = insurancePaymentsReceived;
        //                dValue = Math.Abs(dValue);
        //                dt.Rows[i]["insurancePayments"] = dValue;

        //                insuranceDiscount = insurancePayments - insurancePaymentsReceived;
        //                dValue = insuranceDiscount;
        //                dValue = Math.Abs(dValue);
        //                dt.Rows[i]["insuranceDiscount"] = dValue;

        //                custPrice = dt.Rows[i]["custPrice"].ObjToDouble();
        //                preneedDiscount = dt.Rows[i]["preneedDiscount"].ObjToDouble();
        //                preneedDiscount = Math.Abs(preneedDiscount);

        //                dt.Rows[i]["preneedDiscount"] = preneedDiscount;

        //                totalServices = dt.Rows[i]["currentServices"].ObjToDouble();
        //                totalMerchandise = dt.Rows[i]["currentMerchandise"].ObjToDouble();
        //                totalCashAdvance = dt.Rows[i]["cashAdvance"].ObjToDouble();

        //                custPrice = dt.Rows[i]["currentServices"].ObjToDouble() + dt.Rows[i]["currentMerchandise"].ObjToDouble() + dt.Rows[i]["cashAdvance"].ObjToDouble();
        //                dt.Rows[i]["currentPrice"] = custPrice;
        //                dt.Rows[i]["custPrice"] = custPrice - preneedDiscount;

        //                dt.Rows[i]["currentPrice"] = contractTotal;
        //                dt.Rows[i]["custPrice"] = totalCost;
        //                custPrice = totalCost;
        //                dt.Rows[i]["preneedDiscount"] = Math.Abs(preDiscount);

        //                balanceDue = custPrice - classA - trustPaymentsReceived - cashReceived - insurancePaymentsReceived - trustDiscount - insuranceDiscount - compDiscounts;
        //                //balanceDue = currentPrice - classA - trustPaymentsReceived - cashReceived - insurancePaymentsReceived - trustDiscount - insuranceDiscount - preneedDiscount;
        //                dt.Rows[i]["balanceDue"] = balanceDue;


        //                //payments = payments - classA;
        //                //dt.Rows[i]["payments"] = payments;

        //                //balanceDue = currentPrice - discount - payments;
        //                //dt.Rows[i]["balanceDue"] = balanceDue;
        //            }
        //            else
        //                dt.Rows[i]["GOOD"] = "N";

        //            serviceLoc = dt.Rows[i]["serviceLoc"].ObjToString();

        //            contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
        //            if (loc == "HC")
        //            {
        //                dt.Rows[i]["contractNumber"] = "";
        //                continue;
        //            }
        //            loc = dt.Rows[i]["serviceLoc"].ObjToString();

        //            funeralClass = dt.Rows[i]["funeral_classification"].ObjToString();
        //            if (funeralClass.ToUpper().IndexOf("BURIAL") >= 0)
        //                dt.Rows[i]["burial"] = dt.Rows[i]["custPrice"].ObjToDouble();
        //            else if (funeralClass.ToUpper().IndexOf("CREMATION") >= 0)
        //                dt.Rows[i]["cremation"] = dt.Rows[i]["custPrice"].ObjToDouble();
        //            else
        //                dt.Rows[i]["other"] = dt.Rows[i]["custPrice"].ObjToDouble();

        //            dt.Rows[i]["total"] = dt.Rows[i]["custPrice"].ObjToDouble();

        //            dr = dd.Select("atneedcode='" + loc + "'");
        //            if (dr.Length > 0)
        //                dt.Rows[i]["Location Name"] = dr[0]["LocationCode"].ObjToString();
        //            else
        //                dt.Rows[i]["Location Name"] = loc;

        //            dRows = dd.Select("merchandiseCode='" + serviceLoc + "'");
        //            if (dRows.Length > 0)
        //            {
        //                string lName = dRows[0]["LocationCode"].ObjToString();
        //                dt.Rows[i]["Location Name"] = dRows[0]["LocationCode"].ObjToString();
        //                if (chkExcludeMerch.Checked)
        //                    dt.Rows[i]["GOOD"] = "BAD";
        //            }
        //            dt.Rows[i]["loc"] = loc;

        //            dt.Rows[i]["name"] = dt.Rows[i]["firstName"] + " " + dt.Rows[i]["lastName"].ObjToString();
        //        }
        //        catch (Exception ex)
        //        {
        //        }
        //    }

        //    dRows = dt.Select("GOOD<>'BAD'");
        //    if (dRows.Length > 0)
        //        dt = dRows.CopyToDataTable();
        //    else
        //        dt.Rows.Clear();

        //    DataView tempview = dt.DefaultView;
        //    tempview.Sort = "servicelOC";
        //    dt = tempview.ToTable();

        //    //for ( int i=0; i<dt.Rows.Count; i++)
        //    //{
        //    //    preneedDiscount = dt.Rows[i]["preneedDiscount"].ObjToDouble();
        //    //    preneedDiscount = Math.Abs(preneedDiscount);
        //    //    dt.Rows[i]["preneedDiscount"] = preneedDiscount;
        //    //}

        //    return dt;
        //}
        /****************************************************************************************/
        private void calculateTotalServices ( string contractNumber, ref double contractTotal, ref double totalCost, ref double preDiscount )
        {
            string cmd = "Select * from `fcust_services` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);

            if (G1.get_column_number(dt, "select") < 0)
                dt.Columns.Add("select");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["select"] = dt.Rows[i]["pSelect"].ObjToString();

            dt.Columns["pastPrice"].ColumnName = "currentprice";

            FunServices serviceForm = new FunServices(contractNumber);
            dt = serviceForm.funServicesDT.Copy();

            contractTotal = 0D;
            totalCost = 0D;
            preDiscount = 0D;

            FunServices.CalcTotalServices(dt, ref contractTotal, ref totalCost, ref preDiscount, true);
        }
        /****************************************************************************************/
        private double calculateTotalPayments( string contractNumber, ref double trustPayments, ref double trustPaymentsReceived, ref double insurancePayments, ref double insurancePaymentsReceived, ref double cashReceived, ref double compDiscounts, ref double classA, ref double dbr )
        {
            trustPayments = 0D;
            trustPaymentsReceived = 0D;
            insurancePayments = 0D;
            insurancePaymentsReceived = 0D;
            cashReceived = 0D;
            compDiscounts = 0D;
            classA = 0D;

            string type = "";
            double price = 0D;
            double total = 0D;
            string status = "";
            double paid = 0D;
            double received = 0D;

            string record = "";
            DataRow[] dRows = null;

            string cmd = "Select * from `cust_payments` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                status = dx.Rows[i]["status"].ObjToString().Trim().ToUpper();
                if (status == "CANCELLED")
                    continue;
                type = dx.Rows[i]["type"].ObjToString().Trim().ToUpper();
                if (type == "DISCOUNT")
                {
                    if (status == "ACCEPT" || status == "DEPOSITED")
                        compDiscounts += dx.Rows[i]["payment"].ObjToDouble();
                }
                else if (type == "CLASS A")
                {
                    if (status == "ACCEPT" || status == "DEPOSITED" || status == "PENDING" )
                        classA += dx.Rows[i]["payment"].ObjToDouble();
                }
                //else if (type == "CREDIT CARD")
                //{
                //    if (status == "ACCEPT" || status == "DEPOSITED")
                //        cashReceived += dx.Rows[i]["payment"].ObjToDouble();
                //}
                //else if (type == "CHECK")
                //{
                //    if (status == "ACCEPT" || status == "DEPOSITED")
                //        cashReceived += dx.Rows[i]["payment"].ObjToDouble();
                //}
                //else if (type == "CASH")
                //{
                //    if (status == "ACCEPT" || status == "DEPOSITED")
                //        cashReceived += dx.Rows[i]["payment"].ObjToDouble();
                //}
            }

            cmd = "Select * from `cust_payment_details` where `contractNumber` = '" + contractNumber + "' ;";
            DataTable dt = G1.get_db_data(cmd);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    status = dt.Rows[i]["status"].ObjToString().Trim().ToUpper();
                    if (status == "CANCELLED")
                        continue;
                    type = dt.Rows[i]["type"].ObjToString().Trim().ToUpper();
                    //if (type.ToUpper() == "REFUND")
                    //{
                    //    price = dt.Rows[i]["payment"].ObjToDouble();
                    //    price = Math.Abs(price);
                    //    price = price * -1D;
                    //    //dt.Rows[i]["payment"] = price;
                    //    total += price;
                    //    continue;
                    //}

                    if ( status.ToUpper() != "DEPOSITED")
                        continue;

                    record = dt.Rows[i]["paymentRecord"].ObjToString();
                    //dRows = dx.Select("record='" + record + "'");
                    //if ( dRows.Length > 0 )
                    //{
                    //    if ( dRows[0]["status"].ObjToString().ToUpper() == "ACCEPT" || dRows[0]["status"].ObjToString().ToUpper() == "DEPOSITED")
                    //    {
                    //        if (dRows[0]["type"].ObjToString().ToUpper() == "CHECK" || dRows[0]["type"].ObjToString().ToUpper() == "CREDIT CARD" || dRows[0]["type"].ObjToString().ToUpper() == "CASH" )
                    //            continue;
                    //    }
                    //}


                    paid = dt.Rows[i]["paid"].ObjToDouble();
                    received = dt.Rows[i]["amtActuallyReceived"].ObjToDouble();

                    if (type == "TRUST")
                    {
                        trustPayments += paid;
                        if (received == 0D)
                            received = paid;
                        trustPaymentsReceived += received;
                    }
                    else if (type == "CHECK-REMOTE")
                        cashReceived += paid;
                    else if (type == "CHECK-LOCAL")
                        cashReceived += paid;
                    else if (type == "CASH")
                        cashReceived += paid;
                    else if (type == "CREDIT CARD")
                        cashReceived += paid;
                    else if ( type.IndexOf ( "INSURANCE") == 0 )
                    {
                        insurancePayments += paid;
                        insurancePaymentsReceived += received;
                    }

                    //if (String.IsNullOrWhiteSpace(status) || status.ToUpper() == "DEPOSITED")
                    //{
                    //    price = dt.Rows[i]["payment"].ObjToDouble();
                    //    total += price;
                    //}
                    //else if (type == "DISCOUNT")
                    //{
                    //    price = dt.Rows[i]["payment"].ObjToDouble();
                    //    total += price;
                    //}
                    //else if (status == "ACCEPT")
                    //{
                    //    if (type == "CASH" || type == "CHECK" || type == "CREDIT CARD" || type == "CLASS A")
                    //    {
                    //        price = dt.Rows[i]["payment"].ObjToDouble();
                    //        total += price;
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                }
            }
            //status = G1.ReformatMoney(total);
            return total;
        }
        /****************************************************************************************/
        private double calculateTotalPaymentsx(string contractNumber)
        {
            string cmd = "Select * from `cust_payments` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            double price = 0D;
            double total = 0D;
            string status = "";
            string type = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    status = dt.Rows[i]["status"].ObjToString().Trim().ToUpper();
                    if (status == "CANCELLED")
                        continue;
                    type = dt.Rows[i]["type"].ObjToString().Trim().ToUpper();
                    //if (String.IsNullOrWhiteSpace(status) || status.ToUpper() == "ACCEPT")
                    //{
                    //    price = dt.Rows[i]["payment"].ObjToDouble();
                    //    total += price;
                    //}
                    //else if (String.IsNullOrWhiteSpace(status) || status.ToUpper() == "PENDING")
                    //{
                    //    price = dt.Rows[i]["payment"].ObjToDouble();
                    //    total += price;
                    //}
                    if (type.ToUpper() == "REFUND")
                    {
                        price = dt.Rows[i]["payment"].ObjToDouble();
                        price = Math.Abs(price);
                        price = price * -1D;
                        //dt.Rows[i]["payment"] = price;
                        total += price;
                        continue;
                    }
                    if (String.IsNullOrWhiteSpace(status) || status.ToUpper() == "DEPOSITED")
                    {
                        price = dt.Rows[i]["payment"].ObjToDouble();
                        total += price;
                    }
                    else if (type == "DISCOUNT")
                    {
                        price = dt.Rows[i]["payment"].ObjToDouble();
                        total += price;
                    }
                    else if (status == "ACCEPT")
                    {
                        if (type == "CASH" || type == "CHECK" || type == "CREDIT CARD" || type == "CLASS A")
                        {
                            price = dt.Rows[i]["payment"].ObjToDouble();
                            total += price;
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            status = G1.ReformatMoney(total);
            return total;
        }
        /****************************************************************************************/
        private double getPayments(DataTable dt, int i)
        {
            double payments = 0D;
            string cc = dt.Rows[i]["creditCard"].ObjToString();
            string str = "";
            if (!String.IsNullOrWhiteSpace(cc))
            {
                str = cc.Replace("CC - ", "");
                if (!String.IsNullOrWhiteSpace(str))
                {
                    string[] Lines = str.Split(' ');
                    for (int j = 0; j < Lines.Length; j++)
                    {
                        str = Lines[j].Trim();
                        if (G1.validate_numeric(str))
                            payments += str.ObjToDouble();
                    }
                }
            }
            string cash = dt.Rows[i]["cash"].ObjToString();
            if (!String.IsNullOrWhiteSpace(cash))
            {
                str = cash.Replace("CA - ", "");
                if (!String.IsNullOrWhiteSpace(str))
                {
                    string[] Lines = str.Split(' ');
                    for (int j = 0; j < Lines.Length; j++)
                    {
                        str = Lines[j].Trim();
                        if (G1.validate_numeric(str))
                            payments += str.ObjToDouble();
                    }
                }
            }
            string check = dt.Rows[i]["check"].ObjToString();
            if (!String.IsNullOrWhiteSpace(check))
            {
                str = check.Replace("CK - ", "");
                if (!String.IsNullOrWhiteSpace(str))
                {
                    string[] Lines = str.Split(' ');
                    for (int j = 0; j < Lines.Length; j++)
                    {
                        str = Lines[j].Trim();
                        if (G1.validate_numeric(str))
                            payments += str.ObjToDouble();
                    }
                }
            }
            return payments;
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
                    dt.Rows[i]["Location Name"] = location;
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawGroupRow(object sender, DevExpress.XtraGrid.Views.Base.RowObjectCustomDrawEventArgs e)
        {
            GridGroupRowInfo info = e.Info as GridGroupRowInfo;
            string location = info.GroupText;
            location = location.Replace("$,", "");
            location = location.Replace("$", "").Trim();
            info.GroupText = location;
            //int idx = location.LastIndexOf(']');
            //if (idx > 0)
            //{
            //    location = location.Substring(idx + 1);
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
            gridMain.OptionsPrint.ExpandAllGroups = false;

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

            Font saveFont = gridMain.AppearancePrint.Row.Font;

            if (doSimple)
            {
                Font newFont = new Font(saveFont.FontFamily, 5F);
                gridMain.Appearance.Row.Font = newFont;
            }

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();

            gridMain.Appearance.Row.Font = saveFont;
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gridMain.OptionsPrint.ExpandAllGroups = false;

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

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Regular);
            //string title = "Contract Activity Report";
            //Printer.DrawQuad(6, 8, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            string reportName = "Contract Activity Report";
            string report = reportName + " Report for " + this.dateTimePicker1.Value.ToString("MM/dd/yyyy") + " through " + this.dateTimePicker2.Value.ToString("MM/dd/yyyy");
            Printer.DrawQuad(5, 8, 8, 4, report, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);



            DateTime date = this.dateTimePicker1.Value;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            //Printer.DrawQuad(20, 8, 5, 4, "Month Closing - " + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private bool pageBreak = false;
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            int rowHandle = e.RowHandle;
            if (gridMain.IsDataRow(rowHandle))
            {
                try
                {
                    DataTable dt = (DataTable)dgv.DataSource;
                    int row = gridMain.GetDataSourceRowIndex(rowHandle);

                    string newPage = dt.Rows[row]["contracts"].ObjToString();
                    if (newPage.ToUpper() == "BREAK")
                    {
                        pageBreak = true;
                        e.Cancel = true;
                    }
                }
                catch ( Exception ex)
                {
                }
            }
            if ( e.HasFooter )
            {
                if ( chkPageBreaks.Checked )
                    pageBreak = true;
            }
        }
        /****************************************************************************************/
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (pageBreak)
                e.PS.InsertPageBreak(e.Y);
            pageBreak = false;
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            if ( chkExcludeZero.Checked )
            {
                int row = e.ListSourceRow;
                if (row >= 0)
                {
                    if (gridMain.IsDataRow(row))
                    {
                        DataTable dt = (DataTable)dgv.DataSource;
                        double balance = dt.Rows[row]["balanceDue"].ObjToDouble();
                        balance = G1.RoundValue(balance);
                        string serviceId = dt.Rows[row]["serviceId"].ObjToString();
                        if (balance == 0D)
                        {
                            e.Visible = false;
                            e.Handled = true;
                            return;
                        }
                    }
                }
            }
            if ( chkCliffDiff.Checked )
            {
                int row = e.ListSourceRow;
                if (row >= 0)
                {
                    if (gridMain.IsDataRow(row))
                    {
                        DataTable dt = (DataTable)dgv.DataSource;
                        double balance = dt.Rows[row]["balanceDue"].ObjToDouble();
                        balance = G1.RoundValue(balance);
                        double cliffdiff = dt.Rows[row]["cliffBalance"].ObjToDouble();
                        cliffdiff = G1.RoundValue(cliffdiff);
                        if (balance == cliffdiff)
                        {
                            e.Visible = false;
                            e.Handled = true;
                            return;
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            G1.ShowHideFindPanel(gridMain);
        }
        /****************************************************************************************/
        private double originalSize = 0D;
        private Font mainFont = null;
        private Font newFont = null;
        private Font HeaderFont = null;
        private double originalHeaderSize = 0D;
        private void ScaleCells()
        {
            if (originalSize == 0D)
            {
                //                originalSize = gridMain.Columns["address1"].AppearanceCell.FontSizeDelta.ObjToDouble();
                originalSize = gridMain.Columns["Location Name"].AppearanceCell.Font.Size;
                mainFont = gridMain.Columns["Location Name"].AppearanceCell.Font;
                HeaderFont = gridMain.Appearance.HeaderPanel.Font;
                originalHeaderSize = gridMain.Appearance.HeaderPanel.Font.Size;
            }
            double scale = txtScale.Text.ObjToDouble();
            double size = scale / 100D * originalSize;
            Font font = new Font(mainFont.Name, (float)size);

            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                gridMain.Columns[i].AppearanceCell.Font = font;
            }
            gridMain.Appearance.GroupFooter.Font = font;
            gridMain.Appearance.FooterPanel.Font = font;
            gridMain.AppearancePrint.FooterPanel.Font = font;
            gridMain.AppearancePrint.GroupFooter.Font = font;
            newFont = font;
            size = scale / 100D * originalHeaderSize;
            font = new Font(HeaderFont.Name, (float)size, FontStyle.Bold);
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                gridMain.Columns[i].AppearanceHeader.Font = font;
            }
            //gridMain.Appearance.HeaderPanel.Font = font;
            //gridMain.AppearancePrint.HeaderPanel.Font = font;
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
        private void runData ()
        {
            gridMain.Columns["Location Name"].GroupIndex = 0;

            DateTime start = this.dateTimePicker1.Value;
            DateTime stop = this.dateTimePicker2.Value;

            DataTable dt = null;

            //if ( chkSwitch.Checked )
            //    dt = LoadData(start, stop, null, false);
            //else
                dt = LoadDataFast(start, stop, null, false);

            string loc = "";

            DataRow dR = null;
            DataRow[] dRows = null;

            DataView tempview = dt.DefaultView;
            tempview.Sort = "Location Name asc, serviceId asc";
            dt = tempview.ToTable();

            BuildSummaryTable(dt);

            if ( chkHonorDeposits.Checked )
            {
                dt = honorDeposits(dt);
            }

            dt = CliffBalance(dt);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.Refresh();

            gridMain.Columns["num"].Visible = true;
            gridMain.Columns["loc"].Visible = false;
            gridMain.Columns["Location Name"].Visible = false;
            gridMain.Columns["contractNumber"].Visible = false;

            gridMain.ClearSorting();
            gridMain.Columns["ServiceId"].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;

            gridMain.OptionsView.ShowFooter = true;
            gridMain.OptionsView.ShowBands = false;

            gridMain.OptionsPrint.PrintBandHeader = false;
            gridMain.OptionsPrint.PrintFooter = true;

            gridMain.Appearance.Row.Font = new Font("Tahoma", 9F);
            gridMain.AppearancePrint.Row.Font = new Font("Tahoma", 9F);

            gridMain.ExpandAllGroups();
        }
        /****************************************************************************************/
        private DataTable CliffBalance ( DataTable dt )
        {
            if (G1.get_column_number(dt, "cliffBalance") < 0)
                dt.Columns.Add("cliffBalance", Type.GetType("System.Double"));

            gridMain.Columns["cliffBalance"].Visible = true;
            double currentPrice = 0D;
            double preneedDiscount = 0D;
            double custPrice = 0D;
            double netAdjust = 0D;
            double netPrice = 0D;
            double classa = 0D;
            double compDiscount = 0D;
            double trustDiscount = 0D;
            double trustGrowth = 0D;
            double trustAmountReceived = 0D;
            double insuranceDiscount = 0D;
            double insuranceGrowth = 0D;
            double insurancePayments = 0D;
            double cashReceived = 0D;
            double totalAllPayments = 0D;
            double refund = 0D;
            double dbr = 0D;
            double balanceDue = 0D;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                try
                {
                    currentPrice = dt.Rows[i]["currentPrice"].ObjToDouble();
                    preneedDiscount = dt.Rows[i]["preneedDiscount"].ObjToDouble();
                    custPrice = dt.Rows[i]["custPrice"].ObjToDouble();
                    netAdjust = dt.Rows[i]["netAdjust"].ObjToDouble();
                    netPrice = dt.Rows[i]["netPrice"].ObjToDouble();
                    classa = dt.Rows[i]["classa"].ObjToDouble();
                    compDiscount = dt.Rows[i]["compDiscount"].ObjToDouble();
                    trustDiscount = dt.Rows[i]["trustDiscount"].ObjToDouble();
                    trustGrowth = dt.Rows[i]["trustGrowth"].ObjToDouble();
                    trustAmountReceived = dt.Rows[i]["trustAmountReceived"].ObjToDouble();
                    insuranceDiscount = dt.Rows[i]["insuranceDiscount"].ObjToDouble();
                    insuranceGrowth = dt.Rows[i]["insuranceGrowth"].ObjToDouble();
                    insurancePayments = dt.Rows[i]["insurancePayments"].ObjToDouble();
                    cashReceived = dt.Rows[i]["cashReceived"].ObjToDouble();
                    totalAllPayments = dt.Rows[i]["totalAllPayments"].ObjToDouble();
                    refund = dt.Rows[i]["refund"].ObjToDouble();
                    dbr = dt.Rows[i]["dbr"].ObjToDouble();
                    balanceDue = currentPrice - preneedDiscount - compDiscount - classa - trustAmountReceived - trustDiscount - dbr - insurancePayments - insuranceDiscount - cashReceived + trustGrowth + insuranceGrowth + refund;
                    dt.Rows[i]["cliffBalance"] = balanceDue;
                }
                catch ( Exception ex)
                {
                }
            }
            return dt;
        }
        /****************************************************************************************/
        private DataTable summaryDt = null;
        private void BuildSummaryTable ( DataTable dt)
        {
            summaryDt = dt.Clone();

            double burial = 0D;
            double cremation = 0D;
            double other = 0D;
            string loc = "";
            DataRow dR = null;
            DataRow[] dRows = null;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                loc = dt.Rows[i]["loc"].ObjToString();

                burial = dt.Rows[i]["burial"].ObjToDouble();
                cremation = dt.Rows[i]["cremation"].ObjToDouble();
                other = dt.Rows[i]["other"].ObjToDouble();

                dRows = summaryDt.Select("loc='" + loc + "'");
                if ( dRows.Length <= 0 )
                {
                    dR = summaryDt.NewRow();
                    dR["loc"] = loc;
                    dR["burial"] = 0D;
                    dR["cremation"] = 0D;
                    dR["other"] = 0D;
                    if (burial > 0D)
                        dR["burial"] = 1D;
                    if (cremation > 0D)
                        dR["cremation"] = 1D;
                    if (other > 0D)
                        dR["other"] = 1D;

                    summaryDt.Rows.Add(dR);
                }
                else
                {
                    if (burial > 0D)
                    {
                        burial = dRows[0]["burial"].ObjToDouble();
                        dRows[0]["burial"] = burial + 1D;
                    }
                    if (cremation > 0D)
                    {
                        cremation = dRows[0]["cremation"].ObjToDouble();
                        dRows[0]["cremation"] = cremation + 1D;
                    }
                    if (other > 0D)
                    {
                        other = dRows[0]["other"].ObjToDouble();
                        dRows[0]["other"] = other + 1D;
                    }
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.ListSourceRowIndex == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return;
            double dValue = 0D;
            if (e.Column.FieldName.ToUpper() == "BURIAL")
            {
                dValue = e.DisplayText.ObjToDouble();
                if (dValue > 0D)
                    e.DisplayText = "X";
                else
                    e.DisplayText = "";
            }
            if (e.Column.FieldName.ToUpper() == "CREMATION")
            {
                dValue = e.DisplayText.ObjToDouble();
                if (dValue > 0D)
                    e.DisplayText = "X";
                else
                    e.DisplayText = "";
            }
            if (e.Column.FieldName.ToUpper() == "OTHER")
            {
                dValue = e.DisplayText.ObjToDouble();
                if (dValue > 0D)
                    e.DisplayText = "X";
                else
                    e.DisplayText = "";
            }
            else if (e.Column.FieldName.ToUpper() == "TOTAL")
            {
                if (e.DisplayText.Trim() == "0.00")
                    e.DisplayText = "-          ";
            }
            else if (e.Column.FieldName.ToUpper() == "PRENEEDDISCOUNT")
            {
                e.DisplayText = e.DisplayText.Replace("-", "");
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

            if (e.DisplayText.Trim() == "0.00")
                e.DisplayText = "";

        }
        /****************************************************************************************/
        private void gridMain_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                double dValue = 0D;
                string column = e.Column.FieldName.ToUpper();
                DataTable dt = (DataTable)dgv.DataSource;
                int row = gridMain.GetDataSourceRowIndex(e.RowHandle);

                if (column.ToUpper() == "BURIAL")
                {
                    string data = dt.Rows[row][column].ObjToString();
                    if (!String.IsNullOrWhiteSpace(data))
                    {
                        dValue = data.ObjToDouble();
                        if ( dValue > 0D )
                            e.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
                    }
                }
                else if (column.ToUpper() == "CREMATION")
                {
                    string data = dt.Rows[row][column].ObjToString();
                    if (!String.IsNullOrWhiteSpace(data))
                    {
                        dValue = data.ObjToDouble();
                        if (dValue > 0D)
                            e.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
                    }
                }
                else if (column.ToUpper() == "OTHER")
                {
                    string data = dt.Rows[row][column].ObjToString();
                    if (!String.IsNullOrWhiteSpace(data))
                    {
                        dValue = data.ObjToDouble();
                        if (dValue > 0D)
                            e.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
                    }
                }

                if ( chkHonorDeposits.Checked )
                {
                    if (G1.get_column_number(dt, "gotPayments") < 0)
                        return;
                    string data = dt.Rows[row]["gotPayments"].ObjToString().ToUpper();
                    if (data == "YES")
                    {
                        if (column.Trim().ToUpper() == "TOTALALLPAYMENTS")
                            e.Appearance.BackColor = Color.Yellow;
                        else if (column.Trim().ToUpper() == "TRUSTAMOUNTRECEIVED")
                            e.Appearance.BackColor = Color.Yellow;
                        else if (column.Trim().ToUpper() == "TRUSTPAYMENTS")
                            e.Appearance.BackColor = Color.Yellow;
                        else if (column.Trim().ToUpper() == "TRUSTDISCOUNT")
                            e.Appearance.BackColor = Color.Yellow;
                        else if (column.Trim().ToUpper() == "CASHRECEIVED")
                            e.Appearance.BackColor = Color.Yellow;
                        else if (column.Trim().ToUpper() == "INSURANCEAMOUNTRECEIVED")
                            e.Appearance.BackColor = Color.Yellow;
                        else if (column.Trim().ToUpper() == "INSURANCEPAYMENTS")
                            e.Appearance.BackColor = Color.Yellow;
                        else if (column.Trim().ToUpper() == "BALANCEDUE")
                            e.Appearance.BackColor = Color.Yellow;
                        else if (column.Trim().ToUpper() == "CLASSA")
                            e.Appearance.BackColor = Color.Yellow;
                    }
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                if (chkHonorDeposits.Checked)
                {
                    this.Cursor = Cursors.WaitCursor;
                    using (FunPayments editFunPayments = new FunPayments(this, contract, "", false, true))
                    {
                        editFunPayments.TopMost = true;
                        editFunPayments.ShowDialog();
                    }
                    this.Cursor = Cursors.Default;
                }
                else
                {
                    Form form = G1.IsFormOpen("EditCust", contract);
                    if (form != null)
                    {
                        form.Show();
                        form.WindowState = FormWindowState.Maximized;
                        form.Visible = true;
                        form.BringToFront();
                    }
                    else
                    {
                        EditCust custForm = new EditCust(contract);
                        custForm.Tag = contract;
                        custForm.Show();
                    }
                }
                //CustomerDetails clientForm = new CustomerDetails(contract);
                //clientForm.ShowDialog();
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
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
            //if ( e.Column.FieldName.ToUpper() == "CONTRACTS")
            //{
            //    if (e.RowHandle >= 0)
            //    {
            //        if (e.DisplayText.ToUpper() == "BREAK")
            //            e.DisplayText = "";
            //    }
            //}
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawRowFooterCell(object sender, FooterCellCustomDrawEventArgs e)
        {
            if (1 == 1)
                return;
            string columnName = e.Column.FieldName.ObjToString().ToUpper();
            if (columnName != "LOCATION NAME")
                return;
            int rowHandle = e.RowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataTable dt = (DataTable)dgv.DataSource;

            string location = "XYZZY";

            string loc = dt.Rows[row]["loc"].ObjToString();
            DataRow[] dRows = summaryDt.Select("loc='" + loc + "'");
            if (dRows.Length > 0)
                location = "B=" + dRows[0]["burial"].ToString();

            int dx = e.Bounds.Height;
            Brush brush = e.Cache.GetGradientBrush(e.Bounds, Color.Wheat, Color.FloralWhite, LinearGradientMode.Vertical);
            Rectangle r = e.Bounds;
            //Draw a 3D border 
            BorderPainter painter = BorderHelper.GetPainter(DevExpress.XtraEditors.Controls.BorderStyles.Style3D);
            AppearanceObject borderAppearance = new AppearanceObject(e.Appearance);
            borderAppearance.BorderColor = Color.DarkGray;
            painter.DrawObject(new BorderObjectInfoArgs(e.Cache, borderAppearance, r));
            //Fill the inner region of the cell 
            r.Inflate(-1, -1);
            e.Cache.FillRectangle(brush, r);
            //Draw a summary value 
            r.Inflate(-2, 0);
            e.Appearance.DrawString(e.Cache, location, r);
            //            e.Appearance.DrawString(e.Cache, e.Info.DisplayText, r);
            //Prevent default drawing of the cell 
            e.Handled = true;
        }
        /****************************************************************************************/
        private int mainCount = 0;
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            if (1 == 1)
                return;
            string field = (e.Item as GridSummaryItem).FieldName.ObjToString();
            if (field.ToUpper() != "SERVICEID" && field.ToUpper() != "LOCATION NAME" && field.ToUpper() != "NAME" && field.ToUpper() != "CONTRACTNUMBER" )
                return;

            if (!e.IsGroupSummary && !e.IsTotalSummary )
                return;


            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = e.RowHandle;

            double burials = 0;
            double cremations = 0;
            double other = 0;
            double total = 0;

            if (e.IsGroupSummary)
            {
                int row = gridMain.GetDataSourceRowIndex(rowHandle);
                string loc = dt.Rows[row]["loc"].ObjToString();
                DataRow[] dRows = summaryDt.Select("loc='" + loc + "'");

                if (dRows.Length > 0)
                {
                    burials = dRows[0]["burial"].ObjToDouble();
                    cremations = dRows[0]["cremation"].ObjToDouble();
                    other = dRows[0]["other"].ObjToDouble();
                    total = burials + cremations + other;
                }
            }
            else
            {
                for ( int i=0; i<summaryDt.Rows.Count; i++)
                {
                    burials += summaryDt.Rows[i]["burial"].ObjToDouble();
                    cremations += summaryDt.Rows[i]["cremation"].ObjToDouble();
                    other += summaryDt.Rows[i]["other"].ObjToDouble();
                }
                total = burials + cremations + other;
            }

            string unprintable = "   ";
            //StringBuilder sb = new StringBuilder(unprintable);
            //for (int i = 0; i < unprintable.Length; i++)
            //    sb[i] = (char)127;
            //unprintable = sb.ToString();


            if (field.ToUpper() == "LOCATION NAME")
                e.TotalValue = "Burials=" + burials.ToString() + unprintable;
            else if (field.ToUpper() == "SERVICEID")
                e.TotalValue = "Cremations=" + cremations.ToString() + unprintable;
            else if (field.ToUpper() == "NAME")
                e.TotalValue = "Other=" + other.ToString() + unprintable;
            else if (field.ToUpper() == "CONTRACTNUMBER")
                e.TotalValue = "Total=" + total.ToString() + unprintable;
        }

        private void gridMain_CustomDrawFooterCell(object sender, FooterCellCustomDrawEventArgs e)
        {
            string columnName = e.Column.FieldName.ObjToString().ToUpper();
            if (columnName != "BURIAL")
                return;
            int rowHandle = e.RowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataTable dt = (DataTable)dgv.DataSource;
            string location = "ABC123";
            //bandLocation = location;

            int dx = e.Bounds.Height;
            Brush brush = e.Cache.GetGradientBrush(e.Bounds, Color.Wheat, Color.FloralWhite, LinearGradientMode.Vertical);
            Rectangle r = e.Bounds;
            //Draw a 3D border 
            BorderPainter painter = BorderHelper.GetPainter(DevExpress.XtraEditors.Controls.BorderStyles.Style3D);
            AppearanceObject borderAppearance = new AppearanceObject(e.Appearance);
            borderAppearance.BorderColor = Color.DarkGray;
            painter.DrawObject(new BorderObjectInfoArgs(e.Cache, borderAppearance, r));
            //Fill the inner region of the cell 
            r.Inflate(-1, -1);
            e.Cache.FillRectangle(brush, r);
            //Draw a summary value 
            r.Inflate(-2, 0);
            e.Appearance.DrawString(e.Cache, location, r);
            //            e.Appearance.DrawString(e.Cache, e.Info.DisplayText, r);
            //Prevent default drawing of the cell 
            e.Handled = true;

        }
        /****************************************************************************************/
        private void chkExcludeZero_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void rebalanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();

            DataTable dt = (DataTable)dgv.DataSource;

            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string record = dr["record1"].ObjToString();
            string record2 = dr["record"].ObjToString();
            string contractNumber = dr["contractNumber"].ObjToString();

            string cmd = "Select * from `fcust_extended` WHERE `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                record = dx.Rows[0]["record"].ObjToString();
                Funerals.CalculateCustomerDetails(contractNumber, record, dr);
            }
        }
        /****************************************************************************************/
        private void txtContract_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string serviceId = txtContract.Text.Trim();
                if (!String.IsNullOrWhiteSpace(serviceId))
                    btnRun_Click(null, null);
            }
        }
        /****************************************************************************************/
        private void rebalanceAllDiffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow[] dRows = dt.Select("diff='Y'");
            if ( dRows.Length <= 0 )
            {
                MessageBox.Show("*** Info *** There are no Differences to Rebalance", "Rebalance Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            DataRow dr = null;
            string record = "";
            string contractNumber = "";
            DataTable dx = dRows.CopyToDataTable();
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                record = dx.Rows[i]["record1"].ObjToString();
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                dr = dx.Rows[i];
                Funerals.CalculateCustomerDetails(contractNumber, record, dr);
            }
            MessageBox.Show("*** Info *** Finished Rebalancing", "Rebalance Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        /****************************************************************************************/
        private void cmbDisplay_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetupDisplay();
        }
        /****************************************************************************************/
        private void chkCompress_CheckedChanged(object sender, EventArgs e)
        {
            if ( !chkCompress.Checked )
                gridMain.ExpandAllGroups();
            else
                gridMain.CollapseAllGroups();
        }
        /****************************************************************************************/
        private void showPaymentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DataRow dr = gridMain.GetFocusedDataRow();

            string contractNumber = dr["contractNumber"].ObjToString();
            FunPayments editFunPayments = new FunPayments(null, contractNumber, "", false, true);
            editFunPayments.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void menuRebalanceAll_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = null;
            string record = "";
            string contractNumber = "";

            int[] rows = gridMain.GetSelectedRows();
            int lastRow = dt.Rows.Count;
            lastRow = rows.Length;
            int row = 0;

            for (int i = 0; i < lastRow; i++)
            {
                Application.DoEvents();

                row = rows[i];
                row = gridMain.GetDataSourceRowIndex(row);

                record = dt.Rows[row]["record1"].ObjToString();
                contractNumber = dt.Rows[row]["contractNumber"].ObjToString();
                dr = dt.Rows[row];
                Funerals.CalculateCustomerDetails(contractNumber, record, dr);

                gridMain.RefreshData();
                gridMain.RefreshEditor(true);
                dgv.Refresh();
            }
            MessageBox.Show("*** Info *** Finished Rebalancing", "Rebalance Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        /****************************************************************************************/
        private void button2_Click(object sender, EventArgs e)
        { // Move Left
            DateTime now = this.dateTimePicker3.Value;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker3.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker4.Value = new DateTime(now.Year, now.Month, days);
        }
        /****************************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        { // Move Right
            DateTime now = this.dateTimePicker4.Value;
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker3.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker4.Value = new DateTime(now.Year, now.Month, days);
        }
        /****************************************************************************************/
        private DataTable honorDeposits(DataTable dt)
        {
            barImport.Refresh();

            DateTime start = this.dateTimePicker3.Value;
            DateTime stop = this.dateTimePicker4.Value;
            string fStart = start.ToString("yyyy-MM-dd");
            string fStop = stop.ToString("yyyy-MM-dd");
            string contractNumber = "";
            DataTable payDt = null;
            DataTable dx = null;

            //dt = RemoveDuplicates(dt);

            string cmd = "Select * from `cust_payment_details` WHERE `dateReceived` >= '" + fStart + "' AND `dateReceived` <= '" + fStop + "';";
            payDt = G1.get_db_data(cmd);
            DataRow[] dRows = null;

            dt.Columns.Add("gotPayments");

            double trustPayments = 0D;
            double totalPayments = 0D;

            double trustPaymentsReceived, insurancePayments, insurancePaymentsReceived, cashReceived, compDiscounts, classA, trustFiledRemaining, thirdDiscount, trustGrowth, insuranceGrowth, preDiscount;
            double refund = 0D;
            double totalAllPayments = 0D;
            double oldAllPayments = 0D;
            double dbr = 0D;
            double dValue = 0D;
            double trustDiscount = 0D;
            double insuranceDiscount = 0D;
            double custPrice = 0D;
            double preneedDiscount = 0D;
            double balanceDue = 0D;
            double oldBalanceDue = 0D;
            double newBalanceDue = 0D;
            double compD = 0D;
            double trustFiled = 0D;
            double totalServices = 0D;
            double totalMerchandise = 0D;
            double totalCashAdvance = 0D;
            double totalCost = 0D;
            double netPrice = 0D;
            double netAdjust = 0D;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    continue;
                dRows = payDt.Select("contractNumber='" + contractNumber + "'");
                if (dRows.Length <= 0)
                    continue;
                dt.Rows[i]["gotPayments"] = "YES";
            }

            string str = "";
            //for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            //{
            //    str = dt.Rows[i]["gotPayments"].ObjToString().ToUpper();
            //    if (str != "YES")
            //    {
            //        dt.Rows.RemoveAt(i);
            //        continue;
            //    }
            //}

            DataTable ddx = null;

            barImport.Show();
            barImport.Maximum = dt.Rows.Count;
            barImport.Minimum = 0;
            barImport.Value = 0;
            barImport.Refresh();

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                Application.DoEvents();

                barImport.Value = i + 1;
                barImport.Refresh();

                netPrice = dt.Rows[i]["netPrice"].ObjToDouble();
                if (netPrice <= 0D)
                    continue;

                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    continue;
                if ( contractNumber == "C23022L" )
                {
                }
                dRows = payDt.Select("contractNumber='" + contractNumber + "'");
                if (dRows.Length <= 0)
                    continue;
                dt.Rows[i]["gotPayments"] = "YES";
                dx = dRows.CopyToDataTable();

                trustPaymentsReceived = 0D;
                insurancePayments = 0D;
                insurancePaymentsReceived = 0D;
                preDiscount = 0D;
                insuranceGrowth = 0D;
                trustGrowth = 0D;
                thirdDiscount = 0D;
                trustFiledRemaining = 0D;
                classA = 0D;
                refund = 0D;
                compDiscounts = 0D;
                cashReceived = 0D;

                str = dt.Rows[i]["gotPayments"].ObjToString().ToUpper();

                if (str != "YES")
                {
                    dt.Rows[i]["totalAllPayments"] = 0D;
                    dt.Rows[i]["trustAmountReceived"] = 0D;
                    dt.Rows[i]["trustPayments"] = 0D;
                    dt.Rows[i]["classa"] = 0D;
                    dt.Rows[i]["dbr"] = 0D;
                    dt.Rows[i]["refund"] = 0D;
                    dt.Rows[i]["trustDiscount"] = 0D;
                    dt.Rows[i]["cashReceived"] = 0D;
                    dt.Rows[i]["insuranceAmountReceived"] = 0D;
                    dt.Rows[i]["insurancePayments"] = 0D;
                    continue;
                }

                cmd = "Select * from `cust_payments` where `contractNumber` = '" + contractNumber + "';";
                ddx = G1.get_db_data(cmd);

                totalPayments = calculateTotalPaymentsNew(dx, ddx, contractNumber, ref trustPayments, ref trustPaymentsReceived, ref insurancePayments, ref insurancePaymentsReceived, ref cashReceived, ref compDiscounts, ref classA, ref trustFiledRemaining, ref thirdDiscount, ref trustGrowth, ref insuranceGrowth, ref preDiscount, ref dbr );

                trustFiled = trustFiledRemaining;

                dbr = dt.Rows[i]["dbr"].ObjToDouble();
                netPrice = dt.Rows[i]["netPrice"].ObjToDouble();
                refund = dt.Rows[i]["refund"].ObjToDouble();
                netAdjust = dt.Rows[i]["netAdjust"].ObjToDouble();
                //netPrice -= netAdjust;

                newBalanceDue = CalculateBalanceDue(contractNumber, ddx, netPrice, dbr );

                oldAllPayments = dt.Rows[i]["totalAllPayments"].ObjToDouble();
                oldBalanceDue = dt.Rows[i]["balanceDue"].ObjToDouble();

                totalAllPayments = trustPaymentsReceived + insurancePaymentsReceived + cashReceived + dbr + classA;
                totalAllPayments = G1.RoundValue(totalAllPayments);
                dt.Rows[i]["totalAllPayments"] = totalAllPayments;

                oldAllPayments = oldBalanceDue + totalAllPayments;

                oldBalanceDue = oldAllPayments;
                balanceDue = oldAllPayments - totalAllPayments;
                balanceDue = G1.RoundValue(balanceDue);
                //dt.Rows[i]["balanceDue"] = balanceDue;

                //oldAllPayments = totalAllPayments;

                //balanceDue = netPrice - (oldBalanceDue + totalAllPayments);

                dValue = trustPaymentsReceived;
                dValue = Math.Abs(dValue);
                dt.Rows[i]["trustAmountReceived"] = dValue;

                dValue = trustPayments;
                dValue = Math.Abs(dValue);
                dt.Rows[i]["trustPayments"] = dValue;

                dValue = classA;
                dValue = Math.Abs(dValue);
                dt.Rows[i]["classa"] = dValue;

                //if (1 == 1)
                //    continue;

                if (trustPaymentsReceived > trustPayments)
                {
                    //compD = trustPayments - trustPaymentsReceived;
                    //compD = G1.RoundValue(compD);

                    //dt.Rows[i]["classa"] = dt.Rows[i]["classa"].ObjToDouble() + compD;
                }
                else
                {
                    trustDiscount = trustPayments - trustPaymentsReceived - dbr; // Not used
                    trustDiscount = trustFiled - trustPaymentsReceived - dbr; // Not used
                    trustDiscount = trustPayments - trustFiled; // Not used, had to add trustFiledRemaining to get proper trust discount
                    trustDiscount = trustPayments - trustPaymentsReceived - trustFiledRemaining - dbr;
                    trustDiscount = trustPayments - trustPaymentsReceived - dbr;
                    dValue = trustDiscount;
                    dValue = Math.Abs(dValue);
                    dt.Rows[i]["trustDiscount"] = dValue;
                    if (trustPayments > 0D && trustPaymentsReceived > 0D)
                        trustDiscount = trustPayments - trustPaymentsReceived - dbr;
                    else
                    {
                        if (dValue != 0D)
                        {
                        }
                        trustDiscount = 0D;
                        dt.Rows[i]["trustDiscount"] = trustDiscount;
                    }
                }

                dValue = cashReceived;
                dValue = Math.Abs(dValue);
                dt.Rows[i]["cashReceived"] = dValue;

                dValue = insurancePaymentsReceived;
                dValue = Math.Abs(dValue);
                dt.Rows[i]["insuranceAmountReceived"] = dValue;

                dValue = insurancePaymentsReceived;
                dValue = Math.Abs(dValue);
                dt.Rows[i]["insurancePayments"] = dValue;

                insuranceDiscount = insurancePayments - insurancePaymentsReceived;
                insuranceDiscount += insuranceGrowth;
                dValue = insuranceDiscount;
                dValue = Math.Abs(dValue);
                //dt.Rows[i]["insuranceDiscount"] = dValue;

                custPrice = dt.Rows[i]["custPrice"].ObjToDouble();
                preneedDiscount = dt.Rows[i]["preneedDiscount"].ObjToDouble();
                preneedDiscount = Math.Abs(preneedDiscount);

                thirdDiscount = dt.Rows[i]["thirdDiscount"].ObjToDouble();
                thirdDiscount = G1.RoundValue(thirdDiscount);

                //dt.Rows[i]["preneedDiscount"] = preneedDiscount;

                totalServices = dt.Rows[i]["currentServices"].ObjToDouble();
                totalMerchandise = dt.Rows[i]["currentMerchandise"].ObjToDouble();
                totalCashAdvance = dt.Rows[i]["cashAdvance"].ObjToDouble();
                totalCost = dt.Rows[i]["custPrice"].ObjToDouble();

                custPrice = dt.Rows[i]["currentServices"].ObjToDouble() + dt.Rows[i]["currentMerchandise"].ObjToDouble() + dt.Rows[i]["cashAdvance"].ObjToDouble();
                //dt.Rows[i]["currentPrice"] = custPrice;
                //dt.Rows[i]["custPrice"] = custPrice - preneedDiscount;

                //dt.Rows[i]["currentPrice"] = contractTotal;
//                dt.Rows[i]["custPrice"] = totalCost;
                custPrice = totalCost;
                //dt.Rows[i]["preneedDiscount"] = Math.Abs (preDiscount);

                //dt.Rows[i]["compDiscount"] = compDiscounts;

                if (thirdDiscount != 0D)
                {
                    preneedDiscount = dt.Rows[i]["preneedDiscount"].ObjToDouble();
                    preneedDiscount += thirdDiscount;
                    //dt.Rows[i]["preneedDiscount"] = preneedDiscount;
                    custPrice -= thirdDiscount;
                    //dt.Rows[i]["custPrice"] = custPrice;
                }

                balanceDue = custPrice - classA - trustPaymentsReceived - cashReceived - insurancePaymentsReceived - trustDiscount - insuranceDiscount - compDiscounts - dbr - compD;

                balanceDue = custPrice - compD - compDiscounts - classA - trustDiscount + trustGrowth - trustPaymentsReceived - dbr - insuranceDiscount + insuranceGrowth - insurancePaymentsReceived - cashReceived;

                balanceDue = netPrice - totalAllPayments;
                //balanceDue = netPrice - oldAllPayments;

                //netAdjust = trustGrowth + insuranceGrowth - compD - compDiscounts - trustDiscount - insuranceDiscount;
                //netPrice = custPrice + netAdjust;
                //dt.Rows[i]["netAdjust"] = netAdjust;
                //dt.Rows[i]["netPrice"] = netPrice;

                //balanceDue = currentPrice - classA - trustPaymentsReceived - cashReceived - insurancePaymentsReceived - trustDiscount - insuranceDiscount - preneedDiscount;
                //balanceDue -= refund;
                //balanceDue += refund;
                balanceDue = G1.RoundValue(balanceDue);
                dt.Rows[i]["balanceDue"] = balanceDue;

                newBalanceDue = G1.RoundValue(newBalanceDue);
                dt.Rows[i]["balanceDue"] = newBalanceDue;
            }

            barImport.Refresh();

            return dt;
        }
        /****************************************************************************************/
        private DataTable RemoveDuplicates ( DataTable dt)
        {
            DateTime date = DateTime.Now;
            try
            {
                for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                {
                    date = dt.Rows[i]["serviceDate"].ObjToDateTime();
                    if (date.Year < 1000)
                        dt.Rows.RemoveAt(i);
                }
            }
            catch ( Exception ex)
            {
            }
            return dt;
        }
        /****************************************************************************************/
        private double calculateTotalPaymentsNew ( DataTable dt, DataTable ddx, string contractNumber, ref double trustPayments, ref double trustPaymentsReceived, ref double insurancePayments, ref double insurancePaymentsReceived, ref double cashReceived, ref double compDiscounts, ref double classA, ref double trustFiledRemaining, ref double thirdDiscount, ref double trustGrowth, ref double insuranceGrowth, ref double preDiscount, ref double dbr )
        {
            trustPayments = 0D;
            trustPaymentsReceived = 0D;
            trustGrowth = 0D;
            insurancePayments = 0D;
            insurancePaymentsReceived = 0D;
            insuranceGrowth = 0D;
            cashReceived = 0D;
            compDiscounts = 0D;
            classA = 0D;
            dbr = 0D;
            trustFiledRemaining = 0D;
            thirdDiscount = 0D;

            DateTime start = this.dateTimePicker3.Value;
            DateTime stop = this.dateTimePicker4.Value;
            DateTime date = DateTime.Now;

            string type = "";
            double price = 0D;
            double total = 0D;
            string status = "";
            double paid = 0D;
            double received = 0D;

            string record = "";
            string paymentRecord = "";
            string paymentType = "";
            DataRow[] dRows = null;

            //string cmd = "Select * from `cust_payments` where `contractNumber` = '" + contractNumber + "';";
            //DataTable ddx = G1.get_db_data(cmd);

            for (int i = 0; i < ddx.Rows.Count; i++)
            {
                status = ddx.Rows[i]["status"].ObjToString().Trim().ToUpper();
                if (status == "CANCELLED")
                    continue;
                type = ddx.Rows[i]["type"].ObjToString().Trim().ToUpper();
                //if (type == "DISCOUNT")
                //{
                //    if (status == "ACCEPT" || status == "DEPOSITED")
                //    {
                //        if (SMFS.activeSystem.ToUpper() == "OTHER")
                //            preDiscount += dx.Rows[i]["payment"].ObjToDouble();
                //        else
                //            compDiscounts += dx.Rows[i]["payment"].ObjToDouble();
                //    }
                //}
                if (type == "CLASS A")
                {
                    date = ddx.Rows[i]["dateEntered"].ObjToDateTime();
                    if (date >= start && date <= stop)
                    {
                        if (status == "ACCEPT" || status == "DEPOSITED" || status == "PENDING")
                            classA += ddx.Rows[i]["payment"].ObjToDouble();
                    }
                }
                //else if (type == "OTHER")
                //{
                //    if (status == "ACCEPT" || status == "DEPOSITED")
                //    {
                //        cashReceived += dx.Rows[i]["payment"].ObjToDouble();
                //    }
                //}
            }

            //cmd = "Select * from `cust_payment_details` where `contractNumber` = '" + contractNumber + "' ;";
            //DataTable dt = G1.get_db_data(cmd);

            dRows = dt.Select("type='trust' AND amtActuallyReceived = '0' AND trustAmtFiled > '0'");
            if (dRows.Length > 0)
            {
                for (int i = 0; i < dRows.Length; i++)
                    trustFiledRemaining += dRows[i]["trustAmtFiled"].ObjToDouble();
            }

            string paidFrom = "";
            string fatherType = "";

            double trustPaid = 0D;
            double trustReceived = 0D;

            dRows = dt.Select("status='DEPOSITED' AND type='Trust'");
            if (dRows.Length > 1)
            {
                trustPaid = dRows[0]["paid"].ObjToDouble();
                trustReceived = dRows[0]["amtActuallyReceived"].ObjToDouble();
                for (int i = 1; i < dRows.Length; i++)
                {
                    paid = dRows[i]["paid"].ObjToDouble();
                    received = dRows[i]["amtActuallyReceived"].ObjToDouble();
                    trustPaid += paid;
                    trustReceived += received;
                    dRows[i]["paid"] = 0D;
                    dRows[i]["amtActuallyReceived"] = 0D;
                }
                dRows[0]["paid"] = trustPaid;
                dRows[0]["amtActuallyReceived"] = trustReceived;
            }

            bool trustIsPaid = false;
            bool insuranceIsPaid = false;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    trustIsPaid = false;
                    insuranceIsPaid = false;

                    status = dt.Rows[i]["status"].ObjToString().Trim().ToUpper();
                    if (status == "CANCELLED")
                        continue;
                    type = dt.Rows[i]["type"].ObjToString().Trim().ToUpper();
                    fatherType = type;

                    if (status.ToUpper() != "DEPOSITED")
                        continue;

                    paidFrom = dt.Rows[i]["paidFrom"].ObjToString();
                    paid = dt.Rows[i]["paid"].ObjToDouble();
                    received = dt.Rows[i]["amtActuallyReceived"].ObjToDouble();

                    record = dt.Rows[i]["paymentRecord"].ObjToString();
                    dRows = ddx.Select("record='" + record + "'");
                    if (dRows.Length > 0)
                    {
                        fatherType = dRows[0]["type"].ObjToString().ToUpper();
                        if (fatherType.ToUpper() == "TRUST")
                        {
                            if (received > paid && paid == 0D)
                            {
                            }
                            if (received > paid && paid != 0D)
                            {
                                trustGrowth += received - paid;
                                trustPaymentsReceived += received;
                                trustIsPaid = true;
                            }
                        }
                        else if (fatherType.ToUpper().IndexOf("INSURANCE") == 0)
                        {
                            //if (fatherType.ToUpper().IndexOf("DIRECT") > 0)
                            //{
                            if (received > paid)
                                insuranceGrowth += received - paid;
                            //else
                            //    insuranceGrowth += received - paid;
                            //}
                        }
                        else if (fatherType.ToUpper().IndexOf("3RD PARTY") == 0)
                        {
                            //if (fatherType.ToUpper().IndexOf("DIRECT") > 0)
                            //{
                            if (received > paid)
                                insuranceGrowth += received - paid;
                            //else
                            //    insuranceGrowth += received - paid;
                            //}
                        }

                        //    if ( dRows[0]["status"].ObjToString().ToUpper() == "ACCEPT" || dRows[0]["status"].ObjToString().ToUpper() == "DEPOSITED")
                        //    {
                        //        if (dRows[0]["type"].ObjToString().ToUpper() == "CHECK" || dRows[0]["type"].ObjToString().ToUpper() == "CREDIT CARD" || dRows[0]["type"].ObjToString().ToUpper() == "CASH" )
                        //            continue;
                        //    }
                    }


                    paidFrom = dt.Rows[i]["paidFrom"].ObjToString();
                    paid = dt.Rows[i]["paid"].ObjToDouble();
                    received = dt.Rows[i]["amtActuallyReceived"].ObjToDouble();
                    if (received > 0D && paid != received)
                    {
                        paymentRecord = dt.Rows[i]["paymentRecord"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(paymentRecord))
                        {
                            dRows = ddx.Select("record='" + paymentRecord + "'");
                            if (dRows.Length > 0)
                            {
                                paymentType = dRows[0]["type"].ObjToString().ToUpper();
                                if (paymentType == "3RD PARTY")
                                {
                                    //thirdDiscount += paid - received;
                                    //paid = received;
                                    insurancePayments += paid;
                                    insurancePaymentsReceived += received;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (fatherType == "3RD PARTY")
                        {
                            //thirdDiscount += paid - received;
                            //paid = received;
                            insurancePayments += paid;
                            insurancePaymentsReceived += received;
                        }
                        else if (fatherType == "TRUST")
                        {

                        }
                    }

                    if (fatherType.ToUpper() == "3RD PARTY")
                        continue;

                    if (type == "TRUST")
                    {
                        trustPayments += paid;
                        if (received == 0D)
                            received = paid;
                        if (!trustIsPaid)
                            trustPaymentsReceived += received;
                    }
                    else if (type == "CHECK-REMOTE")
                    {
                        if (fatherType.IndexOf("INSURANCE") == 0)
                        {
                            insurancePayments += paid;
                            insurancePaymentsReceived += received;
                        }
                        else
                        {
                            if (fatherType != "TRUST")
                            {
                                if (String.IsNullOrWhiteSpace(paidFrom))
                                    cashReceived += paid;
                                else
                                    cashReceived += received;
                            }
                            else
                            {
                                trustPaymentsReceived += received;
                            }
                        }
                    }
                    else if (type == "CHECK-LOCAL")
                    {
                        if (fatherType.IndexOf("INSURANCE") == 0)
                        {
                            insurancePayments += paid;
                            insurancePaymentsReceived += received;
                        }
                        else
                        {
                            if (String.IsNullOrWhiteSpace(paidFrom))
                                cashReceived += paid;
                            else
                            {
                                if (paidFrom.ToUpper() != "MFDA")
                                    cashReceived += received;
                            }
                        }
                    }
                    else if (type == "CASH")
                        cashReceived += paid;
                    else if (type == "LOCKBOX DBR")
                        cashReceived += paid;
                    else if (type == "CREDIT CARD")
                        cashReceived += paid;
                    else if (type.IndexOf("INSURANCE") == 0)
                    {
                        insurancePayments += paid;
                        insurancePaymentsReceived += received;
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return total;
        }
        /****************************************************************************************/
        private double CalculateBalanceDue ( string contractNumber, DataTable ddx, double newPrice, double dbr )
        {
            double balanceDue = 0D;
            DateTime start = this.dateTimePicker3.Value;
            DateTime stop = this.dateTimePicker4.Value;
            string fStart = start.ToString("yyyy-MM-dd");
            string fStop = stop.ToString("yyyy-MM-dd");

            DateTime dateReceived = DateTime.Now;
            string status = "";
            double amountActuallyReceived = 0D;
            double totalPayments = 0D;
            double classA = 0D;
            double refund = 0D;
            string type = "";
            DateTime date = DateTime.Now;
            double preDiscount = 0D;
            double compDiscounts = 0D;

            for (int i = 0; i < ddx.Rows.Count; i++)
            {
                dateReceived = ddx.Rows[i]["dateEntered"].ObjToDateTime();
                if (dateReceived < start || dateReceived > stop)
                    continue;
                status = ddx.Rows[i]["status"].ObjToString().Trim().ToUpper();
                if (status == "CANCELLED")
                    continue;
                type = ddx.Rows[i]["type"].ObjToString().Trim().ToUpper();
                if (type == "DISCOUNT")
                {
                    if (status == "ACCEPT" || status == "DEPOSITED")
                    {
                        if (SMFS.activeSystem.ToUpper() == "OTHER")
                            preDiscount += ddx.Rows[i]["payment"].ObjToDouble();
                        else
                            compDiscounts += ddx.Rows[i]["payment"].ObjToDouble();
                    }
                }
                if (type == "CLASS A")
                {
                    date = ddx.Rows[i]["dateEntered"].ObjToDateTime();
                    if ( date <= stop)
                    {
                        if (status == "ACCEPT" || status == "DEPOSITED" || status == "PENDING")
                            classA += ddx.Rows[i]["payment"].ObjToDouble();
                    }
                }
                if (type == "REFUND")
                {
                    date = ddx.Rows[i]["dateEntered"].ObjToDateTime();
                    if (date <= stop)
                    {
                        if (status == "ACCEPT" || status == "DEPOSITED" || status == "PENDING")
                            refund += ddx.Rows[i]["payment"].ObjToDouble();
                    }
                }
                //else if (type == "OTHER")

                //else if (type == "OTHER")
                //{
                //    if (status == "ACCEPT" || status == "DEPOSITED")
                //    {
                //        cashReceived += dx.Rows[i]["payment"].ObjToDouble();
                //    }
                //}
            }


            string cmd = "Select * from `cust_payment_details` where `contractNumber` = '" + contractNumber + "'AND `dateReceived` <= '" + fStop + "';";
            DataTable dt = G1.get_db_data(cmd);

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                dateReceived = dt.Rows[i]["dateReceived"].ObjToDateTime();
                status = dt.Rows[i]["status"].ObjToString();
                if ( status.ToUpper() == "DEPOSITED")
                {
                    amountActuallyReceived = dt.Rows[i]["amtActuallyReceived"].ObjToDouble();
                    if (amountActuallyReceived == 0D)
                        amountActuallyReceived = dt.Rows[i]["paid"].ObjToDouble();
                    totalPayments += amountActuallyReceived;
                }
            }

            //newPrice += preDiscount + compDiscounts;

            balanceDue = newPrice - totalPayments - classA - refund - dbr;
            return balanceDue;
        }
        /****************************************************************************************/
        private void cmbSelectColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.ComboBox combo = (System.Windows.Forms.ComboBox)sender;
            string comboName = combo.Text;
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                SetupSelectedColumns("Contract Activity", comboName, dgv);
                string name = "Contract Activity " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                SetupTotalsSummary();
                gridMain.OptionsView.ShowFooter = true;
            }
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns()
        {
            string group = cmbSelectColumns.Text.Trim().ToUpper();
            if (group.Trim().Length == 0)
                return;
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = 'Contract Activity' order by seq";
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
        private void btnSelectColumns_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;
            SelectColumns sform = new SelectColumns(dgv, "Contract Activity", "Primary", actualName);
            sform.Done += new SelectColumns.d_void_eventdone(sform_Done);
            sform.Show();
        }
        /***********************************************************************************************/
        void sform_Done()
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "Contract Activity " + name;
            string skinName = "";
            SetupSelectedColumns("Contract Activity", name, dgv);
            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            gridMain.OptionsView.ShowFooter = true;
            SetupTotalsSummary();
            //string field = "";
            //string select = "";
            //DataTable dt = (DataTable)dgv.DataSource;
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    field = dt.Rows[i]["field"].ObjToString();
            //    select = dt.Rows[i]["select"].ObjToString();
            //    if (G1.get_column_number(gridMain, field) >= 0)
            //    {
            //        if (select == "0")
            //            gridMain.Columns[field].Visible = false;
            //        else
            //            gridMain.Columns[field].Visible = true;
            //    }
            //}
            dgv.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns(string procType, string group, DevExpress.XtraGrid.GridControl dgv)
        {
            if (String.IsNullOrWhiteSpace(group))
                return;
            if (String.IsNullOrWhiteSpace(procType))
                procType = "Contract Activity";
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
        private void lockScreenFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "Contract Activity " + name;
            G1.SaveLocalPreferences(this, gridMain, LoginForm.username, saveName);
        }
        /****************************************************************************************/
        private void unlockScreenFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string comboName = cmbSelectColumns.Text;
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                string name = "Contract Activity " + comboName;
                G1.RemoveLocalPreferences(LoginForm.username, name);
                foundLocalPreference = false;
            }
        }
        /****************************************************************************************/
        private void chkCliffDiff_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
        }
        /***********************************************************************************************/
        private void gridMain_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                string cancelled = View.GetRowCellDisplayText(e.RowHandle, View.Columns["policyNumber"]);
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
        /***********************************************************************************************/
        private void gridMain_CalcRowHeightx(object sender, RowHeightEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                int maxHeight = 0;
                int newHeight = 0;
                bool doit = false;
                string name = "";
                foreach (GridColumn column in gridMain.Columns)
                {
                    name = column.FieldName.ToUpper();
                    if (name == "ISSUED" || name == "POLICYNUMBER" || name == "PAYERNNUMBER" )
                        doit = true;
                    if (doit)
                    {
                        using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
                        {
                            using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
                            {
                                viewInfo.EditValue = gridMain.GetRowCellValue(e.RowHandle, column.FieldName);
                                viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv.Height);
                                using (Graphics graphics = dgv.CreateGraphics())
                                using (GraphicsCache cache = new GraphicsCache(graphics))
                                {
                                    viewInfo.CalcViewInfo(graphics);
                                    var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
                                    newHeight = Math.Max(height, maxHeight);
                                    if (newHeight > maxHeight)
                                        maxHeight = newHeight;
                                }
                            }
                        }
                    }
                }

                if (maxHeight > 0)
                    e.RowHeight = maxHeight;
            }
        }
        /****************************************************************************************/
    }
}