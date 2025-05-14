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
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class BurialSummaryTest : DevExpress.XtraEditors.XtraForm
    {
        private DataTable groupContracts = null;
        private bool runAgents = false;
        private DataTable originalDt = null;
        /****************************************************************************************/
        private bool doSimple = false;
        private bool doLocDetail = false;
        /****************************************************************************************/
        public BurialSummaryTest()
        {
            InitializeComponent();
            SetupTotalsSummary();
            if ( !G1.oldCopy )
                miscToolStripMenuItem.Visible = false;
            miscToolStripMenuItem.Visible = false; // Just force the Misc Menu to not be visible
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("burial", null);
            AddSummaryColumn("cremation", null);
            AddSummaryColumn("other", null);
            AddSummaryColumn("total", null);

            //gridMain.Columns["cash"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            //gridMain.Columns["cash"].SummaryItem.DisplayFormat = "{0:N2}";
            gridMain.Columns["contractNumber"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain.Columns["contractNumber"].SummaryItem.DisplayFormat = "{0:N2}";


            gridMain.CustomDrawScroll += GridMain_CustomDrawScroll;
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "")
        {
            if (gMain == null)
                gMain = gridMain;
//            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
//            gMain.Columns[columnName].SummaryItem.DisplayFormat = "${0:0,0.00}";
//            gMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
            gMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:0,0.00}";
            if (string.IsNullOrWhiteSpace(format))
                format = "{0:N2}";
            G1.AddSummaryItem(gridMain2, columnName, format);
        }
        /****************************************************************************************/
        private void BurialSummary_Load(object sender, EventArgs e)
        {
            chkInclude.Hide();
            chkExcludeBlankLine.Hide();

            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            DateTime stop = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Value = stop;

            gridMain.Columns["num"].Visible = false;
//            gridMain.Columns["loc"].Visible = false;
            gridMain.Columns["Location Name"].Visible = true;
            gridMain.Columns["contractNumber"].Visible = false;

            gridMain.OptionsView.ShowFooter = false;
            gridMain.OptionsView.ShowBands = false;

            gridMain.OptionsPrint.PrintBandHeader = false;
            gridMain.OptionsPrint.PrintFooter = false;

            gridMain.Appearance.Row.Font = new Font("Tahoma", 9F);
            gridMain.AppearancePrint.Row.Font = new Font("Tahoma", 9F);
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
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            runData();

            ScaleCells();
            this.Cursor = Cursors.Default;
        }

        private void GridMain_CustomDrawScroll(object sender, ScrollBarCustomDrawEventArgs e)
        {
            if (gridMain != null)
                gridMain.RefreshEditor(true);
        }

        /****************************************************************************************/
        private DataTable GetDataSet(DateTime startDate, DateTime stopDate)
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

            //string names = getLocationNameQuery();
            //if (!String.IsNullOrWhiteSpace(names))
            //    cmd += " AND " + names + " ";

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

            //if (!String.IsNullOrWhiteSpace(names))
            //    cmd += " AND " + names + " ";

            cmd += " AND f.`OpenCloseFuneral` <> 'Y' ";

            contract = txtContract.Text.Trim();
            if (!string.IsNullOrWhiteSpace(contract))
            {
                cmd = "Select * from `fcustomers` p JOIN `fcust_extended` f ON p.`contractNumber` = f.`contractNumber` where p.`deceasedDate` <= '" + date2 + "' ";
                cmd += " AND f.`serviceId` = '" + contract + "' ";
            }
            cmd += " ORDER BY p.`deceasedDate`;";

            DataTable dt = G1.get_db_data(cmd);

            Trust85.FindContract(dt, "SX23045");


            if (String.IsNullOrWhiteSpace(contract))
                dt = SalesTaxReport.ProcessTheData(dt, start, stop);

            return dt;
        }
        /****************************************************************************************/
        private DataTable LoadData(DateTime startDate, DateTime stopDate, DataTable mainDt, bool ytd)
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
            double netPrice = 0D;
            int idx = 0;
            string ch = "";

            //string cmd = "Select * from `fcust_extended` c JOIN `fcustomers` a on c.`contractNumber` = a.`contractNumber` where `serviceDate` >= '" + date1 + "' AND `serviceDate` <='" + date2 + "';";

            string search = cmbSearch.Text.ToUpper();

            string cmd = "Select * from `fcust_extended` c JOIN `fcustomers` a on c.`contractNumber` = a.`contractNumber` where ";
            if (search == "DECEASED DATE")
                cmd += " a.`deceasedDate` >= '" + date1 + "' AND a.`deceasedDate` <= '" + date2 + "' ";
            else if (search == "CREATE DATE")
                cmd += " c.`caseCreatedDate` >= '" + date1 + "' AND c.`caseCreatedDate` <= '" + date2 + "' ";
            else
                cmd += " c.`serviceDate` >= '" + date1 + "' AND c.`serviceDate` <= '" + date2 + "' ";

            cmd += " AND c.`OpenCloseFuneral` <> 'Y' ";

            contract = txtContract.Text.Trim();
            if (!string.IsNullOrWhiteSpace(contract))
            {
                cmd = "Select * from `fcust_extended` c JOIN `fcustomers` a on c.`contractNumber` = a.`contractNumber` where ";
                cmd += " c.`serviceId` = '" + contract + "' ";
            }

            cmd += ";";

            int year = start.Year;

            string yy = (year % 100).ToString("D2");

            cmd = "Select * from `fcustomers` p JOIN `fcust_extended` f ON p.`contractNumber` = f.`contractNumber` where ";
            if (search == "DECEASED DATE")
                cmd += " p.`deceasedDate` >= '" + date1 + "' AND p.`deceasedDate` <= '" + date2 + "' ";
            else if (search == "CREATE DATE")
                cmd += " f.`caseCreatedDate` >= '" + date1 + "' AND f.`caseCreatedDate` <= '" + date2 + "' ";
            else
                cmd += " f.`serviceDate` >= '" + date1 + "' AND f.`serviceDate` <= '" + date2 + "' ";
            cmd += " AND p.`serviceId` LIKE '__" + yy + "%' ";
            cmd += " ORDER BY p.`deceasedDate`;";

            DataTable dt = GetDataSet(startDate, stopDate);

            //DataTable dt = G1.get_db_data(cmd);

            Trust85.FindContract(dt, "SX23045");
            Trust85.FindContract(dt, "SX23031");

            dt.Columns.Add("name");
            dt.Columns.Add("burial", Type.GetType("System.Double"));
            dt.Columns.Add("cremation", Type.GetType("System.Double"));
            dt.Columns.Add("other", Type.GetType("System.Double"));
            dt.Columns.Add("total", Type.GetType("System.Double"));
            dt.Columns.Add("loc");
            dt.Columns.Add("Location Name");
            dt.Columns.Add("GOOD");

            //dt.Columns.Add("trustPayments", Type.GetType("System.Double"));
            dt.Columns.Add("trustDiscount", Type.GetType("System.Double"));
            //dt.Columns.Add("trustGrowth", Type.GetType("System.Double"));

            dt.Columns.Add("insuranceAmountReceived", Type.GetType("System.Double"));
            //dt.Columns.Add("insurancePayments", Type.GetType("System.Double"));
            dt.Columns.Add("insuranceDiscount", Type.GetType("System.Double"));
            //dt.Columns.Add("insuranceGrowth", Type.GetType("System.Double"));

            dt.Columns.Add("totalAllPayments", Type.GetType("System.Double"));


            cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable dd = G1.get_db_data(cmd);

            DataRow[] dr = null;
            string deceasedDate = "";
            DateTime ddate = DateTime.Now;
            string funeralClass = "";
            string serviceLoc = "";
            string serviceId = "";
            DataRow[] dRows = null;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber == "SX23045")
                    {
                    }
                    if (DailyHistory.gotCemetery(contractNumber))
                        continue;
                    contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                    if ( loc == "NM")
                    {
                        dt.Rows[i]["contractNumber"] = "";
                        continue;
                    }
                    if (loc == "HC")
                    {
                        dt.Rows[i]["contractNumber"] = "";
                        continue;
                    }
                    serviceId = dt.Rows[i]["serviceId"].ObjToString();
                    if (DailyHistory.gotCemetery(serviceId))
                    {
                        dt.Rows[i]["contractNumber"] = "";
                        continue;
                    }
                    //if ( dt.Rows[i]["OpenCloseFuneral"].ObjToString().ToUpper() == "Y" )
                    //{
                    //    dt.Rows[i]["contractNumber"] = "";
                    //    continue;
                    //}
                    loc = dt.Rows[i]["serviceLoc"].ObjToString();

                    dt.Rows[i]["burial"] = 0D;
                    dt.Rows[i]["cremation"] = 0D;
                    dt.Rows[i]["other"] = 0D;

                    netPrice = 0D;

                    funeralClass = dt.Rows[i]["funeral_classification"].ObjToString().ToUpper();
                    if (funeralClass.ToUpper().IndexOf("BURIAL") >= 0)
                    {
                        netPrice = GetNetPrice(dt, i);
                        dt.Rows[i]["burial"] = netPrice;
                        //dt.Rows[i]["burial"] = dt.Rows[i]["custPrice"].ObjToDouble();
                    }
                    else if (funeralClass.ToUpper().IndexOf("CREMATION") >= 0)
                    {
                        netPrice = dt.Rows[i]["custPrice"].ObjToDouble();
                        netPrice = GetNetPrice(dt, i);
                        dt.Rows[i]["cremation"] = netPrice;
                    }
                    else
                    {
                        netPrice = dt.Rows[i]["custPrice"].ObjToDouble();
                        netPrice = GetNetPrice(dt, i);
                        dt.Rows[i]["other"] = netPrice;
                    }

                    //dt.Rows[i]["total"] = dt.Rows[i]["custPrice"].ObjToDouble();
                    dt.Rows[i]["total"] = netPrice;

                    dr = dd.Select("atneedcode='" + loc + "'");
                    if (dr.Length > 0)
                        dt.Rows[i]["Location Name"] = dr[0]["LocationCode"].ObjToString();
                    else
                        dt.Rows[i]["Location Name"] = loc;
                    dt.Rows[i]["loc"] = loc;

                    dt.Rows[i]["name"] = dt.Rows[i]["firstName"] + " " + dt.Rows[i]["lastName"].ObjToString();
                    dt.Rows[i]["GOOD"] = "Y";

                    serviceLoc = dt.Rows[i]["serviceLoc"].ObjToString();
                    dRows = dd.Select("merchandiseCode='" + serviceLoc + "'");
                    if (dRows.Length > 0)
                    {
                        dt.Rows[i]["Location Name"] = dRows[0]["LocationCode"].ObjToString();
                        if (chkExcludeMerch.Checked)
                            dt.Rows[i]["GOOD"] = "BAD";
                    }
                    //contractValue = DailyHistory.GetContractValue(dt.Rows[i]);
                    //contractValue = G1.RoundValue(contractValue);
                    //dt.Rows[i]["contractValue"] = contractValue;
                }
                catch (Exception ex)
                {
                }
            }

            dRows = dt.Select("GOOD<>'BAD'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            else
                dt.Rows.Clear();

            Trust85.FindContract(dt, "SX23045");

            return dt;
        }
        /****************************************************************************************/
        private double GetNetPrice ( DataTable dt, int i )
        {
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

            DataRow[] dRows = null;

            try
            {
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
                //discount += classA;
                totalReceived = dt.Rows[i]["trustAmountReceived"].ObjToDouble();
                totalReceived = Math.Abs(totalReceived);

                oldBalanceDue = dt.Rows[i]["balanceDue"].ObjToDouble();

                //balanceDue = currentPrice - discount - totalReceived;
                //dt.Rows[i]["balanceDue"] = balanceDue;

                dValue = dt.Rows[i]["preneedDiscount"].ObjToDouble();
                dValue = Math.Abs(dValue);
                //dt.Rows[i]["preneedDiscount"] = discount;

                //payments = getPayments(dt, i);

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
                totalAllPayments = trustPaymentsReceived + insurancePaymentsReceived + cashReceived + dbr;
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
            }
            catch ( Exception ex)
            {
            }
            return netPrice;
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
            if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;
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
            string title = "Burial/Cremation Summary Report";
            Printer.DrawQuad(6, 8, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            DateTime date = this.dateTimePicker1.Value;
            string workDate1 = date.ToString("MM/dd/yyyy");

            date = this.dateTimePicker2.Value;
            string workDate2 = date.ToString("MM/dd/yyyy");

            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            Printer.DrawQuad(19, 8, 6, 4, "Date Range - " + workDate1 + " - " + workDate2, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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
            if (e.HasFooter)
            {
                if (chkPageBreaks.Checked)
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
            //if (runAgents)
            //{
            //    int row = e.ListSourceRow;
            //    if (row >= 0)
            //    {
            //        //if (gridMain.IsDataRow(row))
            //        //{
            //        //    e.Visible = false;
            //        //    e.Handled = true;
            //        //    return;
            //        //}
            //    }
            //}
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
            int year = start.Year;
            int year2 = stop.Year;

            string yy = (year % 100).ToString("D2");

            string cmd = "";

            if (year == year2)
            {
                dt = LoadData(start, stop, null, false);
                //dt = SalesTaxReport.ProcessTheData(dt, start, stop);
                Trust85.FindContract(dt, "SX23045");
            }
            else
            {
                DateTime testDate = new DateTime(start.Year, 12, 31);
                //dt = LoadData(start, testDate, null, false);

                testDate = new DateTime(stop.Year, 1, 1);
                DataTable dx = LoadData(testDate, stop, null, false);
                dt.Merge(dx);
            }

            string loc = "";

            DataRow dR = null;
            DataRow[] dRows = null;

            if (chkInclude.Checked)
            {
                DataTable locDt = G1.get_db_data("Select * from `funeralhomes`;");
                for (int i = 0; i < locDt.Rows.Count; i++)
                {
                    loc = locDt.Rows[i]["keycode"].ObjToString();
                    dRows = dt.Select("loc='" + loc + "'");
                    if (dRows.Length <= 0)
                    {
                        dR = dt.NewRow();
                        dR["loc"] = loc;
                        dR["Location Name"] = locDt.Rows[i]["LocationCode"].ObjToString();
                        dt.Rows.Add(dR);
                    }
                }
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = "Location Name asc";
            dt = tempview.ToTable();

            BuildSummaryTable(dt);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.Refresh();

            gridMain.Columns["num"].Visible = true;
            gridMain.Columns["loc"].Visible = false;
            gridMain.Columns["Location Name"].Visible = true;
            gridMain.Columns["contractNumber"].Visible = true;

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
        private DataTable summaryDt = null;
        private DataTable summaryCt = null;
        private void BuildSummaryTable ( DataTable dt)
        {
            summaryDt = dt.Clone();

            DataTable ddd = null;

            double burial = 0D;
            double cremation = 0D;
            double other = 0D;
            string serviceId = "";
            string loc = "";
            DataRow dR = null;
            int count = 0;
            DataRow[] dRows = null;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                loc = dt.Rows[i]["loc"].ObjToString();
                
                count++;
                //dt.Rows[i]["num"] = count.ToString();

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
                    //ddd = dRows.CopyToDataTable();
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
                if (dValue > 0D )
                {
                    if (chkShowNumbers.Checked)
                        e.DisplayText = G1.ReformatMoney(dValue);
                    else
                        e.DisplayText = "X";
                }
                else
                    e.DisplayText = "";
            }
            if (e.Column.FieldName.ToUpper() == "CREMATION")
            {
                dValue = e.DisplayText.ObjToDouble();
                if (dValue > 0D)
                {
                    if (chkShowNumbers.Checked)
                        e.DisplayText = G1.ReformatMoney(dValue);
                    else
                        e.DisplayText = "X";
                }
                else
                    e.DisplayText = "";
            }
            if (e.Column.FieldName.ToUpper() == "OTHER")
            {
                dValue = e.DisplayText.ObjToDouble();
                if (dValue > 0D)
                {
                    if (chkShowNumbers.Checked)
                        e.DisplayText = G1.ReformatMoney(dValue);
                    else
                        e.DisplayText = "X";
                }
                else
                    e.DisplayText = "";
            }
            else if (e.Column.FieldName.ToUpper() == "TOTAL")
            {
                if (e.DisplayText.Trim() == "0.00")
                    e.DisplayText = "-          ";
            }
        }
        /****************************************************************************************/
        private void gridMain_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            //if (1 == 1)
            //    return;
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
                        if (dValue > 0D)
                        {
                            if ( chkShowNumbers.Checked )
                                e.Appearance.TextOptions.HAlignment = HorzAlignment.Far;
                            else
                                e.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
                        }
                    }
                }
                else if (column.ToUpper() == "CREMATION")
                {
                    string data = dt.Rows[row][column].ObjToString();
                    if (!String.IsNullOrWhiteSpace(data))
                    {
                        dValue = data.ObjToDouble();
                        if (dValue > 0D)
                        {
                            if (chkShowNumbers.Checked)
                                e.Appearance.TextOptions.HAlignment = HorzAlignment.Far;
                            else
                                e.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
                        }
                    }
                }
                else if (column.ToUpper() == "OTHER")
                {
                    string data = dt.Rows[row][column].ObjToString();
                    if (!String.IsNullOrWhiteSpace(data))
                    {
                        dValue = data.ObjToDouble();
                        if (dValue > 0D)
                        {
                            if (chkShowNumbers.Checked)
                                e.Appearance.TextOptions.HAlignment = HorzAlignment.Far;
                            else
                                e.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
                        }
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
                EditCust custForm = new EditCust(contract);
                custForm.Tag = contract;
                custForm.ShowDialog();
                gridMain.RefreshEditor(true);
                this.Refresh();
            }
        }
        /****************************************************************************************/
        private void gridMain_DoubleClickx(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
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
                //CustomerDetails clientForm = new CustomerDetails(contract);
                //clientForm.ShowDialog();
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            double dValue = 0D;
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
            string loc = "";

            try
            {
                if (e.IsGroupSummary)
                {
                    int row = gridMain.GetDataSourceRowIndex(rowHandle);
                    loc = dt.Rows[row]["loc"].ObjToString();
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
                    for (int i = 0; i < summaryDt.Rows.Count; i++)
                    {
                        burials += summaryDt.Rows[i]["burial"].ObjToDouble();
                        cremations += summaryDt.Rows[i]["cremation"].ObjToDouble();
                        other += summaryDt.Rows[i]["other"].ObjToDouble();
                    }
                    total = burials + cremations + other;
                    if (field.ToUpper() == "CONTRACTNUMBER")
                    {
                    }
                }

                if (e.IsTotalSummary && field.ToUpper() != "NAME")
                {

                }
                if (e.IsTotalSummary && field.ToUpper() == "CONTRACTNUMBER")
                {

                }
                if (!e.IsTotalSummary && field.ToUpper() == "SERVICEID")
                {

                }

                if (cremations == 38D)
                {
                }
            }
            catch ( Exception ex)
            {
            }

            string unprintable = "   ";
            //StringBuilder sb = new StringBuilder(unprintable);
            //for (int i = 0; i < unprintable.Length; i++)
            //    sb[i] = (char)127;
            //unprintable = sb.ToString();

            double newTotal = burials + cremations;
            if (newTotal <= 0D)
                return;
            double bp = burials / newTotal * 100D;
            bp = G1.RoundValue(bp);
            double cp = cremations / newTotal * 100D;
            cp = G1.RoundValue(cp);

            if (field.ToUpper() == "LOCATION NAME")
            {
               //e.TotalValue = "Burials=" + burials.ToString() + unprintable;
                e.TotalValue = bp.ToString() + "% Burials=" + burials.ToString();
            }
            else if (field.ToUpper() == "SERVICEID")
            {
                //e.TotalValue = "Cremations=" + cremations.ToString() + unprintable;
                e.TotalValue = cp.ToString() + "% Cremations=" + cremations.ToString() + unprintable;
            }
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
        private void chkShowNumbers_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.BeginDataUpdate();
            gridMain.RefreshEditor(true);
            gridMain.RefreshData();
            this.dgv.Refresh();
            gridMain.EndDataUpdate();
            this.Refresh();
        }
        /****************************************************************************************/
        private void recalculateCustomerPriceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!G1.oldCopy)
                return;
            string contract = "B17083";
            string contractNumber = "";
            string cmd = "";
            DataTable dx = null;

            double price = 0D;
            double custPrice = 0D;
            string record = "";

            DateTime start = this.dateTimePicker1.Value;
            DateTime stop = this.dateTimePicker2.Value;

            DataTable dt = LoadData(start, stop, null, false);

            this.Cursor = Cursors.WaitCursor;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                Application.DoEvents();

                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();

                //contractNumber = contract;

                custPrice = 0D;

                cmd = "SELECT * FROM fcust_services WHERE contractNumber = '" + contractNumber + "'";
                dx = G1.get_db_data(cmd);
                for ( int j=0; j<dx.Rows.Count; j++)
                {
                    price = dx.Rows[j]["price"].ObjToDouble();
                    custPrice += price;
                }

                cmd = "Select * from `fcust_extended` where `contractNumber` = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count > 0 )
                {
                    record = dx.Rows[0]["record"].ObjToString();
                    G1.update_db_table("fcust_extended", "record", record, new string[] {"custPrice", custPrice.ToString() });
                }
                //if (1 == 1)
                //    break;
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabControl tabControl = (TabControl)sender;
            int selectedIndex = tabControl.SelectedIndex;
            string pageName = tabControl.TabPages[selectedIndex].Name.Trim();
            if (pageName.ToUpper() != "TABPAGE2")
                return;

            try
            {
                DataTable dt = (DataTable)dgv.DataSource;
                if (dt == null)
                    return;
                DataTable dx = G1.GetGroupBy(dt, "funeral_classification");
                DataRow[] dRows = dx.Select("funeral_classification LIKE 'cremation%'");
                if (dRows.Length <= 0)
                {
                    MessageBox.Show("*ERROR*** No Data to Display.");
                    return;
                }
                    
                dx = dRows.CopyToDataTable();

                dRows = dt.Select("funeral_classification LIKE 'cremation%'");
                if (dRows.Length <= 0)
                    return;

                string funeralClass = "";
                DataTable dd = dRows.CopyToDataTable();
                dd = AddCremationColumns(dx, dd);

                int k = 0;
                ClearAllPositions(gridMain2);
                G1.SetColumnPosition(gridMain2, "num", k++);
                G1.SetColumnPosition(gridMain2, "loc", k++);
                G1.SetColumnPosition(gridMain2, "Location Name", k++);
                G1.SetColumnPosition(gridMain2, "ServiceId", k++);
                G1.SetColumnPosition(gridMain2, "deceasedDate", k++);
                G1.SetColumnPosition(gridMain2, "name", k++);
                G1.SetColumnPosition(gridMain2, "contractNumber", k++);

                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    funeralClass = dx.Rows[i]["funeral_classification"].ObjToString();
                    G1.SetColumnPosition(gridMain2, funeralClass, k++);
                    gridMain2.Columns[funeralClass].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
                    gridMain2.Columns[funeralClass].SummaryItem.DisplayFormat = "{0:N2}";
//                    AddSummaryColumn(funeralClass, gridMain2);
                    G1.SetColumnPosition(gridMain2, funeralClass + " CNT", k++);
                }

                G1.SetColumnPosition(gridMain2, "total", k++);
                
                double netPrice = 0D;
                for (int i = 0; i < dd.Rows.Count; i++)
                {
                    funeralClass = dd.Rows[i]["funeral_classification"].ObjToString();
                    netPrice = GetNetPrice(dd, i);
                    dd.Rows[i][funeralClass] = netPrice;
                    if (netPrice > 0)
                        dd.Rows[i][funeralClass + " CNT"] = 1;
                    else 
                        dd.Rows[i][funeralClass + " CNT"] = 0;
                }
                // Need another routine to calculate Summary CT. Pass the dd (rows) AND dt (columns) into the subroutine
                // if something isNull then add new table
                // Then add a new column called loc
                // Then you'll have a for loop ... said that wasn't right
                // if summaryCT == null then create
                buildCremationTable(dd, dx);
                dgv2.DataSource = dd;
            }
            catch (Exception ex)
            {
                MessageBox.Show("*ERROR*** " + ex.Message.ToString());
            }

            gridMain2.ExpandAllGroups();
            dgv2.Refresh();

        }
        /****************************************************************************************/
        private void buildCremationTable(DataTable dd, DataTable dx)
        {
            string loc = "";
            int count = 0;
            DataRow[] dRows = null;
            DataRow dR = null;
            string funeralClass = "";
            double dValue = 0D;
            double dValue2 = 0D;
            for (int i = 0; i < dd.Rows.Count; i++)
            {
                loc = dd.Rows[i]["loc"].ObjToString();

                count++;

                dRows = summaryCt.Select("loc='" + loc + "'");
                if (dRows.Length <= 0)
                {
                    dR = summaryCt.NewRow();
                    dR["loc"] = loc;
                    for (int j = 0; j < dx.Rows.Count; j++)
                    {
                        funeralClass = dx.Rows[j]["funeral_classification"].ObjToString();
                        dR[funeralClass] = 0D;
                    }
                    summaryCt.Rows.Add(dR);
                }
                else
                {
                    for (int j = 0; j < dx.Rows.Count; j++)
                    {
                        funeralClass = dx.Rows[j]["funeral_classification"].ObjToString();
                        dValue = dd.Rows[i][funeralClass].ObjToDouble();
                        if (dValue > 0D)
                        {
                            dValue2 = dRows[0][funeralClass].ObjToDouble();
                            dValue2++;
                            dRows[0][funeralClass] = dValue2;
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
        private DataTable AddCremationColumns(DataTable dx, DataTable dd)
        {
            string funeralClass = "";
            if (summaryCt == null)
            {
                summaryCt = new DataTable();
                summaryCt.Columns.Add("loc");
            }
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                funeralClass = dx.Rows[i]["funeral_classification"].ObjToString();
                if (G1.get_column_number(gridMain2, funeralClass) < 0)
                {
                    AddNewColumn(funeralClass, funeralClass, 80, FormatType.Numeric, "N2");
                    AddSummaryColumn(funeralClass, gridMain2);
                    summaryCt.Columns.Add(funeralClass, Type.GetType("System.Double"));
                }
                dd.Columns.Add(funeralClass, Type.GetType("System.Double"));

                if (G1.get_column_number(gridMain2, funeralClass + " CNT") < 0)
                {
                    AddNewColumn(funeralClass + " CNT", "Count", 50, FormatType.Numeric, "N0");
                    AddSummaryColumn(funeralClass + " CNT", gridMain2, "{0:N0}");
                }
                dd.Columns.Add(funeralClass + " CNT", Type.GetType("System.Double"));
            }
            return dd;
        }
        /****************************************************************************************/
        private void AddNewColumn(string fieldName, string caption, int width, FormatType type, string format = "")
        {
            if (G1.get_column_number(gridMain2, fieldName) < 0)
                G1.AddNewColumn(gridMain2, fieldName, caption, format, type, width, true);
            else
                gridMain2.Columns[fieldName].Visible = true;
            G1.SetColumnWidth(gridMain2, fieldName, width);
            gridMain2.Columns[fieldName].OptionsColumn.FixedWidth = true;
            gridMain2.Columns[fieldName].AppearanceHeader.ForeColor = Color.Black;
        }
        /****************************************************************************************/
        private void ClearAllPositions(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            for (int i = 0; i < gMain.Columns.Count; i++)
            {
                gMain.Columns[i].Visible = false;
                //gMain.Columns[i].VisibleIndex = 0;
                //gMain.Columns[i].AbsoluteIndex = 0;
            }
        }
        /****************************************************************************************/
        private void gridMain2_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            string field = (e.Item as GridSummaryItem).FieldName.ObjToString();
            string loc = "";
            if (e.IsGroupSummary)
            { 
                
            }
            if (!e.IsGroupSummary && !e.IsTotalSummary)
                return;

            DataTable dt = (DataTable)dgv2.DataSource;
            int rowHandle = e.RowHandle;

            if (e.IsGroupSummary)
            {
                int row = gridMain2.GetDataSourceRowIndex(rowHandle);
                loc = dt.Rows[row]["loc"].ObjToString();
                DataRow[] dRows = summaryCt.Select("loc='" + loc + "'");

                if (dRows.Length > 0)
                {
                    /*
                    burials = dRows[0]["burial"].ObjToDouble();
                    cremations = dRows[0]["cremation"].ObjToDouble();
                    other = dRows[0]["other"].ObjToDouble();
                    total = burials + cremations + other;*/
                }
            }

        }
        /****************************************************************************************/
    }
}