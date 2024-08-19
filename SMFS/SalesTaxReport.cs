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
using System.Globalization;
using System.Security.AccessControl;
using System.Security.Principal;

/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class SalesTaxReport : DevExpress.XtraEditors.XtraForm
    {
        private DataTable groupContracts = null;
        private bool runAgents = false;
        private DataTable originalDt = null;
        /****************************************************************************************/
        private bool doSimple = false;
        private bool doLocDetail = false;
        /****************************************************************************************/
        public SalesTaxReport()
        {
            InitializeComponent();
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("totalsale", null);
            AddSummaryColumn("preneedDiscount", null);
            AddSummaryColumn("custPrice", null);
            AddSummaryColumn("classa", null);
            AddSummaryColumn("trustDiscount", null);
            AddSummaryColumn("trustAmountReceived", null);
            AddSummaryColumn("insuranceDiscount", null);
            AddSummaryColumn("insurancePayments", null);
            AddSummaryColumn("cashReceived", null);
            AddSummaryColumn("totalAllPayments", null);
            AddSummaryColumn("balanceDue", null);
            AddSummaryColumn("taxMerchandise", null);
            AddSummaryColumn("taxAmount", null);

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
            gMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:C2}";
        }
        /****************************************************************************************/
        private void SalesTaxReport_Load(object sender, EventArgs e)
        {
            chkInclude.Hide();
            chkExcludeBlankLine.Hide();

            label3.Hide();
            cmbSearch.Hide();

            barImport.Hide();

            gridMain.Columns["Location Name"].Visible = false;
            gridMain.Columns["contractNumber"].Visible = false;

            DateTime start = DateTime.Now;
            DateTime stop = DateTime.Now;

            DateTime now = DateTime.Now;
            if (now.Month >= 10)
            {
                start = new DateTime(now.Year, 10, 1);
                stop = new DateTime(now.Year, 12, 31);
            }
            else if (now.Month >= 7)
            {
                start = new DateTime(now.Year, 7, 1);
                stop = new DateTime(now.Year, 9, 31);
            }
            else if (now.Month >= 4)
            {
                start = new DateTime(now.Year, 4, 1);
                stop = new DateTime(now.Year, 6, 30);
            }
            else if (now.Month >= 1)
            {
                start = new DateTime(now.Year, 1, 1);
                stop = new DateTime(now.Year, 3, 31);
            }
            this.dateTimePicker1.Value = start;
            this.dateTimePicker2.Value = stop;

            gridMain.Columns["num"].Visible = true;
            //            gridMain.Columns["loc"].Visible = false;
            gridMain.Columns["Location Name"].Visible = false;
            gridMain.Columns["contractNumber"].Visible = false;

            gridMain.OptionsView.ShowFooter = false;
            gridMain.OptionsView.ShowBands = false;

            gridMain.OptionsPrint.PrintBandHeader = false;
            gridMain.OptionsPrint.PrintFooter = false;

            getLocations();

            //gridMain.Appearance.Row.Font = new Font("Tahoma", 9F);
            //gridMain.AppearancePrint.Row.Font = new Font("Tahoma", 9F);
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
            string mercCode = "";
            string[] locIDs = this.chkComboLocation.EditValue.ToString().Split('|');
            DataRow[] dRows = null;
            DataTable dt = (DataTable)chkComboLocation.Properties.DataSource;
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
                        mercCode = dRows[0]["merchandiseCode"].ObjToString().Trim();
                        if (!String.IsNullOrWhiteSpace(mercCode))
                            procLoc += ",'" + mercCode + "'";

                        //procLoc += "'" + locIDs[i].Trim() + "'";
                    }
                }
            }
            return procLoc.Length > 0 ? " `serviceLoc` IN (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(3);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            now = now.AddMonths(2);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(-3);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            now = now.AddMonths(2);
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
        /***********************************************************************************************/
        private bool CheckForContract(string contractNumber, ref DateTime gsDate)
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

            string cmd = "Select * from `fcust_extended` c JOIN `fcustomers` a on c.`contractNumber` = a.`contractNumber` where ";
            if (search == "DECEASED DATE")
                cmd += " a.`deceasedDate` >= '" + date1 + "' AND a.`deceasedDate` <= '" + date2 + "' ";
            else if (search == "CREATE DATE")
            {
                cmd += " ( c.`caseCreatedDate` >= '" + date1 + "' AND c.`caseCreatedDate` <= '" + date2 + "' OR a.`deceasedDate` >= '" + date1 + "' AND a.`deceasedDate` <= '" + date2 + "' ) ";
            }
            else
                cmd += " c.`serviceDate` >= '" + date1 + "' AND c.`serviceDate` <= '" + date2 + "' ";

            string names = getLocationNameQuery();
            if (!String.IsNullOrWhiteSpace(names))
                cmd += " AND " + names + " ";

            //cmd += " AND `serviceLoc` <> 'NM' ";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("name");
            dt.Columns.Add("burial", Type.GetType("System.Double"));
            dt.Columns.Add("cremation", Type.GetType("System.Double"));
            dt.Columns.Add("other", Type.GetType("System.Double"));
            dt.Columns.Add("total", Type.GetType("System.Double"));
            dt.Columns.Add("payments", Type.GetType("System.Double"));

            //dt.Columns.Add("trustPayments", Type.GetType("System.Double"));
            dt.Columns.Add("trustDiscount", Type.GetType("System.Double"));

            dt.Columns.Add("insuranceAmountReceived", Type.GetType("System.Double"));
            //dt.Columns.Add("insurancePayments", Type.GetType("System.Double"));
            dt.Columns.Add("insuranceDiscount", Type.GetType("System.Double"));

            dt.Columns.Add("totalAllPayments", Type.GetType("System.Double"));

            //dt.Columns.Add("cashReceived", Type.GetType("System.Double"));
            dt.Columns.Add("loc");
            dt.Columns.Add("Location Name");
            dt.Columns.Add("GOOD");
            dt.Columns.Add("gsdate");

            bool isGood = false;
            DateTime gsDate = DateTime.Now;

            cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable dd = G1.get_db_data(cmd);

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
            double custPrice = 0D;

            double trustPayments = 0D;
            double trustPaymentsReceived = 0D;
            double insurancePayments = 0D;
            double insurancePaymentsReceived = 0;
            double cashReceived = 0D;
            double totalAllPayments = 0D;
            double totalIgnore = 0D;
            double compDiscounts = 0D;

            double totalServices = 0D;
            double totalMerchandise = 0D;
            double totalCashAdvance = 0D;

            double trustDiscount = 0D;
            double insuranceDiscount = 0D;
            double preneedDiscount = 0D;

            double contractTotal = 0D;
            double totalCost = 0D;
            double preDiscount = 0D;

            string prefix = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string suffix = "";

            DataRow[] dRows = null;

            barImport.Show();
            barImport.Maximum = dt.Rows.Count;
            barImport.Minimum = 0;
            barImport.Value = 0;
            barImport.Refresh();

            if (G1.get_column_number(dt, "totalsale") < 0)
                dt.Columns.Add("totalsale", Type.GetType("System.Double"));

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    Application.DoEvents();

                    barImport.Value = i + 1;
                    barImport.Refresh();

                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber == "B18019L")
                    {
                    }
                    serviceId = dt.Rows[i]["serviceId"].ObjToString();
                    if (serviceId.ToUpper() == "BS22002")
                    {
                    }
                    isGood = CheckForContract(contractNumber, ref gsDate);

                    if (isGood || !isGood)
                    {
                        dt.Rows[i]["GOOD"] = "Y";
                        dt.Rows[i]["gsdate"] = gsDate.ToString("yyyy-MM-dd");

                        currentPrice = dt.Rows[i]["currentPrice"].ObjToDouble();
                        discount = dt.Rows[i]["preneedDiscount"].ObjToDouble();
                        discount = Math.Abs(discount);

                        dt.Rows[i]["preneedDiscount"] = discount;

                        //currentPrice = currentPrice + discount;
                        dt.Rows[i]["currentPrice"] = currentPrice;

                        classA = dt.Rows[i]["classa"].ObjToDouble();
                        discount += classA;
                        totalReceived = dt.Rows[i]["trustAmountReceived"].ObjToDouble();
                        totalReceived = Math.Abs(totalReceived);

                        balanceDue = currentPrice - discount - totalReceived;
                        //dt.Rows[i]["balanceDue"] = balanceDue;

                        dValue = dt.Rows[i]["preneedDiscount"].ObjToDouble();
                        dValue = Math.Abs(dValue);
                        //dt.Rows[i]["preneedDiscount"] = discount;

                        payments = getPayments(dt, i);

                        totalAllPayments = 0D;
                        contractTotal = currentPrice;
                        totalCost = dt.Rows[i]["custPrice"].ObjToDouble();

                        //calculateTotalServices(contractNumber, ref contractTotal, ref totalCost, ref preDiscount);

                        trustPayments = dt.Rows[i]["trustPayments"].ObjToDouble();
                        trustPaymentsReceived = dt.Rows[i]["trustPaymentsreceived"].ObjToDouble();
                        insurancePayments = dt.Rows[i]["insurancePayments"].ObjToDouble();
                        insurancePaymentsReceived = dt.Rows[i]["insurancePaymentsreceived"].ObjToDouble();
                        cashReceived = dt.Rows[i]["cashReceived"].ObjToDouble();
                        compDiscounts = dt.Rows[i]["compDiscount"].ObjToDouble();

                        dt.Rows[i]["totalsale"] = dt.Rows[i]["taxMerchandise"].ObjToDouble() + dt.Rows[i]["taxAmount"].ObjToDouble();

                        //payments = calculateTotalPayments(contractNumber, ref trustPayments, ref trustPaymentsReceived, ref insurancePayments, ref insurancePaymentsReceived, ref cashReceived, ref compDiscounts, ref classA );

                        //dt.Rows[i]["classa"] = 0D;

                        //classA += compDiscounts;
                        //if (compDiscounts > 0D)
                        dt.Rows[i]["classa"] = compDiscounts + classA;

                        totalAllPayments = trustPaymentsReceived + insurancePaymentsReceived + cashReceived;
                        dt.Rows[i]["totalAllPayments"] = totalAllPayments;

                        dValue = trustPaymentsReceived;
                        dValue = Math.Abs(dValue);
                        dt.Rows[i]["trustAmountReceived"] = dValue;

                        dValue = trustPayments;
                        dValue = Math.Abs(dValue);
                        dt.Rows[i]["trustPayments"] = dValue;

                        trustDiscount = trustPayments - trustPaymentsReceived;
                        dValue = trustDiscount;
                        dValue = Math.Abs(dValue);
                        dt.Rows[i]["trustDiscount"] = dValue;

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
                        dValue = insuranceDiscount;
                        dValue = Math.Abs(dValue);
                        dt.Rows[i]["insuranceDiscount"] = dValue;

                        custPrice = dt.Rows[i]["custPrice"].ObjToDouble();
                        preneedDiscount = dt.Rows[i]["preneedDiscount"].ObjToDouble();
                        preneedDiscount = Math.Abs(preneedDiscount);

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

                        balanceDue = custPrice - classA - trustPaymentsReceived - cashReceived - insurancePaymentsReceived - trustDiscount - insuranceDiscount - compDiscounts;
                        //balanceDue = currentPrice - classA - trustPaymentsReceived - cashReceived - insurancePaymentsReceived - trustDiscount - insuranceDiscount - preneedDiscount;
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

                    prefix = dt.Rows[i]["prefix"].ObjToString();
                    firstName = dt.Rows[i]["firstName"].ObjToString();
                    middleName = dt.Rows[i]["middleName"].ObjToString();
                    lastName = dt.Rows[i]["lastName"].ObjToString();
                    suffix = dt.Rows[i]["suffix"].ObjToString();

                    dt.Rows[i]["name"] = G1.BuildFullName(prefix, firstName, middleName, lastName, suffix);
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
        /****************************************************************************************/
        public static int DecodeServiceNumber ( string serviceId )
        {
            if (serviceId.Length <= 4)
                return 0;
            string s = serviceId.Substring(4);
            if (!G1.validate_numeric(s))
                return 0;
            int rv = s.ObjToInt32();
            return rv;
        }
        /****************************************************************************************/
        public static DataTable ProcessTheData(DataTable dt, DateTime start, DateTime stop)
        {
            DateTime deceasedDate = DateTime.Now;
            DateTime caseCreatedDate = DateTime.Now;
            DataRow[] dRows = null;
            string serviceId = "";
            string oldServiceId = "";
            string str = "";
            int oldCount = 0;
            int newCount = 0;
            string s1 = "";
            string s2 = "";
            int month = 0;
            int year = start.Year;
            string yy = (year % 100).ToString("D2");
            int serviceIndex = 0;

            if (G1.get_column_number(dt, "quarter") < 0)
                dt.Columns.Add("quarter", Type.GetType("System.Int32"));

            //DataView tempView = null;

            DataView tempview = dt.DefaultView;
            //            tempview.Sort = "loc asc, agentName asc";
            tempview.Sort = "serviceId asc";
            dt = tempview.ToTable();


            try
            {

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                    caseCreatedDate = dt.Rows[i]["caseCreatedDate"].ObjToDateTime();
                    serviceId = dt.Rows[i]["serviceId"].ObjToString();
                    if (serviceId == "AM23001")
                    {
                    }
                    if (serviceId == "MH22001")
                    {
                    }
                    if (serviceId == "CT22043")
                    {
                    }
                    if (serviceId == "CT22004")
                    {
                    }
                    if (serviceId == "CT22006")
                    {
                    }
                    if (serviceId == "BN22006")
                    {
                    }
                    dt.Rows[i]["quarter"] = 0;
                    if (deceasedDate >= start && deceasedDate <= stop)
                    {
                        dt.Rows[i]["quarter"] = deceasedDate.Month;
                        oldServiceId = serviceId;
                        continue;
                    }
                    if (deceasedDate < start && caseCreatedDate >= start && caseCreatedDate <= stop)
                    {
                        //if ( deceasedDate >= start.AddMonths (-1) && deceasedDate <= stop.AddMonths (-1))
                        //{
                        //    month = caseCreatedDate.Month;
                        //    s1 = serviceId.Substring(0, 2);
                        //    if ( !String.IsNullOrWhiteSpace ( s1 ))
                        //    {
                        //        s1 += yy + "%";
                        //        dRows = dt.Select("quarter='" + month.ToString() + "' AND serviceId LIKE '" + s1 + "'");
                        //        if (dRows.Length <= 0)
                        //        {
                        //            serviceIndex = DecodeServiceNumber(serviceId);
                        //            //if (serviceIndex != 1)
                        //            //    continue;
                        //        }
                        //    }
                        //}

                        if ( deceasedDate >= start.AddMonths (-1) )
                        {
                            month = caseCreatedDate.Month;
                            s1 = serviceId.Substring(0, 2);
                            if (!String.IsNullOrWhiteSpace(s1))
                            {
                                s1 += yy + "%";
                                dRows = dt.Select("quarter='" + month.ToString() + "' AND serviceId LIKE '" + s1 + "'");
                                if (dRows.Length == 0)
                                {
                                //    serviceIndex = DecodeServiceNumber(serviceId);
                                //    if (serviceIndex != 1)
                                    continue;
                                }
                            }

                        }
                        dt.Rows[i]["quarter"] = caseCreatedDate.Month;
                        oldServiceId = serviceId;
                        continue;
                    }

                    oldServiceId = serviceId;
                }
            }
            catch ( Exception ex )
            {
            }

            try
            {
                DataTable ddt = null;
                int beginMonth = start.Month;
                int endMonth = stop.Month;
                dRows = dt.Select("quarter>='" + beginMonth.ToString() + "' AND quarter <= '" + endMonth + "'");
                if (dRows.Length > 0)
                {
                    dt = dRows.CopyToDataTable();
                    //ddt = dRows.CopyToDataTable();
                    //ViewDataTable viewForm = new ViewDataTable(ddt, "contractNumber,serviceId,deceasedDate,caseCreatedDate");
                    //viewForm.Text = "Service Id's that didn't qualify for a Quarter . . .";
                    //viewForm.ShowDialog();
                }
                else
                {
                    dt.Rows.Clear();
                    return dt;
                }

                ddt = dt.Clone();
                DataTable tempDt = dt.Copy();

                tempview = tempDt.DefaultView;
                tempview.Sort = "serviceId asc, quarter asc";
                tempDt = tempview.ToTable();

                string lastServiceId = "";
                string newServiceId = "";
                int k = 0;
                int j = 0;
                int quarter = 0;
                int lastQuarter = 0;
                string oldLoc = "";
                string newLoc = "";
                int lastFun = 0;
                int newFun = 0;

                for (int i = 0; i < tempDt.Rows.Count; i++)
                {
                    serviceId = tempDt.Rows[i]["serviceId"].ObjToString();
                    if (String.IsNullOrWhiteSpace(lastServiceId))
                    {
                        lastServiceId = serviceId;
                        lastFun = serviceId.Substring(4).ObjToString().ObjToInt32();
                        continue;
                    }
                    oldLoc = lastServiceId.Substring(0, 2);
                    newLoc = serviceId.Substring(0, 2);
                    if ( newLoc != oldLoc )
                    {
                        lastServiceId = serviceId;
                        lastFun = serviceId.Substring(4).ObjToString().ObjToInt32();
                        oldLoc = newLoc;
                        continue;
                    }
                    else
                    {
                        newFun = serviceId.Substring(4).ObjToString().ObjToInt32();
                        if ( (newFun-1) != lastFun )
                        {
                            ddt.ImportRow(tempDt.Rows[i]);
                        }
                        lastFun = newFun;
                    }

                    lastServiceId = serviceId;
                }
                if (ddt.Rows.Count > 0)
                {
                    //ddt = dRows.CopyToDataTable();
                    ViewDataTable viewForm = new ViewDataTable(ddt, "contractNumber,serviceId,deceasedDate,caseCreatedDate,quarter");
                    viewForm.Text = "Funeral Out of Sequence . . .";
                    viewForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
            }

            return dt;
        }
        /****************************************************************************************/
        public static DataTable ProcessTheDatax(DataTable dt, DateTime start, DateTime stop)
        {
            DateTime deceasedDate = DateTime.Now;
            DateTime caseCreatedDate = DateTime.Now;
            string serviceId = "";

            if (G1.get_column_number(dt, "quarter") < 0)
                dt.Columns.Add("quarter", Type.GetType("System.Int32"));

            int year = start.Year;
            DateTime January = new DateTime(year, 1, 1);
            DateTime March = new DateTime(year, 3, 31);
            DateTime April = new DateTime(year, 4, 30);
            DateTime June = new DateTime(year, 6, 30);
            DateTime July = new DateTime(year, 7, 31);
            DateTime Sept = new DateTime(year, 9, 30);
            DateTime Oct = new DateTime(year, 8, 31);
            DateTime Dec = new DateTime(year, 12, 31);

            int quarter = 0;
            int currentQuarter = 1;
            if (start >= January && stop <= March)
                currentQuarter = 1;
            else if (start > March && stop <= June)
                currentQuarter = 2;
            else if (start > June && stop <= Sept)
                currentQuarter = 3;
            else
                currentQuarter = 4;

            if (start.Month == 1)
                currentQuarter = 1;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                caseCreatedDate = dt.Rows[i]["caseCreatedDate"].ObjToDateTime();
                serviceId = dt.Rows[i]["serviceId"].ObjToString();

                if (serviceId == "CT22043")
                {
                }

                if (deceasedDate >= January && deceasedDate <= March && caseCreatedDate >= January && caseCreatedDate <= March)
                {
                    dt.Rows[i]["quarter"] = 1;
                    continue;
                }
                if (deceasedDate > March && deceasedDate <= June && caseCreatedDate > March && caseCreatedDate <= June)
                {
                    dt.Rows[i]["quarter"] = 2;
                    continue;
                }
                if (deceasedDate > June && deceasedDate <= Sept && caseCreatedDate > June && caseCreatedDate <= Sept)
                {
                    dt.Rows[i]["quarter"] = 3;
                    continue;
                }
                if (deceasedDate > Sept && deceasedDate <= Dec && caseCreatedDate > Sept && caseCreatedDate <= Dec)
                {
                    dt.Rows[i]["quarter"] = 4;
                    continue;
                }
                if (deceasedDate < January && caseCreatedDate >= January && caseCreatedDate <= March)
                {
                    dt.Rows[i]["quarter"] = 1;
                    continue;
                }
                if (deceasedDate < January && caseCreatedDate > March && caseCreatedDate <= June)
                {
                    dt.Rows[i]["quarter"] = 2;
                    continue;
                }
                if (deceasedDate < January && caseCreatedDate > June && caseCreatedDate <= Sept)
                {
                    dt.Rows[i]["quarter"] = 3;
                    continue;
                }
                if (deceasedDate < January && caseCreatedDate > Sept && caseCreatedDate <= Dec)
                {
                    dt.Rows[i]["quarter"] = 4;
                    continue;
                }
                if (deceasedDate >= January && deceasedDate <= March && caseCreatedDate > March && caseCreatedDate <= April)
                {
                    dt.Rows[i]["quarter"] = 1;
                    continue;
                }
                if (deceasedDate > March && deceasedDate <= June && caseCreatedDate > June && caseCreatedDate <= July)
                {
                    dt.Rows[i]["quarter"] = 2;
                    continue;
                }
                if (deceasedDate > June && deceasedDate <= Sept && caseCreatedDate > Sept && caseCreatedDate <= Oct)
                {
                    dt.Rows[i]["quarter"] = 3;
                    continue;
                }
                if (deceasedDate > Sept && deceasedDate <= Dec && caseCreatedDate > Oct && caseCreatedDate <= Dec)
                {
                    dt.Rows[i]["quarter"] = 4;
                    continue;
                }
                if (deceasedDate >= January && caseCreatedDate > Sept)
                {
                    dt.Rows[i]["quarter"] = 4;
                    continue;
                }
                if (deceasedDate >= January && caseCreatedDate > June)
                {
                    dt.Rows[i]["quarter"] = 3;
                    continue;
                }
                if (deceasedDate >= January && caseCreatedDate > March)
                {
                    dt.Rows[i]["quarter"] = 2;
                    continue;
                }
                dt.Rows[i]["quarter"] = 0;
            }

            DataTable ddt = null;
            DataRow[] dRows = dt.Select("quarter='" + currentQuarter.ToString() + "'");
            if (dRows.Length > 0)
            {
                ddt = dRows.CopyToDataTable();
                ViewDataTable viewForm = new ViewDataTable(ddt, "contractNumber,serviceId,deceasedDate,caseCreatedDate");
                viewForm.Text = "Service Id's that didn't qualify for a Quarter . . .";
                viewForm.ShowDialog();
            }

            ddt = dt.Clone();
            DataTable tempDt = dt.Copy();

            DataView tempview = tempDt.DefaultView;
            tempview.Sort = "serviceId asc, quarter asc";
            tempDt = tempview.ToTable();

            string lastServiceId = "";
            string newServiceId = "";
            int k = 0;
            int j = 0;
            quarter = 0;
            int lastQuarter = 0;

            for (int i = 0; i < tempDt.Rows.Count; i++)
            {
                lastServiceId = serviceId;
                serviceId = tempDt.Rows[i]["serviceId"].ObjToString();
                quarter = tempDt.Rows[i]["quarter"].ObjToInt32();
                if (serviceId == "CT22043")
                {
                }
                if (serviceId.IndexOf("001") > 0)
                {
                    k = 1;
                    quarter = tempDt.Rows[i]["quarter"].ObjToInt32();
                    lastQuarter = quarter;
                }
                else
                {
                    if (quarter < lastQuarter)
                    {
                        if (i > 0)
                            ddt.ImportRow(tempDt.Rows[i - 1]);
                        ddt.ImportRow(tempDt.Rows[i]);
                    }
                    lastQuarter = quarter;
                    newServiceId = serviceId.Substring(4);
                }
            }
            if (ddt.Rows.Count > 0)
            {
                ViewDataTable viewForm = new ViewDataTable(ddt, "contractNumber,serviceId,deceasedDate,caseCreatedDate,quarter");
                viewForm.Text = "Funeral Out of Sequence . . .";
                viewForm.ShowDialog();
            }

            dRows = dt.Select("quarter='" + currentQuarter.ToString() + "'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            else
                dt.Rows.Clear();
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
            double payments = 0D;
            int idx = 0;
            string ch = "";
            string serviceId = "";
            string search = cmbSearch.Text.ToUpper();

            int year = startDate.Year;

            string yy = (year % 100).ToString("D2");

            string cmd = "Select * from `fcust_extended` c JOIN `fcustomers` a on c.`contractNumber` = a.`contractNumber` where ";
            cmd += " a.`deceasedDate` <= '" + date2 + "' ";
            cmd += " AND c.`serviceId` LIKE '__" + yy + "%' ";

            string names = getLocationNameQuery();
            if (!String.IsNullOrWhiteSpace(names))
                cmd += " AND " + names + " ";

            cmd += " ORDER BY c.`serviceId` ";
            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);

            dt = ProcessTheData(dt, start, stop);


            dt.Columns.Add("name");
            dt.Columns.Add("burial", Type.GetType("System.Double"));
            dt.Columns.Add("cremation", Type.GetType("System.Double"));
            dt.Columns.Add("other", Type.GetType("System.Double"));
            dt.Columns.Add("total", Type.GetType("System.Double"));
            dt.Columns.Add("payments", Type.GetType("System.Double"));

            //dt.Columns.Add("trustPayments", Type.GetType("System.Double"));
            dt.Columns.Add("trustDiscount", Type.GetType("System.Double"));

            dt.Columns.Add("insuranceAmountReceived", Type.GetType("System.Double"));
            //dt.Columns.Add("insurancePayments", Type.GetType("System.Double"));
            dt.Columns.Add("insuranceDiscount", Type.GetType("System.Double"));

            dt.Columns.Add("totalAllPayments", Type.GetType("System.Double"));

            //dt.Columns.Add("cashReceived", Type.GetType("System.Double"));
            dt.Columns.Add("loc");
            dt.Columns.Add("Location Name");
            dt.Columns.Add("GOOD");
            dt.Columns.Add("gsdate");

            bool isGood = false;
            DateTime gsDate = DateTime.Now;

            cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable dd = G1.get_db_data(cmd);

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
            double custPrice = 0D;

            double trustPayments = 0D;
            double trustPaymentsReceived = 0D;
            double insurancePayments = 0D;
            double insurancePaymentsReceived = 0;
            double cashReceived = 0D;
            double totalAllPayments = 0D;
            double totalIgnore = 0D;
            double compDiscounts = 0D;

            double totalServices = 0D;
            double totalMerchandise = 0D;
            double totalCashAdvance = 0D;

            double trustDiscount = 0D;
            double insuranceDiscount = 0D;
            double preneedDiscount = 0D;

            double contractTotal = 0D;
            double totalCost = 0D;
            double preDiscount = 0D;

            string prefix = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string suffix = "";

            DataRow[] dRows = null;

            barImport.Show();
            barImport.Maximum = dt.Rows.Count;
            barImport.Minimum = 0;
            barImport.Value = 0;
            barImport.Refresh();

            if (G1.get_column_number(dt, "totalsale") < 0)
                dt.Columns.Add("totalsale", Type.GetType("System.Double"));

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    Application.DoEvents();

                    barImport.Value = i + 1;
                    barImport.Refresh();

                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber == "B18019L")
                    {
                    }
                    serviceId = dt.Rows[i]["serviceId"].ObjToString();
                    if (serviceId.ToUpper() == "MW22001")
                    {
                    }
                    isGood = CheckForContract(contractNumber, ref gsDate);

                    if (isGood || !isGood)
                    {
                        dt.Rows[i]["GOOD"] = "Y";
                        dt.Rows[i]["gsdate"] = gsDate.ToString("yyyy-MM-dd");

                        currentPrice = dt.Rows[i]["currentPrice"].ObjToDouble();
                        discount = dt.Rows[i]["preneedDiscount"].ObjToDouble();
                        discount = Math.Abs(discount);

                        dt.Rows[i]["preneedDiscount"] = discount;

                        //currentPrice = currentPrice + discount;
                        dt.Rows[i]["currentPrice"] = currentPrice;

                        classA = dt.Rows[i]["classa"].ObjToDouble();
                        discount += classA;
                        totalReceived = dt.Rows[i]["trustAmountReceived"].ObjToDouble();
                        totalReceived = Math.Abs(totalReceived);

                        balanceDue = currentPrice - discount - totalReceived;
                        //dt.Rows[i]["balanceDue"] = balanceDue;

                        dValue = dt.Rows[i]["preneedDiscount"].ObjToDouble();
                        dValue = Math.Abs(dValue);
                        //dt.Rows[i]["preneedDiscount"] = discount;

                        payments = getPayments(dt, i);

                        totalAllPayments = 0D;
                        contractTotal = currentPrice;
                        totalCost = dt.Rows[i]["custPrice"].ObjToDouble();

                        //calculateTotalServices(contractNumber, ref contractTotal, ref totalCost, ref preDiscount);

                        trustPayments = dt.Rows[i]["trustPayments"].ObjToDouble();
                        trustPaymentsReceived = dt.Rows[i]["trustPaymentsreceived"].ObjToDouble();
                        insurancePayments = dt.Rows[i]["insurancePayments"].ObjToDouble();
                        insurancePaymentsReceived = dt.Rows[i]["insurancePaymentsreceived"].ObjToDouble();
                        cashReceived = dt.Rows[i]["cashReceived"].ObjToDouble();
                        compDiscounts = dt.Rows[i]["compDiscount"].ObjToDouble();

                        dt.Rows[i]["totalsale"] = dt.Rows[i]["taxMerchandise"].ObjToDouble() + dt.Rows[i]["taxAmount"].ObjToDouble();

                        //payments = calculateTotalPayments(contractNumber, ref trustPayments, ref trustPaymentsReceived, ref insurancePayments, ref insurancePaymentsReceived, ref cashReceived, ref compDiscounts, ref classA );

                        //dt.Rows[i]["classa"] = 0D;

                        //classA += compDiscounts;
                        //if (compDiscounts > 0D)
                        dt.Rows[i]["classa"] = compDiscounts + classA;

                        totalAllPayments = trustPaymentsReceived + insurancePaymentsReceived + cashReceived;
                        dt.Rows[i]["totalAllPayments"] = totalAllPayments;

                        dValue = trustPaymentsReceived;
                        dValue = Math.Abs(dValue);
                        dt.Rows[i]["trustAmountReceived"] = dValue;

                        dValue = trustPayments;
                        dValue = Math.Abs(dValue);
                        dt.Rows[i]["trustPayments"] = dValue;

                        trustDiscount = trustPayments - trustPaymentsReceived;
                        dValue = trustDiscount;
                        dValue = Math.Abs(dValue);
                        dt.Rows[i]["trustDiscount"] = dValue;

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
                        dValue = insuranceDiscount;
                        dValue = Math.Abs(dValue);
                        dt.Rows[i]["insuranceDiscount"] = dValue;

                        custPrice = dt.Rows[i]["custPrice"].ObjToDouble();
                        preneedDiscount = dt.Rows[i]["preneedDiscount"].ObjToDouble();
                        preneedDiscount = Math.Abs(preneedDiscount);

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

                        balanceDue = custPrice - classA - trustPaymentsReceived - cashReceived - insurancePaymentsReceived - trustDiscount - insuranceDiscount - compDiscounts;
                        //balanceDue = currentPrice - classA - trustPaymentsReceived - cashReceived - insurancePaymentsReceived - trustDiscount - insuranceDiscount - preneedDiscount;
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

                    prefix = dt.Rows[i]["prefix"].ObjToString();
                    firstName = dt.Rows[i]["firstName"].ObjToString();
                    middleName = dt.Rows[i]["middleName"].ObjToString();
                    lastName = dt.Rows[i]["lastName"].ObjToString();
                    suffix = dt.Rows[i]["suffix"].ObjToString();

                    dt.Rows[i]["name"] = G1.BuildFullName(prefix, firstName, middleName, lastName, suffix);
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
        private double calculateTotalPayments( string contractNumber, ref double trustPayments, ref double trustPaymentsReceived, ref double insurancePayments, ref double insurancePaymentsReceived, ref double cashReceived, ref double compDiscounts, ref double classA )
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

        private bool withHeaders = true;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            withHeaders = true;
            DialogResult result = MessageBox.Show("***Question***\nView WITH Headers?", "Print Priview Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if ( result == DialogResult.No)
                withHeaders = false;
            
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

            //ExportOptions options = printingSystem1.ExportOptions;
            //// Set Text-specific export options.
            //options.Text.Encoding = Encoding.Unicode;
            //options.Text.Separator =
            //    CultureInfo.CurrentCulture.TextInfo.ListSeparator.ToString();

            //// Set XLS-specific export options.
            //options.Xls.ShowGridLines = true;
            //options.Xls.SheetName = "Page 1";
            //options.Xls.TextExportMode = TextExportMode.Value;

            //// Set XLSX-specific export options.
            //options.Xlsx.ShowGridLines = true;
            //options.Xlsx.SheetName = "Page 1";
            //options.Xlsx.TextExportMode = TextExportMode.Text;

            printingSystem1.AddCommandHandler(new PrintDocumentCommandHandler());

            Font saveFont = gridMain.AppearancePrint.Row.Font;

            if (doSimple)
            {
                Font newFont = new Font(saveFont.FontFamily, 5F);
                gridMain.Appearance.Row.Font = newFont;
            }

            this.Cursor = Cursors.WaitCursor;

            G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();

            G1.AdjustColumnWidths(gridMain, 0.65D, false);

            this.Cursor = Cursors.Arrow;

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

            this.Cursor = Cursors.WaitCursor;
            G1.AdjustColumnWidths(gridMain, 0.65D, true);
            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();
            G1.AdjustColumnWidths(gridMain, 0.65D, false);
            this.Cursor = Cursors.Arrow;
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
            if (!withHeaders)
                return;

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

            string reportName = "Sales Tax";
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

            //dt = LoadDataFast(start, stop, null, false);
            dt = LoadData(start, stop, null, false);

            string loc = "";

            DataRow dR = null;
            DataRow[] dRows = null;

            DataView tempview = dt.DefaultView;
            tempview.Sort = "Location Name asc, serviceId asc";
            dt = tempview.ToTable();

            BuildSummaryTable(dt);

            //dt = FilterServices(dt);

            //GetSalesTax(dt);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.Refresh();

            gridMain.Columns["num"].Visible = true;
            gridMain.Columns["loc"].Visible = false;
            gridMain.Columns["Location Name"].Visible = false;
            gridMain.Columns["contractNumber"].Visible = false;

            gridMain.ClearSorting();
            gridMain.Columns["serviceId"].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;

            gridMain.OptionsView.ShowFooter = true;
            gridMain.OptionsView.ShowBands = false;

            gridMain.OptionsPrint.PrintBandHeader = false;
            gridMain.OptionsPrint.PrintFooter = true;

            gridMain.Appearance.Row.Font = new Font("Tahoma", 9F);
            gridMain.AppearancePrint.Row.Font = new Font("Tahoma", 9F);

            gridMain.ExpandAllGroups();
        }
        /****************************************************************************************/
        private void GetSalesTax ( DataTable dt )
        {
            string contractNumber = "";
            string cmd = "";
            DataTable dx = null;
            double tax = 0D;
            double merchandise = 0D;

            try
            {
                if (G1.get_column_number(dt, "tax") < 0)
                    dt.Columns.Add("tax", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "merchandise") < 0)
                    dt.Columns.Add("merchandise", Type.GetType("System.Double"));
                if (G1.get_column_number(dt, "totalsale") < 0)
                    dt.Columns.Add("totalsale", Type.GetType("System.Double"));

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    cmd = "Select * from `fcust_services` where `contractNumber` = '" + contractNumber + "' AND `taxAmount` <> '0';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        tax = 0D;
                        merchandise = 0D;
                        for (int j = 0; j < dx.Rows.Count; j++)
                        {
                            tax += dx.Rows[j]["taxAmount"].ObjToDouble();
                            merchandise += dx.Rows[j]["price"].ObjToDouble();
                        }
                        dt.Rows[i]["tax"] = tax;
                        dt.Rows[i]["merchandise"] = merchandise;
                        dt.Rows[i]["totalsale"] = tax + merchandise;
                    }
                    else
                    {
                        dt.Rows[i]["tax"] = 0D;
                        dt.Rows[i]["merchandise"] = 0D;
                        dt.Rows[i]["totalsale"] = 0D;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private DataTable FilterServices ( DataTable dt )
        {
            DateTime date = this.dateTimePicker1.Value;
            string year = date.Year.ToString("D4");
            year = year.Substring(2);

            string serviceId = "";
            string thisYear = "";

            for ( int i=(dt.Rows.Count-1); i>=0; i-- )
            {
                serviceId = dt.Rows[i]["serviceId"].ObjToString();

                if (String.IsNullOrWhiteSpace(serviceId))
                {
                    dt.Rows.RemoveAt(i);
                    continue;
                }
                if ( serviceId.Length < 4 )
                {
                    dt.Rows.RemoveAt(i);
                    continue;
                }
                thisYear = serviceId.Substring(2, 2);
                if ( thisYear != year )
                {
                    dt.Rows.RemoveAt(i);
                    continue;
                }
            }

            DateTime start = this.dateTimePicker1.Value;
            DateTime stop = this.dateTimePicker2.Value;
            start = start.AddMonths(-3);
            stop = start.AddMonths(2);
            int days = DateTime.DaysInMonth(stop.Year, stop.Month);
            stop = new DateTime(stop.Year, stop.Month, days);

            DateTime deceasedDate = DateTime.Now;
            DateTime createDate = DateTime.Now;
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                createDate = dt.Rows[i]["caseCreatedDate"].ObjToDateTime();
                if ( deceasedDate >= start && deceasedDate <= stop )
                {
                    dt.Rows.RemoveAt(i);
                    continue;
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
            //double dValue = 0D;
            //if (e.Column.FieldName.ToUpper() == "BURIAL")
            //{
            //    dValue = e.DisplayText.ObjToDouble();
            //    if (dValue > 0D)
            //        e.DisplayText = "X";
            //    else
            //        e.DisplayText = "";
            //}
            //if (e.Column.FieldName.ToUpper() == "CREMATION")
            //{
            //    dValue = e.DisplayText.ObjToDouble();
            //    if (dValue > 0D)
            //        e.DisplayText = "X";
            //    else
            //        e.DisplayText = "";
            //}
            //if (e.Column.FieldName.ToUpper() == "OTHER")
            //{
            //    dValue = e.DisplayText.ObjToDouble();
            //    if (dValue > 0D)
            //        e.DisplayText = "X";
            //    else
            //        e.DisplayText = "";
            //}
            //else if (e.Column.FieldName.ToUpper() == "TOTAL")
            //{
            //    if (e.DisplayText.Trim() == "0.00")
            //        e.DisplayText = "-          ";
            //}
            //else if (e.Column.FieldName.ToUpper() == "PRENEEDDISCOUNT")
            //{
            //    e.DisplayText = e.DisplayText.Replace("-", "");
            //}

            //if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            //{
            //    if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
            //        e.DisplayText = "";
            //    else
            //    {
            //        DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
            //        e.DisplayText = date.ToString("MM/dd/yyyy");
            //        if (date.Year < 30)
            //            e.DisplayText = "";
            //    }
            //}

            //if (e.DisplayText.Trim() == "0.00")
            //    e.DisplayText = "";
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
        private void chkSort_CheckedChanged(object sender, EventArgs e)
        {
            if (dgv.DataSource == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (chkSort.Checked)
            {
                if (dt != null)
                {
                    DataView tempview = dt.DefaultView;
                    tempview.Sort = "serviceId";
                    dt = tempview.ToTable();
                    G1.NumberDataTable(dt);
                    dgv.DataSource = dt;
                }

                gridMain.Columns["Location Name"].GroupIndex = 0;
                gridMain.OptionsView.ShowFooter = true;
                this.gridMain.ExpandAllGroups();
            }
            else
            {
                if (dt != null)
                {
                    DataView tempview = dt.DefaultView;
                    tempview.Sort = "serviceId";
                    dt = tempview.ToTable();
                    G1.NumberDataTable(dt);
                    dgv.DataSource = dt;
                }

                gridMain.Columns["Location Name"].GroupIndex = -1;
                gridMain.OptionsView.ShowFooter = true;
                gridMain.CollapseAllGroups();
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /****************************************************************************************/
        public class PrintDocumentCommandHandler : ICommandHandler
        {
            public virtual void HandleCommand(PrintingSystemCommand command, object[] args, IPrintControl printControl, ref bool handled)
            {
                string text = command.ToString();
                if (!CanHandleCommand(command, printControl))
                    return;
                if (MessageBox.Show("Contract Is Being Printed!!?", "Contract Printed Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
                {
                }
            }
            public virtual bool CanHandleCommand(PrintingSystemCommand command, IPrintControl printControl)
            {
                return command == PrintingSystemCommand.ExportXls;
            }
        }
        /****************************************************************************************/
        private void exportToExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string columnName = "";
            string data = "";
            DataTable dt = (DataTable)dgv.DataSource;

            string outfile = @"c:\rag\demo.xls";
            FileStream stream = new FileStream(@outfile, FileMode.OpenOrCreate);
            ExcelWriter writer = new ExcelWriter(stream);
            writer.BeginWrite();

            int col = 0;

            for (int j = 0; j < gridMain.Columns.Count; j++)
            {
                BandedGridColumn column = gridMain.Columns[j];
                if (column.Visible)
                {
                    columnName = column.FieldName;
                    if ( G1.get_column_number ( dt, columnName) >= 0 )
                    {
                        columnName = column.Caption;
                        writer.WriteCell(0, col, columnName);
                        col++;
                    }
                }
            }

            col = 0;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                col = 0;
                for (int j = 0; j < gridMain.Columns.Count; j++)
                {
                    BandedGridColumn column = gridMain.Columns[j];
                    if (column.Visible)
                    {
                        columnName = column.FieldName;
                        if (G1.get_column_number(dt, columnName) >= 0)
                        {
                            data = dt.Rows[i][columnName].ObjToString();
                            if (!String.IsNullOrWhiteSpace(data))
                                writer.WriteCell(i + 1, col, data);
                            col++;
                        }
                    }
                }
            }
            writer.EndWrite();
            stream.Close();

            GrantAccess(@outfile);
            System.Diagnostics.Process.Start(@outfile);
        }
        /****************************************************************************************/
        private void GrantAccess(string fullPath)
        {
            DirectoryInfo dInfo = new DirectoryInfo(fullPath);

            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule( new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));

            dInfo.SetAccessControl(dSecurity);
        }        
        /****************************************************************************************/
    }

}