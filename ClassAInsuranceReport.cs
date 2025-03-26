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
    public partial class ClassAInsuranceReport : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private DataTable originalDt = null;
        private DataTable bankDt = null;

        private string cci = "MA,ME,MO,RA";
        private string bfh = "FR,HH,TY,WM";
        private string smfs = "BN,CT,EV,FF,HA,RF";
        private string jcc = "BK,LR,TV";
        private string bsrf = "BS,CW,FO,MC,NC";
        private string wf = "WC,WF,WM",WR;
        /****************************************************************************************/
        public ClassAInsuranceReport()
        {
            InitializeComponent();
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void ClassAInsuranceReport_Load(object sender, EventArgs e)
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
            AddSummaryColumn("netAmount", null);
            AddSummaryColumn("locationAmount", null);
            AddSummaryColumn("companyAmount", null);
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

            DataTable funDt = null;

            DateTime date = dateTimePicker1.Value;
            DateTime date1 = this.dateTimePicker1.Value;
            DateTime date2 = this.dateTimePicker2.Value;
            string startDate = date1.ToString("yyyy-MM-dd");
            string stopDate = date2.ToString("yyyy-MM-dd");
            string sdate1 = date1.ToString("yyyy-MM-dd") + " 00:00:00";
            string sdate2 = date2.ToString("yyyy-MM-dd") + " 23:59:59";
            string cmd = "Select * from `fcustomers` p LEFT JOIN `cust_payments` c ON p.`contractNumber` = c.`contractNumber` where p.`deceasedDate` >= '" + sdate1 + "' AND p.`deceasedDate` <= '" + sdate2 + "' ";
            //            cmd += " AND c.`type` = 'Class A' AND c.`dateEntered` >= '" + sdate1 + "' AND c.`dateEntered` <= '" + sdate2 + "' AND c.`description` <> 'Class A Discount' ";
            //cmd += " AND c.`dateEntered` >= '" + sdate1 + "' AND c.`dateEntered` <= '" + sdate2 + "' AND c.`description` <> 'Class A Discount' ";
            //cmd += " AND c.`description` <> 'Class A Discount' ";

            string contract = txtContract.Text.Trim();
            if (!string.IsNullOrWhiteSpace(contract))
            {
                cmd = "Select * from `fcustomers` p LEFT JOIN `cust_payments` c ON p.`contractNumber` = c.`contractNumber` where  ";
                cmd += " p.`serviceId` = '" + contract + "' ";
                //cmd += " AND c.`description` <> 'Class A Discount' ";
            }

            //cmd += " AND p.`contractNumber` = 'SX22223' ";
            cmd += " ORDER BY p.`deceasedDate`;";

            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("ddate");
            dt.Columns.Add("name");
            dt.Columns.Add("location");
            dt.Columns.Add("casket");
            dt.Columns.Add("netAmount", Type.GetType("System.Double"));
            dt.Columns.Add("count");
            dt.Columns.Add("locationAmount", Type.GetType("System.Double"));
            dt.Columns.Add("companyAmount", Type.GetType("System.Double"));

            string serviceId = "";
            string trust = "";
            string loc = "";
            string contractNumber = "";
            string oldContract = "";
            string casket = "";
            string type = "";
            string[] Lines = null;
            DataTable dx = null;

            this.Cursor = Cursors.WaitCursor;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (date.Year > 100)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(oldContract))
                        oldContract = contractNumber;
                    dt.Rows[i]["ddate"] = date.ToString("yyyy-MM-dd");
                    dt.Rows[i]["name"] = dt.Rows[i]["lastName"].ObjToString() + ", " + dt.Rows[i]["firstName"].ObjToString() + " " + dt.Rows[i]["middleName"].ObjToString();
                    serviceId = dt.Rows[i]["ServiceId"].ObjToString();
                    type = dt.Rows[i]["type"].ObjToString();
                    if (type.ToUpper() == "CLASS A")
                        dt.Rows[i]["netAmount"] = dt.Rows[i]["payment"].ObjToDouble();
                    else
                    {
                        dt.Rows[i]["netAmount"] = 0D;
                        //if (contractNumber == oldContract)
                        //    dt.Rows[i]["netAmount"] = -1D;
                    }

                    contract = Trust85.decodeContractNumber(serviceId, ref trust, ref loc);
                    dt.Rows[i]["location"] = loc;
                    oldContract = contractNumber;
                }
            }

            //AddFromInventory(dt);

            //GetPrices(dt);

            dt = Consolidate(dt);

            DataView tempview = dt.DefaultView;
            tempview.Sort = "location";
            dt = tempview.ToTable();

            dt = SummarizeGroups(dt);

            G1.NumberDataTable(dt);

            originalDt = dt;
            dgv.DataSource = dt;

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable SummarizeGroups ( DataTable dt )
        {
            DataView tempview = dt.DefaultView;
            tempview.Sort = "location";
            dt = tempview.ToTable();

            DataTable dx = dt.Copy();

            //private string cci = "MA,ME,MO,RA";
            //private string bfh = "FR,HH,TY,WM";
            //private string smfs = "BN,CT,EV,FF,HA,RF";
            //private string jcc = "BK,LR,TV";
            //private string bsrf = "BS,CW,FO,MC,NC";
            //private string wf = "WC,WF,WM", WR;

            double cci_d = 0D;
            double bfh_d = 0D;
            double smfs_d = 0D;
            double jcc_d = 0D;
            double bsrf_d = 0D;
            double wf_d = 0D;

            DataRow dR = null;

            for ( int i=0; i<3; i++)
            {
                dR = dt.NewRow();
                dt.Rows.Add(dR);
            }

            string oldLoc = "";
            string loc = "";

            double payment = 0D;
            double totalPayment = 0D;

            try
            {
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    loc = dx.Rows[i]["location"].ObjToString();
                    if (String.IsNullOrWhiteSpace(oldLoc))
                        oldLoc = loc;
                    payment = dx.Rows[i]["netAmount"].ObjToDouble();
                    if (loc != oldLoc || i == (dx.Rows.Count - 1))
                    {
                        dR = dt.NewRow();
                        dR["location"] = oldLoc;
                        dR["locationAmount"] = totalPayment;
                        dt.Rows.Add(dR);

                        if (cci.Contains(oldLoc))
                            cci_d += totalPayment;
                        else if (bfh.Contains(oldLoc))
                            bfh_d += totalPayment;
                        else if (smfs.Contains(oldLoc))
                            smfs_d += totalPayment;
                        else if (jcc.Contains(oldLoc))
                            jcc_d += totalPayment;
                        else if (bsrf.Contains(oldLoc))
                            bsrf_d += totalPayment;
                        else if (wf.Contains(oldLoc))
                            wf_d += totalPayment;

                        oldLoc = loc;
                        totalPayment = payment;
                        continue;
                    }
                    totalPayment += payment;
                }


                for (int i = 0; i < 3; i++)
                {
                    dR = dt.NewRow();
                    dt.Rows.Add(dR);
                }

                dR = dt.NewRow();
                dR["location"] = "CCI";
                dR["companyAmount"] = cci_d;
                dt.Rows.Add(dR);

                dR = dt.NewRow();
                dR["location"] = "BFH";
                dR["companyAmount"] = bfh_d;
                dt.Rows.Add(dR);

                dR = dt.NewRow();
                dR["location"] = "SMFS";
                dR["companyAmount"] = smfs_d;
                dt.Rows.Add(dR);

                dR = dt.NewRow();
                dR["location"] = "JCC";
                dR["companyAmount"] = jcc_d;
                dt.Rows.Add(dR);

                dR = dt.NewRow();
                dR["location"] = "BSRF";
                dR["companyAmount"] = bsrf_d;
                dt.Rows.Add(dR);

                dR = dt.NewRow();
                dR["location"] = "W&F";
                dR["companyAmount"] = wf_d;
                dt.Rows.Add(dR);
            }
            catch ( Exception ex)
            {
            }
            return dt;
        }
        /****************************************************************************************/
        private DataTable Consolidate ( DataTable dt )
        {
            string serviceId = "";
            string oldServiceId = "";
            DateTime deceasedDate = DateTime.Now;
            DateTime oldDeceasedDate = DateTime.Now;

            DataView tempview = dt.DefaultView;
            tempview.Sort = "serviceId asc, deceasedDate asc";
            dt = tempview.ToTable();


            string contractNumber = "";

            double payment = 0D;

            for ( int i=(dt.Rows.Count-1); i>0; i-- )
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                serviceId = dt.Rows[i]["serviceId"].ObjToString();
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                payment = dt.Rows[i]["netAmount"].ObjToDouble();
                if (serviceId == "FO22003")
                {
                }
                if (serviceId == "HA21999")
                {
                }
                if ( String.IsNullOrWhiteSpace ( serviceId ))
                {
                    oldServiceId = serviceId;
                    oldDeceasedDate = deceasedDate;
                }
                serviceId = dt.Rows[i - 1]["serviceID"].ObjToString();
                deceasedDate = dt.Rows[i - 1]["deceasedDate"].ObjToDateTime();
                if ( serviceId == oldServiceId && deceasedDate == oldDeceasedDate )
                {
                    payment += dt.Rows[i - 1]["netAmount"].ObjToDouble();
                    dt.Rows[i - 1]["netAmount"] = payment;
                    dt.Rows.RemoveAt(i);
                }
                oldServiceId = serviceId;
                oldDeceasedDate = deceasedDate;
            }
            return dt;
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
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                serialNumber = dt.Rows[i]["SerialNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(serialNumber))
                    continue;
                cmd = "Select * from `invoices` where `SerialNumber` = '" + serialNumber + "';";
                dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count > 0 )
                {
                    amount = dx.Rows[0]["amount"].ObjToDouble();
                    netAmount = G1.ReformatMoney(amount);
                    dt.Rows[i]["netAmount"] = amount;
                    dt.Rows[i]["casket"] = dx.Rows[0]["casket"].ObjToString();
                    count = dx.Rows[0]["count"].ObjToInt32();
                    if (count <= 0)
                        count = 1;
                    dt.Rows[i]["count"] = count.ToString();
                }
            }
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
        private void AddFromInventory ( DataTable dt)
        {
            DataTable funDt = G1.get_db_data ( "Select * from `funeralHomes`;");
            string serviceId = "";
            string casket = "";
            string locationCode = "";
            string contractNumber = "";
            string[] Lines = null;
            DataRow [] dR = null;
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
                    if (dR.Length <= 0)
                    {
                        dRow = dt.NewRow();
                        dRow["dDate"] = dx.Rows[i]["deceasedDate"].ObjToDateTime().ToString("yyyy-MM-dd");
                        dRow["ServiceId"] = serviceId;
                        dRow["SerialNumber"] = dx.Rows[i]["SerialNumber"].ObjToString();
                        casket = dx.Rows[i]["CasketDescription"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(casket))
                        {
                            Lines = casket.Split(' ');
                            if (Lines.Length > 0)
                                dRow["casket"] = Lines[0].Trim();
                        }
                        locationCode = dx.Rows[i]["locationCode"].ObjToString();
                        dR = funDt.Select("locationCode='" + locationCode + "'");
                        if (dR.Length > 0)
                            dRow["location"] = dR[0]["atneedcode"].ObjToString();
                        else
                            dRow["location"] = "XX";
                        cmd = "Select * from `cust_extended` where `serviceId` = '" + serviceId + "';";
                        tt = G1.get_db_data(cmd);
                        if ( tt.Rows.Count > 0 )
                        {
                            contractNumber = tt.Rows[0]["contractNumber"].ObjToString();
                            dRow["contractNumber"] = contractNumber;
                            cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
                            tt = G1.get_db_data(cmd);
                            if ( tt.Rows.Count > 0 )
                            {
                                dRow["name"] = tt.Rows[0]["lastName"].ObjToString() + ", " + tt.Rows[0]["firstName"].ObjToString() + " " + tt.Rows[0]["middleName"].ObjToString();
                            }
                        }
                        dt.Rows.Add(dRow);
                    }
                }
            }
            catch ( Exception ex)
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
            //CustomerDetails clientForm = new CustomerDetails(cnum);
            //clientForm.Show();
            EditCust clientForm = new EditCust(cnum);
            clientForm.Show();
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

            Printer.setupPrinterMargins(10, 10, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();
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

            Printer.setupPrinterMargins(10, 10, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();
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
            string reportName = "Class A Insurance";
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
                    if (gridMain.OptionsPrint.ExpandAllGroups == true )
                        e.PS.InsertPageBreak(e.Y);
                }
            }
        }
        /****************************************************************************************/
        private void chkCollapse_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkCollapse.Checked)
            {
                gridMain.ExpandAllGroups();
                gridMain.OptionsPrint.ExpandAllGroups = true;
            }
            else
            {
                gridMain.OptionsPrint.ExpandAllGroups = false;
                gridMain.CollapseAllGroups();
            }
        }
        /****************************************************************************************/
        private void chkGroupLocation_CheckedChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (chkGroupLocation.Checked)
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "location";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

                gridMain.Columns["location"].GroupIndex = 0;
                gridMain.OptionsView.ShowFooter = true;
                this.gridMain.ExpandAllGroups();
            }
            else
            {
                dt = originalDt;
                dgv.DataSource = dt;
                dgv.RefreshDataSource();
                gridMain.RefreshData();
                gridMain.Columns["location"].GroupIndex = -1;
                gridMain.OptionsView.ShowFooter = true;
//                gridMain.CollapseAllGroups();

            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /****************************************************************************************/
    }
}