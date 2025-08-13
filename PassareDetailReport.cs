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
using System.Linq;

/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class PassareDetailReport : DevExpress.XtraEditors.XtraForm
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
        public PassareDetailReport()
        {
            InitializeComponent();

            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            //AddSummaryColumn("amountReceived", null, "Custom");
            //AddSummaryColumn("amountFiled", null, "Custom");
            //AddSummaryColumn("custPrice", null, "Custom");
            //AddSummaryColumn("custMerchandise", null, "Custom");
            //AddSummaryColumn("custServices", null, "Custom");
            //AddSummaryColumn("totalDiscount", null, "Custom");
            //AddSummaryColumn("currentPrice", null, "Custom");
            //AddSummaryColumn("currentMerchandise", null, "Custom");
            //AddSummaryColumn("currentServices", null, "Custom");
            //AddSummaryColumn("balanceDue", null, "Custom");
            //AddSummaryColumn("additionalDiscount", null, "Custom");
            //AddSummaryColumn("classa", null, "Custom");
            //AddSummaryColumn("grossAmountReceived", null, "Custom");
            //AddSummaryColumn("amountDiscount", null, "Custom");
            //AddSummaryColumn("amountGrowth1", null, "Custom");
            //AddSummaryColumn("cashAdvance", null, "Custom");
            //AddSummaryColumn("trustAmountFiled", null, "Custom");
            //AddSummaryColumn("trustAmountReceived", null, "Custom");
            //AddSummaryColumn("insAmountFiled", null, "Custom");
            //AddSummaryColumn("insAmountReceived", null, "Custom");
            //AddSummaryColumn("trustPayment", null, "Custom");
            //AddSummaryColumn("netFuneral", null, "Custom");
            //AddSummaryColumn("cashCheck", null, "Custom");
            //AddSummaryColumn("cc", null, "Custom");

            AddSummaryColumn("casketAmount", null);
            AddSummaryColumn("vaultAmount", null);
            AddSummaryColumn("urnAmount", null);

            AddSummaryColumn("trusts", null);
            AddSummaryColumn("insurance", null);
            AddSummaryColumn("checks", null);
            AddSummaryColumn("totalCash", null);
            AddSummaryColumn("card", null);
            AddSummaryColumn("payments", null);
            AddSummaryColumn("refunds", null);

            AddSummaryColumn("netFuneral", null);
            AddSummaryColumn("totalCollected", null);

            AddSummaryColumn("custPrice", null);
            AddSummaryColumn("discounts", null);

            //AddSummaryColumn("casketCost", null, "Custom");
            //AddSummaryColumn("vaultCost", null, "Custom");
            //AddSummaryColumn("endingBalance", null, "Custom");
            //AddSummaryColumn("upgrade", null, "Custom");
            //AddSummaryColumn("otherBonuses", null, "Custom");
            //AddSummaryColumn("urn", null, "Custom");
            //AddSummaryColumn("newDiscount", null, "Custom");
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.Grid.GridView gMain = null, string summaryItemType = "", string format = "")
        {
            if (gMain == null)
                gMain = gridMain;
            if (String.IsNullOrWhiteSpace(format))
                format = "{0:N2}";
            if (summaryItemType.ToUpper() == "CUSTOM")
            {
                gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
                GridSummaryItem item = null;
                bool found = false;
                for (int i = 0; i < gMain.GroupSummary.Count; i++)
                {
                    item = gMain.GroupSummary[i];
                    if (item.FieldName == columnName)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    gMain.GroupSummary.Add(new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Custom, columnName, gMain.Columns[columnName], format));
                }
            }
            else
            {
                gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
                GridSummaryItem item = null;
                bool found = false;
                for (int i = 0; i < gMain.GroupSummary.Count; i++)
                {
                    item = gMain.GroupSummary[i];
                    if (item.FieldName == columnName)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    gMain.GroupSummary.Add(new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Sum, columnName, gMain.Columns[columnName], format));
                }
            }
            gMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /****************************************************************************************/
        private void PassareDetailReport_Load(object sender, EventArgs e)
        {
            string title = "Funeral Detail Report";
            if (G1.oldCopy)
            {
                menuStrip1.BackColor = Color.LightBlue;
                title = "Passare Funeral Detail Report";
            }

            this.Text = title;

            btnGenerate.Hide();
            btnSendEmail.Hide();

            gridMain.Columns["ID"].Visible = false;
            gridMain.Columns["tmstamp"].Visible = false;

            if ( !G1.isAdmin() )
            {
                miscToolStripMenuItem.Dispose();
                deletePaymentsToolStripMenuItem.Dispose();
            }

            btnGenerate.Hide();

            dgv2.Hide();
            dgv.Dock = DockStyle.Fill;

            DateTime now = DateTime.Now;

            now = new DateTime(2020, 1, 1);

            DateTime startDate = now.AddMonths(-1);
            startDate = new DateTime(startDate.Year, startDate.Month, 1);
            this.dateTimePicker1.Value = startDate;
            int days = DateTime.DaysInMonth(startDate.Year, startDate.Month);
            DateTime stopDate = new DateTime(startDate.Year, startDate.Month, days);
            this.dateTimePicker2.Value = stopDate;

            gridMain.Columns["num"].Visible = true;
            //gridMain.Columns["contractNumber"].Visible = true;

            gridMain.OptionsPrint.PrintBandHeader = false;
            gridMain.OptionsPrint.PrintFooter = true;

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

            //btnGenerate.Show();
            //btnGenerate.Refresh();
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
            dt.Columns.Add("discounts", Type.GetType("System.Double"));
            //dt.Columns.Add("casketAmount", Type.GetType("System.Double"));
            //dt.Columns.Add("vaultAmount", Type.GetType("System.Double"));
            dt.Columns.Add("urnAmount", Type.GetType("System.Double"));

            dt.Columns.Add("trusts", Type.GetType("System.Double"));
            dt.Columns.Add("insurance", Type.GetType("System.Double"));
            dt.Columns.Add("payments", Type.GetType("System.Double"));
            dt.Columns.Add("totalCash", Type.GetType("System.Double"));
            dt.Columns.Add("checks", Type.GetType("System.Double"));
            dt.Columns.Add("refunds", Type.GetType("System.Double"));
            dt.Columns.Add("card", Type.GetType("System.Double"));

            dt.Columns.Add("trustDepositDate");
            dt.Columns.Add("insuranceDepositDate");
            dt.Columns.Add("paymentDepositDate");
            dt.Columns.Add("checkDepositDate");
            dt.Columns.Add("refundDepositDate");
            dt.Columns.Add("cashDepositDate");
            dt.Columns.Add("cardDepositDate");

            dt.Columns.Add("netFuneral", Type.GetType("System.Double"));
            dt.Columns.Add("totalCollected", Type.GetType("System.Double"));

            if (G1.get_column_number(dt, "casketdesc") < 0)
                dt.Columns.Add("casketdesc");
            if (G1.get_column_number(dt, "vaultDesc") < 0)
                dt.Columns.Add("vaultDesc");
            if (G1.get_column_number(dt, "urnDesc") < 0)
                dt.Columns.Add("urnDesc");


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
            string status = "";
            string type = "";
            string what = "";
            double amount = 0D;

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
            double totalDiscount = 0D;
            double additionalDiscount = 0D;
            double compDiscount = 0D;
            double payment = 0D;

            double contractTotal = 0D;
            double totalCost = 0D;
            double preDiscount = 0D;
            string description = "";

            string casket = "";
            string vault = "";
            string record = "";

            //string prospects = "";
            //string relationship = "";
            //string firstName = "";
            //string middleName = "";
            //string lastName = "";
            //string phone = "";
            //string phoneType = "";
            //string address = "";

            //string proNames = "";
            //string proRelation = "";
            //string proPhone = "";
            //string proAddress = "";

            double trusts = 0D;
            double insurance = 0D;
            double checks = 0D;
            double cash = 0D;
            double refunds = 0D;
            double card = 0D;

            string trustDepositDate = "";
            string insuranceDepositDate = "";
            string checkDepositDate = "";
            string cashDepositDate = "";
            string refundDepositDate = "";
            string paymentDepositDate = "";
            string cardDepositDate = "";



            double netFuneral = 0D;
            double totalCollected = 0D;
            double grossFuneral = 0D;

            string paidBy = "";
            string paid = "";


            DataRow[] dRows = null;
            DataRow dRow = null;
            DataTable dx = null;
            DataTable rDt = null;
            DataTable pDt = null;

            DataTable rtDt = null;

            int count = 0;

            int lastRow = dt.Rows.Count;

            barImport.Show();
            barImport.Minimum = 0;
            barImport.Maximum = dt.Rows.Count;
            barImport.Refresh();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    Application.DoEvents();

                    barImport.Value = i + 1;
                    barImport.Refresh();

                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    serviceId = dt.Rows[i]["serviceId"].ObjToString();

                    serviceLoc = dt.Rows[i]["serviceLoc"].ObjToString();

                    contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                    if (DailyHistory.gotCemetery(contractNumber))
                    {
                        dt.Rows[i]["contractNumber"] = "";
                        continue;
                    }

                    if ( String.IsNullOrWhiteSpace ( serviceLoc))
                    {
                        Trust85.decodeContractNumber(serviceId, ref trust, ref loc);
                        serviceLoc = loc;
                        dt.Rows[i]["serviceLoc"] = loc;
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

                    cmd = "Select * from `cust_payments` where `contractNumber` = '" + contractNumber + "';";
                    pDt = G1.get_db_data(cmd);

                    if (G1.get_column_number(pDt, "description") < 0)
                        pDt.Columns.Add("descrition");

                    totalDiscount = 0D;
                    totalAllPayments = 0D;
                    for (int j = 0; j < pDt.Rows.Count; j++)
                    {
                        status = pDt.Rows[j]["status"].ObjToString().ToUpper();
                        if (status != "DEPOSITED" && status != "ACCEPT")
                            continue;
                        type = pDt.Rows[j]["type"].ObjToString().ToUpper();
                        payment = pDt.Rows[j]["payment"].ObjToDouble();
                        description = pDt.Rows[j]["description"].ObjToString().ToUpper();
                        if (type.IndexOf("ADJUSTMENT") >= 0 || type.IndexOf ( "DISCOUNT") >= 0 || description.IndexOf ( "DISCOUNT") >= 0 )
                            totalDiscount += payment;
                    }

                    dt.Rows[i]["discounts"] = totalDiscount;
 
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

                    dRow = dt.Rows[i];

                    //string contractNumber = dr["contractNumber"].ObjToString();

                    cmd = "Select * from `fcust_extended` where `contractNumber` = '" + contractNumber + "';";
                    DataTable ddt = G1.get_db_data(cmd);
                    if (ddt.Rows.Count <= 0)
                        continue;
                    record = ddt.Rows[0]["record"].ObjToString();
                    dRow = ddt.Rows[0];
                    //string number = dr["num"].ObjToString();
                    //string serviceId = dr["serviceId"].ObjToString();
                    //string srvloc = dr["SRVLOC"].ObjToString();
                    //string serviceLoc = dr["serviceLoc"].ObjToString();

                    rtDt = CalculateCustomerDetails(contractNumber, null, dRow, true);

                    for ( int j=0; j<rtDt.Rows.Count; j++)
                    {
                        type = rtDt.Rows[j]["type"].ObjToString().ToUpper();
                        what = rtDt.Rows[j]["what"].ObjToString();
                        amount = rtDt.Rows[j]["amount"].ObjToDouble();

                        if (type == "CASKET")
                        {
                            dt.Rows[i]["casketdesc"] = what;
                            dt.Rows[i]["casketAmount"] = amount;
                        }
                        if (type == "VAULT")
                        {
                            dt.Rows[i]["vaultDesc"] = what;
                            dt.Rows[i]["vaultAmount"] = amount;
                        }
                        if (type == "URN")
                        {
                            dt.Rows[i]["urnDesc"] = what;
                            dt.Rows[i]["urnAmount"] = amount;
                        }
                    }

                    trusts = 0D;
                    insurance = 0D;
                    checks = 0D;
                    cash = 0D;
                    refunds = 0D;
                    payments = 0D;
                    card = 0D;
                    totalCollected = 0D;

                    trustDepositDate = "";
                    insuranceDepositDate = "";
                    checkDepositDate = "";
                    cashDepositDate = "";
                    refundDepositDate = "";
                    paymentDepositDate = "";
                    cardDepositDate = "";
                    DateTime dateModified = DateTime.Now;


                    for (int j = 0; j < pDt.Rows.Count; j++)
                    {
                        status = pDt.Rows[j]["status"].ObjToString().ToUpper();
                        if (status != "DEPOSITED" && status != "ACCEPT")
                            continue;
                        type = pDt.Rows[j]["type"].ObjToString().ToUpper();
                        payment = pDt.Rows[j]["payment"].ObjToDouble();
                        description = pDt.Rows[j]["description"].ObjToString().ToUpper();
                        dateModified = pDt.Rows[j]["dateModified"].ObjToDateTime();
                        if (type == "TRUST")
                        {
                            trusts += payment;
                            if (!String.IsNullOrWhiteSpace(trustDepositDate))
                                trustDepositDate += "\n";
                            trustDepositDate += dateModified.ToString("MM/dd/yyyy");
                        }
                        else if (type == "CHECK")
                        {
                            checks += payment;
                            if (!String.IsNullOrWhiteSpace(checkDepositDate))
                                checkDepositDate += "\n";
                            checkDepositDate += dateModified.ToString("MM/dd/yyyy");
                        }
                        else if (type == "CASH")
                        {
                            cash += payment;
                            if (!String.IsNullOrWhiteSpace(cashDepositDate))
                                cashDepositDate += "\n";
                            cashDepositDate += dateModified.ToString("MM/dd/yyyy");
                        }
                        else if (type == "REFUND")
                        {
                            refunds += payment;
                            if (!String.IsNullOrWhiteSpace(refundDepositDate))
                                refundDepositDate += "\n";
                            refundDepositDate += dateModified.ToString("MM/dd/yyyy");
                        }
                        else if (type.IndexOf("INSURANCE") >= 0)
                        {
                            insurance += payment;
                            if (!String.IsNullOrWhiteSpace(insuranceDepositDate))
                                insuranceDepositDate += "\n";
                            insuranceDepositDate += dateModified.ToString("MM/dd/yyyy");
                        }
                        else if (type == "PAYMENTS")
                        {
                            if (description.IndexOf("CHECK") >= 0)
                            {
                                checks += payment;
                                if (!String.IsNullOrWhiteSpace(checkDepositDate))
                                    checkDepositDate += "\n";
                                checkDepositDate += dateModified.ToString("MM/dd/yyyy");
                            }
                            else if (description.IndexOf("CASH") >= 0)
                            {
                                cash += payment;
                                if (!String.IsNullOrWhiteSpace(cashDepositDate))
                                    cashDepositDate += "\n";
                                cashDepositDate += dateModified.ToString("MM/dd/yyyy");
                            }
                            else if (description.IndexOf("CARD") >= 0)
                            {
                                card += payment;
                                if (!String.IsNullOrWhiteSpace(cardDepositDate))
                                    cardDepositDate += "\n";
                                cardDepositDate += dateModified.ToString("MM/dd/yyyy");
                            }
                            else
                            {
                                payments += payment;
                                if (!String.IsNullOrWhiteSpace(paymentDepositDate))
                                    paymentDepositDate += "\n";
                                paymentDepositDate += dateModified.ToString("MM/dd/yyyy");
                            }
                        }
                    }

                    dt.Rows[i]["trusts"] = trusts;
                    dt.Rows[i]["trustDepositDate"] = trustDepositDate;
                    dt.Rows[i]["insurance"] = insurance;
                    dt.Rows[i]["insuranceDepositDate"] = insuranceDepositDate;
                    dt.Rows[i]["checks"] = checks;
                    dt.Rows[i]["checkDepositDate"] = checkDepositDate;
                    dt.Rows[i]["totalCash"] = cash;
                    dt.Rows[i]["cashDepositDate"] = cashDepositDate;
                    dt.Rows[i]["card"] = card;
                    dt.Rows[i]["cardDepositDate"] = cardDepositDate;
                    dt.Rows[i]["refunds"] = refunds;
                    dt.Rows[i]["refundDepositDate"] = refundDepositDate;
                    dt.Rows[i]["payments"] = payments;
                    dt.Rows[i]["paymentDepositDate"] = paymentDepositDate;

                    totalCollected = trusts + insurance + checks + cash + card + refunds + payments;
                    grossFuneral = dt.Rows[i]["custPrice"].ObjToDouble();
                    netFuneral = grossFuneral - totalDiscount;

                    dt.Rows[i]["netFuneral"] = netFuneral;
                    dt.Rows[i]["totalCollected"] = totalCollected;

                    count++;
                }
                catch (Exception ex)
                {
                }
            }

            barImport.Value = lastRow;
            barImport.Refresh();

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

                printingSystem1.Links.AddRange(new object[] {printableComponentLink1});


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
            reportName = this.Text;
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
            double columnWidth = 0D;
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                gridMain.Columns[i].AppearanceHeader.Font = font;
                columnWidth = (double) gridMain.Columns[i].Width;
                gridMain.Columns[i].Width = (int) (columnWidth * (scale/100D));
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
            DataTable dt = (DataTable)dgv.DataSource;

            if (G1.get_column_number(dt, "contractNumber") < 0)
                dt.Columns.Add("contractNumber");

            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                EditCust custForm = new EditCust(contract);
                custForm.Tag = contract;
                custForm.Show();
                this.Cursor = Cursors.Default;
            }
            else
            {
                string ID = dr["ID"].ObjToString();
                if ( !String.IsNullOrWhiteSpace (ID))
                {
                    string[] Lines = ID.Split('~');
                    if (Lines.Length <= 0)
                        return;
                    contract = Lines[0].Trim();
                    this.Cursor = Cursors.WaitCursor;
                    EditCust custForm = new EditCust(contract);
                    custForm.Tag = contract;
                    custForm.Show();
                    this.Cursor = Cursors.Default;
                }
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
                string[] Lines = phone.Split(',');
                if (Lines.Length == 2)
                    phoneType = Lines[1];
                string str = Lines[0].Trim();
                str = str.Replace("-", "");
                str = str.Replace("(", "");
                str = str.Replace(")", "");
                if (str.Length >= 9)
                {
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
        private void locateDuplicatePaymentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string cmd = "Select * from `cust_payments`;";
            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("ID");

            string contractNumber = "";
            string serviceId = "";
            string type = "";
            double payment = 0D;
            string sValue = "";
            string detail = "";

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                //serviceId = dt.Rows[i]["serviceId"].ObjToString();
                type = dt.Rows[i]["type"].ObjToString();
                payment = dt.Rows[i]["payment"].ObjToDouble();
                sValue = payment.ToString();
                detail = contractNumber + "~" + type + "~" + sValue;

                dt.Rows[i]["ID"] = detail;
            }

            var duplicateRows = dt.AsEnumerable()
                .GroupBy(row => new { ID = row.Field<string>("ID") })
                .Where(group => group.Count() > 1)
                .SelectMany(group => group); // Select all rows from duplicate groups

            DataTable dx = new DataTable();
            dx.Columns.Add("ID");
            dx.Columns.Add("tmstamp");
            dx.Columns.Add("record");

            DataRow dRow = null;
            string tmStamp = "";
            string record = "";

            try
            {
                foreach (DataRow row in duplicateRows)
                {
                    detail = row["ID"].ObjToString();
                    tmStamp = row["tmstamp"].ObjToString();
                    record = row["record"].ObjToString();
                    dRow = dx.NewRow();
                    dRow["ID"] = detail;
                    dRow["tmstamp"] = tmStamp;
                    dRow["record"] = record;
                    dx.Rows.Add(dRow);
                }
            }
            catch ( Exception ex)
            {
            }

            gridMain.Columns["ID"].Visible = true;
            gridMain.Columns["tmstamp"].Visible = true;

            G1.NumberDataTable(dx);

            dgv.DataSource = dx;
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void deletePaymentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();

            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);

            int[] rows = gridMain.GetSelectedRows();
            DataTable dt = (DataTable)dgv.DataSource;
            int lastRow = dt.Rows.Count;
            lastRow = rows.Length;

            string contractNumber = "";
            string record = "";

            this.Cursor = Cursors.WaitCursor;

            for (int i = 0; i < lastRow; i++)
            {
                row = rows[i];
                try
                {
                    row = gridMain.GetDataSourceRowIndex(row);

                    dr = dt.Rows[row];
                    record = dr["record"].ObjToString();

                    G1.delete_db_table("cust_payments", "record", record);
                }
                catch ( Exception ex)
                {
                    MessageBox.Show("***ERROR*** " + ex.Message.ToString(), "Delete Payment(s) Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        public DataTable CalculateCustomerDetails(string contractNumber, string custExtendedRecord, DataRow dR, bool Rebalance = false)
        {
            DateTime startTime = DateTime.Now; // ZAMMA

            //PleaseWait pleaseForm = null;
            //pleaseForm = new PleaseWait("Please Wait!\nUpdating Funeral Informtion");
            //pleaseForm.Show();
            //pleaseForm.Refresh();

            //this.Cursor = Cursors.WaitCursor;

            DataTable rtDt = new DataTable();
            rtDt.Columns.Add("type");
            rtDt.Columns.Add("what");
            rtDt.Columns.Add("amount", Type.GetType("System.Double"));

            string cmd = "Select * from `cust_payments` c LEFT JOIN `cust_payment_details` x ON c.`record` = x.`paymentRecord` where c.`contractNumber` = '" + contractNumber + "' order by c.`record` ;";
            //cmd = "Select * from `cust_payments` c where c.`contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);

            double oldDiscount = dR["totalDiscount"].ObjToDouble();

            string amountFiled = "";
            string amountReceived = "";
            string amountDiscount = "";
            string amountGrowth = "";
            string grossAmountReceived = "";
            double totalFiled = 0D;
            double totalReceived = 0D;
            double totalDiscount = 0D;
            double totalAmountGrowth = 0D;
            double totalAmountDiscount = 0D;
            double totalGross = 0D;
            double payment = 0D;
            double totalPayments = 0D;
            double actualPayments = 0D;
            double totalDBR = 0D;
            double dbr = 0D;
            double totalRefund = 0D;
            double refund = 0D;
            double money = 0D;
            double totalMoney = 0D;

            double trustAmountFiled = 0D;
            double trustFiledRemaining = 0D;
            double tempReceived = 0D;
            double insAmountFiled = 0D;
            double trustAmountReceived = 0D;
            double trustPaid = 0D;
            double trustDiscount = 0D;
            double insAmountReceived = 0D;
            double amtActuallyReceived = 0D;
            double insuraceDirectGrowth = 0D;

            DataTable exceptionDt = G1.get_db_data("Select * from `funeral_master`;");

            DateTime date = DateTime.Now;

            string str = "";
            string type = "";
            string cash = "";
            string status = "";
            string deposit = "";
            string creditCard = "";
            string check = "";
            string ccDepNumber = "";
            string chkDepNumber = "";
            string trustDepNumber = "";
            string insDepNumber = "";
            double dValue = 0D;
            double balanceDue = 0D;
            double discount = 0D;
            double classa = 0D;
            double insuranceDirectGrowth = 0D;
            string approvedBy = "";
            DateTime dateEntered = DateTime.Now;
            DateTime dateModified = DateTime.Now;
            string lastRecord = "";
            string record = "";
            string trustNumber = "";
            DataRow[] dRows = null;
            DataRow dRow = null;
            string pRecord = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    if (lastRecord != record)
                    {
                        status = dt.Rows[i]["status"].ObjToString().ToUpper();
                        if (status.ToUpper() == "CANCELLED" || status.ToUpper() == "REJECTED")
                            continue;
                        amountFiled = dt.Rows[i]["amountFiled"].ObjToString();
                        amountReceived = dt.Rows[i]["amountReceived"].ObjToString();
                        amountDiscount = dt.Rows[i]["amountDiscount"].ObjToString();
                        amountGrowth = dt.Rows[i]["amountGrowth"].ObjToString();
                        grossAmountReceived = dt.Rows[i]["grossAmountReceived"].ObjToString();
                        payment = dt.Rows[i]["payment"].ObjToDouble();
                        if (status.ToUpper() == "DEPOSITED" && payment > 0D && grossAmountReceived.ObjToDouble() == 0D)
                        {
                            grossAmountReceived = payment.ToString();
                            dt.Rows[i]["grossAmountReceived"] = grossAmountReceived;
                        }
                        totalFiled += amountFiled.ObjToDouble();
                        //totalReceived += amountReceived.ObjToDouble();
                        totalAmountDiscount += amountDiscount.ObjToDouble();
                        type = dt.Rows[i]["type"].ObjToString().ToUpper();
                        if (type != "TRUST")
                            totalAmountGrowth += amountGrowth.ObjToDouble();
                        totalGross += grossAmountReceived.ObjToDouble();

                        amtActuallyReceived += dt.Rows[i]["amtActuallyReceived"].ObjToDouble();

                        type = dt.Rows[i]["type"].ObjToString().ToUpper();
                        if (type.ToUpper() == "CHECK" || type.ToUpper() == "CASH" || type.ToUpper() == "CREDIT CARD" || type.ToUpper().IndexOf("ACH") > 0)
                        {
                            money = dt.Rows[i]["payment"].ObjToDouble();
                            totalMoney += money;
                        }
                        if (type.ToUpper() == "REFUND")
                        {
                            refund = dt.Rows[i]["payment"].ObjToDouble();
                            totalRefund += refund;
                            continue;
                        }
                        if (status.ToUpper() == "DEPOSITED" || status.ToUpper() == "ACCEPT")
                            actualPayments += payment;
                        if (type.ToUpper() == "DISCOUNT" || status.ToUpper() == "DEPOSITED")
                        {
                            totalPayments += payment;
                            if (status.ToUpper() == "DEPOSITED")
                                totalReceived += payment;
                        }
                        if (type == "INSURANCE DIRECT" && status == "DEPOSITED")
                            insuranceDirectGrowth += amountGrowth.ObjToDouble();

                        if (type.ToUpper() == "CHECK" && (status.ToUpper() == "ACCEPT" || status.ToUpper() == "DEPOSITED"))
                        {
                            totalPayments += payment;
                            totalReceived += payment;

                            str = G1.ReformatMoney(payment);
                            check += "CK - " + str + " ";
                        }
                        if (type == "CASH")
                        {
                            totalPayments += payment;
                            totalReceived += payment;

                            dValue = dt.Rows[i]["payment"].ObjToDouble();
                            str = G1.ReformatMoney(dValue);
                            cash += "CA - " + str + " ";
                        }
                        else if (type == "OTHER")
                        {
                            totalPayments += payment;
                            totalReceived += payment;

                            dValue = dt.Rows[i]["payment"].ObjToDouble();
                            str = G1.ReformatMoney(dValue);
                            cash += "CA - " + str + " ";
                        }
                        else if (type == "CREDIT CARD")
                        {
                            totalPayments += payment;
                            totalReceived += payment;

                            dValue = dt.Rows[i]["payment"].ObjToDouble();
                            str = G1.ReformatMoney(dValue);
                            creditCard += "CC - " + str + " ";
                        }
                        else if (type == "CLASS A")
                        {
                            classa += dt.Rows[i]["payment"].ObjToDouble();
                        }
                        str = dt.Rows[i]["depositNumber"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(str))
                        {
                            if (type == "CASH")
                                deposit += str + " ";
                            else if (type == "CREDIT CARD")
                                ccDepNumber += str + " ";
                            else if (type == "CHECK")
                                chkDepNumber += str + " ";
                        }
                        if (type == "DISCOUNT")
                        {
                            discount += dt.Rows[i]["payment"].ObjToDouble();
                            str = dt.Rows[i]["approvedBy"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(str))
                                approvedBy += str + " ";
                        }
                        if (type == "TRUST")
                        {
                            trustPaid += dt.Rows[i]["paid"].ObjToDouble();
                            trustDepNumber += str + " ";
                            amountFiled = dt.Rows[i]["trustAmtFiled"].ObjToString();
                            tempReceived = dt.Rows[i]["amtActuallyReceived"].ObjToDouble();
                            if (tempReceived == 0D)
                                trustFiledRemaining += amountFiled.ObjToDouble();
                            if (status == "PENDING")
                            {
                                if (amountFiled.ObjToDouble() > 0D)
                                    trustAmountFiled += amountFiled.ObjToDouble();
                                //else
                                //    trustAmountFiled += payment;
                            }
                            else if (status == "DEPOSITED")
                            {
                                if (amountFiled.ObjToDouble() > 0D)
                                    trustAmountFiled += amountFiled.ObjToDouble();
                                trustAmountReceived += amountReceived.ObjToDouble();
                            }
                            dbr = dt.Rows[i]["dbr"].ObjToDouble();
                            if (dbr == 0D || 1 == 1)
                            {
                                trustNumber = dt.Rows[i]["trust_policy"].ObjToString();
                                if (!String.IsNullOrWhiteSpace(trustNumber))
                                {
                                    dbr = DailyHistory.GetPossibleDBR(trustNumber);
                                    dbr = G1.RoundValue(dbr);
                                    if (dbr > 0D)
                                    {
                                        pRecord = dt.Rows[i]["paymentRecord"].ObjToString();
                                        if (!String.IsNullOrWhiteSpace(pRecord))
                                        {
                                            dRows = dt.Select("paymentRecord='" + pRecord + "' AND status1='CANCELLED'");
                                            if (dRows.Length > 0)
                                                dbr = 0D;
                                        }
                                    }
                                    totalDBR += dbr;
                                }
                            }
                            else
                                totalDBR += dbr;
                        }
                        else if (type.ToUpper().IndexOf("INSURANCE") >= 0)
                        {
                            insDepNumber += str + " ";
                            if (status == "PENDING")
                            {
                                if (amountFiled.ObjToDouble() > 0D)
                                    insAmountFiled += amountFiled.ObjToDouble();
                                else
                                    insAmountFiled += payment;
                                //else
                                //    insAmountFiled += payment;
                            }
                            else if (status == "DEPOSITED")
                            {
                                if (amountFiled.ObjToDouble() > 0D)
                                    insAmountFiled += amountFiled.ObjToDouble();
                                else
                                    insAmountFiled += payment;
                                insAmountReceived += amountReceived.ObjToDouble();
                            }
                        }
                    }
                    else
                    {
                        status = dt.Rows[i]["status"].ObjToString().ToUpper();
                        if (status.ToUpper() == "CANCELLED")
                            continue;
                        amountFiled = dt.Rows[i]["amountFiled"].ObjToString();
                        amountReceived = dt.Rows[i]["amountReceived"].ObjToString();
                        amountDiscount = dt.Rows[i]["amountDiscount"].ObjToString();
                        amountGrowth = dt.Rows[i]["amountGrowth"].ObjToString();
                        grossAmountReceived = dt.Rows[i]["grossAmountReceived"].ObjToString();
                        payment = dt.Rows[i]["payment"].ObjToDouble();
                        type = dt.Rows[i]["type"].ObjToString().ToUpper();
                        if (type == "TRUST")
                        {
                            amountFiled = dt.Rows[i]["TrustAmtFiled"].ObjToString();
                            trustAmountFiled += amountFiled.ObjToDouble();
                            if (amountReceived.ObjToDouble() == 0D)
                                trustDiscount += amountFiled.ObjToDouble();
                            tempReceived = dt.Rows[i]["amtActuallyReceived"].ObjToDouble();
                            if (tempReceived == 0D)
                                trustFiledRemaining += amountFiled.ObjToDouble();
                        }

                    }
                    lastRecord = record;
                    //date = dt.Rows[i]["dateFiled"].ObjToDateTime();
                    //if (date.Year > 100)
                    //    dateFiled += date.ToString("MM/dd/yyyy") + " ";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** 1 " + ex.Message.ToString(), "Rebalance Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }

            amtActuallyReceived = G1.RoundValue(amtActuallyReceived);
            trustDiscount = G1.RoundValue(trustDiscount);
            trustDiscount = trustAmountFiled - trustAmountReceived;
            trustDiscount = G1.RoundValue(trustDiscount);

            trustDiscount = trustPaid - trustAmountFiled;
            trustDiscount = G1.RoundValue(trustDiscount);

            string serviceId = dR["ServiceId"].ObjToString();
            EditCust.DetermineActiveGroups(contractNumber, serviceId);

            string myActiveFuneralHomeGroup = EditCust.activeFuneralHomeGroup;
            string myActiveFuneralHomeCasketGroup = EditCust.activeFuneralHomeCasketGroup;

            EditCustomer.activeFuneralHomeGroup = myActiveFuneralHomeGroup;
            EditCustomer.activeFuneralHomeCasketGroup = myActiveFuneralHomeCasketGroup;


            double newContractTotal = 0D;
            double newTotalCost = 0D;
            double newPreDiscount = 0D;
            Funerals.calculateTotalServices(contractNumber, ref newContractTotal, ref newTotalCost, ref newPreDiscount);


            FunServices funForm = new FunServices(contractNumber);
            DataTable funDt = funForm.funServicesDT;

            double price = 0D;
            double totalServices = 0D;
            double totalMerchandise = 0D;
            double totalCashAdvance = 0D;
            double totalCost = 0D;
            double difference = 0D;
            double totalDifference = 0D;
            double currentPrice = 0D;
            double totalCurrentPrice = 0D;
            double contractTotal = 0D;
            double preDiscount = 0D;
            double thirdDiscount = 0D;

            //FunServices.CalcTotalServices(funDt, ref contractTotal, ref totalCost, ref preDiscount);

            if (SMFS.activeSystem.ToUpper() == "OTHER")
            {
                preDiscount = Math.Abs(preDiscount);
                preDiscount = 0D;
            }

            double trustPayments = 0D;
            double trustPaymentsReceived = 0D;
            double trustGrowth = 0D;
            double insurancePayments = 0D;
            double insurancePaymentsReceived = 0D;
            double insuranceGrowth = 0D;
            double cashReceived = 0D;
            double compDiscounts = 0D;
            double classA = 0D;
            double otherPreDiscount = 0D;
            double insGrowth = 0D;
            trustFiledRemaining = 0D;
            string cc = "";

            double payments = Funerals.calculateTotalPayments(contractNumber, ref trustPayments, ref trustPaymentsReceived, ref insurancePayments, ref insurancePaymentsReceived, ref cashReceived, ref compDiscounts, ref classA, ref trustFiledRemaining, ref thirdDiscount, ref trustGrowth, ref insuranceGrowth, ref otherPreDiscount, ref insGrowth);
            if (SMFS.activeSystem.ToUpper() == "OTHER")
            {
                trustAmountReceived = trustPaymentsReceived;
                if (trustPayments > 0D)
                    trustPayments += otherPreDiscount;
                else
                    compDiscounts += otherPreDiscount;
                trustDiscount = otherPreDiscount;
                //compDiscounts = otherPreDiscount;
            }

            string service = "";
            string oldService = "";

            double currentServices = 0D;
            double currentMerchandise = 0D;

            double merchandiseDiscount = 0D;
            double servicesDiscount = 0D;

            double totalPackagePrice = 0D;
            double packagePrice = 0D;
            double packageDiscount = 0D;
            bool gotPackage = false;

            bool gotDesc = false;

            string[] Lines = null;
            DataTable mDt = null;
            string casket = "";
            string vault = "";
            string vaultDesc = "";
            string casketCode = "";
            string serialNumber = "";
            string casketDesc = "";
            double casketAmount = 0D;
            double casketCost = 0D;
            double vaultAmount = 0D;
            double vaultCost = 0D;
            double urnCost = 0D;
            string urnDesc = "";
            string casketType = "";
            string casketGauge = "";
            double urn = 0D;
            double headCapPanel = 0D;
            double misc = 0D;
            double upgrade = 0D;
            double salesTax = 0D;
            double taxMerchandise = 0D;
            double taxAmount = 0D;
            double asService = 0D;
            double asCash = 0D;
            double asNothing = 0D;
            double asMerc = 0D;
            double fromService = 0D;
            double fromMerc = 0D;
            string zeroData = "";
            bool getCost = false;
            string gotRental = "";
            string isCash = "";
            string ignore = "";
            bool gotIgnore = false;

            string fpc = "";
            DataTable bateDt = null;


            if (funDt != null)
            {
                dRows = funDt.Select("serialNumber<>''");
                if (dRows.Length == 0)
                {
                    dRows = funDt.Select("service LIKE '%Family Provided Casket%'");
                    if (dRows.Length > 0)
                        fpc = "Y";
                }
                for (int i = 0; i < funDt.Rows.Count; i++)
                {
                    getCost = false;
                    ignore = funDt.Rows[i]["ignore"].ObjToString().ToUpper();
                    if (ignore == "Y")
                    {
                        gotIgnore = true;
                        type = funDt.Rows[i]["type"].ObjToString().ToUpper();
                        price = funDt.Rows[i]["price"].ObjToDouble();
                        currentPrice = funDt.Rows[i]["currentPrice"].ObjToDouble();
                        difference = currentPrice - price;
                        if (type == "SERVICE")
                        {
                            servicesDiscount += difference;
                            servicesDiscount -= currentPrice;

                        }
                        else if (type == "MERCHANDISE")
                        {
                            merchandiseDiscount += difference;
                            merchandiseDiscount -= currentPrice;
                        }
                        else if (type == "CASH ADVANCE")
                        {
                        }
                        continue;
                    }

                    isCash = funDt.Rows[i]["asCash"].ObjToString().ToUpper();
                    zeroData = funDt.Rows[i]["data"].ObjToString();
                    upgrade = funDt.Rows[i]["upgrade"].ObjToDouble();
                    price = funDt.Rows[i]["price"].ObjToDouble();
                    if (price <= 0D && upgrade <= 0D)
                    {
                        service = funDt.Rows[i]["service"].ObjToString().ToUpper();
                        if (service != "PACKAGE DISCOUNT")
                        {
                            if (zeroData.ToUpper() != "ZERO")
                                continue;
                        }
                    }

                    //if (price == 0D) // Changed 3/13/2025 because of Funeral Bonus // 3/14/2025 Changed Back Because of BN25016
                    //    continue;
                    currentPrice = funDt.Rows[i]["currentPrice"].ObjToDouble();
                    difference = currentPrice - price;

                    type = funDt.Rows[i]["type"].ObjToString().ToUpper();
                    service = funDt.Rows[i]["service"].ObjToString().ToUpper();
                    if (service.IndexOf("D- ") == 0)
                        service = service.Replace("D- ", "");
                    else if (service.IndexOf("D-") == 0)
                        service = service.Replace("D-", "");

                    dRows = exceptionDt.Select("service='" + service + "'");

                    if (isCash == "Y")
                    {
                        if (dRows.Length > 0)
                            dRows[0]["asCash"] = 1;
                        else
                        {
                            dRow = exceptionDt.NewRow();
                            dRow["service"] = service;
                            dRow["asCash"] = 1;
                            exceptionDt.Rows.Add(dRow);
                            dRows = exceptionDt.Select("service='" + service + "'");
                        }
                    }

                    if (dRows.Length > 0 && type.ToUpper() != "CASH ADVANCE")
                    {
                        if (dRows[0]["asService"].ObjToString() == "1")
                            asService += funDt.Rows[i]["currentprice"].ObjToDouble();
                        if (dRows[0]["fromService"].ObjToString() == "1")
                        {
                            if (type == "SERVICE")
                                fromService += funDt.Rows[i]["currentprice"].ObjToDouble();
                        }
                        if (dRows[0]["fromMerc"].ObjToString() == "1")
                        {
                            if (type == "MERCHANDISE")
                            {
                                fromMerc += funDt.Rows[i]["currentprice"].ObjToDouble();
                                //casketCost += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                        }
                        if (dRows[0]["asCash"].ObjToString() == "1")
                        {
                            if (type.ToUpper() == "MERCHANDISE")
                            {
                                asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                                asService += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                            else
                                asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                        }
                        if (dRows[0]["asNothing"].ObjToString() == "1")
                            asNothing += funDt.Rows[i]["currentprice"].ObjToDouble();
                        if (dRows[0]["asMerc"].ObjToString() == "1")
                        {
                            if (type.ToUpper() != "MERCHANDISE")
                                asMerc += funDt.Rows[i]["price"].ObjToDouble();
                        }
                    }
                    else
                    {
                        if (service.ToUpper().IndexOf("MILEAGE") >= 0)
                        {
                            if (type.ToUpper() != "CASH ADVANCE")
                            {
                                if (type.ToUpper() == "MERCHANDISE")
                                    fromMerc += funDt.Rows[i]["currentprice"].ObjToDouble();
                                else
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                        }
                        else if (service.ToUpper().IndexOf("TRANSPORTATION") >= 0)
                        {
                            if (type.ToUpper() != "CASH ADVANCE")
                            {
                                if (type.ToUpper() == "MERCHANDISE")
                                    fromMerc += funDt.Rows[i]["currentprice"].ObjToDouble();
                                else
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                        }
                        else if (service.ToUpper().IndexOf("MILES") >= 0)
                        {
                            if (type.ToUpper() != "CASH ADVANCE")
                            {
                                if (type.ToUpper() == "MERCHANDISE")
                                    fromMerc += funDt.Rows[i]["currentprice"].ObjToDouble();
                                else
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                        }
                        else if (service.ToUpper().IndexOf("ENGRAV") >= 0)
                        {
                            if (type.ToUpper() != "CASH ADVANCE")
                            {
                                if (type.ToUpper() == "MERCHANDISE")
                                {
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                                    asService += funDt.Rows[i]["currentprice"].ObjToDouble();
                                }
                                else
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                        }
                        else if (service.ToUpper().IndexOf("MEDALLION") >= 0)
                        {
                            if (type.ToUpper() != "CASH ADVANCE")
                            {
                                if (type.ToUpper() == "MERCHANDISE")
                                {
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                                    asService += funDt.Rows[i]["currentprice"].ObjToDouble();
                                }
                                else
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                        }
                        else if (service.ToUpper().IndexOf("KEEPSAKE") >= 0)
                        {
                            if (type.ToUpper() != "CASH ADVANCE")
                            {
                                if (type.ToUpper() == "MERCHANDISE")
                                {
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                                    asService += funDt.Rows[i]["currentprice"].ObjToDouble();
                                }
                                else
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                        }
                        else if (service.ToUpper().IndexOf("LIFE PRINT") >= 0)
                        {
                            if (type.ToUpper() != "CASH ADVANCE")
                            {
                                getCost = true;
                                //casketAmount += funDt.Rows[i]["price"].ObjToDouble();
                                if (type.ToUpper() == "MERCHANDISE")
                                {
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                                    asService += funDt.Rows[i]["currentprice"].ObjToDouble();
                                }
                                else
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                        }
                        else if (service.ToUpper().IndexOf("LIFE STOR") >= 0)
                        {
                            if (type.ToUpper() != "CASH ADVANCE")
                            {
                                if (type.ToUpper() == "MERCHANDISE")
                                {
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                                    asService += funDt.Rows[i]["currentprice"].ObjToDouble();
                                }
                                else
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                        }
                        else if (service.ToUpper().IndexOf("BOOKMARK") >= 0)
                        {
                            if (type.ToUpper() != "CASH ADVANCE")
                            {
                                if (type.ToUpper() == "MERCHANDISE")
                                {
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                                    asService += funDt.Rows[i]["currentprice"].ObjToDouble();
                                }
                                else
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                        }
                        else if (service.ToUpper().IndexOf("SHIPPING") >= 0)
                        {
                            if (type.ToUpper() != "CASH ADVANCE")
                            {
                                casketAmount += funDt.Rows[i]["price"].ObjToDouble();
                                if (type.ToUpper() == "MERCHANDISE")
                                {
                                    fromMerc += funDt.Rows[i]["currentprice"].ObjToDouble();
                                    casketCost += funDt.Rows[i]["currentprice"].ObjToDouble();
                                }
                                else
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                        }
                        else if (type.ToUpper() == "SERVICE")
                        {
                            if (service.ToUpper().IndexOf("D-") == 0)
                            {
                                if (service.ToUpper().IndexOf("INFANT") < 0)
                                    asCash += funDt.Rows[i]["currentprice"].ObjToDouble();
                            }
                        }
                    }
                    if (service == "TOTAL LISTED PRICE")
                    {
                        totalPackagePrice = funDt.Rows[i]["price"].ObjToDouble();
                        continue;
                    }
                    else if (service == "PACKAGE PRICE")
                    {
                        gotPackage = true;
                        packagePrice = funDt.Rows[i]["price"].ObjToDouble();
                        continue;
                    }
                    else if (service == "PACKAGE DISCOUNT")
                    {
                        packageDiscount = funDt.Rows[i]["price"].ObjToDouble();
                        packageDiscount = Math.Abs(packageDiscount);
                        continue;
                    }

                    type = funDt.Rows[i]["type"].ObjToString().ToUpper();
                    if (price < 0D)
                    {
                        price = Math.Abs(price);
                        if (type.ToUpper() == "SERVICE")
                            servicesDiscount += funDt.Rows[i]["difference"].ObjToDouble();
                        else if (type.ToUpper() == "MERCHANDISE")
                            merchandiseDiscount += funDt.Rows[i]["difference"].ObjToDouble();
                        continue;
                    }

                    taxAmount = funDt.Rows[i]["taxAmount"].ObjToDouble();
                    if (taxAmount > 0D)
                    {
                        salesTax += taxAmount;
                        taxMerchandise += price;
                    }

                    if (type == "SERVICE")
                    {
                        totalServices += price;
                        servicesDiscount += difference;
                        currentServices += currentPrice;
                        if (service.IndexOf("RENTAL CASKET") >= 0)
                        {
                            casketDesc = service;
                            casketCode = "Rental";
                            gotRental = "Y";
                        }
                        else if (service.ToUpper().IndexOf("INFANT") >= 0)
                        {
                            casketDesc = service;
                            casketCost = price;
                            casketCode = "Infant";
                        }
                    }
                    else if (type == "MERCHANDISE")
                    {
                        if (service.IndexOf("RENTAL CASKET") >= 0)
                        {
                            casketDesc = service;
                            casketCode = "Rental";
                            gotRental = "Y";
                        }
                        if (service.ToUpper().IndexOf("INFANT") >= 0)
                        {
                            casketDesc = service;
                            casketCost = price;
                            casketCode = "Infant";
                        }
                        totalMerchandise += price;
                        merchandiseDiscount += difference;
                        currentMerchandise += currentPrice;
                        if (service.IndexOf("D-") == 0)
                        {
                            service = service.Substring(2).Trim();
                            if (service.ToUpper().IndexOf("BATESVILLE") >= 0)
                                service = service.ToUpper().Replace("BATESVILLE", "").Trim();
                        }
                        oldService = service;
                        if (service.IndexOf("ACKNOW") < 0 && service.IndexOf("GRAVE MARKER") < 0 && service.ToUpper().IndexOf("REGISTER BOOK") < 0)
                        {
                            if (service.IndexOf("V") == 0)
                            {
                                Lines = service.Split(' ');
                                service = service.Replace(Lines[0].Trim(), "").Trim();
                                if (String.IsNullOrWhiteSpace(service))
                                    service = oldService;
                            }

                            cmd = "SELECT * FROM `casket_master` WHERE `casketDesc` = '" + service + "';";
                            mDt = G1.get_db_data(cmd);
                            if (mDt.Rows.Count <= 0)
                            {
                                Lines = service.Split(' ');
                                str = Lines[0].Trim();
                                cmd = "SELECT * FROM `casket_master` WHERE `casketcode` = '" + str + "';";
                                mDt = G1.get_db_data(cmd);
                                if (mDt.Rows.Count <= 0)
                                {
                                    service = service.Replace(Lines[0].Trim(), "").Trim();
                                    cmd = "SELECT * FROM `casket_master` WHERE `casketDesc` = '" + service + "';";
                                    mDt = G1.get_db_data(cmd);
                                    if (mDt.Rows.Count <= 0)
                                    {
                                        cmd = "SELECT * FROM `casket_master` WHERE `casketDesc` LIKE '" + str + "%';";
                                        mDt = G1.get_db_data(cmd);
                                    }
                                }
                            }
                            if (mDt.Rows.Count > 0)
                            {
                                casketCode = mDt.Rows[0]["casketcode"].ObjToString();
                                if (casketCode.ToUpper().IndexOf("V") == 0)
                                {
                                    vault = casketCode;
                                    vaultAmount = funDt.Rows[i]["currentprice"].ObjToDouble();
                                    if (vaultAmount <= 0D)
                                        vaultAmount = funDt.Rows[i]["price"].ObjToDouble();
                                    vaultCost = mDt.Rows[0]["casketcost"].ObjToDouble();
                                    vaultDesc = mDt.Rows[0]["casketdesc"].ObjToString();
                                }
                                else if (casketCode.ToUpper().IndexOf("URN") == 0)
                                {
                                    dValue = funDt.Rows[i]["currentprice"].ObjToDouble();
                                    if (dValue <= 0D)
                                        dValue = funDt.Rows[i]["price"].ObjToDouble();
                                    urn += dValue;
                                    if (mDt.Rows.Count > 0)
                                    {
                                        urnCost = mDt.Rows[0]["casketcost"].ObjToDouble();
                                        urnDesc = mDt.Rows[0]["casketdesc"].ObjToString();
                                    }
                                }
                                else if (casketCode.ToUpper().IndexOf("UV") == 0)
                                {
                                    dValue = funDt.Rows[i]["price"].ObjToDouble();
                                    urn += dValue;
                                }
                                else if (casketCode.ToUpper() != "MISC" || getCost)
                                {
                                    casket = casketCode;
                                    dValue = funDt.Rows[i]["currentprice"].ObjToDouble();
                                    if (dValue <= 0D)
                                        dValue = funDt.Rows[i]["price"].ObjToDouble();
                                    casketAmount += dValue;
                                    serialNumber = funDt.Rows[i]["SerialNumber"].ObjToString();
                                    //if (!String.IsNullOrWhiteSpace(serialNumber))
                                    //{
                                    casketCost += mDt.Rows[0]["casketcost"].ObjToDouble();
                                    casketDesc = mDt.Rows[0]["casketdesc"].ObjToString();
                                    casketGauge = Funerals.getCasketGauge(serialNumber, casketCode, casketDesc, ref casketType);
                                    if (casketCode.ToUpper() == "INFANT" && vaultCost > 0D)
                                        vaultCost = 0D;

                                    //}
                                }
                            }
                            else
                            {
                                str = funDt.Rows[i]["serialNumber"].ObjToString();
                                if (!String.IsNullOrWhiteSpace(str))
                                {
                                    serialNumber = str;
                                    dValue = funDt.Rows[i]["currentprice"].ObjToDouble();
                                    if (dValue <= 0D)
                                        dValue = funDt.Rows[i]["price"].ObjToDouble();
                                    casketAmount += dValue;
                                    casketDesc = funDt.Rows[i]["service"].ObjToString();

                                    service = funDt.Rows[i]["service"].ObjToString().ToUpper();
                                    if (service.IndexOf("D-") == 0)
                                    {
                                        service = service.Replace("D-", "").Trim();
                                        if (service.ToUpper().IndexOf("BATESVILLE") >= 0)
                                            service = service.ToUpper().Replace("BATESVILLE", "").Trim();
                                    }
                                    bateDt = G1.get_db_data("Select * from `batesville_inventory` where `casketDescription` = '" + service + "';");
                                    if (bateDt.Rows.Count <= 0)
                                    {
                                        Lines = service.Split(' ');
                                        if (Lines.Length > 0)
                                        {
                                            cc = Lines[0].Trim();
                                            bateDt = G1.get_db_data("Select * from `batesville_inventory` where `casketCode` = '" + cc + "';");
                                        }
                                    }
                                    if (bateDt.Rows.Count > 0)
                                    {
                                        casketCode = bateDt.Rows[0]["casketCode"].ObjToString().ToUpper();
                                        if (casketCode.IndexOf("V") == 0)
                                            vaultCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                        else
                                            casketCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                    }
                                    else
                                    {
                                        bateDt = G1.get_db_data("Select * from `secondary_inventory` where `casketDesc` = '" + service + "';");
                                        if (bateDt.Rows.Count > 0)
                                        {
                                            str = bateDt.Rows[0]["type"].ObjToString().ToUpper();
                                            if (str == "CASKET")
                                            {
                                                casketCode = bateDt.Rows[0]["casketCode"].ObjToString();
                                                casketDesc = service;
                                                casketCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                                casketGauge = bateDt.Rows[0]["casketgauge"].ObjToString();
                                            }
                                            else if (str == "VAULT")
                                            {
                                                casketCode = bateDt.Rows[0]["casketCode"].ObjToString();
                                                casketDesc = service;
                                                vaultCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                                casketGauge = bateDt.Rows[0]["casketgauge"].ObjToString();
                                                vaultAmount = currentPrice;
                                            }
                                            else if (str == "URN")
                                            {
                                                casketCode = bateDt.Rows[0]["casketCode"].ObjToString();
                                                casketDesc = service;
                                                urnCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    service = funDt.Rows[i]["service"].ObjToString().ToUpper();
                                    if (service.IndexOf("D-") == 0)
                                    {
                                        service = service.Replace("D-", "").Trim();
                                        if (service.ToUpper().IndexOf("BATESVILLE") >= 0)
                                            service = service.ToUpper().Replace("BATESVILLE", "").Trim();
                                    }
                                    bateDt = G1.get_db_data("Select * from `batesville_inventory` where `casketDescription` = '" + service + "';");
                                    if (bateDt.Rows.Count > 0)
                                    {
                                        casketCode = bateDt.Rows[0]["casketCode"].ObjToString().ToUpper();
                                        if (casketCode.IndexOf("V") == 0)
                                            vaultCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                        else
                                            casketCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                    }
                                    if (bateDt.Rows.Count <= 0)
                                    {
                                        Lines = service.Split(' ');
                                        if (Lines.Length > 0)
                                        {
                                            str = Lines[0].Trim();
                                            bateDt = G1.get_db_data("Select * from `batesville_inventory` where `casketCode` = '" + str + "';");
                                            if (bateDt.Rows.Count > 0)
                                            {
                                                if (casketCode.IndexOf("V") == 0)
                                                {
                                                    vault = service;
                                                    vaultCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                                    vaultAmount = currentPrice;
                                                }
                                                else
                                                {
                                                    casketCode = bateDt.Rows[0]["casketCode"].ObjToString().ToUpper();
                                                    casketCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                                    casketAmount = currentPrice;
                                                    casketDesc = bateDt.Rows[0]["casketDescription"].ObjToString();
                                                }
                                            }
                                        }
                                    }
                                    if (bateDt.Rows.Count <= 0)
                                    {
                                        bateDt = G1.get_db_data("Select * from `secondary_inventory` where `casketDesc` = '" + service + "';");
                                        if (bateDt.Rows.Count > 0)
                                        {
                                            str = bateDt.Rows[0]["type"].ObjToString().ToUpper();
                                            if (str == "CASKET")
                                            {
                                                casketCode = bateDt.Rows[0]["casketCode"].ObjToString();
                                                casketDesc = service;
                                                casket = service;
                                                casketCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                                casketGauge = bateDt.Rows[0]["casketgauge"].ObjToString();
                                            }
                                            else if (str == "VAULT")
                                            {
                                                vault = service;
                                                vaultDesc = service;
                                                vaultCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                                vaultAmount = currentPrice;
                                            }
                                            else if (str == "URN")
                                            {
                                                urnDesc = service;
                                                urnCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                            }
                                        }
                                        else if (service.ToUpper().IndexOf("URN") >= 0)
                                        {
                                            Lines = service.Split(' ');
                                            str = "";
                                            bool foundIt = false;
                                            for (int kk = 0; kk < Lines.Length; kk++)
                                            {
                                                if (String.IsNullOrWhiteSpace(Lines[kk].ObjToString()))
                                                    continue;
                                                if (str.Length > 0)
                                                    str += " ";
                                                str += Lines[kk].ObjToString().Trim();
                                                cmd = "Select * from `batesville_inventory` where `casketDescription` LIKE '" + str + "%';";
                                                bateDt = G1.get_db_data(cmd);
                                                if (bateDt.Rows.Count >= 1 && kk >= 1)
                                                {
                                                    urnDesc = service;
                                                    urnCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                                    foundIt = true;
                                                    break;
                                                }
                                            }
                                            if (!foundIt)
                                            {
                                                str = "";
                                                for (int kk = 0; kk < Lines.Length; kk++)
                                                {
                                                    if (String.IsNullOrWhiteSpace(Lines[kk].ObjToString()))
                                                        continue;
                                                    if (str.Length > 0)
                                                        str += " ";
                                                    str += Lines[kk].ObjToString().Trim();
                                                    cmd = "Select * from `secondary_inventory` where `casketDesc` LIKE '" + str + "%';";
                                                    bateDt = G1.get_db_data(cmd);
                                                    if (bateDt.Rows.Count >= 1 && kk >= 1)
                                                    {
                                                        urnDesc = service;
                                                        urnCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                                        foundIt = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Lines = service.Split(' ');
                                            str = "";
                                            for (int kk = 0; kk < Lines.Length; kk++)
                                            {
                                                if (String.IsNullOrWhiteSpace(Lines[kk].ObjToString()))
                                                    continue;
                                                if (str.Length > 0)
                                                    str += " ";
                                                str += Lines[kk].ObjToString().Trim();
                                                cmd = "Select * from `secondary_inventory` where `casketDesc` LIKE '" + str + "%';";
                                                bateDt = G1.get_db_data(cmd);
                                                if (bateDt.Rows.Count >= 1 && kk >= 1)
                                                {
                                                    //urnDesc = service;
                                                    //urnCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                                    str = bateDt.Rows[0]["type"].ObjToString().ToUpper();
                                                    if (str == "CASKET")
                                                    {
                                                        casketCode = bateDt.Rows[0]["casketCode"].ObjToString();
                                                        casketDesc = service;
                                                        casketCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                                        casketGauge = bateDt.Rows[0]["casketgauge"].ObjToString();
                                                    }
                                                    else if (str == "VAULT")
                                                    {
                                                        casketCode = bateDt.Rows[0]["casketCode"].ObjToString();
                                                        casketDesc = service;
                                                        vaultCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                                        casketGauge = bateDt.Rows[0]["casketgauge"].ObjToString();
                                                        vaultAmount = currentPrice;
                                                    }
                                                    else if (str == "URN")
                                                    {
                                                        casketCode = bateDt.Rows[0]["casketCode"].ObjToString();
                                                        casketDesc = service;
                                                        urnCost = bateDt.Rows[0]["cost"].ObjToDouble();
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (type == "CASH ADVANCE")
                        totalCashAdvance += currentPrice; // Changed 3/13/2025 for Funeral Bonus
                    totalCurrentPrice += currentPrice;
                    totalDifference += (currentPrice - price);
                }
            }

            if (String.IsNullOrWhiteSpace(casketDesc) && funDt.Rows.Count > 0)
            {
                DataView tempview = funDt.DefaultView; // Check for Discrestionary
                tempview.Sort = "price desc";
                funDt = tempview.ToTable();

                for (int i = 0; i < funDt.Rows.Count; i++)
                {
                    type = funDt.Rows[i]["type"].ObjToString().ToUpper();
                    if (type == "MERCHANDISE")
                    {
                        service = funDt.Rows[i]["service"].ObjToString().ToUpper();
                        if (service.IndexOf("D-") == 0)
                        {
                            dValue = funDt.Rows[i]["currentprice"].ObjToDouble();
                            if (dValue <= 0D)
                                dValue = funDt.Rows[i]["price"].ObjToDouble();
                            casketAmount += dValue;
                            serialNumber = funDt.Rows[i]["SerialNumber"].ObjToString();
                            casketDesc = service;
                            break;
                        }
                    }
                }
            }

            totalCost = totalCurrentPrice - totalDifference;
            if (gotPackage)
                totalCost = packagePrice;
            totalCurrentPrice = newContractTotal;
            totalCost = newTotalCost;

            if (trustPaymentsReceived > trustPayments)
            {
                //compD = trustPayments - trustPaymentsReceived;
                //compD = G1.RoundValue(compD);

                //dt.Rows[i]["classa"] = dt.Rows[i]["classa"].ObjToDouble() + compD;
            }
            else
            {
                trustDiscount = trustPayments - trustPaymentsReceived - dbr; // Not used
                trustDiscount = trustAmountFiled - trustPaymentsReceived - dbr; // Not used
                trustDiscount = trustPayments - trustAmountFiled; // Not used, had to add trustFiledRemaining to get proper trust discount
                trustDiscount = trustPayments - trustPaymentsReceived - trustFiledRemaining - dbr;
                trustDiscount = trustPayments - trustPaymentsReceived - dbr;
                dValue = trustDiscount;
                dValue = Math.Abs(dValue);
                //dt.Rows[i]["trustDiscount"] = dValue;
                trustDiscount = trustPayments - trustPaymentsReceived - dbr;
            }
            //trustDiscount = trustPayments - trustPaymentsReceived - dbr;

            double insuranceDiscount = insurancePayments - insurancePaymentsReceived;
            insuranceDiscount += insuranceGrowth;

            //balanceDue = custPrice - classa - trustPaymentsReceived - cashReceived - insurancePaymentsReceived - trustDiscount - insuranceDiscount;

            classa = classA;
            balanceDue = newTotalCost - classa - trustPaymentsReceived - cashReceived - insurancePaymentsReceived - trustDiscount - insuranceDiscount - compDiscounts - totalDBR - thirdDiscount + insuranceGrowth;

            balanceDue = newTotalCost - compDiscounts - classA - trustDiscount + trustGrowth - trustPaymentsReceived - dbr - insuranceDiscount + insuranceGrowth - insurancePaymentsReceived - cashReceived;

            double TotalsbalanceDue = newTotalCost - compDiscounts - classA + trustGrowth - trustPaymentsReceived - dbr - insuranceDiscount + insuranceGrowth - insurancePaymentsReceived - cashReceived;
            if (SMFS.activeSystem.ToUpper() == "OTHER")
                TotalsbalanceDue -= trustDiscount;
            balanceDue = TotalsbalanceDue;

            totalRefund = Math.Abs(totalRefund);
            balanceDue += totalRefund;
            balanceDue = G1.RoundValue(balanceDue);

            if (balanceDue + totalAmountGrowth == 0D)
            {
                cmd = "Select * from `cust_payments` WHERE `contractNumber` = '" + contractNumber + "' AND `type` = 'Insurance Direct';";
                DataTable payDt = G1.get_db_data(cmd);
                if (payDt.Rows.Count > 0)
                    balanceDue = 0D;
            }


            //balanceDue = totalCost - totalPayments;
            totalDiscount = servicesDiscount + merchandiseDiscount;
            if (totalDifference > totalDiscount)
            {
                if (!gotIgnore)
                    totalDiscount = totalDifference;
                //preDiscount = totalDiscount;
            }
            if (gotPackage)
            {
                if (preDiscount < packageDiscount)
                    packageDiscount = preDiscount;
                totalDiscount = packageDiscount;
                preDiscount = 0D;
            }
            if (oldDiscount != totalDiscount)
            {
            }

            string isPackage = "";
            if (gotPackage)
                isPackage = "Y";

            rtDt = AddNewItem(rtDt, "casket", casketDesc, casketAmount );
            rtDt = AddNewItem(rtDt, "vault", vaultDesc, vaultAmount);
            rtDt = AddNewItem(rtDt, "urn", urnDesc, urn);

            //if (!String.IsNullOrWhiteSpace(custExtendedRecord))
            //{
            //    //if ( !Rebalance )
            //    G1.update_db_table("fcust_extended", "record", custExtendedRecord, new string[] { "casketCost", casketCost.ToString(), "vaultCost", vaultCost.ToString(), "insGrowth", insGrowth.ToString(), "gotRental", gotRental, "totalPayments", actualPayments.ToString() });
            //    G1.update_db_table("fcust_extended", "record", custExtendedRecord, new string[] { "amountFiled", totalFiled.ToString(), "amountReceived", totalReceived.ToString(), "cash", cash, "check", check, "depositNumber", deposit, "balanceDue", balanceDue.ToString(), "additionalDiscount", discount.ToString(), "approvedBy", approvedBy, "creditCard", creditCard, "ccDepNumber", ccDepNumber, "checkDepNumber", chkDepNumber, "grossAmountReceived", totalGross.ObjToString(), "classa", classa.ToString(), "amountDiscount", totalAmountDiscount.ObjToString(), "amountGrowth", totalAmountGrowth.ObjToString(), "gotPackage", isPackage, "casket", casket, "vault", vault, "casketAmount", casketAmount.ToString(), "vaultAmount", vaultAmount.ToString(), "urnDesc", urnDesc, "urnCost", urnCost.ToString() });
            //    G1.update_db_table("fcust_extended", "record", custExtendedRecord, new string[] { "custPrice", totalCost.ToString(), "custMerchandise", totalMerchandise.ToString(), "custServices", totalServices.ToString(), "merchandiseDiscount", merchandiseDiscount.ToString(), "servicesDiscount", servicesDiscount.ToString(), "totalDiscount", totalDiscount.ToString(), "currentPrice", totalCurrentPrice.ToString(), "currentMerchandise", currentMerchandise.ToString(), "currentServices", currentServices.ToString(), "serialNumber", serialNumber, "casketdesc", casketDesc, "preneedDiscount", preDiscount.ToString(), "packageDiscount", packageDiscount.ToString(), "cashAdvance", totalCashAdvance.ToString() });
            //    G1.update_db_table("fcust_extended", "record", custExtendedRecord, new string[] { "trustAmountFiled", trustAmountFiled.ObjToString(), "trustAmountReceived", trustAmountReceived.ObjToString(), "insAmountFiled", insAmountFiled.ObjToString(), "insAmountReceived", insAmountReceived.ObjToString(), "casketgauge", casketGauge, "caskettype", casketType, "urn", urn.ToString(), "trustDepNumber", trustDepNumber, "insDepNumber", insDepNumber, "refund", totalRefund.ToString(), "FPC", fpc, "thirdDiscount", thirdDiscount.ToString(), "trustGrowth", trustGrowth.ToString(), "insuranceGrowth", insuranceGrowth.ToString(), "money", totalMoney.ToString() });
            //    G1.update_db_table("fcust_extended", "record", custExtendedRecord, new string[] { "compDiscount", compDiscounts.ToString(), "cashReceived", cashReceived.ToString(), "trustPayments", trustPayments.ToString(), "trustPaymentsReceived", trustPaymentsReceived.ToString(), "insurancePayments", insurancePayments.ToString(), "insurancePaymentsReceived", insurancePaymentsReceived.ToString(), "taxAmount", salesTax.ToString(), "taxMerchandise", taxMerchandise.ToString(), "dbr", totalDBR.ToString(), "trustFiledRemaining", trustFiledRemaining.ToString(), "asService", asService.ToString(), "asCash", asCash.ToString(), "asNothing", asNothing.ToString(), "asMerc", asMerc.ToString(), "fromService", fromService.ToString(), "fromMerc", fromMerc.ToString() });

            //    //cmd = "UPDATE `fcust_extended` SET `tmstamp` = CURRENT_TIMESTAMP() WHERE `record` = '" + custExtendedRecord + "'; ";
            //    //G1.get_db_data(cmd);

            //    //UpdateTimeStamp("fcust_extended", "tmstamp", custExtendedRecord);
            //}
            try // Ramma Zamma
            {
                //dR["balanceDue"] = balanceDue;
                //dR["custPrice"] = totalCost;
                //dR["custMerchandise"] = totalMerchandise;
                //dR["custServices"] = totalServices;
                //dR["merchandiseDiscount"] = merchandiseDiscount;
                //dR["servicesDiscount"] = servicesDiscount;
                //dR["additionalDiscount"] = discount;
                //dR["totalDiscount"] = totalDiscount;

                //dR["amountFiled"] = totalFiled;
                //dR["amountReceived"] = totalReceived;

                //dR["currentPrice"] = totalCurrentPrice;
                //dR["currentMerchandise"] = currentMerchandise;
                //dR["currentServices"] = currentServices;

                //dR["grossAmountReceived"] = totalGross;

                //dR["amountDiscount"] = totalAmountDiscount;
                //dR["amountGrowth"] = totalAmountGrowth;
                //dR["serialNumber"] = serialNumber;
                //dR["casketdesc"] = casketDesc;

                //dR["check"] = check;
                //dR["checkDepNumber"] = chkDepNumber;


                //DataTable newDt = ReplaceData(serviceId);

                //if (newDt != null)
                //{
                //    string field = "";
                //    for (int i = 0; i < newDt.Columns.Count; i++)
                //    {
                //        try
                //        {
                //            field = newDt.Columns[i].ColumnName.Trim();
                //            dR[field] = newDt.Rows[0][i];
                //        }
                //        catch (Exception ex)
                //        {
                //        }
                //    }
                //    dValue = insurancePaymentsReceived;
                //    dValue = Math.Abs(dValue);
                //    if (G1.is_valid_column(dR, "insuranceAmountReceived"))
                //        dR["insuranceAmountReceived"] = dValue;

                //    dValue = insurancePaymentsReceived;
                //    dValue = Math.Abs(dValue);
                //    if (G1.is_valid_column(dR, "insurancePayments"))
                //        dR["insurancePayments"] = dValue;
                //    if (SMFS.activeSystem.ToUpper() == "OTHER")
                //    {
                //        if (preDiscount == 0D && packageDiscount > 0D)
                //            dR["preneedDiscount"] = packageDiscount;
                //    }
                //    else
                //    {
                //        if (preDiscount > 0D)
                //        {
                //        }
                //        if (preDiscount == 0D && packageDiscount > 0D)
                //            dR["preneedDiscount"] = packageDiscount;
                //    }
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** 2 " + ex.Message.ToString(), "Rebalance Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }

            //this.Cursor = Cursors.Default;
            //pleaseForm.FireEvent1();
            //pleaseForm.Dispose();
            //pleaseForm = null;

            //DateTime stopTime = DateTime.Now;
            //TimeSpan ts = stopTime - startTime;

            return rtDt;
        }
        /****************************************************************************************/
        public DataTable AddNewItem ( DataTable dt, string type, string what, double amount )
        {
            DataRow dRow = dt.NewRow();
            dRow["type"] = type;
            dRow["what"] = what;
            dRow["amount"] = amount;
            dt.Rows.Add(dRow);
            return dt;
        }
        /****************************************************************************************/
    }
}