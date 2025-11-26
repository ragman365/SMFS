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
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraGrid.Columns;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net.Mail;

/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class AgentProspectReport : DevExpress.XtraEditors.XtraForm
    {
        private DataTable originalDt = null;
        private DataTable originalDt2 = null;
        private bool autoRun = false;
        private bool autoForce = false;
        private string workReport = "";
        private string sendTo = "";
        private string sendWhere = "";
        private string da = "";
        private bool loading = true;
        /****************************************************************************************/
        public AgentProspectReport()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        public AgentProspectReport (bool auto, bool force )
        {
            InitializeComponent();
            autoRun = auto;
            autoForce = force;
            RunAutoReports();
        }
        /****************************************************************************************/
        private void RunAutoReports()
        {
            //G1.AddToAudit("System", "AutoRun", "AT Funeral Activity Report", "Starting Funeral Autorun . . . . . . . ", "");
            workReport = "Agent Family Report for " + DateTime.Now.ToString("MM/dd/yyyy");
            string cmd = "Select * from `remote_processing`;";
            DataTable dt = G1.get_db_data(cmd);
            string report = "";
            DateTime date = DateTime.Now;
            int presentDay = date.Day;
            int dayToRun = 0;
            string status = "";
            string frequency = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                status = dt.Rows[i]["status"].ObjToString();
                if (status.ToUpper() == "INACTIVE")
                    continue;
                if (!autoForce)
                {
                    dayToRun = dt.Rows[i]["day_to_run"].ObjToInt32();
                    frequency = dt.Rows[i]["dateIncrement"].ObjToString();
                    if (!AutoRunSetup.CheckOkToRun(dayToRun, frequency))
                        return;
                }
                report = dt.Rows[i]["report"].ObjToString();
                sendTo = dt.Rows[i]["sendTo"].ObjToString();
                sendWhere = dt.Rows[i]["sendWhere"].ObjToString();
                da = dt.Rows[i]["da"].ObjToString();
                if (report.ToUpper() == "AGENT FAMILY REPORT")
                {
                    //G1.AddToAudit("System", "AutoRun", "Funeral Activity Report Load", "Starting Load . . . . . . . ", "");
                    AgentFamilyReport_Load(null, null);
                }
            }
        }
        /****************************************************************************************/
        private void AgentFamilyReport_Load(object sender, EventArgs e)
        {
            btnGenerate.Hide();

            dgv2.Hide();
            dgv.Dock = DockStyle.Fill;

            DateTime now = DateTime.Now;
            DateTime startDate = now.AddMonths(-1);
            startDate = new DateTime(startDate.Year, startDate.Month, 1);
            this.dateTimePicker1.Value = startDate;
            int days = DateTime.DaysInMonth(startDate.Year, startDate.Month);
            DateTime stopDate = new DateTime(startDate.Year, startDate.Month, days);
            this.dateTimePicker2.Value = stopDate;

            gridMain.Columns["num"].Visible = true;
            gridMain.Columns["contractNumber"].Visible = true;

            gridMain.OptionsPrint.PrintBandHeader = false;
            gridMain.OptionsPrint.PrintFooter = false;

            getLocations();

            if (autoRun)
            {
                btnRun_Click(null, null);

                //G1.AddToAudit("System", "AutoRun", "Funeral Activity Print Preview", "Starting Report . . . . . . . ", "");
                printPreviewToolStripMenuItem_Click(null, null);
                this.Close();
            }

            loading = false;
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
        private string getLocationNameQuery2()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboLocation.EditValue.ToString().Split('|');
            DataRow[] dRows = null;
            DataTable dt = (DataTable)chkComboLocation.Properties.DataSource;
            try
            {
                for (int i = 0; i < locIDs.Length; i++)
                {
                    if (!String.IsNullOrWhiteSpace(locIDs[i]))
                    {
                        if (procLoc.Trim().Length > 0)
                            procLoc += ",";
                        dRows = dt.Select("LocationCode='" + locIDs[i].Trim() + "'");
                        if (dRows.Length > 0)
                        {
                            procLoc += "'" + dRows[0]["LocationCode"].ObjToString().Trim() + "'";
                            //procLoc += "'" + locIDs[i].Trim() + "'";
                        }
                    }
                }
            }
            catch ( Exception ex)
            {
            }
            return procLoc.Length > 0 ? " `Location Name` IN (" + procLoc + ") " : "";
        }
        /*******************************************************************************************/
        private string getLocationNameQuery()
        {
            string procLoc = "";
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
            now = now.AddMonths(1);
            this.dateTimePicker1.Value = now;

            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(-1);
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

            btnGenerate.Show();
            btnGenerate.Refresh();
        }
        /****************************************************************************************/
        private void runData()
        {
            DateTime start = this.dateTimePicker1.Value;
            DateTime stop = this.dateTimePicker2.Value;

            DataTable dt = LoadData(start, stop, null, false);

            string loc = "";

            DataRow dR = null;
            DataRow[] dRows = null;

            //gridMain.Appearance.Row.Font = new Font("Tahoma", 10F);
            //gridMain.AppearancePrint.Row.Font = new Font("Tahoma", 10F);

            originalDt = dt;

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.Refresh();
            //dgv.Hide();

            //dgv2.DataSource = dt;
            //dgv2.Dock = DockStyle.Fill;

            gridMain.Columns["num"].Visible = true;
            //gridMain.Columns["loc"].Visible = false;
            //gridMain.Columns["Location Name"].Visible = false;
            gridMain.Columns["contractNumber"].Visible = false;
            //gridMain.Appearance.Row.Font = new Font("Tahoma", 10F);
            //gridMain.AppearancePrint.Row.Font = new Font("Tahoma", 10F);
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
                cmd += " AND c.`serviceId` = '" + contract + "' ";

            if (search == "DECEASED DATE")
                cmd += " ORDER BY a.`deceasedDate` ";
            else if (search == "CREATE DATE")
                cmd += " ORDER BY c.`caseCreateDate` ";
            else
                cmd += " ORDER BY c.`serviceDate` ";

            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("loc");
            dt.Columns.Add("Location Name");
            dt.Columns.Add("prospects");
            dt.Columns.Add("name");
            dt.Columns.Add("relation");
            dt.Columns.Add("phone");
            dt.Columns.Add("address");
            dt.Columns.Add("paidBy");
            //dt.Columns.Add("GOOD");
            //dt.Columns.Add("gsdate");

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

            string casket = "";
            string vault = "";

            string prospects = "";
            string relationship = "";
            string record = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string phone = "";
            string phoneType = "";
            string address = "";

            string proNames = "";
            string proRelation = "";
            string proPhone = "";
            string proAddress = "";

            string paidBy = "";
            string paid = "";


            DataRow[] dRows = null;
            DataTable dx = null;
            DataTable rDt = null;

            int count = 0;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    Application.DoEvents();

                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber == "B18019L")
                    {
                    }
                    serviceId = dt.Rows[i]["serviceId"].ObjToString();
                    if (serviceId.ToUpper() == "BS22002")
                    {
                    }

                    serviceLoc = dt.Rows[i]["serviceLoc"].ObjToString();

                    contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                    if (DailyHistory.gotCemetery(contractNumber))
                    {
                        dt.Rows[i]["contractNumber"] = "";
                        continue;
                    }

                    if (DailyHistory.gotCemetery(serviceId))
                    {
                        dt.Rows[i]["contractNumber"] = "";
                        continue;
                    }

                    loc = dt.Rows[i]["serviceLoc"].ObjToString();

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
                    }
                    dt.Rows[i]["loc"] = loc;

                    casket = dt.Rows[i]["casket"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(casket))
                    {
                        cmd = "Select * from `casket_master` where `casketCode` = '" + casket + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                            dt.Rows[i]["casket"] = dx.Rows[0]["casketdesc"].ObjToString();
                    }
                    vault = dt.Rows[i]["vault"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(vault))
                    {
                        cmd = "Select * from `casket_master` where `casketCode` = '" + vault + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                            dt.Rows[i]["vault"] = dx.Rows[0]["casketdesc"].ObjToString();
                    }

                    prospects = "";
                    proNames = "";
                    proPhone = "";
                    proRelation = "";
                    proAddress = "";
                    
                    cmd = "Select * from `relatives` where `contractNumber` = '" + contractNumber + "';";
                    rDt = G1.get_db_data(cmd);
                    for ( int j=0; j<rDt.Rows.Count; j++)
                    {
                        relationship = rDt.Rows[j]["depRelationship"].ObjToString().Trim();
                        if (String.IsNullOrWhiteSpace(relationship))
                            continue;
                        if (relationship == "PB" || relationship == "HPB" || relationship == "CLERGY" || relationship == "MUSICIAN")
                            continue;
                        if (relationship == "FUNERAL DIRECTOR" || relationship == "DISCLOSURES")
                            continue;
                        record = rDt.Rows[j]["record"].ObjToString();
                        firstName = rDt.Rows[j]["depFirstName"].ObjToString().Trim();
                        middleName = rDt.Rows[j]["depMI"].ObjToString();
                        lastName = rDt.Rows[j]["depLastName"].ObjToString();

                        phoneType = rDt.Rows[j]["phoneType"].ObjToString();
                        phone = rDt.Rows[j]["phone"].ObjToString();
                        address = rDt.Rows[j]["address"].ObjToString() + ", " + rDt.Rows[j]["city"].ObjToString() + ", " + rDt.Rows[j]["state"].ObjToString() + ", " + rDt.Rows[j]["zip"].ObjToString();

                        prospects += record + ",";

                        proPhone += phone + ", " + phoneType + "~";
                        proAddress += address + "~";
                        proNames += firstName + ", " + middleName + ", " + lastName + "~";
                        proRelation += relationship + "~";
                    }

                    prospects = prospects.TrimEnd(',');
                    dt.Rows[i]["prospects"] = prospects;

                    proNames = proNames.TrimEnd('~');
                    proAddress = proAddress.TrimEnd('~');
                    proPhone = proPhone.TrimEnd('~');
                    proRelation = proRelation.TrimEnd('~');

                    dt.Rows[i]["name"] = proNames;
                    dt.Rows[i]["relation"] = proRelation;
                    dt.Rows[i]["address"] = proAddress;
                    dt.Rows[i]["phone"] = proPhone;

                    cmd = "Select * from `cust_payment_details` where `contractNumber` = '" + contractNumber + "';";
                    rDt = G1.get_db_data(cmd);
                    paidBy = "";
                    for (int j = 0; j < rDt.Rows.Count; j++)
                    {
                        paid = rDt.Rows[j]["type"].ObjToString();
                        if (paid.ToUpper().IndexOf("CHECK") >= 0)
                            paid = "Check";
                        if (!paidBy.Contains(paid))
                            paidBy += paid + ", ";
                    }

                    paidBy = paidBy.Trim();
                    paidBy = paidBy.TrimEnd(',');
                    dt.Rows[i]["paidBy"] = paidBy;
                    count++;
                }
                catch (Exception ex)
                {
                }
            }
            return dt;
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
        private DataTable BuildReport(DataTable dt)
        {
            DataTable funDt = G1.get_db_data("Select * from `funeralhomes`;");
            DataRow[] dRows = null;

            DataTable dx = new DataTable();
            dx.Columns.Add("Location Name");
            dx.Columns.Add("contractNumber");
            dx.Columns.Add("C1");
            dx.Columns.Add("C2");
            dx.Columns.Add("C3");
            dx.Columns.Add("C4");
            dx.Columns.Add("C5");

            DateTime date1 = this.dateTimePicker1.Value;
            DateTime date2 = this.dateTimePicker2.Value;

            string contract = "";
            string location = "";
            string locationName = "";

            DateTime serviceDate = DateTime.Now;
            DateTime serviceTime = DateTime.Now;
            string sTime = "";
            string serviceId = "";

            string dateRange = date1.ToString("MM/dd/yyyy") + " - " + date2.ToString("MM/dd/yyyy");

            AddNewRow(dx, "", "", "Daily Funerals", "Date Range :", dateRange, "");

            DataTable rDt = new DataTable();
            rDt.Columns.Add("serviceId");
            rDt.Columns.Add("serviceLoc");
            rDt.Columns.Add("contractNumber");
            rDt.Columns.Add("funeralHome");
            rDt.Columns.Add("Location Name");
            rDt.Columns.Add("name");
            rDt.Columns.Add("what");
            rDt.Columns.Add("date");
            rDt.Columns.Add("type");
            rDt.Columns.Add("casket");
            rDt.Columns.Add("vault");
            rDt.Columns.Add("serialNumber");
            rDt.Columns.Add("director");
            rDt.Columns.Add("arranger");
            rDt.Columns.Add("interment");
            rDt.Columns.Add("informant");
            rDt.Columns.Add("address");
            rDt.Columns.Add("PP");

            DateTime date = this.dateTimePicker1.Value;
            DateTime startDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            date = this.dateTimePicker2.Value;
            DateTime stopDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);

            DataRow dr = null;

            string str = "";
            string type = "";
            string fName = "";
            string mName = "";
            string lName = "";
            string prefix = "";
            string suffix = "";
            string name = "";
            int idx = 0;

            string director = "";
            string arranger = "";
            string casket = "";
            string vault = "";
            string serialNumber = "";
            string informant = "";
            string relation = "";
            string phone;
            string address = "";
            string city = "";
            string state = "";
            string zip = "";
            string commAddress = "";
            string interment = "";

            string what = "";
            string pp = "";

            string serviceLocation = "";
            string serviceLocation2 = "";

            string cmd = "";
            DataTable ddx = null;


            dt.Columns.Add("RAGSRVDATE");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    contract = dt.Rows[i]["contractNumber"].ObjToString();
                    location = dt.Rows[i]["serviceLoc"].ObjToString();
                    serviceId = dt.Rows[i]["serviceId"].ObjToString();
                    serviceDate = dt.Rows[i]["srvdate"].ObjToDateTime();
                    fName = dt.Rows[i]["firstName"].ObjToString();
                    mName = dt.Rows[i]["middleName"].ObjToString();
                    lName = dt.Rows[i]["lastName"].ObjToString();
                    prefix = dt.Rows[i]["prefix"].ObjToString();
                    suffix = dt.Rows[i]["suffix"].ObjToString();
                    name = G1.BuildFullName(prefix, fName, mName, lName, suffix);

                    address = dt.Rows[i]["cemAddress"].ObjToString().Trim();
                    city = dt.Rows[i]["cemCity"].ObjToString();
                    state = dt.Rows[i]["cemState"].ObjToString();
                    zip = dt.Rows[i]["cemZip"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(address))
                        address += ", ";
                    if (!String.IsNullOrWhiteSpace(city))
                        address += city + ", ";
                    if (!String.IsNullOrWhiteSpace(state))
                        address += state;
                    address = address.TrimEnd(',');
                    if (!String.IsNullOrWhiteSpace(zip))
                        address += " " + zip;

                    commAddress = address;
                    interment = dt.Rows[i]["cem"].ObjToString();



                    informant = "";
                    relation = "";
                    phone = "";
                    cmd = "Select * from `relatives` WHERE `contractNumber` = '" + contract + "' AND `informant` = '1';";
                    ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count > 0)
                    {
                        fName = ddx.Rows[0]["depFirstName"].ObjToString();
                        mName = ddx.Rows[0]["depMI"].ObjToString();
                        lName = ddx.Rows[0]["depLastName"].ObjToString();
                        prefix = ddx.Rows[0]["depPrefix"].ObjToString();
                        suffix = ddx.Rows[0]["depSuffix"].ObjToString();
                        informant = G1.BuildFullName(prefix, fName, mName, lName, suffix);

                        relation = ddx.Rows[0]["depRelationship"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(relation))
                            informant += " - " + relation;
                        phone = ddx.Rows[0]["phone"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(phone))
                            informant += " " + phone;
                    }

                    arranger = dt.Rows[i]["Funeral Arranger"].ObjToString();
                    idx = arranger.IndexOf("[");
                    if (idx > 0)
                        arranger = arranger.Substring(0, idx);

                    director = dt.Rows[i]["Funeral Director"].ObjToString();
                    idx = director.IndexOf("[");
                    if (idx > 0)
                        director = director.Substring(0, idx);

                    if (serviceDate.Year > 1000)
                    {
                        if (serviceDate >= startDate && serviceDate <= stopDate)
                        {
                            serviceLocation = dt.Rows[i]["srvloc"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(serviceLocation))
                            {

                                str = dt.Rows[i]["srvtime"].ObjToString();
                                if (String.IsNullOrWhiteSpace(str))
                                    str = "11 PM";
                                serviceTime = str.ObjToDateTime();
                                dr = rDt.NewRow();
                                dr["serviceId"] = serviceId;
                                dr["funeralhome"] = dt.Rows[i]["serviceLoc"].ObjToString();
                                dr["serviceLoc"] = dt.Rows[i]["srvloc"].ObjToString();
                                dr["PP"] = dt.Rows[i]["SRVType"].ObjToString();
                                dr["contractNumber"] = contract;
                                dr["what"] = "Service";
                                dr["date"] = serviceDate.ToString("yyyyMMdd") + serviceTime.ToString("hhmm tt");
                                dr["name"] = name;
                                casket = dt.Rows[i]["casketdesc"].ObjToString();
                                if (String.IsNullOrWhiteSpace(casket))
                                {
                                    casket = dt.Rows[i]["casket"].ObjToString();
                                    if (!String.IsNullOrWhiteSpace(casket))
                                    {
                                        cmd = "Select * from `casket_master` where `casketcode` = '" + casket + "';";
                                        ddx = G1.get_db_data(cmd);
                                        if (ddx.Rows.Count > 0)
                                            casket = ddx.Rows[0]["casketdesc"].ObjToString();
                                    }
                                }
                                dr["casket"] = casket;
                                dr["serialNumber"] = dt.Rows[i]["serialNumber"].ObjToString();
                                vault = dt.Rows[i]["vault"].ObjToString();
                                if ( !String.IsNullOrWhiteSpace ( vault ))
                                {
                                    cmd = "Select * from `casket_master` where `casketcode` = '" + vault + "';";
                                    ddx = G1.get_db_data(cmd);
                                    if (ddx.Rows.Count > 0)
                                        vault = ddx.Rows[0]["casketdesc"].ObjToString();
                                }
                                dr["vault"] = vault;
                                type = dt.Rows[i]["SRVType"].ObjToString();
                                dr["type"] = type;
                                dr["address"] = commAddress;
                                dr["interment"] = interment;
                                dr["arranger"] = arranger;
                                dr["director"] = director;
                                dr["informant"] = informant;
                                rDt.Rows.Add(dr);
                            }
                        }
                    }
                    str = dt.Rows[i]["SRV2Date"].ObjToString();
                    if (serviceDate.Year > 1000)
                    {
                        if (serviceDate >= startDate && serviceDate <= stopDate)
                        {
                            serviceLocation2 = dt.Rows[i]["srv2loc"].ObjToString();

                            if (!String.IsNullOrWhiteSpace(serviceLocation2))
                            {
                                str = dt.Rows[i]["srv2time"].ObjToString();
                                if (String.IsNullOrWhiteSpace(str))
                                    str = "11 PM";
                                serviceTime = str.ObjToDateTime();
                                dr = rDt.NewRow();
                                dr["serviceId"] = serviceId;
                                dr["funeralhome"] = dt.Rows[i]["serviceLoc"].ObjToString();
                                dr["serviceLoc"] = dt.Rows[i]["srv2loc"].ObjToString();
                                dr["PP"] = dt.Rows[i]["SRV2Type"].ObjToString();
                                dr["contractNumber"] = contract;
                                dr["what"] = "Service";
                                dr["date"] = serviceDate.ToString("yyyyMMdd") + serviceTime.ToString("HHmmss");
                                dr["name"] = name;
                                dr["casket"] = "";
                                dr["vault"] = "";
                                dr["serialNumber"] = "";
                                type = dt.Rows[i]["SRV2Type"].ObjToString();
                                dr["type"] = type;
                                dr["address"] = commAddress;
                                dr["interment"] = interment;
                                dr["arranger"] = arranger;
                                dr["director"] = director;
                                dr["informant"] = informant;
                                rDt.Rows.Add(dr);
                            }
                        }
                    }

                    str = dt.Rows[i]["vstDate"].ObjToString();
                    serviceDate = str.ObjToDateTime();
                    if (serviceDate.Year > 1000)
                    {
                        if (serviceDate >= startDate && serviceDate <= stopDate)
                        {
                            serviceLocation = dt.Rows[i]["VIS1Loc"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(serviceLocation))
                            {
                                str = dt.Rows[i]["vstStart"].ObjToString();
                                if (String.IsNullOrWhiteSpace(str))
                                    str = "11 PM";
                                serviceTime = str.ObjToDateTime();
                                dr = rDt.NewRow();
                                dr["serviceId"] = serviceId;
                                dr["funeralhome"] = dt.Rows[i]["serviceLoc"].ObjToString();
                                dr["serviceLoc"] = dt.Rows[i]["VIS1Loc"].ObjToString();
                                dr["PP"] = dt.Rows[i]["Vis1Type"].ObjToString();
                                dr["contractNumber"] = contract;
                                dr["what"] = "Visitation";
                                dr["date"] = serviceDate.ToString("yyyyMMdd") + serviceTime.ToString("HHmmss");
                                dr["name"] = name;
                                dr["arranger"] = arranger;
                                dr["director"] = director;
                                dr["informant"] = informant;
                                dr["address"] = commAddress;
                                dr["interment"] = interment;
                                rDt.Rows.Add(dr);
                            }
                        }
                    }
                    str = dt.Rows[i]["vis2Date"].ObjToString();
                    serviceDate = str.ObjToDateTime();
                    if (serviceDate.Year > 1000)
                    {
                        if (serviceDate >= startDate && serviceDate <= stopDate)
                        {
                            serviceLocation = dt.Rows[i]["VIS2Loc"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(serviceLocation))
                            {
                                str = dt.Rows[i]["vis2TimeStart"].ObjToString();
                                if (String.IsNullOrWhiteSpace(str))
                                    str = "11 PM";
                                serviceTime = str.ObjToDateTime();
                                dr = rDt.NewRow();
                                dr["serviceId"] = serviceId;
                                dr["funeralhome"] = dt.Rows[i]["serviceLoc"].ObjToString();
                                dr["serviceLoc"] = dt.Rows[i]["VIS2Loc"].ObjToString();
                                dr["PP"] = dt.Rows[i]["Vis2Type"].ObjToString();
                                dr["contractNumber"] = contract;
                                dr["what"] = "Visitation";
                                dr["date"] = serviceDate.ToString("yyyyMMdd") + serviceTime.ToString("HHmmss");
                                dr["name"] = name;
                                dr["arranger"] = arranger;
                                dr["director"] = director;
                                dr["informant"] = informant;
                                dr["address"] = commAddress;
                                dr["interment"] = interment;
                                rDt.Rows.Add(dr);
                            }
                        }
                    }
                    str = dt.Rows[i]["CommDate"].ObjToString();
                    serviceDate = str.ObjToDateTime();
                    if (serviceDate.Year > 1000)
                    {
                        if (serviceDate >= startDate && serviceDate <= stopDate)
                        {
                            if (!String.IsNullOrWhiteSpace(interment))
                            {
                                str = dt.Rows[i]["commtime"].ObjToString();
                                if (String.IsNullOrWhiteSpace(str))
                                    str = "11 PM";
                                serviceTime = str.ObjToDateTime();
                                dr = rDt.NewRow();
                                dr["serviceId"] = serviceId;
                                dr["serviceLoc"] = dt.Rows[i]["cem"].ObjToString();
                                dr["funeralhome"] = dt.Rows[i]["serviceLoc"].ObjToString();
                                dr["contractNumber"] = contract;
                                dr["what"] = "COMMITTAL";
                                dr["date"] = serviceDate.ToString("yyyyMMdd") + serviceTime.ToString("HHmmss");
                                dr["name"] = name;
                                dr["type"] = dt.Rows[i]["SRV2Type"].ObjToString();
                                dr["arranger"] = arranger;
                                dr["director"] = director;
                                dr["informant"] = informant;
                                dr["PP"] = dt.Rows[i]["COMMType"].ObjToString();
                                dr["interment"] = interment;
                                dr["address"] = commAddress;
                                rDt.Rows.Add(dr);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }

            }

            DataView tempview = rDt.DefaultView;
            tempview.Sort = "date asc";
            rDt = tempview.ToTable();

            int count = 0;
            string pageBreak = "";
            string DateOfService = "";

            for (int i = 0; i < rDt.Rows.Count; i++)
            {
                try
                {
                    contract = rDt.Rows[i]["contractNumber"].ObjToString();
                    location = rDt.Rows[i]["funeralHome"].ObjToString();
                    locationName = location;
                    dRows = funDt.Select("atneedcode='" + location + "'");
                    if (dRows.Length > 0)
                        locationName = dRows[0]["name"].ObjToString();
                    what = rDt.Rows[i]["what"].ObjToString();
                    pp = rDt.Rows[i]["PP"].ObjToString();
                    serviceId = rDt.Rows[i]["serviceId"].ObjToString();

                    AddEmptyRows(dx, contract, locationName, 1);

                    str = rDt.Rows[i]["date"].ObjToString();
                    serviceDate = str.ObjToDateTime();
                    name = rDt.Rows[i]["name"].ObjToString();

                    casket = rDt.Rows[i]["casket"].ObjToString();
                    str = "";
                    if (!String.IsNullOrWhiteSpace(casket))
                        str = "CASKET";

                    vault = rDt.Rows[i]["vault"].ObjToString();
                    serialNumber = rDt.Rows[i]["serialNumber"].ObjToString();
                    DateOfService = serviceDate.ToString("hh:mm tt");
                    if (DateOfService == "12:00 AM")
                        DateOfService = "N/A";

                    AddNewRow(dx, contract, locationName, locationName, name, serviceId, str);
                    AddNewRow(dx, contract, locationName, DateOfService, "", "", casket);

                    location = rDt.Rows[i]["serviceLoc"].ObjToString();
                    director = rDt.Rows[i]["director"].ObjToString();
                    arranger = rDt.Rows[i]["arranger"].ObjToString();
                    informant = rDt.Rows[i]["informant"].ObjToString();
                    interment = rDt.Rows[i]["interment"].ObjToString();
                    type = rDt.Rows[i]["type"].ObjToString();
                    if (what.ToUpper() == "SERVICE")
                        what = "Funeral Service";
                    else if (what.ToUpper() == "VISITATION")
                        what = "Visitation";
                    else if (what.ToUpper() == "COMMITTAL")
                        what = "Committal";
                    if (type.ToUpper() == "PUBLIC" || type.ToUpper() == "PRIVATE")
                        what = type + " " + what;
                    else if (pp.ToUpper() == "PUBLIC" || pp.ToUpper() == "PRIVATE")
                        what = type + " " + what;

                    str = "";
                    if (!String.IsNullOrWhiteSpace(vault))
                        str = "Vault: " + vault;
                    AddNewRow(dx, contract, locationName, "", "", "", str);

                    str = "";
                    if (!String.IsNullOrWhiteSpace(serialNumber))
                        str = "SN: " + serialNumber;
                    AddNewRow(dx, contract, locationName, what, "LOCATION:", location, str);
                    AddNewRow(dx, contract, locationName, "", "ARRANGER:", arranger, "");
                    AddNewRow(dx, contract, locationName, "", "DIRECTOR:", director, "");
                    AddNewRow(dx, contract, locationName, "", "INTERMENT:", interment, address);
                    AddNewRow(dx, contract, locationName, "", "INFORMANT:", informant, "");

                    count++;
                    pageBreak = "";

                    if ((count % 3) == 0)
                        AddNewRow(dx, contract, locationName, "", "", "", "", "BREAK");
                    //if ( count == 2 )
                    //{
                    //    AddNewRow(dx, contract, locationName, "", "", "", "", "BREAK");
                    //}
                    //else if (count > 2)
                    //{
                    //    if (((count-2) % 3) == 0)
                    //        AddNewRow(dx, contract, locationName, "", "", "", "", "BREAK" );
                    //}
                }
                catch (Exception ex)
                {
                }
            }

            return dx;
        }
        /****************************************************************************************/
        private void AddEmptyRows(DataTable dt, string contract, string location, int count)
        {
            for (int i = 0; i < count; i++)
                AddNewRow(dt, contract, location, "", "", "", "");
        }
        /****************************************************************************************/
        private void AddNewRow(DataTable dt, string contractNumber, string location, string c1, string c2, string c3, string c4, string c5="" )
        {
            DataRow dRow = dt.NewRow();
            dRow["contractNumber"] = contractNumber;
            dRow["Location Name"] = location;
            dRow["C1"] = c1;
            dRow["C2"] = c2;
            dRow["C3"] = c3;
            dRow["C4"] = c4;
            dRow["C5"] = c5;
            dt.Rows.Add(dRow);
        }
        /****************************************************************************************/
        private void calculateTotalServices(string contractNumber, ref double contractTotal, ref double totalCost, ref double preDiscount)
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
        private double calculateTotalPayments(string contractNumber, ref double trustPayments, ref double trustPaymentsReceived, ref double insurancePayments, ref double insurancePaymentsReceived, ref double cashReceived, ref double compDiscounts)
        {
            trustPayments = 0D;
            trustPaymentsReceived = 0D;
            insurancePayments = 0D;
            insurancePaymentsReceived = 0D;
            cashReceived = 0D;
            compDiscounts = 0D;

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

                    if (status.ToUpper() != "DEPOSITED")
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
                    else if (type.IndexOf("INSURANCE") == 0)
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
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
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

                G1.AdjustColumnWidths(gridMain, 0.65D, true);

                printableComponentLink1.CreateDocument();

                //DateTime today = DateTime.Now;
                //workReport = "Funeral Activity Report";
                ////string filename = "C:/SMFS_Reports/" + workReport + "_" + today.Year.ToString("D4") + today.Month.ToString("D2") + today.Day.ToString("D2") + "_" + today.Hour.ToString("D2") + today.Minute.ToString("D2") + ".pdf";
                //string filename = "C:/SMFS_Reports/" + workReport + ".pdf";
                //if (File.Exists(filename))
                //    File.Delete(filename);

                //try
                //{
                //    printableComponentLink1.ExportToPdf(filename);
                //}
                //catch ( Exception ex)
                //{
                //}

                if (autoRun)
                {
                    DataTable dt = null;
                    try
                    {
                        //DevExpress.XtraGrid.GridControl xDGV = (DevExpress.XtraGrid.GridControl)printableComponentLink1.Component;
                        //dt = (DataTable)xDGV.DataSource;
                        //if (dt == null)
                        //    G1.AddToAudit("System", "AutoRun", "Agent Family Report", "DT is NULL", "");
                        //else
                        //{
                        //    int lastRow = dt.Rows.Count;
                        //    //G1.AddToAudit("System", "AutoRun", "Funeral Activity DT=", lastRow.ToString(), "");
                        //}
                    }
                    catch (Exception ex)
                    {
                        G1.AddToAudit("System", workReport, "AutoRun", "FAILED", "");
                        return;
                    }
                    string emailLocations = DailyHistory.ParseOutLocations(dt);

                    string path = G1.GetReportPath();
                    DateTime today = DateTime.Now;

                    //string filename = "C:/SMFS_Reports/" + workReport + "_" + today.Year.ToString("D4") + today.Month.ToString("D2") + today.Day.ToString("D2") + "_" + today.Hour.ToString("D2") + today.Minute.ToString("D2") + ".pdf";
                    workReport = "Agent Family Report";
                    //string filename = "C:/SMFS_Reports/" + workReport + "_" + today.Year.ToString("D4") + today.Month.ToString("D2") + today.Day.ToString("D2") + "_" + today.Hour.ToString("D2") + today.Minute.ToString("D2") + ".pdf";
                    string filename = "C:/SMFS_Reports/" + workReport + ".pdf";
                    if (File.Exists(filename))
                        File.Delete(filename);

                    //G1.AddToAudit("System", "AutoRun", "Funeral Activity PDF", filename, "");
                    //G1.AddToAudit("System", "AutoRun", "Funeral Activity Send to", sendTo, "");
                    //G1.AddToAudit("System", "AutoRun", "Funeral Activity Send Where", sendWhere, "");
                    //G1.AddToAudit("System", "AutoRun", "Funeral Activity Send DA", da, "");

                    try
                    {
                        printableComponentLink1.ExportToPdf(filename);
                        //if (File.Exists(filename))
                        //    G1.AddToAudit("System", "AutoRun", "Funeral Activity", "FILE WAS CREATED!!!!!", "");
                        //else
                        //    G1.AddToAudit("System", "AutoRun", "Funeral Activity", "No File Created", "");
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message.ToString();
                        G1.AddToAudit("System", "AutoRun", "Agent Family Report", message, "");
                    }

                    if (File.Exists(filename))
                    {
                        //G1.AddToAudit("System", "AutoRun", "Funeral Activity Send To", "Starting Email . . . . . . . ", "");
                        string textDate = today.ToString("MM/dd/yyyy");
                        //RemoteProcessing.AutoRunSendTo(workReport + " for " + textDate, filename, sendTo, sendWhere, da, emailLocations);
                        RemoteProcessing.AutoRunSend(workReport + " for " + textDate, filename, sendTo, sendWhere, da, emailLocations);
                    }
                }
                else
                    printableComponentLink1.ShowPreviewDialog();

                //            printableComponentLink1.ShowPreview();

                G1.AdjustColumnWidths(gridMain, 0.65D, false);
                gridMain.Appearance.Row.Font = saveFont;
            }
            catch ( Exception ex )
            {
            }
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
            if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;

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

            string reportName = "Daily Schedule of Services";
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

                    string newPage = dt.Rows[row]["C5"].ObjToString();
                    if (newPage.ToUpper() == "BREAK")
                    {
                        pageBreak = true;
                        e.Cancel = true;
                    }
                }
                catch (Exception ex)
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
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            if ( dgv2.Visible )
                G1.ShowHideFindPanel(gridMain2);
            else
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
        private double originalSize2 = 0D;
        private Font mainFont2 = null;
        private Font newFont2 = null;
        private Font HeaderFont2 = null;
        private double originalHeaderSize2 = 0D;
        private void ScaleCells2()
        {
            if (originalSize2 == 0D)
            {
                //                originalSize = gridMain.Columns["address1"].AppearanceCell.FontSizeDelta.ObjToDouble();
                originalSize2 = gridMain2.Columns["Location Name"].AppearanceCell.Font.Size;
                mainFont2 = gridMain2.Columns["Location Name"].AppearanceCell.Font;
                HeaderFont2= gridMain.Appearance.HeaderPanel.Font;
                originalHeaderSize2 = gridMain2.Appearance.HeaderPanel.Font.Size;
            }
            double scale = txtScale.Text.ObjToDouble();
            double size = scale / 100D * originalSize;
            Font font = new Font(mainFont2.Name, (float)size);

            for (int i = 0; i < gridMain2.Columns.Count; i++)
            {
                gridMain2.Columns[i].AppearanceCell.Font = font;
            }
            gridMain2.Appearance.GroupFooter.Font = font;
            gridMain2.Appearance.FooterPanel.Font = font;
            gridMain2.AppearancePrint.FooterPanel.Font = font;
            gridMain2.AppearancePrint.GroupFooter.Font = font;
            newFont = font;
            size = scale / 100D * originalHeaderSize;
            font = new Font(HeaderFont.Name, (float)size, FontStyle.Bold);
            for (int i = 0; i < gridMain2.Columns.Count; i++)
            {
                gridMain2.Columns[i].AppearanceHeader.Font = font;
            }
            //gridMain.Appearance.HeaderPanel.Font = font;
            //gridMain.AppearancePrint.HeaderPanel.Font = font;
            gridMain2.RefreshData();
            gridMain2.RefreshEditor(true);

            dgv2.Refresh();
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
                if ( dgv2.Visible )
                    ScaleCells2();
                else
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
        private DataTable summaryDt = null;
        private void BuildSummaryTable(DataTable dt)
        {
            summaryDt = dt.Clone();

            double burial = 0D;
            double cremation = 0D;
            double other = 0D;
            string loc = "";
            DataRow dR = null;
            DataRow[] dRows = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                loc = dt.Rows[i]["loc"].ObjToString();

                burial = dt.Rows[i]["burial"].ObjToDouble();
                cremation = dt.Rows[i]["cremation"].ObjToDouble();
                other = dt.Rows[i]["other"].ObjToDouble();

                dRows = summaryDt.Select("loc='" + loc + "'");
                if (dRows.Length <= 0)
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
        }
        /****************************************************************************************/
        private void gridMain_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            //GridView View = sender as GridView;
            //if (e.RowHandle >= 0)
            //{
            //    double dValue = 0D;
            //    string column = e.Column.FieldName.ToUpper();
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
            //}
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                EditCust custForm = new EditCust(contract);
                custForm.Tag = contract;
                custForm.Show();
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
        private void gridMain_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                int maxHeight = 0;
                int newHeight = 0;
                string name = "";
                bool doit = false;
                foreach (GridColumn column in gridMain.Columns)
                {
                    doit = true;
                    doit = false;
                    name = column.FieldName.ToUpper();
                    if (name == "C1")
                        doit = true;
                    if (doit)
                    {
                        DataTable dt = (DataTable)dgv.DataSource;
                        int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                        string data = dt.Rows[row]["C1"].ObjToString().ToUpper();
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
                                    if ( data.IndexOf ( "FUNERAL") > 0 )
                                        maxHeight = 35;
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
        private void gridMain_RowCellStyle_1(object sender, RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            //if (e.RowHandle >= 0)
            //{
            //    string column = e.Column.FieldName.ToUpper();
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
            //    string str = dt.Rows[row]["C1"].ObjToString().ToUpper();
            //    if ( str.IndexOf ( "FUNERAL") > 0 )
            //    {
            //        if (str.Trim().ToUpper().IndexOf("PUBLIC") == 0)
            //            return;
            //        else if (str.Trim().ToUpper().IndexOf("PRIVATE") == 0)
            //            return;
            //        e.Appearance.BackColor = Color.LightGray;
            //    }
            //}
        }
        /****************************************************************************************/
        private void btnSendEmail_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            string newDirectory = @"C:\SMFSData\";
            newDirectory += "/Daily/";
            if (!Directory.Exists(newDirectory))
                Directory.CreateDirectory(newDirectory);
            string filename= newDirectory + "DAILYSCHEDULE_" + date.ToString("yyyy-MM-dd") + ".PDF";
            gridMain.ExportToPdf(filename);

            SendEmail ("Daily Schedule for " + date.ToString("MM/dd/yyyy"), "Daily Schedule", filename, "robbyxyzzy@gmail.com");
        }
        /***********************************************************************************************/
        private void SendEmail ( string subject, string body, string attachment, string toWho )
        {
            this.Cursor = Cursors.WaitCursor;


            string from = "robbyxyzzy@gmail.com";
            //from = "cliffjenkins@colonialtel.com";
            //string pw = "Cliff@Colonial";
            //pw = "xkiypozlptspspwr";
            string pw = "hranncwgetlvkxoi";
            string option = "";
            string answer = "";

            string to = toWho;
            //string subject = "Merchandise Orders Needed";
            //string body = "On-Hand Orders are needed.";

            string senderID = from;
            string senderPassword = pw;
            if (String.IsNullOrWhiteSpace(from))
            {
                MessageBox.Show("***ERROR*** Email From Address is empty!");
                return;
            }
            if (String.IsNullOrWhiteSpace(pw))
            {
                MessageBox.Show("***ERROR*** Email PW is empty!");
                return;
            }
            RemoteCertificateValidationCallback orgCallback = ServicePointManager.ServerCertificateValidationCallback;
            //            string body = "Test";
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(OnValidateCertificate);
                ServicePointManager.Expect100Continue = false;
                MailMessage mail = new MailMessage();

                //for (int i = 0; i < dd.Rows.Count; i++)
                //{
                //    string email = dd.Rows[i]["email"].ObjToString();
                //    if (!String.IsNullOrWhiteSpace(email))
                //        mail.To.Add(email);
                //}
                mail.To.Add(to);
                mail.From = new MailAddress(senderID);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;
                mail.Attachments.Add(new Attachment(attachment));
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Credentials = new System.Net.NetworkCredential(senderID, senderPassword);
                smtp.Send(mail);
                //MessageBox.Show("Email Sent Successfully");
                //                Console.WriteLine("Email Sent Successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Email Unsuccessful\n\n" + ex.Message.ToString());
                //                Console.WriteLine(ex.Message);
            }
            finally
            {
                ServicePointManager.ServerCertificateValidationCallback = orgCallback;
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private static bool OnValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        /****************************************************************************************/
        private void cmbGroupBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.ComboBox box = (System.Windows.Forms.ComboBox)sender;
            string groupBy = box.Text.ToUpper();
            if ( groupBy == "LOCATION")
            {
                gridMain.Columns["loc"].GroupIndex = 1;
                gridMain.ExpandAllGroups();
            }
            else if ( groupBy == "AGENT")
            {
                gridMain.Columns["loc"].GroupIndex = -1;
            }
            else
            {
                gridMain.Columns["loc"].GroupIndex = -1;
            }
            gridMain.RefreshEditor(true);
            gridMain.RefreshData();
        }
        /****************************************************************************************/
        private void agentLocationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditAgentLocationEmails agentForm = new EditAgentLocationEmails();
            agentForm.Show();
        }
        /****************************************************************************************/
        private void gridMain2_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            DataTable dt = (DataTable)dgv2.DataSource;
            GridView view = sender as GridView;
            bool doit = false;
            bool doBoth = false;
            int style = 0;
            string s = "";
            if (e.RowHandle >= 0)
            {
                string column = e.Column.FieldName.ToUpper();
                int row = gridMain.GetDataSourceRowIndex(e.RowHandle);

                if ( row >= 0 )
                    style = dt.Rows[row]["style"].ObjToInt32();
                else
                {
                    row = e.RowHandle;
                    style = dt.Rows[row]["style"].ObjToInt32();
                }

                string C1 = view.GetRowCellDisplayText(e.RowHandle, view.Columns["C1"]);
                if (C1.Trim().ToUpper().IndexOf("LOCATION:") >= 0)
                    doit = true;
                else if (C1.Trim().ToUpper().IndexOf("FUNERAL TYPE:") >= 0)
                    doit = true;
                else if (C1.Trim().ToUpper().IndexOf("SURVIVOR NAME:") >= 0)
                    doBoth = true;

                if ( doit )
                {
                    if ((style % 2) == 0)
                        e.Appearance.BackColor = Color.LightGray;
                    else
                        e.Appearance.BackColor = Color.Transparent;
                    Font font = new Font(e.Appearance.Font.Name, e.Appearance.Font.Size, FontStyle.Bold);
                    e.Appearance.Font = font;
                }
                else if (doBoth)
                {
                    if ((style % 2) == 0)
                        e.Appearance.BackColor = Color.LightGray;
                    else
                        e.Appearance.BackColor = Color.Transparent;
                    Font font = new Font(e.Appearance.Font.Name, e.Appearance.Font.Size, FontStyle.Bold | FontStyle.Underline );
                    e.Appearance.Font = font;
                }
                else
                {
                    if ((style % 2) == 0)
                        e.Appearance.BackColor = Color.LightGray;
                    else
                        e.Appearance.BackColor = Color.Transparent;
                }
            }
        }
        /****************************************************************************************/
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if ( btnGenerate.Text.ToUpper() == "GO BACK")
            {
                btnGenerate.Text = "Generate Report";
                btnGenerate.BackColor = Color.DarkKhaki;
                dgv2.Hide();
                return;
            }

            //dt.Columns.Add("loc");
            //dt.Columns.Add("Location Name");
            //dt.Columns.Add("prospects");
            //dt.Columns.Add("name");
            //dt.Columns.Add("relation");
            //dt.Columns.Add("phone");
            //dt.Columns.Add("address");

            DataTable dx = new DataTable();
            dx.Columns.Add("Location Name");
            dx.Columns.Add("contractNumber");
            dx.Columns.Add("C1");
            dx.Columns.Add("C2");
            dx.Columns.Add("C3");
            dx.Columns.Add("C4");
            dx.Columns.Add("C5");
            dx.Columns.Add("C6");
            dx.Columns.Add("style", Type.GetType("System.Int32"));
            dx.Columns.Add("myBreak");

            string contractNumber = "";
            string paidBy = "";
            string locationName = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string birthDate = "";
            string deceasedDate = "";
            string serviceId = "";
            string names = "";
            string location = "";
            string[] Lines = null;
            string[] Lines2 = null;
            string [] relations = null;
            string[] addresses = null;
            string[] phones = null;

            string relation = "";
            string address = "";
            string phone = "";
            int age = 0;


            DataRow dR = null;

            DataTable dt = (DataTable)dgv.DataSource;

            string sort = cmbGroupBy.Text.Trim().ToUpper();
            if ( sort == "LOCATION")
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "loc asc";
                dt = tempview.ToTable();
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if ( i > 0 )
                {
                    AddBlankRow(dx, (i-1), contractNumber, locationName );
                }

                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                paidBy = dt.Rows[i]["paidBy"].ObjToString();
                locationName = dt.Rows[i]["Location Name"].ObjToString();
                firstName = dt.Rows[i]["firstName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();
                location = dt.Rows[i]["Location Name"].ObjToString();
                birthDate = dt.Rows[i]["birthDate"].ObjToDateTime().ToString("MM/dd/yyyy");
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime().ToString("MM/dd/yyyy");
                serviceId = dt.Rows[i]["serviceId"].ObjToString();

                age = G1.GetAge(birthDate.ObjToDateTime(), deceasedDate.ObjToDateTime() );


                dR = dx.NewRow();
                dR["contractNumber"] = contractNumber;
                dR["Location Name"] = locationName;
                dR["C1"] = "Location: " + location;
                dR["C2"] = "Funeral # " + serviceId;
                dR["C3"] = "Deceased Name: " + firstName + " " + lastName;
                dR["C4"] = "Date of Death: " + deceasedDate;
                dR["C5"] = "Date of Birth: " + birthDate;
                dR["C6"] = "Age: " + age.ToString();
                dR["style"] = i;

                dx.Rows.Add(dR);

                AddBlankRow(dx, i, contractNumber, locationName );

                dR = dx.NewRow();
                dR["contractNumber"] = contractNumber;
                dR["Location Name"] = locationName;
                dR["C1"] = "Funeral Type: " + dt.Rows[i]["funeral_classification"].ObjToString();
                dR["C2"] = "Casket or Urn:" + dt.Rows[i]["casket"].ObjToString();
                dR["C3"] = "Vault:" + dt.Rows[i]["vault"].ObjToString();
                dR["C4"] = "Paid By: " + paidBy;
                dR["style"] = i;
                dx.Rows.Add(dR);

                AddBlankRow(dx, i, contractNumber, locationName );

                dR = dx.NewRow();
                dR["Location Name"] = locationName;
                dR["contractNumber"] = contractNumber;
                dR["C1"] = "Survivor Name:";
                dR["C2"] = "Survivor Relationship:";
                dR["C3"] = "Survivor Phone:";
                dR["C4"] = "Survivor Address. City, State";
                dR["style"] = i;
                dx.Rows.Add(dR);

                names = dt.Rows[i]["name"].ObjToString();
                if (String.IsNullOrWhiteSpace(names))
                    continue;

                AddBlankRow(dx, i, contractNumber, locationName );

                Lines = names.Split('~');

                names = dt.Rows[i]["relation"].ObjToString();
                relations = names.Split('~');

                names = dt.Rows[i]["address"].ObjToString();
                addresses = names.Split('~');

                names = dt.Rows[i]["phone"].ObjToString();
                phones = names.Split('~');

                string name = "";
                for (int j = 0; j < Lines.Length; j++)
                {
                    firstName = "";
                    middleName = "";
                    lastName = "";
                    name = Lines[j].Trim();
                    Lines2 = name.Split(',');
                    for (int k = 0; k < Lines2.Length; k++)
                    {
                        if (k == 0)
                            firstName = Lines2[k].Trim();
                        else if (k == 1)
                            middleName = Lines2[k].Trim();
                        else if (k == 2)
                            lastName = Lines2[k].Trim();
                    }

                    name = "";
                    if (!String.IsNullOrWhiteSpace(firstName))
                        name = firstName;
                    if (!String.IsNullOrWhiteSpace(middleName))
                        name += " " + middleName;
                    if (!String.IsNullOrWhiteSpace(lastName))
                        name += " " + lastName;
                    name = name.Trim();

                    dR = dx.NewRow();
                    dR["C1"] = name;
                    if ((j + 1) <= relations.Length)
                        dR["C2"] = relations[j].Trim();
                    if ((j + 1) <= phones.Length)
                    {
                        phone = phones[j].Trim();
                        if (phone == ",,,")
                            phone = "";
                        if (phone == ",")
                            phone = "";
                        phone = reformatPhone(phone);
                        dR["C3"] = phone;
                    }
                    if ((j + 1) <= addresses.Length)
                    {
                        address = addresses[j].Trim();
                        if (address == ", , ,")
                            address = "";
                        if (address == ",")
                            address = "";
                        dR["C4"] = address;
                    }

                    dR["contractNumber"] = contractNumber;
                    dR["Location Name"] = locationName;
                    dR["style"] = i;
                    dx.Rows.Add(dR);
                }
            }

            btnGenerate.Text = "Go Back";
            btnGenerate.BackColor = Color.Green;

            RenumberTable(dx);
            DetermineBreaks(dx);

            originalDt2 = dx;

            dgv2.DataSource = dx;
            dgv2.Dock = DockStyle.Fill;
            dgv2.Visible = true;
            dgv2.Refresh();
        }
        /****************************************************************************************/
        private void AddBlankRow ( DataTable dx, int row, string contractNumber, string locationName )
        {
            DataRow dR = dx.NewRow();
            dR["contractNumber"] = contractNumber;
            dR["Location Name"] = locationName;
            dR["style"] = row;
            dx.Rows.Add(dR);
        }
        /****************************************************************************************/
        public static string reformatPhone ( string phone, bool noParen = false )
        {
            string newPhone = "";
            string phoneType = "";
            if (String.IsNullOrWhiteSpace(phone))
                return phone;

            try
            {
                if (phone.IndexOf("(") >= 0)
                {
                }

                string[] Lines = phone.Split(',');
                if (Lines.Length == 2)
                    phoneType = Lines[1];
                string str = Lines[0].Trim();
                str = str.Replace("-", "");
                str = str.Replace("(", "");
                str = str.Replace(")", "");
                if (str.Length >= 9)
                {
                    str = str.Replace(" ", "");
                    if ( !noParen )
                        newPhone = "(";
                    newPhone += str.Substring(0, 3);
                    if ( noParen )
                        newPhone += "-";
                    else
                        newPhone += ") ";
                    newPhone += str.Substring(3, 3);
                    newPhone += "-";
                    newPhone += str.Substring(6);
                    if (!String.IsNullOrWhiteSpace(phoneType))
                        newPhone += ", " + phoneType;
                }
                else if ( str.Length == 7 )
                {
                    newPhone = str.Substring(0, 3);
                    newPhone += "-";
                    newPhone += str.Substring(3);
                }
                else if (str.Length >= 7)
                {
                    newPhone = str.Substring(3, 3);
                    newPhone += "-";
                    newPhone += str.Substring(6);
                    if (!String.IsNullOrWhiteSpace(phoneType))
                        newPhone += ", " + phoneType;
                }
            }
            catch ( Exception ex)
            {
                newPhone = phone;
            }
            return newPhone;
        }
        /****************************************************************************************/
        private void gridMain2_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                EditCust custForm = new EditCust(contract);
                custForm.Tag = contract;
                custForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void DetermineBreaks(DataTable dt)
        {
            string oldLocation = "";
            string location = "";
            int row = 0;
            int count = 1;
            bool breakNow = false;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    location = dt.Rows[i]["C1"].ObjToString();
                    if (i > 0)
                    {
                        count++;
                        if (location.ToUpper().IndexOf("LOCATION:") == 0)
                        {
                            breakNow = LookAhead(dt, i, count);
                            if ( breakNow )
                            {
                                count = 0;
                                dt.Rows[i]["myBreak"] = "MYBREAK";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private bool LookAhead ( DataTable dt, int i, int count )
        {
            bool breakNow = false;
            string location = "";
            if ( count > 66 )
            {
            }
            for ( int j=(i+1); j<dt.Rows.Count; j++)
            {
                location = dt.Rows[j]["C1"].ObjToString();
                if (location.ToUpper().IndexOf("LOCATION:") == 0)
                    break;
                if ((count + (j - i)) >= 33)
                {
                    breakNow = true;
                    break;
                }
            }
            return breakNow;
        }
        /****************************************************************************************/
        private void RenumberTable ( DataTable dt )
        {
            string oldLocation = "";
            string location = "";
            int row = 0;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    location = dt.Rows[i]["C1"].ObjToString();
                    if ( i > 0 )
                    {
                        if (location.ToUpper().IndexOf("LOCATION:") == 0)
                            row++;
                    }
                    dt.Rows[i]["style"] = row;
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void chkComboLocation_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (dgv2.Visible)
                {
                    string names = getLocationNameQuery2();
                    DataRow[] dRows = originalDt2.Select(names);
                    DataTable dt = originalDt2.Clone();
                    for (int i = 0; i < dRows.Length; i++)
                        dt.ImportRow(dRows[i]);
                    G1.NumberDataTable(dt);
                    RenumberTable(dt);
                    dgv2.DataSource = dt;
                    dgv2.Refresh();

                }
                else
                {
                    string names = getLocationNameQuery();
                    DataRow[] dRows = originalDt.Select(names);
                    DataTable dt = originalDt.Clone();
                    for (int i = 0; i < dRows.Length; i++)
                        dt.ImportRow(dRows[i]);
                    G1.NumberDataTable(dt);
                    dgv.DataSource = dt;
                    dgv.Refresh();
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void gridMain2_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            int rowHandle = e.RowHandle;
            if (gridMain2.IsDataRow(rowHandle))
            {
                try
                {
                    DataTable dt = (DataTable)dgv2.DataSource;
                    int row = gridMain2.GetDataSourceRowIndex(rowHandle);

                    string newPage = dt.Rows[row]["myBreak"].ObjToString();
                    if (newPage.ToUpper() == "MYBREAK")
                    {
                        pageBreak = true;
                        //e.Cancel = true;
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        /****************************************************************************************/
        private void gridMain2_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (pageBreak)
                e.PS.InsertPageBreak(e.Y);
            pageBreak = false;
        }
        /****************************************************************************************/
    }
}