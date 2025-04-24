using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using GeneralLib;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.Xpo.Helpers;
using System.IO;
using ExcelLibrary.BinaryFileFormat;
using System.Security.Cryptography;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DevExpress.Office.Utils;
using DevExpress.XtraGrid;
using System.Drawing.Drawing2D;
using DevExpress.Utils.Drawing;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
//using System.Web.UI.WebControls;
using DevExpress.Data;
using DevExpress.XtraGrid.Views.Grid;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class TrustContractEOY: DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        DataTable funDt = null;
        DataTable preDt = null;
        DataTable cemDt = null;
        DataTable agentDt = null;
        DataTable originalDt = null;
        /***********************************************************************************************/
        public TrustContractEOY()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void TrustContractEOY_Load(object sender, EventArgs e)
        {
            footerCount = 0;

            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            DateTime stop = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Value = stop;

            loadLocatons();

            agentDt = G1.get_db_data("Select * from `agents`;");
        }
        /***********************************************************************************************/
        private void loadLocatons()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            funDt = G1.get_db_data(cmd);

            cmd = "Select * from `cemeteries` order by `loc`;";
            cemDt = G1.get_db_data(cmd);

            string loc = "";
            string desc = "";

            DataRow dRow = null;

            for ( int i=0; i<cemDt.Rows.Count; i++)
            {
                loc = cemDt.Rows[i]["loc"].ObjToString();
                desc = cemDt.Rows[i]["description"].ObjToString();

                dRow = funDt.NewRow();
                dRow["keycode"] = loc;
                dRow["LocationCode"] = desc;
                dRow["name"] = loc + " " + desc;
                funDt.Rows.Add(dRow);
            }

            chkComboLocNames.Properties.DataSource = funDt;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Add if statement. If dgv2.visible gridMain2 and so on.
            if (dgv.Visible == true)
                G1.ShowHideFindPanel(gridMain);
            else if (dgv2.Visible == true)
                G1.ShowHideFindPanel(gridMain2);
            else if (dgv3.Visible == true)
                G1.ShowHideFindPanel(gridMain3);
            else if (dgv4.Visible == true)
                G1.ShowHideFindPanel(gridMain4);
            else if (dgv5.Visible == true)
                G1.ShowHideFindPanel(gridMain5);
            else if (dgv6.Visible == true)
                G1.ShowHideFindPanel(gridMain6);
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;

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
            if (e.IsForGroupRow ) // Use this for Group Headers ONLY
            {
                //if (e.Column.FieldName.ToUpper() == "LOCATION" )
                //    e.DisplayText = "<your text>";
            }
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                using (CustomerDetails clientForm = new CustomerDetails(contract))
                {
                    clientForm.ShowDialog();
                }
                this.Cursor = Cursors.Default;
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

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();
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

            font = new Font("Ariel", 12);
            string text = this.Text + " " + this.dateTimePicker1.Value.ToString("MM/dd/yyyy") + " - " + this.dateTimePicker2.Value.ToString("MM/dd/yyyy");
            
            Printer.DrawQuad(4, 7, 5, 4, text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Bold);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
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

            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();
            isPrinting = false;
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
//            string status = dt.Rows[row]["status"].ObjToString().Trim().ToUpper();
//            string trustType = dt.Rows[row]["trustType"].ObjToString().ToUpper();
            /*
            string showWhat = cmbType.Text.Trim().ToUpper();
            if ( showWhat == "TRUST")
            {
                if ( trustType != "TRUST")
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
            else if (showWhat == "INSURANCE")
            {
                if (trustType != "INSURANCE")
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
            else if (showWhat == "CEMETERY")
            {
                if (trustType != "CEMETERY")
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
            */
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
            }
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            date = date.AddMonths(-1);
            this.dateTimePicker1.Value = date;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = date;
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            date = date.AddMonths(1);
            this.dateTimePicker1.Value = date;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = date;
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            // This needs to pull from trust2013r
            this.Cursor = Cursors.WaitCursor;

            DateTime date = dateTimePicker1.Value;
            DateTime saveDate1 = date;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            DateTime saveDate2 = date;
            string date2 = G1.DateTimeToSQLDateTime(date);

            string cmd = "SELECT * FROM `trust2013r` t LEFT JOIN `customers` c ON t.`contractNumber` = c.`contractNumber`";
            cmd += " WHERE `payDate8` >= '" + date1 + "' AND `payDate8` <= '" + date2 + "' ";
            cmd += " ORDER by t.`payDate8`, t.`contractNumber`";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);

            dt = processTheData(dt);

            G1.NumberDataTable(dt);

            /*
            if (chkQuarterTotals.Checked)
            {
                gridMain.OptionsView.GroupFooterShowMode = DevExpress.XtraGrid.Views.Grid.GroupFooterShowMode.VisibleIfExpanded;
                gridMain.OptionsCustomization.AllowColumnMoving = true;
            }
            else
            {
                gridMain.OptionsView.GroupFooterShowMode = DevExpress.XtraGrid.Views.Grid.GroupFooterShowMode.Hidden;
                gridMain.OptionsView.ShowGroupPanel = false;
            }
            */
            gridMain.Columns["location"].GroupIndex = 0;
            gridMain.OptionsView.ShowFooter = false;
            gridMain.OptionsView.GroupFooterShowMode = DevExpress.XtraGrid.Views.Grid.GroupFooterShowMode.VisibleIfExpanded;

            dt = processLocations(dt);

            originalDt = dt;
            dgv.DataSource = dt;
            buildSummary(dt);
            gridMain.ExpandAllGroups();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/

        private void buildSummary(DataTable dt)
        {
            string location = "";
            string oldLoc = "";
            string locInd = "";
            string oldLocInd = "";
            string is2002 = "";
            string oldIs2002 = "";

            DataRow dRow = null;
            
            double contracts = 0D;
            double total = 0D;

            double totals = 0D;
            double contractTotals = 0D;

            DataTable dt2 = new DataTable();
            dt2.Columns.Add("contracts", Type.GetType("System.Double"));
            dt2.Columns.Add("total", Type.GetType("System.Double"));
            dt2.Columns.Add("location");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    location = dt.Rows[i]["location"].ObjToString();
                    is2002 = dt.Rows[i]["is2002"].ObjToString();
                    locInd = dt.Rows[i]["locInd"].ObjToString();

                    if (string.IsNullOrWhiteSpace(oldLoc))
                    {
                        oldLoc = location;
                        oldIs2002 = is2002;
                        oldLocInd = locInd;
                    }
                    if (oldLoc != location)
                    {
                        dRow = dt2.NewRow();
                        dRow["location"] = getLocation(oldLoc);
                        dRow["contracts"] = contracts;
                        dRow["total"] = total;
                        dt2.Rows.Add(dRow);

                        totals += total;
                        contractTotals += contracts;
                        contracts = 0D;
                        total = 0D;
                        oldLoc = location;
                    }
                    contracts++;
                    total += dt.Rows[i]["endingBalance"].ObjToDouble();
                }
                catch (Exception ex)
                { 
                    
                }
            }

            if (contracts != 0D)
            {
                dRow = dt2.NewRow();
                dRow["location"] = getLocation(oldLoc);
                dRow["contracts"] = contracts;
                dRow["total"] = total;
                dt2.Rows.Add(dRow);

                totals += total;
                contractTotals += contracts;
            }

            dRow = dt2.NewRow();
            dt2.Rows.Add(dRow);

            dRow = dt2.NewRow();
            dRow["location"] = "Totals";
            dRow["contracts"] = contractTotals;
            dRow["total"] = totals;
            dt2.Rows.Add(dRow);
            dgv2.DataSource = dt2;
        }
        /***********************************************************************************************/
        private DataTable processLocations(DataTable dt)
        {
            string location = "";

            if (funDt == null)
                funDt = G1.get_db_data("SELECT * FROM `funeralHomes`;");

            if (preDt == null)
                preDt = G1.get_db_data("SELECT * FROM `pre2002`;");

            string is2002 = "";
            string locInd = "";
            string riles = "";
            string name = "";
            double balance = 0D;
            double interest = 0D;
            double removals = 0D;

            dt.Columns.Add("serviceLoc");
            DataRow[] dRows = null;
            for (int i = 0; i < dt.Rows.Count; i++) 
            {
                location = dt.Rows[i]["location"].ObjToString().ToUpper();
                if (location == "H")
                    location = "B";
                else if (location == "R")
                    location = "B";
                if (location == "J")
                    location = "C";
                else if (location == "CA")
                    location = "CT";
                else if (location == "JT")
                    location = "E";
                else if (location == "FFN")
                    location = "FF";
                else if (location == "FFO")
                    location = "FF";
                else if (location == "RF")
                    location = "FF";/*
                else if (location == "FO")
                    location = "F";*/
                else if (location == "F")
                    location = "FO";
                else if (location == "U")
                    location = "M";
                else if (location == "UM")
                    location = "M";
                else if (location == "NB")
                    location = "N";
                /*else if (location == "NC")
                    location = "NNM";
                else if (location == "NCOC")
                    location = "NNM";*/
                else if (location == "TY")
                    location = "T";
                else if (location == "W")
                    location = "WM";
                else if (location == "WW")
                    location = "WM";
                else if (location == "WT")
                    location = "N";
                
                is2002 = dt.Rows[i]["is2002"].ObjToString();
                locInd = dt.Rows[i]["locInd"].ObjToString();
                riles = dt.Rows[i]["riles"].ObjToString();

                dt.Rows[i]["location"] = location;
                name = location;
                if (location == "F")
                    name = "Forest";/*
                else if (location == "TY")
                    name = "Capps/Tylertown FH";
                else*/
                    dRows = funDt.Select("keyCode = '" + location + "'");

                if (dRows.Length == 0)
                {
                    dRows = preDt.Select("locind = '" + locInd + "'");
                    if (dRows.Length > 0)
                    {
                        name = dRows[0]["name"].ObjToString();
                    }
                }
                else
                {
                    name = dRows[0]["locationCode"].ObjToString();
                }

                if (string.IsNullOrWhiteSpace(is2002))
                {
                    name += " Pre";
                    balance = dt.Rows[i]["endingBalance"].ObjToDouble();
                    interest = dt.Rows[i]["interest"].ObjToDouble();
                    removals = dt.Rows[i]["currentRemovals"].ObjToDouble();
                    if (removals == 0D)
                        dt.Rows[i]["endingBalance"] = balance + interest;
                }
                else
                {
                    name += " Post";
                }
                
                dt.Rows[i]["serviceLoc"] = name;
            }

            // An attempt at summing the totals in the cemetery tab dgv6
//            gridMain6.Columns["Contract Amount"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
//            gridMain6.Columns["Contract Amount"].SummaryItem.DisplayFormat = "";

            dRows = dt.Select("endingBalance > '0.00' and currentRemovals = '0.00'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();

            // HU Tab - dgv3 - gridMain3
            dRows = dt.Select("serviceLoc = 'Hartman Hughes Pre'");
            DataTable hudt = dt.Clone();
            if (dRows.Length > 0)
                hudt = dRows.CopyToDataTable();

            G1.NumberDataTable(hudt);
            dgv3.DataSource = hudt;

            // Remove Hartman Hughes Pre from Pre/Post/Riles
            dRows = dt.Select("serviceLoc <> 'Hartman Hughes Pre'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            DataView tempView = dt.DefaultView;
            tempView.Sort = "serviceLoc";
            dt = tempView.ToTable();

            // JPN Tab - dgv4 - gridMain4
            dRows = dt.Select("serviceLoc = 'Old Jones PN(Southland) Pre'");
            DataTable jpndt = dt.Clone();
            if (dRows.Length > 0)
                jpndt = dRows.CopyToDataTable();

            G1.NumberDataTable(jpndt);
            dgv4.DataSource = jpndt;

            // Remove Old Jones PN(Southland) Pre from Pre/Post/Riles
            dRows = dt.Select("serviceLoc <> 'Old Jones PN(Southland) Pre'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            DataView tempView2 = dt.DefaultView;
            tempView2.Sort = "serviceLoc";
            dt = tempView2.ToTable();

            // NMOC Tab - dgv5 - gridMain5
            dRows = dt.Select("serviceLoc = 'Newton Mem GRDN O/C Pre'");
            DataTable nmocdt = dt.Clone();
            if (dRows.Length > 0)
                nmocdt = dRows.CopyToDataTable();

            G1.NumberDataTable(nmocdt);
            dgv5.DataSource = nmocdt;

            // Remove Newton Mem GRDN O/C Pre from Pre/Post/Riles
            dRows = dt.Select("serviceLoc <> 'Newton Mem GRDN O/C Pre'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            DataView tempView3 = dt.DefaultView;
            tempView3.Sort = "serviceLoc";
            dt = tempView3.ToTable();

            // Cemeteries Tab - dgv6 - gridMain6
            //dRows = dt.Select("serviceLoc = 'Hillcrest Cemetery Post' or serviceLoc = 'Hillcrest Cemetery Pre' or serviceLoc = 'Newton Memorial Gardens Pre' or serviceLoc = 'Newton Memorial Gardens Post'");
            dRows = dt.Select("serviceLoc = 'Hillcrest Cemetery Pre' or serviceLoc = 'Newton Memorial Gardens Pre'");
            DataTable hcdt = dt.Clone();
            if (dRows.Length > 0)
                hcdt = dRows.CopyToDataTable();

            G1.NumberDataTable(hcdt);

            // Group the different cemeteries and total them.
            gridMain6.Columns["location"].GroupIndex = 0;
            gridMain6.OptionsView.ShowFooter = false;
            gridMain6.OptionsView.GroupFooterShowMode = DevExpress.XtraGrid.Views.Grid.GroupFooterShowMode.VisibleIfExpanded;
            gridMain6.ExpandAllGroups();

            dgv6.DataSource = hcdt;

            // Remove Hillcrest POST from Pre/Post/Riles
            dRows = dt.Select("serviceLoc <> 'Hillcrest Cemetery Post'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            DataView tempView4 = dt.DefaultView;
            tempView4.Sort = "serviceLoc";
            dt = tempView4.ToTable();

            // Remove Hillcrest PRE from Pre/Post/Riles
            dRows = dt.Select("serviceLoc <> 'Hillcrest Cemetery Pre'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            DataView tempView5 = dt.DefaultView;
            tempView5.Sort = "serviceLoc";
            dt = tempView5.ToTable();

            // Remove NMG Pre from Pre/Post/Riles
            dRows = dt.Select("serviceLoc <> 'Newton Memorial Gardens Pre'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            DataView tempView6 = dt.DefaultView;
            tempView6.Sort = "serviceLoc";
            dt = tempView6.ToTable();

            // Remove NMG Post from Pre/Post/Riles
            dRows = dt.Select("serviceLoc <> 'Newton Memorial Gardens Post'");
            if (dRows.Length > 0)
                dt = dRows.CopyToDataTable();
            DataView tempView7 = dt.DefaultView;
            tempView7.Sort = "serviceLoc";
            dt = tempView7.ToTable();


            return dt;
        }
        /***********************************************************************************************/
        private string getLocation(string location)
        {
            try
            {
                if (funDt == null)
                    funDt = G1.get_db_data("SELECT * FROM `funeralHomes`;");

                if (preDt == null)
                    preDt = G1.get_db_data("SELECT * FROM `pre2002`;");

                DataRow[] dRows = funDt.Select("keyCode = '" + location + "'");

                if (dRows.Length > 0)
                    location = dRows[0]["locationCode"].ObjToString();
                
            }
            catch (Exception ex)
            { 
            
            }

            return location;
        }
        /***********************************************************************************************/
        private DataTable processTheData ( DataTable dt )
        {
            /*------------------------------------------------------------------------------------------------*/
//            string tmstamp = "";
//            int record = 0;
            string contractNumber = "";
            string firstName = "";
            string lastName = "";
            string address2013 = "";
            string city2013 = "";
            string state2013 = "";
            string zip2013 = "";
            string ssn2013 = "";
            string payDate8 = "";
            double beginningBalance = 0D;
            double interest = 0D;
            double ytdPrevious = 0D;
            double paymentCurrMonth = 0D;
            double currentPayments = 0D;
            double deathRemYTDprevious = 0D;
            double refundRemYTDprevious = 0D;
            double refundRemCurrMonth = 0D;
            double currentRemovals = 0D;
            double endingBalance = 0D;
            string ServiceID = "";
            string Is2002 = "";
            string location = "";
            string filename = "";
            string riles = "";
            string locind = "";
            /*------------------------------------------------------------------------------------------------
            double downPayment = 0D;
            string contract = "";
            double contractAmount = 0D;
            string trust = "";
//            string location = "";
            string loc = "";
            string cmd = "";
            double contractValue = 0D;
            double allowMerchandise = 0D;
            double allowInsurance = 0D;
            double cashAdvance = 0D;
            bool rtn = false;
            DateTime downPaymentDate = DateTime.Now;
            double trust85_1 = 0D;
            double trust100_1 = 0D;
            double ccFee = 0D;
            string record2 = "";
            string agentCode = "";
            string depositNumber = "";


            /*------------------------------------------------------------------------------------------------*/
            DataRow[] dRows = null;

            try
            {
//                dt.Columns.Add("tmstamp");
//                dt.Columns.Add("record", Type.GetType("System.Int"));
//                dt.Columns.Add("contractNumber");
//                dt.Columns.Add("firstName");
//                dt.Columns.Add("lastName");
//                dt.Columns.Add("address2013");
//                dt.Columns.Add("city2013");
//                dt.Columns.Add("state2013");
//                dt.Columns.Add("zip2013");
//                dt.Columns.Add("ssn2013");
//                dt.Columns.Add("payDate8");
//                dt.Columns.Add("beginningBalance", Type.GetType("System.Double"));
//                dt.Columns.Add("interest", Type.GetType("System.Double"));
//                dt.Columns.Add("ytdPrevious", Type.GetType("System.Double"));
//                dt.Columns.Add("paymentCurrMonth", Type.GetType("System.Double"));
//                dt.Columns.Add("currentPayments", Type.GetType("System.Double"));
//                dt.Columns.Add("deathRemYTDprevious", Type.GetType("System.Double"));
//                dt.Columns.Add("refundRemYTDprevious", Type.GetType("System.Double"));
//                dt.Columns.Add("currentRemovals", Type.GetType("System.Double"));
//                dt.Columns.Add("endingBalance", Type.GetType("System.Double"));
//                dt.Columns.Add("ServiceID");
//                dt.Columns.Add("Is2002");
//                dt.Columns.Add("location");
//                dt.Columns.Add("filename");
//                dt.Columns.Add("riles");
//                dt.Columns.Add("locind");
            } 
            catch(Exception ex) 
            {
                MessageBox.Show(ex.Message);
            }
            string contract = "";
            string trust = "";
            string loc = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
//                tmstamp = dt.Rows[i]["tmstamp"].ObjToString();
//                record = dt.Rows[i]["record"].ObjToInt32();
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                dt.Rows[i]["location"] = loc;
            }

            /*------------------------------------------------------------------------------------------------
            try
            {
                dt.Columns.Add("name");
                dt.Columns.Add("location");
                dt.Columns.Add("loc");
                dt.Columns.Add("agent");
                dt.Columns.Add("contractNumber");
                dt.Columns.Add("depositNumber");
                dt.Columns.Add("lossRecovery");
                dt.Columns.Add("bookOrder");
                dt.Columns.Add("trust", Type.GetType("System.Double"));
                dt.Columns.Add("amount", Type.GetType("System.Double"));
                dt.Columns.Add("downpayment", Type.GetType("System.Double"));
                dt.Columns.Add("ccFee", Type.GetType("System.Double"));
                dt.Columns.Add("status");

                dt.Columns.Add("firstContract");
                dt.Columns.Add("lastContract");
                dt.Columns.Add("count");

                dt.Columns.Add("dataedited");
                dt.Columns.Add("trustType");

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    fName = dt.Rows[i]["firstName"].ObjToString();
                    lName = dt.Rows[i]["lastName"].ObjToString();

                    name = fName + " " + lName;
                    dt.Rows[i]["name"] = name;

                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                    if (!String.IsNullOrWhiteSpace(loc))
                    {
                        dt.Rows[i]["loc"] = loc;
                        dRows = funDt.Select("keycode='" + loc + "'");
                        if (dRows.Length > 0)
                            dt.Rows[i]["location"] = dRows[0]["locationCode"].ObjToString();
                        else
                            dt.Rows[i]["location"] = loc;
                    }

                    contractValue = DailyHistory.GetContractValue(dt.Rows[i]);
                    allowMerchandise = dt.Rows[i]["allowMerchandise"].ObjToDouble();
                    allowInsurance = dt.Rows[i]["allowInsurance"].ObjToDouble();
                    cashAdvance = dt.Rows[i]["cashAdvance"].ObjToDouble();
                    contractValue += allowMerchandise + allowInsurance + cashAdvance;
                    dt.Rows[i]["amount"] = contractValue;
                    contractAmount = contractValue - cashAdvance - allowInsurance;
                    dt.Rows[i]["trust"] = contractAmount;

                    rtn = DailyHistory.GetDownPaymentFromPayments(contractNumber, ref downPayment, ref downPaymentDate, ref trust85_1, ref trust100_1, ref ccFee, ref record2, ref depositNumber);
                    if (rtn)
                    {
                        dt.Rows[i]["downpayment"] = downPayment;
                        dt.Rows[i]["ccFee"] = ccFee;
                        dt.Rows[i]["depositNumber"] = depositNumber;
                        dt.Rows[i]["issueDate8"] = G1.DTtoMySQLDT(downPaymentDate.ToString("yyyy-MM-dd"));
                    }

                    dt.Rows[i]["trustType"] = "Trust";
                    if ( contractNumber == "E24803R" )
                    {
                    }

                    if (allowInsurance == contractValue)
                    {
                        dt.Rows[i]["status"] = "Y";
                        dt.Rows[i]["trustType"] = "Insurance";
                    }

                    if (contractNumber.ToUpper().IndexOf("NNM") == 0 || contractNumber.ToUpper().IndexOf("HC") == 0)
                        dt.Rows[i]["trustType"] = "Cemetery";

                    if (cashAdvance > 0D)
                    {
                        allowInsurance += cashAdvance;
                        dt.Rows[i]["allowInsurance"] = allowInsurance;
                    }

                    agentCode = dt.Rows[i]["agentCode"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(agentCode))
                    {
                        dt.Rows[i]["agent"] = agentCode;
                        dRows = agentDt.Select("agentCode='" + agentCode + "'");
                        if (dRows.Length > 0)
                            dt.Rows[i]["agent"] = dRows[0]["lastName"].ObjToString();
                    }
                }

                //if ( chkCemeteries.Checked )
                //    dt = Trust85.FilterForCemetery(dt);

                dt = ProcessDownPayments(dt);
                dt = ProcessACH(dt);
                dt = ProcessDBR(dt);
                dt = ProcessCustomData(dt);

                DataView tempview = dt.DefaultView;
                tempview.Sort = "location,contractNumber,issueDate8";
                dt = tempview.ToTable();

                BuildGroupSummary(dt);
            }
            catch (Exception ex)
            {
            }
            /***********************************************************************************************/
            
            return dt;
        }

        private DataTable ProcessCustomData ( DataTable dt )
        {
            DateTime date = dateTimePicker1.Value;
            DateTime saveDate1 = date;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            DateTime saveDate2 = date;
            string date2 = G1.DateTimeToSQLDateTime(date);

            string cmd = "Select * from `trust_log_data` WHERE `date` >= '" + date1 + "' AND `date` <= '" + date2 + "' ";
            cmd += " ORDER by `date`";
            cmd += ";";

            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return dt;

            dt.Columns.Add("aIssueDate");

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["issueDate8"].ObjToDateTime();
                date1 = date.ToString("yyyyMMdd");
                dt.Rows[i]["aIssueDate"] = date1;
            }

            DataRow[] dRows = null;
            string contractNumber = "";
            string column = "";
            string detail = "";

            for ( int i=0; i<dx.Rows.Count; i++)
            {
                date = dx.Rows[i]["date"].ObjToDateTime();
                date1 = date.ToString("yyyyMMdd");
                contractNumber = dx.Rows[i]["contractNumber"].ObjToString();
                column = dx.Rows[i]["what"].ObjToString();
                detail = dx.Rows[i]["detail"].ObjToString();
                dRows = dt.Select("contractNumber='" + contractNumber + "' AND `aIssueDate` = '" + date1 + "'");
                if (dRows.Length > 0)
                {
                    if (column.ToUpper() == "ALLOWINSURANCE")
                        dRows[0]["allowInsurance"] = detail.ObjToDouble();
                    else
                        dRows[0][column] = detail;
                    detail = dRows[0]["dataedited"].ObjToString();
                    detail += "," + column;
                    dRows[0]["dataedited"] = detail;

                }
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable ProcessACH ( DataTable dt, string trustNumber = "" )
        {
            string contractNumber = "";
            string cmd = "Select * from `ach`;";
            if (!String.IsNullOrWhiteSpace(trustNumber))
                cmd = "Select * from `ach` where `contractNumber` = '" + trustNumber + "';";
            DataTable dx = G1.get_db_data(cmd);

            DataRow[] dRows = null;

            if ( !String.IsNullOrWhiteSpace ( trustNumber ))
            {
                dRows = dx.Select("contractNumber='" + trustNumber + "'");
                if (dRows.Length > 0)
                {
                    dRows = dt.Select("contractNumber='" + trustNumber + "'");
                    if (dRows.Length > 0)
                    {
                        dRows[0]["bookOrder"] = "DRAFT";
                        dt.AcceptChanges();
                    }
                }
                return dt;
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                dRows = dx.Select("contractNumber='" + contractNumber + "'");
                if (dRows.Length > 0)
                    dt.Rows[i]["bookOrder"] = "DRAFT";
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable ProcessDBR (DataTable dt, string trustNumber = "" )
        {
            string contractNumber = "";
            string cmd = "";
            DataTable dx = null;
            DataRow[] dRows = null;
            string str = "";

            if ( !String.IsNullOrWhiteSpace ( trustNumber ))
            {
                cmd = "Select * from `dbrs` where contractNumber = '" + trustNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    dRows = dt.Select("contractNumber='" + trustNumber + "'");
                    if (dRows.Length > 0)
                    {
                        str = dRows[0]["bookOrder"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(str))
                            str += " / DBR";
                        else
                            str = "DBR";
                        dRows[0]["bookOrder"] = str;
                        dt.AcceptChanges();
                    }
                }
                return dt;
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                cmd = "Select * from `dbrs` where contractNumber = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    str = dt.Rows[i]["bookOrder"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(str))
                        str += " / DBR";
                    else
                        str = "DBR";
                    dt.Rows[i]["bookOrder"] = str;
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable ProcessDownPayments ( DataTable dt )
        {
            DateTime date = dateTimePicker1.Value;
            date = date.AddMonths(-1);
            DateTime saveDate1 = date;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            date = date.AddMonths(1);
            DateTime saveDate2 = date;
            string date2 = G1.DateTimeToSQLDateTime(date);

            try
            {
                string cmd = "Select * from `downpayments` WHERE `date` >= '" + date1 + "' AND `date` <= '" + date2 + "' ";
                cmd += " ORDER by `date` ";
                cmd += ";";

                DataTable dx = G1.get_db_data(cmd);

                dx.Columns.Add("newDate");
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    date = dx.Rows[i]["date"].ObjToDateTime();
                    dx.Rows[i]["newDate"] = date.ToString("yyyyMMdd");
                }
                DataRow[] dRows = null;
                string contractNumber = "";
                string depNumber = "";
                string lName = "";
                string fName = "";
                DateTime depDate = DateTime.Now;
                double oldDownPayment = 0D;
                double downPayment = 0D;
                double ccFee = 0D;
                double lossRecoveryFee = 0D;
                double totalDownPayment = 0D;
                DataTable tempDt = null;
                string trust = "";
                string loc = "";
                string location = "";

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                        if (contractNumber == "CT24042LI")
                        {
                        }

                        Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                        location = loc;
                        if (!String.IsNullOrWhiteSpace(loc))
                        {
                            dRows = funDt.Select("keycode='" + loc + "'");
                            if (dRows.Length > 0)
                                location = dRows[0]["LocationCode"].ObjToString();
                        }

                        depNumber = dt.Rows[i]["depositNumber"].ObjToString();
                        lName = dt.Rows[i]["lastName"].ObjToString();
                        fName = dt.Rows[i]["firstName"].ObjToString();
                        oldDownPayment = dt.Rows[i]["downPayment"].ObjToDouble();
                        //if (lName.Trim().ToUpper() != "BLACKLEDGE")
                        //    continue;
                        depDate = dt.Rows[i]["issueDate8"].ObjToDateTime();

                        if ( !String.IsNullOrWhiteSpace ( location ))
                            dRows = dx.Select("lastName='" + lName + "' AND `depositNumber` = '" + depNumber + "' AND `newDate` = '" + depDate.ToString("yyyyMMdd") + "' and `location` = '" + location + "'");
                        else
                            dRows = dx.Select("lastName='" + lName + "' AND `depositNumber` = '" + depNumber + "' AND `newDate` = '" + depDate.ToString("yyyyMMdd") + "'");
                        if ( dRows.Length > 1 )
                        {
                            tempDt = dRows.CopyToDataTable();
                            dRows = dx.Select("firstName='" + fName + "' AND lastName='" + lName + "' AND `depositNumber` = '" + depNumber + "' AND `newDate` = '" + depDate.ToString("yyyyMMdd") + "'");
                        }
                        if ( dRows.Length <= 0 )
                            dRows = dx.Select("firstName='" + fName + "' AND `depositNumber` = '" + depNumber + "' AND `newDate` = '" + depDate.ToString("yyyyMMdd") + "'");
                        if (dRows.Length > 0)
                        {
                            totalDownPayment = 0D;
                            downPayment = 0D;
                            lossRecoveryFee = 0D;
                            ccFee = 0D;
                            for (int j = 0; j < dRows.Length; j++)
                            {
                                downPayment += dRows[j]["downPayment"].ObjToDouble();
                                lossRecoveryFee += dRows[j]["lossRecoveryFee"].ObjToDouble();
                                ccFee += dRows[j]["ccFee"].ObjToDouble();
                            }
                            if (downPayment < oldDownPayment)
                                downPayment = oldDownPayment;
                            totalDownPayment += downPayment + lossRecoveryFee;
                            totalDownPayment = G1.RoundValue(totalDownPayment);
                            dt.Rows[i]["downpayment"] = totalDownPayment;
                            dt.Rows[i]["ccFee"] = ccFee;
                            if ( dRows.Length == 1 && lossRecoveryFee == 0D )
                            {
                                if (!String.IsNullOrWhiteSpace(location))
                                {
                                    if ( location.ToUpper() == "FLOWOOD" || location.ToUpper() == "CLINTON" )
                                        dRows = dx.Select("lastName='" + lName + "' AND `depositNumber` <> '" + depNumber + "' AND `newDate` = '" + depDate.ToString("yyyyMMdd") + "' and `location` LIKE '%" + location + "%'");
                                    else
                                        dRows = dx.Select("lastName='" + lName + "' AND `depositNumber` <> '" + depNumber + "' AND `newDate` = '" + depDate.ToString("yyyyMMdd") + "' and `location` = '" + location + "'");
                                }
                                else
                                    dRows = dx.Select("lastName='" + lName + "' AND `depositNumber` <> '" + depNumber + "' AND `newDate` = '" + depDate.ToString("yyyyMMdd") + "'");
                                if ( dRows.Length > 0 )
                                {
                                    for ( int j=0; j<dRows.Length; j++)
                                    {
                                        downPayment = dRows[j]["downPayment"].ObjToDouble();
                                        lossRecoveryFee = dRows[j]["lossRecoveryFee"].ObjToDouble();
                                        if ( downPayment == 0D && lossRecoveryFee > 0D )
                                        {
                                            dt.Rows[i]["downpayment"] = totalDownPayment + lossRecoveryFee;
                                            break;
                                        }
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
            catch ( Exception ex)
            {
            }
            return dt;
        }
        /***********************************************************************************************/
        private void BuildGroupSummary ( DataTable dt )
        {
            if (summaryDt != null)
                summaryDt.Rows.Clear();
            else
            {
                summaryDt = new DataTable();
                summaryDt.Columns.Add("location");
                summaryDt.Columns.Add("firstContract");
                summaryDt.Columns.Add("lastContract");
                summaryDt.Columns.Add("count", Type.GetType("System.Int32"));

                summaryDt.Columns.Add("firstIns");
                summaryDt.Columns.Add("lastIns");
                summaryDt.Columns.Add("countIns", Type.GetType("System.Int32"));

                summaryDt.Columns.Add("firstCem");
                summaryDt.Columns.Add("lastCem");
                summaryDt.Columns.Add("countCem", Type.GetType("System.Int32"));
            }

            DataRow dRow = null;
            string location = "";
            string lastLocation = "";
            string contractNumber = "";

            summaryFirstContract = "";
            summaryLastContract = "";
            int count = 0;

            summaryFirstIns = "";
            summaryLastIns = "";
            summaryInsCount = 0;

            summaryFirstCem = "";
            summaryLastCem = "";
            summaryCemCount = 0;

            string trustType = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                location = dt.Rows[i]["location"].ObjToString();
                if (String.IsNullOrWhiteSpace(location))
                    location = "XXXX";
                trustType = dt.Rows[i]["trustType"].ObjToString().ToUpper();
                if (trustType != "TRUST")
                    continue;
                if (location != lastLocation)
                {
                    if (String.IsNullOrWhiteSpace(lastLocation))
                    {
                        contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                        summaryFirstContract = dt.Rows[i]["contractNumber"].ObjToString();
                        summaryLastContract = summaryFirstContract;
                        count = 1;
                        lastLocation = location;
                        continue;
                    }
                    dRow = summaryDt.NewRow();
                    dRow["location"] = lastLocation;
                    dRow["firstContract"] = summaryFirstContract;
                    dRow["lastContract"] = summaryLastContract;
                    dRow["count"] = count;
                    summaryDt.Rows.Add(dRow);

                    lastLocation = location;
                    summaryFirstContract = dt.Rows[i]["contractNumber"].ObjToString();
                    summaryLastContract = summaryFirstContract;
                    count = 1;
                    continue;
                }
                summaryLastContract = dt.Rows[i]["contractNumber"].ObjToString();
                count++;
            }
            dRow = summaryDt.NewRow();
            dRow["location"] = lastLocation;
            dRow["firstContract"] = summaryFirstContract;
            dRow["lastContract"] = summaryLastContract;
            dRow["count"] = count;
            summaryDt.Rows.Add(dRow);
            summaryDt.AcceptChanges();

            BuildInsuranceGroupSummary(dt);
            BuildCemeteryGroupSummary(dt);
        }
        /***********************************************************************************************/
        private void BuildInsuranceGroupSummary ( DataTable dx )
        {
            if (summaryDt == null)
                return;

            DataRow dRow = null;
            string location = "";
            string lastLocation = "";
            string contractNumber = "";

            summaryFirstContract = "";
            summaryLastContract = "";
            int count = 0;

            summaryFirstIns = "";
            summaryLastIns = "";
            summaryInsCount = 0;

            summaryFirstCem = "";
            summaryLastCem = "";
            summaryCemCount = 0;

            DataRow[] dRows = dx.Select("trustType='Insurance'");
            if (dRows.Length <= 0)
                return;

            DataTable dt = dRows.CopyToDataTable();

            string trustType = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                location = dt.Rows[i]["location"].ObjToString();
                if (String.IsNullOrWhiteSpace(location))
                    location = "XXXX";
                trustType = dt.Rows[i]["trustType"].ObjToString().ToUpper();
                if (trustType != "INSURANCE")
                    continue;
                if (location != lastLocation)
                {
                    if (String.IsNullOrWhiteSpace(lastLocation))
                    {
                        contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                        summaryFirstContract = dt.Rows[i]["contractNumber"].ObjToString();
                        summaryLastContract = summaryFirstContract;
                        count = 1;
                        lastLocation = location;
                        continue;
                    }
                    dRows = summaryDt.Select("location='" + lastLocation + "'");
                    if (dRows.Length <= 0)
                    {
                        dRow = summaryDt.NewRow();
                        dRow["location"] = lastLocation;
                        dRow["firstIns"] = summaryFirstContract;
                        dRow["lastIns"] = summaryLastContract;
                        dRow["countIns"] = count;
                        summaryDt.Rows.Add(dRow);
                    }
                    else
                    {
                        dRows[0]["firstIns"] = summaryFirstContract;
                        dRows[0]["lastIns"] = summaryLastContract;
                        dRows[0]["countIns"] = count;
                    }

                    lastLocation = location;
                    summaryFirstContract = dt.Rows[i]["contractNumber"].ObjToString();
                    summaryLastContract = summaryFirstContract;
                    count = 1;
                    continue;
                }
                summaryLastContract = dt.Rows[i]["contractNumber"].ObjToString();
                count++;
            }
            dRows = summaryDt.Select("location='" + lastLocation + "'");
            if (dRows.Length <= 0)
            {
                dRow = summaryDt.NewRow();
                dRow["location"] = lastLocation;
                dRow["firstIns"] = summaryFirstContract;
                dRow["lastIns"] = summaryLastContract;
                dRow["countIns"] = count;
                summaryDt.Rows.Add(dRow);
            }
            else
            {
                dRows[0]["firstIns"] = summaryFirstContract;
                dRows[0]["lastIns"] = summaryLastContract;
                dRows[0]["countIns"] = count;
            }
        }
        /***********************************************************************************************/
        private void BuildCemeteryGroupSummary(DataTable dx)
        {
            if (summaryDt == null)
                return;

            DataRow dRow = null;
            string location = "";
            string lastLocation = "";
            string contractNumber = "";

            summaryFirstContract = "";
            summaryLastContract = "";
            int count = 0;

            summaryFirstIns = "";
            summaryLastIns = "";
            summaryInsCount = 0;

            summaryFirstCem = "";
            summaryLastCem = "";
            summaryCemCount = 0;

            DataRow[] dRows = dx.Select("trustType='Cemetery'");
            if (dRows.Length <= 0)
                return;

            DataTable dt = dRows.CopyToDataTable();

            string trustType = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                location = dt.Rows[i]["location"].ObjToString();
                if (String.IsNullOrWhiteSpace(location))
                    location = "XXXX";
                trustType = dt.Rows[i]["trustType"].ObjToString().ToUpper();
                if (trustType != "CEMETERY")
                    continue;
                if (location != lastLocation)
                {
                    if (String.IsNullOrWhiteSpace(lastLocation))
                    {
                        contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                        summaryFirstContract = dt.Rows[i]["contractNumber"].ObjToString();
                        summaryLastContract = summaryFirstContract;
                        count = 1;
                        lastLocation = location;
                        continue;
                    }
                    dRows = summaryDt.Select("location='" + lastLocation + "'");
                    if (dRows.Length <= 0)
                    {
                        dRow = summaryDt.NewRow();
                        dRow["location"] = lastLocation;
                        dRow["firstCem"] = summaryFirstContract;
                        dRow["lastCem"] = summaryLastContract;
                        dRow["countCem"] = count;
                        summaryDt.Rows.Add(dRow);
                    }
                    else
                    {
                        dRows[0]["firstCem"] = summaryFirstContract;
                        dRows[0]["lastCem"] = summaryLastContract;
                        dRows[0]["countCem"] = count;
                    }

                    lastLocation = location;
                    summaryFirstContract = dt.Rows[i]["contractNumber"].ObjToString();
                    summaryLastContract = summaryFirstContract;
                    count = 1;
                    continue;
                }
                summaryLastContract = dt.Rows[i]["contractNumber"].ObjToString();
                count++;
            }
            dRows = summaryDt.Select("location='" + lastLocation + "'");
            if (dRows.Length <= 0)
            {
                dRow = summaryDt.NewRow();
                dRow["location"] = lastLocation;
                dRow["firstCem"] = summaryFirstContract;
                dRow["lastCem"] = summaryLastContract;
                dRow["countCem"] = count;
                summaryDt.Rows.Add(dRow);
            }
            else
            {
                dRows[0]["firstCem"] = summaryFirstContract;
                dRows[0]["lastCem"] = summaryLastContract;
                dRows[0]["countCem"] = count;
            }
        }
        /***********************************************************************************************/
        private void chkComboLocNames_EditValueChanged(object sender, EventArgs e)
        {
            if (originalDt == null)
                return;

            string names = getLocationNameQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            dgv.Refresh();
            gridMain.RefreshEditor(true);
            gridMain.ExpandAllGroups();
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
        private void chkPageBreaks_CheckedChanged(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private int footerCount = 0;
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            footerCount++;
            if (footerCount >= 1)
            {
                footerCount = 0;
//                AddFooter(e);
                //if (chkSort.Checked || autoRun)
                //    e.PS.InsertPageBreak(e.Y);
            }
            if (pageBreak)
            {
                e.PS.InsertPageBreak(e.Y);
            }
            pageBreak = false;
        }
        /***********************************************************************************************/
        private bool pageBreak = false;
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            int rowHandle = e.RowHandle;
            if (e.HasFooter )
            {
                if (chkPageBreaks.Checked)
                    pageBreak = true;
            }
        }
        /***********************************************************************************************/
        private DataTable summaryDt = null;
        private string summaryLocation = "";
        private string summaryFirstContract = "";
        private string summaryLastContract = "";

        private string summaryFirstIns = "";
        private string summaryLastIns = "";
        private int summaryInsCount = 0;

        private string summaryFirstCem = "";
        private string summaryLastCem = "";
        private int summaryCemCount = 0;
        /***********************************************************************************************/
        private string location = "";
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            GridView view = sender as GridView;
            int groupRowHandle = e.GroupRowHandle;
            groupRowHandle = e.RowHandle;

            string field = (e.Item as GridSummaryItem).FieldName.ObjToString();

            int index = e.GroupLevel;

            if (e.SummaryProcess == CustomSummaryProcess.Start)
            {
            }
            else if ( e.SummaryProcess == CustomSummaryProcess.Calculate )
            {
                location = gridMain.GetRowCellValue(e.RowHandle, "location").ObjToString();
            }
            else if (e.SummaryProcess == CustomSummaryProcess.Finalize)
            {
                object value = CalculateTotal ( field, location );
                e.TotalValue = value;
            }
        }
        /***********************************************************************************************/
        private object CalculateTotal( string field, string loc )
        {
            string sum = "";

            if (summaryDt == null)
                return sum;
            if ( String.IsNullOrWhiteSpace ( loc ))
                return sum;

            try
            {
                DataRow[] dRows = summaryDt.Select("location='" + loc + "'");
                if (dRows.Length > 0)
                {
                    /*
                    string sumType = cmbType.Text.Trim();
                    if (sumType == "Trust")
                    {
                        if (field.ToUpper() == "AMOUNT")
                        {
                            string contract = dRows[0]["firstContract"].ObjToString() + " ";
                            sum = contract;
                        }
                        if (field.ToUpper() == "DOWNPAYMENT")
                        {
                            string contract = dRows[0]["lastContract"].ObjToString() + " ";
                            sum = contract;
                        }
                        if (field.ToUpper() == "TRUST")
                        {
                            string contract = "Total" + " " + dRows[0]["count"].ObjToString() + " ";
                            sum = contract;
                        }
                    }
                    else if (sumType == "Insurance")
                    {
                        if (field.ToUpper() == "AMOUNT")
                        {
                            string contract = dRows[0]["firstIns"].ObjToString() + " ";
                            sum = contract;
                        }
                        if (field.ToUpper() == "DOWNPAYMENT")
                        {
                            string contract = dRows[0]["lastIns"].ObjToString() + " ";
                            sum = contract;
                        }
                        if (field.ToUpper() == "TRUST")
                        {
                            string contract = "Total" + " " + dRows[0]["countIns"].ObjToString() + " ";
                            sum = contract;
                        }
                    }
                    else if (sumType == "Cemetery")
                    {
                        if (field.ToUpper() == "AMOUNT")
                        {
                            string contract = dRows[0]["firstCem"].ObjToString() + " ";
                            sum = contract;
                        }
                        if (field.ToUpper() == "DOWNPAYMENT")
                        {
                            string contract = dRows[0]["lastCem"].ObjToString() + " ";
                            sum = contract;
                        }
                        if (field.ToUpper() == "TRUST")
                        {
                            string contract = "Total" + " " + dRows[0]["countCem"].ObjToString() + " ";
                            sum = contract;
                        }
                    }
                    */
                }
            }
            catch ( Exception ex)
            {
            }
            return sum;
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            string record = dr["record"].ObjToString();
            if (record == "0")
                record = "";

            DateTime date = dr["issueDate8"].ObjToDateTime();
            string sDate = date.ToString("yyyyMMdd");
            string contractNumber = dr["contractNumber"].ObjToString();
            string column = gridMain.FocusedColumn.FieldName;
            string detail = dr[column].ObjToString();

            string cmd = "Select * from `trust_log_data` WHERE `contractNumber` = '" + contractNumber + "' AND `date` = '" + sDate + "' AND `what` = '" + column + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                record = G1.create_record("trust_log_data", "what", column);
            else
                record = dx.Rows[0]["record"].ObjToString();
            if (G1.BadRecord("trust_log_data", record))
                return;
            G1.update_db_table("trust_log_data", "record", record, new string[] { "contractNumber", contractNumber, "date", date.ToString("yyyy-MM-dd"), "what", column, "detail", detail });

            if ( column.ToUpper() == "ALLOWINSURANCE")
            {
                cmd = "Select * from contracts where contractNumber = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    double contractValue = DailyHistory.GetContractValue(dx.Rows[0]);
                    double allowMerchandise = dx.Rows[0]["allowMerchandise"].ObjToDouble();
                    double allowInsurance = dx.Rows[0]["allowInsurance"].ObjToDouble();
                    double cashAdvance = dx.Rows[0]["cashAdvance"].ObjToDouble();
                    contractValue += allowMerchandise + allowInsurance + cashAdvance;
                    dr["amount"] = contractValue;
                    allowInsurance = detail.ObjToDouble();
                    double contractAmount = contractValue - cashAdvance - allowInsurance;
                    dr["trust"] = contractAmount;

                    if (allowInsurance == contractValue)
                        dr["status"] = "Y";

                    if (cashAdvance > 0D)
                    {
                        allowInsurance += cashAdvance;
                        dr["allowInsurance"] = allowInsurance;
                    }
                    else
                        dr["allowInsurance"] = allowInsurance;
                }
            }

            detail = dr["dataedited"].ObjToString();
            detail += "," + column;
            dr["dataedited"] = detail;

            gridMain.PostEditor();
            gridMain.UpdateTotalSummary();
        }
        /***********************************************************************************************/
        private void gridMain_RowCellStyle(object sender, RowCellStyleEventArgs e )
        {
            GridView View = sender as GridView;
            if (e.Column.FieldName.ToUpper() == "ALLOWINSURANCE")
            {
                string str = View.GetRowCellValue(e.RowHandle, "dataedited").ObjToString();
                if (str != null)
                {
                    if (str.ToUpper().Contains("ALLOWINSURANCE"))
                        e.Appearance.BackColor = Color.LightPink;
                }
            }
        }
        /***********************************************************************************************/
        private void clearInsuranceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            DateTime date = dr["issueDate8"].ObjToDateTime();
            string sDate = date.ToString("yyyyMMdd");
            string contractNumber = dr["contractNumber"].ObjToString();
            string column = gridMain.FocusedColumn.FieldName;
            string detail = dr[column].ObjToString();
            string editData = dr["dataedited"].ObjToString();

            string cmd = "Select * from `trust_log_data` WHERE `contractNumber` = '" + contractNumber + "' AND `date` = '" + sDate + "' AND `what` = 'allowInsurance';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                string record = dx.Rows[0]["record"].ObjToString();
                G1.delete_db_table("trust_log_data", "record", record);

                editData = editData.Replace("allowInsurance", "");
                dr["dataedited"] = editData;

                cmd = "Select * from contracts where contractNumber = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count > 0 )
                {
                    double contractValue = DailyHistory.GetContractValue(dx.Rows[0]);
                    double allowMerchandise = dx.Rows[0]["allowMerchandise"].ObjToDouble();
                    double allowInsurance = dx.Rows[0]["allowInsurance"].ObjToDouble();
                    double cashAdvance = dx.Rows[0]["cashAdvance"].ObjToDouble();
                    contractValue += allowMerchandise + allowInsurance + cashAdvance;
                    dr["amount"] = contractValue;
                    double contractAmount = contractValue - cashAdvance - allowInsurance;
                    dr["trust"] = contractAmount;

                    if (allowInsurance == contractValue)
                        dr["status"] = "Y";

                    if (cashAdvance > 0D)
                    {
                        allowInsurance += cashAdvance;
                        dr["allowInsurance"] = allowInsurance;
                    }
                    else
                        dr["allowInsurance"] = allowInsurance;
                    dt.AcceptChanges();

                }


                gridMain.PostEditor();
                gridMain.UpdateTotalSummary();
                gridMain.RefreshEditor(true);
                dgv.Refresh();
            }
        }
        /***********************************************************************************************/
        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            gridMain.PostEditor();
            gridMain.UpdateTotalSummary();
            gridMain.RefreshEditor(true);
            dgv.RefreshDataSource();
            dgv.Refresh();
            gridMain.ExpandAllGroups();
        }
        /***********************************************************************************************/
        private void clearLossRecoveryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            DateTime date = dr["issueDate8"].ObjToDateTime();
            string sDate = date.ToString("yyyyMMdd");
            string contractNumber = dr["contractNumber"].ObjToString();
            string column = gridMain.FocusedColumn.FieldName;
            string detail = dr[column].ObjToString();
            string editData = dr["dataedited"].ObjToString();

            string cmd = "Select * from `trust_log_data` WHERE `contractNumber` = '" + contractNumber + "' AND `date` = '" + sDate + "' AND `what` = 'lossRecovery';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                string record = dx.Rows[0]["record"].ObjToString();
                G1.delete_db_table("trust_log_data", "record", record);

                dr["lossRecovery"] = "";
                dt.AcceptChanges();

                gridMain.PostEditor();
                gridMain.UpdateTotalSummary();
                gridMain.RefreshEditor(true);
                dgv.Refresh();
            }
        }
        /***********************************************************************************************/
        private void clearBooksOrderedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            DateTime date = dr["issueDate8"].ObjToDateTime();
            string sDate = date.ToString("yyyyMMdd");
            string contractNumber = dr["contractNumber"].ObjToString();
            string column = gridMain.FocusedColumn.FieldName;
            string detail = dr[column].ObjToString();
            string editData = dr["dataedited"].ObjToString();

            string cmd = "Select * from `trust_log_data` WHERE `contractNumber` = '" + contractNumber + "' AND `date` = '" + sDate + "' AND `what` = 'bookOrder';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                string record = dx.Rows[0]["record"].ObjToString();
                G1.delete_db_table("trust_log_data", "record", record);

                dr["bookOrder"] = "";
                dt.AcceptChanges();

                ProcessACH ( dt, contractNumber );
                ProcessDBR( dt, contractNumber );

                gridMain.PostEditor();
                gridMain.UpdateTotalSummary();
                gridMain.RefreshEditor(true);
                dgv.Refresh();
            }
        }
        /***********************************************************************************************/
        private void btnEditDownPayments_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            DateTime date = dr["issueDate8"].ObjToDateTime();

            string firstName = dr["firstName"].ObjToString();
            string lastName = dr["lastName"].ObjToString();
            string contractNumber = dr["contractNumber"].ObjToString();

            DownPayments downForm = new DownPayments(contractNumber, lastName, firstName, date, "Edit Deposits");
            downForm.Show();
        }
        /***********************************************************************************************/
    }
}