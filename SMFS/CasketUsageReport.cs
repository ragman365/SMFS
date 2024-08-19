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
    public partial class CasketUsageReport : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private DataTable originalDt = null;
        private DataTable bankDt = null;
        private bool doVaults = false;
        private bool doUrns = false;
        /****************************************************************************************/
        public CasketUsageReport( bool justVaults = false, bool justUrns = false )
        {
            InitializeComponent();
            SetupTotalsSummary();
            doVaults = justVaults;
            doUrns = justUrns;
        }
        /****************************************************************************************/
        private void CasketUsageReport_Load(object sender, EventArgs e)
        {
            if (!doUrns)
                chkPreNeed.Hide();

            if (!LoginForm.isRobby)
                txtChart.Hide();

            loading = false;
            lblTotal.Hide();
            barImport.Hide();

            if (!G1.isAdmin())
                gridMain.Columns["netAmount"].Visible = false;

            btnImportBatesville.Hide();

            if (doVaults)
            {
                this.Text = "Vault Usage";
                gridMain.Columns["serialNumber"].Visible = false;
                gridMain.Columns["netAmount"].Caption = "Vault Cost";
                gridMain.Columns["casket"].Caption = "Vault";
            }
            else if (doUrns)
            {
                this.Text = "Urn Usage";
                gridMain.Columns["serialNumber"].Visible = false;
                gridMain.Columns["netAmount"].Caption = "Urn Cost";
                gridMain.Columns["casket"].Caption = "Urn";
            }

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
            AddSummaryColumn("netAmount", null);
            AddSummaryColumn("count", null, "{0:0}");
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
            G1.ShowHideFindPanel(grid);

            //if (grid.OptionsFind.AlwaysVisible == true)
            //    grid.OptionsFind.AlwaysVisible = false;
            //else
            //    grid.OptionsFind.AlwaysVisible = true;
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
        private void btnRun_Click(object sender, EventArgs e)
        {
            LoadData();
        }
        /****************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;

            DataTable funDt = G1.get_db_data("Select * from `funeralhomes`;");

            DataRow[] dRows = null;

            DateTime date = dateTimePicker1.Value;
            DateTime date1 = this.dateTimePicker1.Value;
            DateTime date2 = this.dateTimePicker2.Value;
            string startDate = date1.ToString("yyyy-MM-dd");
            string stopDate = date2.ToString("yyyy-MM-dd");
            string sdate1 = date1.ToString("yyyy-MM-dd") + " 00:00:00";
            string sdate2 = date2.ToString("yyyy-MM-dd") + " 23:59:59";
            //string cmd = "Select * from `fcustomers` p LEFT JOIN `inventory` c ON p.`serviceId` = c.`ServiceId` where p.`deceasedDate` >= '" + sdate1 + "' AND p.`deceasedDate` <= '" + sdate2 + "' ";


            DataTable dt = null;
            int year = date1.Year;
            int year2 = date2.Year;

            string yy = (year % 100).ToString("D2");

            string cmd = "";
            if ( chkPreNeed.Checked )
            {
                cmd = "Select * from `customers` p JOIN `contracts` c ON p.`contractNumber` = c.`contractNumber` ";
                cmd += " WHERE c.`issueDate8` >= '" + sdate1 + "' and c.`issueDate8` <= '" + sdate2 + "' AND p.`deceasedDate` < '1000-01-01' ";
                cmd += ";";
                dt = G1.get_db_data(cmd);
            }
            else if (year == year2)
            {
                cmd = "Select * from `fcustomers` p JOIN `fcust_extended` f ON p.`contractNumber` = f.`contractNumber` where p.`deceasedDate` <= '" + sdate2 + "' ";
                cmd += " AND p.`serviceId` LIKE '__" + yy + "%' ";
                cmd += " ORDER BY p.`deceasedDate`;";

                dt = G1.get_db_data(cmd);
                int count = dt.Rows.Count;

                dt = SalesTaxReport.ProcessTheData(dt, date1, date2);
            }
            else
            {
                DateTime testDate = new DateTime(date1.Year, 12, 31);
                sdate2 = testDate.ToString("yyyy-MM-dd") + " 23:59:59";
                cmd = "Select * from `fcustomers` p JOIN `fcust_extended` f ON p.`contractNumber` = f.`contractNumber` where p.`deceasedDate` >= '" + sdate1 + "' ";
                cmd += " AND p.`deceasedDate` <= '" + sdate2 + "' ";
                cmd += " AND p.`serviceId` LIKE '__" + yy + "%' ";
                cmd += " ORDER BY p.`deceasedDate`;";

                dt = G1.get_db_data(cmd);

                dt = SalesTaxReport.ProcessTheData(dt, date1, testDate);

                string yy2 = (year2 % 100).ToString("D2");

                testDate = new DateTime(date2.Year, 1, 1, 0, 0, 0);
                sdate1 = testDate.ToString("yyyy-MM-dd");
                testDate = new DateTime(date2.Year, date2.Month, date2.Day );
                sdate2 = testDate.ToString("yyyy-MM-dd") + " 23:59:59";
                cmd = "Select * from `fcustomers` p JOIN `fcust_extended` f ON p.`contractNumber` = f.`contractNumber` where p.`deceasedDate` >= '" + sdate1 + "' ";
                cmd += " AND p.`deceasedDate` <= '" + sdate2 + "' ";
                cmd += " AND p.`serviceId` LIKE '__" + yy2 + "%' ";
                cmd += " ORDER BY p.`deceasedDate`;";

                DataTable ddt = G1.get_db_data(cmd);

                testDate = new DateTime(date2.Year, 1, 1, 0, 0, 0);

                ddt = SalesTaxReport.ProcessTheData(ddt, testDate, date2 );

                dt.Merge(ddt);
            }

            string chart = txtChart.Text.Trim();
            if ( !String.IsNullOrWhiteSpace ( chart ))
            {
                dRows = dt.Select("contractNumber='" + chart + "'");
                if (dRows.Length > 0)
                    dt = dRows.CopyToDataTable();
            }

            dt.Columns.Add("ddate");
            dt.Columns.Add("name");
            dt.Columns.Add("location");
            if ( chkPreNeed.Checked )
                dt.Columns.Add("casket");
            dt.Columns.Add("netAmount", Type.GetType("System.Double"));
            dt.Columns.Add("count", Type.GetType("System.Double"));
            //dt.Columns.Add("SerialNumber");
            dt.Columns.Add("Loc");
            dt.Columns.Add("GOOD");


            string serviceId = "";
            string trust = "";
            string loc = "";
            string contract = "";
            string contractNumber = "";
            string casket = "";
            string prefix = "";
            string suffix = "";
            string name = "";
            string lName = "";
            string[] Lines = null;
            DataTable dx = null;
            DataTable dxx = null;
            string service = "";

            this.Cursor = Cursors.WaitCursor;

            PleaseWait waitForm =  G1.StartWait();

            lblTotal.Text = dt.Rows.Count.ToString();
            barImport.Minimum = 0;
            if ( dt.Rows.Count > 0 )
                barImport.Maximum = dt.Rows.Count - 1;
            barImport.Value = 0;
            barImport.Show();
            lblTotal.Show();
            lblTotal.Refresh();

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                try
                {
                    Application.DoEvents();
                    barImport.Value = i;
                    barImport.Refresh();

                    date = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                    if (date.Year > 100 || chkPreNeed.Checked )
                    {
                        dt.Rows[i]["GOOD"] = "Y";
                        dt.Rows[i]["ddate"] = date.ToString("yyyy-MM-dd");

                        prefix = dt.Rows[i]["prefix"].ObjToString();
                        suffix = dt.Rows[i]["suffix"].ObjToString().Trim();

                        if (!String.IsNullOrWhiteSpace(suffix))
                            name = dt.Rows[i]["lastName"].ObjToString() + " " + suffix + ", " + dt.Rows[i]["firstName"].ObjToString() + " " + dt.Rows[i]["middleName"].ObjToString();
                        else
                            name = dt.Rows[i]["lastName"].ObjToString() + ", " + dt.Rows[i]["firstName"].ObjToString() + " " + dt.Rows[i]["middleName"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(suffix))
                            name += ", " + suffix;
                        dt.Rows[i]["name"] = name;
                        if (!chkPreNeed.Checked)
                            serviceId = dt.Rows[i]["ServiceId"].ObjToString();
                        else
                            serviceId = dt.Rows[i]["contractNumber"].ObjToString();
                        if ( serviceId.ToUpper() == "HA23099")
                        {
                        }
                        if (serviceId == "CT22060")
                        {
                        }
                        if (serviceId == "AM22001")
                        {
                        }
                        contract = Trust85.decodeContractNumber(serviceId, ref trust, ref loc);
                        if (loc.ToUpper() == "CT")
                        {
                        }
                        if (!String.IsNullOrWhiteSpace(loc))
                        {
                            dt.Rows[i]["Loc"] = loc;
                            dRows = funDt.Select("atneedcode='" + loc + "'");
                            if (dRows.Length > 0)
                                loc = dRows[0]["LocationCode"].ObjToString();

                            dRows = funDt.Select("merchandiseCode='" + loc + "'");
                            if (dRows.Length > 0)
                            {
                                loc = dRows[0]["LocationCode"].ObjToString();
                                //dt.Rows[i]["location"] = dRows[0]["LocationCode"].ObjToString();
                                if (chkExcludeMerch.Checked)
                                    dt.Rows[i]["GOOD"] = "BAD";
                            }
                        }


                        dt.Rows[i]["location"] = loc;
                    }
                }
                catch ( Exception ex)
                {
                }
            }

            dRows = dt.Select("GOOD<>'BAD'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            else
                dt.Rows.Clear();
            if (doVaults)
            {
                try
                {
                    dt = AddVaults ( dt );
                }
                catch ( Exception ex)
                {
                }
            }
            else if (doUrns)
            {
                try
                {
                    dt = AddUrns ( dt );
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                AddFromInventory(dt);
                GetPrices(dt);
            }


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //serviceId = dt.Rows[i]["ServiceId"].ObjToString();
                loc = dt.Rows[i]["loc"].ObjToString();

                if (!String.IsNullOrWhiteSpace(loc))
                {
                    dRows = funDt.Select("atneedcode='" + loc + "'");
                    if (dRows.Length > 0)
                        loc = dRows[0]["LocationCode"].ObjToString();
                }
                dt.Rows[i]["location"] = loc;
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = "location";
            dt = tempview.ToTable();

            G1.NumberDataTable(dt);
            originalDt = dt;
            dgv.DataSource = dt;

            this.Cursor = Cursors.Default;

            G1.StopWait(ref waitForm);
        }
        /****************************************************************************************/
        private void GetPrices ( DataTable dt)
        {
            string serialNumber = "";
            double amount = 0D;
            string netAmount = "";
            int count = 0;
            string cmd = "";
            DataTable dx = null;
            string[] Lines = null;
            string desc = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                try
                {
                    serialNumber = dt.Rows[i]["SerialNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(serialNumber))
                        continue;
                    cmd = "Select * from `inventory` where `SerialNumber` = '" + serialNumber + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        amount = dx.Rows[0]["net"].ObjToDouble();
                        netAmount = G1.ReformatMoney(amount);
                        dt.Rows[i]["netAmount"] = amount;
                        desc = dx.Rows[0]["CasketDescription"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(desc))
                        {
                            Lines = desc.Split(' ');
                            if (Lines.Length > 1)
                                desc = Lines[0].Trim();
                        }
                        //dt.Rows[i]["casket"] = desc;
                        dt.Rows[i]["count"] = 1D;
                    }
                }
                catch ( Exception ex )
                {
                }
            }
        }
        /****************************************************************************************/
        private void GetPricesx(DataTable dt)
        {
            string serialNumber = "";
            double amount = 0D;
            string netAmount = "";
            int count = 0;
            string cmd = "";
            DataTable dx = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                serialNumber = dt.Rows[i]["SerialNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(serialNumber))
                    continue;
                cmd = "Select * from `invoices` where `SerialNumber` = '" + serialNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    amount = dx.Rows[0]["amount"].ObjToDouble();
                    netAmount = G1.ReformatMoney(amount);
                    dt.Rows[i]["netAmount"] = amount;
                    dt.Rows[i]["casket"] = dx.Rows[0]["casket"].ObjToString();
                    count = dx.Rows[0]["count"].ObjToInt32();
                    if (count <= 0)
                        count = 1;
                    dt.Rows[i]["count"] = count.ObjToDouble();
                }
            }
        }
        /****************************************************************************************/
        public static string GetCasketCode ( string serialNumber )
        {
            if (String.IsNullOrWhiteSpace(serialNumber))
                return "";

            string casketCode = "";

            string cmd = "Select * from `fcust_services` where `serialNumber` = '" + serialNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count > 0 )
            {
                string service = dt.Rows[0]["service"].ObjToString();
                if (!String.IsNullOrWhiteSpace(service))
                {
                    string[] Lines = service.Split(' ');
                    if (Lines.Length > 0)
                    {
                        casketCode = Lines[0].Trim();
                        if ( casketCode == "D-" )
                        {
                            if (Lines.Length > 1)
                                casketCode = Lines[1].Trim();
                        }
                    }
                }
            }
            return casketCode;
        }
        /****************************************************************************************/
        public static string GetCasket(DataTable dt)
        {
            //string cmd = "Select * from `casket_master`;";
            //DataTable dx = G1.get_db_data(cmd);
            string casket = "";
            string service = "";
            string type = "";
            string casketCode = "";
            string str = "";
            string[] Lines = null;
            DataRow[] dR = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                type = dt.Rows[i]["type"].ObjToString();
                if (type.ToUpper() == "MERCHANDISE")
                {
                    service = dt.Rows[i]["service"].ObjToString();
                    if (String.IsNullOrWhiteSpace(service))
                        continue;
                    Lines = service.Split(' ');
                    if (Lines.Length > 0)
                    {
                        casketCode = Lines[0].Trim();
                        str = casketCode.Substring(0, 1).ToUpper();
                        if (str == "V" && casketCode.Length == 3)
                            continue;
                        break;
                        //dR = dx.Select("casketcode='" + casketCode + "'");
                        //if (dR.Length > 0)
                        //{
                        //    casket = dR[0]["casketdesc"].ObjToString();
                        //    if (casket.IndexOf(casketCode) < 0)
                        //        casket = casketCode + " " + casket;
                        //    break;
                        //}
                    }
                }
            }
            return casket;
        }
        /****************************************************************************************/
        private DataTable AddVaults ( DataTable dt)
        {
            DataTable funDt = G1.get_db_data ( "Select * from `funeralHomes`;");
            DataTable casketDt = G1.get_db_data("Select * from `casket_master`;");
            string serviceId = "";
            string serialNumber = "";
            string casket = "";
            string locationCode = "";
            string contractNumber = "";
            string vaultDescription = "";
            string[] Lines = null;
            DataRow [] dR = null;
            DataRow[] ddR = null;
            DataRow dRow = null;
            DataTable tt = null;

            DateTime date = dateTimePicker1.Value;
            DateTime date1 = this.dateTimePicker1.Value;
            DateTime date2 = this.dateTimePicker2.Value;
            string startDate = date1.ToString("yyyy-MM-dd");
            string stopDate = date2.ToString("yyyy-MM-dd");
            string sdate1 = date1.ToString("yyyy-MM-dd") + " 00:00:00";
            string sdate2 = date2.ToString("yyyy-MM-dd") + " 23:59:59";
            DataRow[] dRows = null;
            string vault = "";
            DataTable vDt = null;
            double price = 0D;
            string casketCode = "";
            string str = "";
            bool found = false;

            barImport.Minimum = 0;
            if (dt.Rows.Count > 0)
                barImport.Maximum = dt.Rows.Count - 1;
            barImport.Value = 0;


            try
            {
                string cmd = "Select * from `casket_master`;";
                DataTable dx = G1.get_db_data(cmd);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Application.DoEvents();
                    barImport.Value = i;
                    barImport.Refresh();

                    serviceId = dt.Rows[i]["ServiceId"].ObjToString();
                    if (String.IsNullOrWhiteSpace(serviceId))
                        continue;
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    try
                    {
                        vault = dt.Rows[i]["vault"].ObjToString();
                        if (String.IsNullOrWhiteSpace(vault))
                        {
                            if ( serviceId == "CT20157")
                            {
                            }
                            found = false;
                            cmd = "Select * from `fcust_services` where `contractNumber` = '" + contractNumber + "' AND `type` = 'Merchandise';";
                            tt = G1.get_db_data(cmd);
                            for (int j = 0; j < tt.Rows.Count; j++)
                            {
                                vaultDescription = tt.Rows[j]["service"].ObjToString();
                                dRows = casketDt.Select("casketdesc='" + vaultDescription + "'");
                                if (dRows.Length > 0)
                                {
                                    casketCode = dRows[0]["casketCode"].ObjToString();
                                    str = casketCode.Substring(0, 1);
                                    if (str == "V")
                                    {
                                        dt.Rows[i]["casket"] = vaultDescription;
                                        dt.Rows[i]["serialNumber"] = "";
                                        dt.Rows[i]["netAmount"] = dRows[0]["casketCost"].ObjToDouble();
                                        dt.Rows[i]["count"] = 1D;
                                        found = true;
                                        break;
                                    }
                                }
                            }
                            if (!found)
                            {
                                dt.Rows[i]["casket"] = "";
                                dt.Rows[i]["serialNumber"] = "";
                                dt.Rows[i]["netAmount"] = 0D;
                            }
                            continue;
                        }
                        dRows = dx.Select("casketcode='" + vault + "'");
                        if ( dRows.Length > 0 )
                        {
                            vDt = dRows.CopyToDataTable();
                            dt.Rows[i]["casket"] = vDt.Rows[0]["casketdesc"].ObjToString();
                            dt.Rows[i]["serialNumber"] = "";
                            dt.Rows[i]["netAmount"] = vDt.Rows[0]["casketCost"].ObjToDouble();
                            dt.Rows[i]["count"] = 1D;

                        }
                    }
                    catch ( Exception ex)
                    {
                    }
                }
                dRows = dt.Select("casket<>''");
                if (dRows.Length > 0)
                    dt = dRows.CopyToDataTable();
            }
            catch ( Exception ex)
            {
            }
            return dt;
        }
        /****************************************************************************************/
        private DataTable AddUrns(DataTable dt)
        {
            DataTable funDt = G1.get_db_data("Select * from `funeralHomes`;");
            DataTable casketDt = G1.get_db_data("Select * from `casket_master`;");
            string serviceId = "";
            string serialNumber = "";
            string casket = "";
            string locationCode = "";
            string contractNumber = "";
            string vaultDescription = "";
            string urnDescription = "";
            string[] Lines = null;
            DataRow[] dR = null;
            DataRow[] ddR = null;
            DataRow dRow = null;
            DataTable tt = null;

            DateTime date = dateTimePicker1.Value;
            DateTime date1 = this.dateTimePicker1.Value;
            DateTime date2 = this.dateTimePicker2.Value;
            string startDate = date1.ToString("yyyy-MM-dd");
            string stopDate = date2.ToString("yyyy-MM-dd");
            string sdate1 = date1.ToString("yyyy-MM-dd") + " 00:00:00";
            string sdate2 = date2.ToString("yyyy-MM-dd") + " 23:59:59";
            DataRow[] dRows = null;
            string vault = "";
            DataTable vDt = null;
            double price = 0D;
            string casketCode = "";
            string str = "";
            bool found = false;
            bool isUrn = false;
            bool isVault = false;
            bool isUrnVault = false;
            bool isMisc = false;
            string cmd = "";

            if (!chkPreNeed.Checked)
            {
                dRows = dt.Select("funeral_classification LIKE 'Cremation%'");
                if (dRows.Length > 0)
                    dt = dRows.CopyToDataTable();
            }
            else
            {
                dRows = casketDt.Select("casketCode LIKE 'URN%'");
                if (dRows.Length > 0)
                {
                    casketDt = dRows.CopyToDataTable();
                    //casketDt.Columns.Add("Urn Count", Type.GetType("System.Int32"));
                    //for ( int i=0; i<casketDt.Rows.Count; i++)
                    //{
                    //    urnDescription = casketDt.Rows[i]["casketdesc"].ObjToString();
                    //    cmd = "Select * from `cust_services` where `service` = '" + urnDescription + "';";
                    //    DataTable dx = G1.get_db_data(cmd);
                    //    casketDt.Rows[i]["Urn Count"] = dx.Rows.Count;
                    //}
                }
            }

            barImport.Minimum = 0;
            if (dt.Rows.Count > 0)
                barImport.Maximum = dt.Rows.Count - 1;
            barImport.Value = 0;


            try
            {
                cmd = "Select * from `casket_master`;";
                DataTable dx = G1.get_db_data(cmd);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Application.DoEvents();

                    barImport.Value = i;
                    barImport.Refresh();

                    if (!chkPreNeed.Checked)
                    {
                        serviceId = dt.Rows[i]["ServiceId"].ObjToString();
                        if (String.IsNullOrWhiteSpace(serviceId))
                            continue;
                    }
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    try
                    {
                        found = false;
                        cmd = "Select * from `fcust_services` where `contractNumber` = '" + contractNumber + "' AND `type` = 'Merchandise';";
                        if ( chkPreNeed.Checked )
                            cmd = "Select * from `cust_services` where `contractNumber` = '" + contractNumber + "' AND `type` = 'Merchandise';";
                        tt = G1.get_db_data(cmd);
                        for (int j = 0; j < tt.Rows.Count; j++)
                        {
                            isVault = false;
                            isUrn = false;
                            isUrnVault = false;
                            isMisc = false;
                            urnDescription = tt.Rows[j]["service"].ObjToString();
                            dRows = casketDt.Select("casketdesc='" + urnDescription + "'");
                            if (dRows.Length > 0)
                            {
                                casketCode = dRows[0]["casketCode"].ObjToString();
                                str = casketCode.Substring(0, 1);
                                if (str == "V")
                                    isVault = true;
                                else if (casketCode.ToUpper().IndexOf("URN") == 0)
                                    isUrn = true;
                                else if (casketCode.ToUpper().IndexOf("UV") == 0)
                                {
                                    isUrn = true;
                                    isUrnVault = true;
                                }
                                else
                                    isMisc = true;
                            }
                            if (isUrn)
                            {
                                dt.Rows[i]["casket"] = urnDescription;
                                if ( !chkPreNeed.Checked )
                                    dt.Rows[i]["serialNumber"] = "";
                                dt.Rows[i]["netAmount"] = dRows[0]["casketCost"].ObjToDouble();
                                dt.Rows[i]["count"] = 1D;
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            dt.Rows[i]["casket"] = "";
                            if (!chkPreNeed.Checked)
                                dt.Rows[i]["serialNumber"] = "";
                            dt.Rows[i]["netAmount"] = 0D;
                        }
                        continue;
                    }
                    catch (Exception ex)
                    {
                    }
                }
                dRows = dt.Select("casket<>''");
                if (dRows.Length > 0)
                    dt = dRows.CopyToDataTable();
            }
            catch (Exception ex)
            {
            }
            return dt;
        }
        /****************************************************************************************/
        private void AddFromInventory(DataTable dt)
        {
            DataTable funDt = G1.get_db_data("Select * from `funeralHomes`;");
            string serviceId = "";
            string serialNumber = "";
            string casket = "";
            string locationCode = "";
            string contractNumber = "";
            string[] Lines = null;
            DataRow[] dR = null;
            DataRow[] ddR = null;
            DataRow dRow = null;
            DataTable tt = null;

            DateTime date = dateTimePicker1.Value;
            DateTime date1 = this.dateTimePicker1.Value;
            DateTime date2 = this.dateTimePicker2.Value;
            string startDate = date1.ToString("yyyy-MM-dd");
            string stopDate = date2.ToString("yyyy-MM-dd");
            string sdate1 = date1.ToString("yyyy-MM-dd") + " 00:00:00";
            string sdate2 = date2.ToString("yyyy-MM-dd") + " 23:59:59";
            try
            {
                string cmd = "Select * from `inventory` where `deceasedDate` >= '" + sdate1 + "' AND `deceasedDate` <= '" + sdate2 + "' ";
                cmd += " ORDER BY `deceasedDate`;";
                DataTable dx = G1.get_db_data(cmd);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    serviceId = dt.Rows[i]["ServiceId"].ObjToString();
                    if (String.IsNullOrWhiteSpace(serviceId))
                        continue;
                    if (serviceId == "MA22011")
                    {
                    }
                    if (serviceId == "WM22078")
                    {
                    }
                    if (serviceId == "CW22035")
                    {
                    }
                    if (serviceId == "BS22079")
                    {
                    }
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber == "SX22219")
                    {
                    }
                    try
                    {
                        dR = dx.Select("ServiceId='" + serviceId + "'"); // Lookup Service ID in Inventory
                        if (dR.Length <= 0)
                        {
                            serialNumber = dt.Rows[i]["serialNumber"].ObjToString();
                            dR = dx.Select("SerialNumber='" + serialNumber + "'"); // Lookup SerialNumber
                            if (dR.Length <= 0)
                            {
                                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                                cmd = "Select * from `fcust_services` where `contractNumber` = '" + contractNumber + "' AND `serialNumber` <> '';";
                                tt = G1.get_db_data(cmd);
                                if (tt.Rows.Count > 0)
                                {
                                    serialNumber = tt.Rows[0]["serialNumber"].ObjToString();
                                    dR = dx.Select("SerialNumber='" + serialNumber + "'"); // Lookup SerialNumber
                                    if (dR.Length <= 0)
                                    {
                                        cmd = "Select * from `inventory` where `serialNumber` = '" + serialNumber + "' AND `serviceId` = '" + serviceId + "';";
                                        dx = G1.get_db_data(cmd);
                                        if (dx.Rows.Count > 0)
                                            dR = dx.Select("SerialNumber='" + serialNumber + "'"); // Lookup SerialNumber
                                    }
                                }
                            }
                        }
                        if (dR.Length > 0)
                        {
                            serialNumber = dR[0]["SerialNumber"].ObjToString(); // Got One
                            dt.Rows[i]["SerialNumber"] = serialNumber;
                            casket = dR[0]["CasketDescription"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(serialNumber))
                            {
                                cmd = "Select * from `fcust_services` where `serialNumber` = '" + serialNumber + "';";
                                tt = G1.get_db_data(cmd);
                                if (tt.Rows.Count > 0)
                                    casket = tt.Rows[0]["service"].ObjToString();
                            }
                            if (!String.IsNullOrWhiteSpace(casket))
                            {
                                //Lines = casket.Split(' ');
                                //if (Lines.Length > 0)
                                //    dRow["casket"] = Lines[0].Trim();
                                dt.Rows[i]["casket"] = casket;
                            }
                            locationCode = dR[0]["locationCode"].ObjToString();
                            dR = funDt.Select("locationCode='" + locationCode + "'");
                            if (dR.Length > 0)
                                dt.Rows[i]["location"] = dR[0]["LocationCode"].ObjToString();
                            else
                                dt.Rows[i]["location"] = "XX";
                            if (serviceId == "HA23099")
                            {

                            }
                            cmd = "Select * from `fcust_extended` where `serviceId` = '" + serviceId + "';";
                            tt = G1.get_db_data(cmd);
                            if (tt.Rows.Count > 0)
                            {
                                contractNumber = tt.Rows[0]["contractNumber"].ObjToString();
                                if (String.IsNullOrWhiteSpace(serialNumber))
                                {
                                    serialNumber = tt.Rows[0]["serialNumber"].ObjToString();
                                    if (!String.IsNullOrWhiteSpace(serialNumber))
                                        dt.Rows[i]["SerialNumber"] = serialNumber;
                                    else
                                        dt.Rows[i]["casket"] = tt.Rows[0]["funeral_classification"].ObjToString();
                                }
                                cmd = "Select * from `fcust_services` where `contractNumber` = '" + contractNumber + "' AND `type` = 'Merchandise';";
                                tt = G1.get_db_data(cmd);
                                if (tt.Rows.Count > 0)
                                {
                                    dR = tt.Select("service LIKE '%Infant Casket%'");
                                    if (dR.Length > 0)
                                        dt.Rows[i]["casket"] = dR[0]["service"].ObjToString();
                                    else
                                    {
                                        cmd = checkForOtherCasket(tt);
                                        if (!String.IsNullOrWhiteSpace(cmd))
                                            dt.Rows[i]["casket"] = cmd;
                                        else if (serviceId.ToUpper().IndexOf("ML") == 0)
                                            dt.Rows[i]["casket"] = "Merchandise Contract";
                                    }
                                }
                            }
                            //dt.Rows.Add(dRow);
                        }
                        else
                        {
                            if (!String.IsNullOrWhiteSpace(contractNumber))
                            {
                                cmd = "Select * from `fcust_extended` where `contractNumber` = '" + contractNumber + "';";
                                tt = G1.get_db_data(cmd);
                                if (tt.Rows.Count > 0)
                                    dt.Rows[i]["casket"] = tt.Rows[0]["funeral_classification"].ObjToString();

                                cmd = "Select * from `fcust_services` where `contractNumber` = '" + contractNumber + "' AND `type` = 'Merchandise';";
                                tt = G1.get_db_data(cmd);
                                if (tt.Rows.Count > 0)
                                {
                                    dR = tt.Select("service LIKE '%Infant Casket%'");
                                    if (dR.Length > 0)
                                        dt.Rows[i]["casket"] = dR[0]["service"].ObjToString();
                                    else
                                    {
                                        cmd = checkForOtherCasket(tt);
                                        if (!String.IsNullOrWhiteSpace(cmd))
                                            dt.Rows[i]["casket"] = cmd;
                                        else if (serviceId.ToUpper().IndexOf("ML") == 0)
                                            dt.Rows[i]["casket"] = "Merchandise Contract";
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private string checkForOtherCasket ( DataTable dt )
        {
            string other = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                if ( dt.Rows[i]["service"].ObjToString().IndexOf( "**") == 0 )
                {
                    other = dt.Rows[i]["service"].ObjToString();
                    break;
                }
            }
            return other;
        }
        /****************************************************************************************/
        private void AddFromInventoryx(DataTable dt)
        {
            DataTable funDt = G1.get_db_data("Select * from `funeralHomes`;");
            string serviceId = "";
            string serialNumber = "";
            string casket = "";
            string locationCode = "";
            string contractNumber = "";
            string[] Lines = null;
            DataRow[] dR = null;
            DataRow[] ddR = null;
            DataRow dRow = null;
            DataTable tt = null;

            DateTime date = dateTimePicker1.Value;
            DateTime date1 = this.dateTimePicker1.Value;
            DateTime date2 = this.dateTimePicker2.Value;
            string startDate = date1.ToString("yyyy-MM-dd");
            string stopDate = date2.ToString("yyyy-MM-dd");
            string sdate1 = date1.ToString("yyyy-MM-dd") + " 00:00:00";
            string sdate2 = date2.ToString("yyyy-MM-dd") + " 23:59:59";
            try
            {
                string cmd = "Select * from `inventory` where `deceasedDate` >= '" + sdate1 + "' AND `deceasedDate` <= '" + sdate2 + "' ";
                cmd += " ORDER BY `deceasedDate`;";
                DataTable dx = G1.get_db_data(cmd);

                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    serviceId = dx.Rows[i]["ServiceId"].ObjToString();
                    if (String.IsNullOrWhiteSpace(serviceId))
                        continue;
                    dR = dt.Select("ServiceId='" + serviceId + "'");
                    if (dR.Length >= 0)
                    {
                        dRow = dt.NewRow();
                        dRow["dDate"] = dx.Rows[i]["deceasedDate"].ObjToDateTime().ToString("yyyy-MM-dd");
                        dRow["ServiceId"] = serviceId;
                        serialNumber = dx.Rows[i]["SerialNumber"].ObjToString();
                        dRow["SerialNumber"] = serialNumber;
                        casket = dx.Rows[i]["CasketDescription"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(serialNumber))
                        {
                            cmd = "Select * from `fcust_services` where `serialNumber` = '" + serialNumber + "';";
                            tt = G1.get_db_data(cmd);
                            if (tt.Rows.Count > 0)
                                casket = tt.Rows[0]["service"].ObjToString();
                        }
                        if (!String.IsNullOrWhiteSpace(casket))
                        {
                            //Lines = casket.Split(' ');
                            //if (Lines.Length > 0)
                            //    dRow["casket"] = Lines[0].Trim();
                            dRow["casket"] = casket;
                        }
                        locationCode = dx.Rows[i]["locationCode"].ObjToString();
                        dR = funDt.Select("locationCode='" + locationCode + "'");
                        if (dR.Length > 0)
                            dRow["location"] = dR[0]["atneedcode"].ObjToString();
                        else
                            dRow["location"] = "XX";
                        cmd = "Select * from `fcust_extended` where `serviceId` = '" + serviceId + "';";
                        tt = G1.get_db_data(cmd);
                        if (tt.Rows.Count > 0)
                        {
                            contractNumber = tt.Rows[0]["contractNumber"].ObjToString();
                            dRow["contractNumber"] = contractNumber;
                            cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
                            tt = G1.get_db_data(cmd);
                            if (tt.Rows.Count > 0)
                            {
                                dRow["name"] = tt.Rows[0]["lastName"].ObjToString() + ", " + tt.Rows[0]["firstName"].ObjToString() + " " + tt.Rows[0]["middleName"].ObjToString();
                            }
                        }
                        dt.Rows.Add(dRow);
                    }
                    else
                    {
                        //dRow = dt.NewRow();
                        dR[0]["dDate"] = dx.Rows[i]["deceasedDate"].ObjToDateTime().ToString("yyyy-MM-dd");
                        dR[0]["ServiceId"] = serviceId;
                        serialNumber = dx.Rows[i]["SerialNumber"].ObjToString();
                        dR[0]["SerialNumber"] = serialNumber;
                        casket = dx.Rows[i]["CasketDescription"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(serialNumber))
                        {
                            cmd = "Select * from `fcust_services` where `serialNumber` = '" + serialNumber + "';";
                            tt = G1.get_db_data(cmd);
                            if (tt.Rows.Count > 0)
                                casket = tt.Rows[0]["service"].ObjToString();
                        }
                        if (!String.IsNullOrWhiteSpace(casket))
                        {
                            //Lines = casket.Split(' ');
                            //if (Lines.Length > 0)
                            //    dRow["casket"] = Lines[0].Trim();
                            dR[0]["casket"] = casket;
                        }
                        locationCode = dx.Rows[i]["locationCode"].ObjToString();
                        ddR = funDt.Select("locationCode='" + locationCode + "'");
                        if (ddR.Length > 0)
                            dR[0]["location"] = dR[0]["atneedcode"].ObjToString();
                        else
                            dR[0]["location"] = "XX";
                        cmd = "Select * from `fcust_extended` where `serviceId` = '" + serviceId + "';";
                        tt = G1.get_db_data(cmd);
                        if (tt.Rows.Count > 0)
                        {
                            contractNumber = tt.Rows[0]["contractNumber"].ObjToString();
                            //dRow["contractNumber"] = contractNumber;
                            cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
                            tt = G1.get_db_data(cmd);
                            if (tt.Rows.Count > 0)
                            {
                                dR[0]["name"] = tt.Rows[0]["lastName"].ObjToString() + ", " + tt.Rows[0]["firstName"].ObjToString() + " " + tt.Rows[0]["middleName"].ObjToString();
                            }
                        }
                        //dt.Rows.Add(dRow);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void gridMain_DoubleClickx(object sender, EventArgs e)
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
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contractNumber = dr["contractNumber"].ObjToString();
            string cnum = FindCustomer(contractNumber, "fcustomers");
            if (String.IsNullOrWhiteSpace(cnum))
                cnum = FindCustomer(contractNumber, "customers");
            if (String.IsNullOrWhiteSpace(cnum))
                return;
            this.Cursor = Cursors.WaitCursor;
            if (chkPreNeed.Checked)
            {
                CustomerDetails clientForm = new CustomerDetails(cnum);
                clientForm.Show();
            }
            else
            {
                EditCust clientForm = new EditCust(cnum);
                clientForm.Show();
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private string FindCustomer ( string cnum, string where )
        {
            string contractNumber = "";
            string cmd = "Select * from `" + where + "` where `contractNumber` = '" + cnum + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                contractNumber = dt.Rows[0]["contractNumber"].ObjToString();
            return contractNumber;
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
            if (chkExcludeHeader.Checked)
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
            string reportName = "Casket Usage";
            string report = reportName + " Report for " + this.dateTimePicker1.Value.ToString("MM/dd/yyyy") + " through " + this.dateTimePicker2.Value.ToString("MM/dd/yyyy");
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
        private void chkGroupLocation_CheckedChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (chkGroupLocation.Checked)
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "location,ServiceId";
                //tempview.Sort = "ServiceId";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

                gridMain.Columns["location"].GroupIndex = 0;
                //gridMain.Columns["ServiceId"].GroupIndex = 0;
                gridMain.OptionsView.ShowFooter = true;
                this.gridMain.ExpandAllGroups();
            }
            else
            {
                gridMain.Columns["location"].GroupIndex = -1;
                //gridMain.Columns["ServiceId"].GroupIndex = -1;
                gridMain.OptionsView.ShowFooter = true;
                gridMain.CollapseAllGroups();

                dt = originalDt;
                DataView tempview = dt.DefaultView;
                tempview.Sort = "location,ServiceId";
                //tempview.Sort = "ServiceId";
                dt = tempview.ToTable();
                dgv.DataSource = dt;
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void btnImportBatesville_Click(object sender, EventArgs e)
        {
            DataTable dt = null;
            this.Cursor = Cursors.WaitCursor;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    dgv.DataSource = null;
                    try
                    {
                        dt = Import.ImportCSVfile(file);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            this.Cursor = Cursors.Default;
            if (dt == null)
                return;
            DataTable tempDt = null;
            DataTable inventoryDt = null;
            //DataTable dx = workDt.Clone();
            //dx.Columns.Add("comment");
            //dx.Columns.Add("bsid");
            //dx.Columns.Add("sidResult");
            DataRow dRow = null;
            string serialNumber = "";
            string description = "";
            string serviceId = "";
            string desc2 = "";
            string cmd = "";
            int i = 0;
            this.Cursor = Cursors.WaitCursor;
            //try
            //{
            //    for (i = 0; i < dt.Rows.Count; i++)
            //    {
            //        serialNumber = dt.Rows[i]["Serial #"].ObjToString();
            //        if (String.IsNullOrWhiteSpace(serialNumber))
            //            continue;
            //        description = dt.Rows[i]["Desc Ln 1"].ObjToString();
            //        serviceId = dt.Rows[i]["Customer PO"].ObjToString();
            //        cmd = "Select * from `inventory` where `SerialNumber` = '" + serialNumber + "';";
            //        tempDt = G1.get_db_data(cmd);
            //        if (tempDt.Rows.Count > 0)
            //        {
            //            for (int j = 0; j < tempDt.Rows.Count; j++)
            //            {
            //                dx.ImportRow(tempDt.Rows[j]);
            //                int row = dx.Rows.Count - 1;
            //                dx.Rows[row]["bsid"] = serviceId;
            //                if (j > 0)
            //                    dx.Rows[row]["comment"] = "Duplicate SS#";
            //                desc2 = tempDt.Rows[j]["CasketDescription"].ObjToString();
            //                cmd = "Select * from `inventorylist` where `casketdesc` = '" + desc2 + "';";
            //                inventoryDt = G1.get_db_data(cmd);
            //                if (inventoryDt.Rows.Count > 0)
            //                {
            //                    dx.Rows[row]["casketguage"] = inventoryDt.Rows[0]["casketguage"].ObjToString();
            //                    dx.Rows[row]["caskettype"] = inventoryDt.Rows[0]["caskettype"].ObjToString();
            //                }
            //            }
            //        }
            //        else
            //        {
            //            dRow = dx.NewRow();
            //            dRow["SerialNumber"] = serialNumber;
            //            dRow["CasketDescription"] = description;
            //            dRow["ServiceID"] = serviceId;
            //            dRow["bsid"] = serviceId;
            //            dRow["comment"] = "NOT IN INVENTORY";

            //            cmd = "Select * from `inventorylist` where `casketdesc` = '" + description + "';";
            //            inventoryDt = G1.get_db_data(cmd);
            //            if (inventoryDt.Rows.Count > 0)
            //            {
            //                dRow["casketguage"] = inventoryDt.Rows[0]["casketguage"].ObjToString();
            //                dRow["caskettype"] = inventoryDt.Rows[0]["caskettype"].ObjToString();
            //            }
            //            dx.Rows.Add(dRow);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            //}
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
            else
            {
                bool doit = false;
                if (e.Column.FieldName.ToUpper().IndexOf("SERIALNUMBER") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("NETAMOUNT") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                else if (e.Column.FieldName.ToUpper().IndexOf("COUNT") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                    doit = true;
                if (doit)
                {
                    string str = e.DisplayText;
                    if (String.IsNullOrWhiteSpace(str))
                    {
                        DataTable dt = (DataTable)dgv.DataSource;
                        int row = e.ListSourceRowIndex;
                        string  casket = dt.Rows[row]["casket"].ObjToString().ToUpper();
                        if (!chkPreNeed.Checked)
                        {
                            string fpc = dt.Rows[row]["FPC"].ObjToString();
                            if ( String.IsNullOrWhiteSpace ( fpc ) )
                            {
                                if (casket.ToUpper().IndexOf("FAMILY PROVIDED CASKET") >= 0)
                                    fpc = "Y";
                            }
                            if (casket.ToUpper().IndexOf("CREMATION") == 0)
                                e.DisplayText = "N/A";
                            else if (casket.ToUpper().IndexOf("OTHER") == 0)
                                e.DisplayText = "N/A";
                            else if (fpc == "Y")
                                e.DisplayText = "N/A";
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
    }
}