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
using DevExpress.XtraGrid.Views.Base;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class WeeklyClose : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        DataTable originalDt = null;
        private bool loading = false;
        private string workTitle = "";
        public static DataTable balanceDt = null;
        private DateTime workStart;
        private DateTime workStop;
        private bool workingByDate = false;
        private string workWhat = "";
        private bool previousDateRead = false;
        /***********************************************************************************************/
        public WeeklyClose( string title = "")
        {
            InitializeComponent();
            workTitle = title;
        }
        /***********************************************************************************************/
        public WeeklyClose(DateTime start, DateTime stop, string what = "", string title = "" )
        {
            InitializeComponent();
            workTitle = title;
            workStart = start;
            workStop = stop;
            workWhat = what;
            workingByDate = true;
        }
        /***********************************************************************************************/
        private void WeeklyClose_Load(object sender, EventArgs e)
        {

            this.dateTimePicker3.Visible = false;
            this.dateTimePicker4.Visible = false;
            lblAllOther.Hide();
            lblAllOtherTo.Hide();
            chkACH.Hide();
            chkACH1.Hide();
            btnSave.Hide();
            btnSave.Refresh();

            barImport.Hide();
            barImport.Refresh();

            chkShowCredits.Hide();

            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker3.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker4.Value = new DateTime(now.Year, now.Month, days);
            DateTime start = now.AddDays(-1);
            DateTime stop = new DateTime(now.Year, now.Month, days - 1);
            this.dateTimePicker1.Value = start;
            this.dateTimePicker2.Value = stop;

            if ( workingByDate )
            {
                this.dateTimePicker1.Value = workStart;
                this.dateTimePicker2.Value = workStop;
            }


            gridMain.Columns["fill"].Visible = false;
            G1.SetupVisibleColumns(gridMain, this.columnsToolStripMenuItem, nmenu_Click);
            SetupTotalsSummary();
            LoadData();
            gridMain.Columns["downPayment"].Visible = false;
            gridMain.Columns["dpp"].Visible = false;

            this.Text = workTitle;
            if ( workTitle.ToUpper().IndexOf ( "WEEKLY") >= 0 )
            {
                cmbDateType.SelectedIndex = 1;
            }

            //            this.dateTimePicker2.Value = DateTime.Now;

            this.dateTimePicker3.Visible = false;
            this.dateTimePicker4.Visible = false;
            lblAllOther.Hide();
            lblAllOtherTo.Hide();
            chkACH.Hide();
            chkACH1.Hide();
            cmbDateType.Hide();

            if (workingByDate)
                btnRun_Click(null, null);
        }
        /****************************************************************************************/
        private void LoadData()
        {
            string cmd = "Select * from `payments` where `contractNumber` = 'XYZZYAAA';";
            DataTable dt = G1.get_db_data(cmd);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            loadLocatons();
            ScaleCells();
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
            AddSummaryColumn("ccFee", gridMain);
            AddSummaryColumn("ap", gridMain);
            AddSummaryColumn("dpp", gridMain);
            AddSummaryColumn("debitAdjustment", gridMain);
            AddSummaryColumn("creditAdjustment", gridMain);
            AddSummaryColumn("interestPaid", gridMain);
            AddSummaryColumn("retained", gridMain);
            AddSummaryColumn("LiInterest", gridMain);
            AddSummaryColumn("downPayment", gridMain);
            AddSummaryColumn("trust100P", gridMain);
            AddSummaryColumn("trust85P", gridMain);
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

            Printer.setupPrinterMargins(10, 10, 130, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printableComponentLink1.CreateDocument();
            //printableComponentLink1.ShowPreview();

            if (continuousPrint)
            {
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
                if (fullPath.ToUpper().IndexOf(".PDF") > 0)
                    printableComponentLink1.ExportToPdf(fullPath);
                else
                    printableComponentLink1.ExportToCsv(fullPath);
            }
            else
                printableComponentLink1.ShowPreviewDialog();

            G1.AdjustColumnWidths(gridMain, 0.65D, false);

            if ( !continuousPrint )
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

            Printer.setupPrinterMargins(10, 10, 130, 50);

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
            string title = "Trust Payment Weekly Balance Sheet";
            if ( cmbDateType.Text.ToUpper() == "MONTHLY")
                title = "Trust Payment Monthly Balance Sheet";
            Printer.DrawQuad(6, 7, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            DateTime date = this.dateTimePicker2.Value;
            if ( cmbDateType.Text.ToUpper() == "WEEKLY")
                date = date.AddDays(1);
            string workDate = date.ToString("MM/dd/yyyy");
            Printer.SetQuadSize(24, 12);
//            font = new Font("Ariel", 9, FontStyle.Bold);
            font = new Font("Ariel", 9, FontStyle.Regular);
            title = "Week Ending: ";
            if (cmbDateType.Text.ToUpper() == "MONTHLY")
                title = "Month Ending: ";
            string lock1 = this.dateTimePicker1.Value.ToString("MM/dd/yyyy");
            string lock2 = this.dateTimePicker2.Value.ToString("MM/dd/yyyy");
            string ach1 = this.dateTimePicker3.Value.ToString("MM/dd/yyyy");
            string ach2 = this.dateTimePicker4.Value.ToString("MM/dd/yyyy");
            if (chkACH.Checked)
            {
                Printer.DrawQuad(20, 7, 5, 4, "ACH  Stop  " + ach2, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
                Printer.DrawQuad(20, 5, 5, 4, "ACH  Start  " + ach1, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            }
            Printer.DrawQuad(20, 3, 5, 4, "LKBX Stop " + lock2, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(20, 1, 5, 4, "LKBX Start " + lock1, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


//            Printer.DrawQuad(20, 8, 5, 4, title + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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
            G1.ShowHideFindPanel(grid);

            //if (grid.OptionsFind.AlwaysVisible == true)
            //    grid.OptionsFind.AlwaysVisible = false;
            //else
            //    grid.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            barImport.Hide();
            barImport.Refresh();

            btnSave.Hide();
            btnSave.Refresh();

            menuStrip1.BackColor = panelTop.BackColor;
            menuStrip1.Refresh();

            DateTime date = dateTimePicker1.Value;
            DateTime beginningDate = date;
            DateTime saveDate1 = date;
            DateTime oldImportDate = DailyHistory.majorDate;
            oldImportDate = new DateTime(2019, 11, 1);

            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            DateTime saveDate2 = date;

            string date2 = G1.DateTimeToSQLDateTime(date);

            dateTimePicker3.Value = dateTimePicker1.Value;
            dateTimePicker4.Value = dateTimePicker2.Value;

            date = dateTimePicker3.Value;
            string date3 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker4.Value;
            string date4 = G1.DateTimeToSQLDateTime(date);

            DateTime now = DateTime.Now;

            DateTime paidout = new DateTime(2039, 12, 31);

            //DateTime date = dateTimePicker1.Value;
            //string date1 = G1.DateTimeToSQLDateTime(date);
            //date = dateTimePicker2.Value;
            //string date2 = G1.DateTimeToSQLDateTime(date);

            string paidDate = "`payDate8` >= 'XYZZY1' and `payDate8` <= 'XYZZY2' ";

            string cmd = "Select p.*,d.*,a.`firstName`,a.`lastName` from `payments` p LEFT JOIN `contracts` d on p.`contractNumber` = d.`contractNumber` LEFT JOIN `agents` a ON p.`agentNumber` = a.`agentCode` ";
            //            cmd += " LEFT JOIN `customers` c ON p.`contractNumber` = c.`contractNumber` ";
            cmd += " WHERE ";
            cmd += paidDate;
            string saveDate = cmd;
            if (cmbDateType.Text.ToUpper() == "MONTHLY" || cmbDateType.Text.ToUpper() == "WEEKLY")
            {
                if (beginningDate >= oldImportDate)
                {
                    if (!chkACH.Checked || chkACH1.Checked)
                    {
                        cmd += " AND (`depositNumber` LIKE 'T%' OR `depositNumber` LIKE 'CCTD%' OR  depositNumber` = '' OR `depositNumber` LIKE 'A%') ";
                    }
                    else
                        cmd += " AND (`depositNumber` LIKE 'T%' OR `depositNumber` LIKE 'CCTD%' OR `depositNumber` = '') ";
                }
                cmd += " AND ( `edited` <> 'MANUAL' AND `edited` <> 'TRUSTADJ' ) ";
            }

            string contractNumber = txtContract.Text.Trim();
            if (!String.IsNullOrWhiteSpace(contractNumber))
                cmd += " AND p.`contractNumber` = '" + contractNumber + "' ";
            cmd += ";";

            string saveCmd = cmd;
            cmd = cmd.Replace("XYZZY1", date1);
            cmd = cmd.Replace("XYZZY2", date2);

            DataTable dt = G1.get_db_data(cmd);
            //Trust85.FindContract(dt, "B22021LI");

            dt.Columns.Add("paymentType");

            DailyHistory.RemoveDeletedPayments(dt);

            DateTime payDate8 = DateTime.Now;

            double debit = 0D;
            double credit = 0D;
            if (beginningDate < oldImportDate)
            { // Force out Debits and Credits coming from the AS400
                payDate8 = DateTime.Now;
                for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                {
                    debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    if (debit != 0D || credit != 0D)
                    {
                        payDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();
                        if (chkACH.Checked)
                        {
                            if (payDate8 < this.dateTimePicker3.Value || payDate8 > this.dateTimePicker4.Value)
                                dt.Rows.RemoveAt(i);
                        }
                        if (chkACH1.Checked)
                        {
                            if (payDate8 < this.dateTimePicker1.Value || payDate8 > this.dateTimePicker2.Value)
                                dt.Rows.RemoveAt(i);
                        }
                        if (chkACH1.Checked)
                        {
                            cmd = cmd.Replace("XYZZY1", date3);
                            cmd = cmd.Replace("XYZZY2", date4);
                        }
                        else
                        {
                            cmd = cmd.Replace("XYZZY1", date1);
                            cmd = cmd.Replace("XYZZY2", date2);
                        }
                        cmd = saveDate;
                        cmd = cmd.Replace("XYZZY1", date3);
                        cmd = cmd.Replace("XYZZY2", date4);
                        cmd += " AND ( `edited` = 'Manual' OR `edited` = 'TrustAdj' ) ";
                        DataTable ddt = G1.get_db_data(cmd);
                        Trust85.FindContract(ddt, "CT19006LI");
                        ProcessManualPayments(dt, ddt);
                        Trust85.FindContract(dt, "CT19006LI");
                    }
                }
            }

            //Trust85.FindContract(dt, "E22044LI");

            if (cmbDateType.Text.ToUpper() == "MONTHLY" || cmbDateType.Text.ToUpper() == "WEEKLY")
            {
                cmd = saveCmd;
                if (beginningDate >= oldImportDate)
                {
                    if (!chkACH.Checked || chkACH1.Checked)
                    {
                        if (chkACH1.Checked)
                        {
                            cmd = cmd.Replace("XYZZY1", date3);
                            cmd = cmd.Replace("XYZZY2", date4);
                        }
                        else
                        {
                            cmd = cmd.Replace("XYZZY1", date1);
                            cmd = cmd.Replace("XYZZY2", date2);
                        }
                        cmd = cmd.Replace("XYZZY1", date1);
                        cmd = cmd.Replace("XYZZY2", date2);
                        //cmd = cmd.Replace("(`depositNumber` LIKE 'T%' OR `depositNumber` LIKE 'A%')", "(`depositNumber` NOT LIKE 'T%' AND `depositNumber` NOT LIKE 'A%')");
                        cmd = cmd.TrimEnd(';');
                        if (chkACH1.Checked)
                        {
                            int idx = cmd.IndexOf("AND (`depositNumber` LIKE");
                            if (idx > 0)
                                cmd = cmd.Substring(0, idx);
                        }
                        cmd += " AND ( `edited` = 'Manual' OR `edited` = 'TrustAdj' OR `edited` = 'Cemetery' ) ;";
                        DataTable ddt = G1.get_db_data(cmd);
                        Trust85.FindContract(ddt, "HT17066UI");
                        ProcessManualPayments(dt, ddt);
                        //for (int i = 0; i < ddt.Rows.Count; i++)
                        //{
                        //    dt.ImportRow(ddt.Rows[i]);
                        //}
                    }
                    else
                    {
                        cmd = cmd.Replace("XYZZY1", date3);
                        cmd = cmd.Replace("XYZZY2", date4);
                        if (!chkACH.Checked)
                            cmd = cmd.Replace("(`depositNumber` LIKE 'T%' OR `depositNumber` LIKE 'CCTD' OR `depositNumber` LIKE 'A%')", "(`depositNumber` NOT LIKE 'T%' AND `depositNumber` NOT LIKE 'CCTD' AND `depositNumber` NOT LIKE 'A%')");
                        else
                        {
                            //cmd = cmd.Replace("(`depositNumber` LIKE 'T%' OR `depositNumber` LIKE 'CCTD')", "(`depositNumber` LIKE 'A%') ");
                            cmd = cmd.Replace("T%", "A%");
                            cmd = cmd.Replace("CCTD%", "ABCXYZ3");
                            cmd = cmd.Replace(" OR `depositNumber` = ''", "");
                        }

                        DataTable ddt = G1.get_db_data(cmd);
                        //rust85.FindContract(ddt, "B22021LI");
                        for (int i = 0; i < ddt.Rows.Count; i++)
                        {
                            dt.ImportRow(ddt.Rows[i]);
                        }
                        cmd = saveDate;
                        cmd = cmd.Replace("XYZZY1", date3);
                        cmd = cmd.Replace("XYZZY2", date4);
                        // cmd += " AND (`depositNumber` NOT LIKE 'T%' AND `depositNumber` NOT LIKE 'A%')";
                        cmd += " AND ( `edited` = 'Manual' OR `edited` = 'TrustAdj' OR `edited` = 'Cemetery' ) ";
                        ddt = G1.get_db_data(cmd);
                        //Trust85.FindContract(ddt, "B22021LI");
                        ProcessManualPayments(dt, ddt);
                        //for (int i = 0; i < ddt.Rows.Count; i++)
                        //{
                        //    dt.ImportRow(ddt.Rows[i]);
                        //}
                    }
                }
            }

            Trust85.LoadTrustAdjustments(dt, this.dateTimePicker1.Value, this.dateTimePicker2.Value);

            dt = SMFS.FilterForRiles(dt);

            //Trust85.FindContract(dt, "B22021LI");

            dt.Columns.Add("num");
            dt.Columns.Add("customer");
            if (G1.get_column_number(dt, "retained") < 0)
                dt.Columns.Add("retained", Type.GetType("System.Double"));
            dt.Columns.Add("monthsPaid", Type.GetType("System.Double"));
            dt.Columns.Add("loc");
            dt.Columns.Add("Location Name");

            //LoadOtherCombos(dt);

            DataTable funDt = G1.get_db_data("Select * from `funeralhomes`;");

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
            contractNumber = "";
            string oldContractNumber = "";
            string miniContract = "";
            string trust = "";
            string loc = "";
            string fill1 = "";
            string depositNumber = "";
            string depositLocation = "";
            bool manual = true;
            string edited = "";
            string finale = "";
            int finaleCount = 0;
            bool honorFinale = false;
            double saveRetained = 0D;
            double downpayment = 0D;
            bool calculateTrust100 = false;

            Trust85.FindContract(dt, "WC24009L");

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

            Trust85.FindContract(dt, "L15077UI");
            string creditReason = "";
            string nn = "";
            string findRecord = "";
            double newInterest = 0D;
            double newTrust85 = 0D;
            double newTrust100 = 0D;
            bool foundLI = false;
            string lockInterest = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    foundLI = false;
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if (contractNumber == "WC24009L")
                    {
                    }
                    if (contractNumber.ToUpper().EndsWith("LI"))
                    {
                        findRecord = dt.Rows[i]["record"].ObjToString();
                        DailyHistory.CalcPaymentData(contractNumber, findRecord, ref newInterest, ref newTrust85, ref newTrust100);
                        foundLI = true;
                        //Trust85.FindContract(dt, "B18035LI");
                    }
                    creditReason = dt.Rows[i]["creditReason"].ObjToString().ToUpper().Trim();

                    //downpayment = dt.Rows[i]["downPayment"].ObjToDouble();
                    downPayment = DailyHistory.getDownPayment(dt, i);
                    downpayment = G1.RoundValue(downpayment);

                    payDate8 = dt.Rows[i]["payDate8"].ObjToDateTime();
                    edited = dt.Rows[i]["edited"].ObjToString();
                    if (oldContractNumber != contractNumber)
                        finaleCount = 0;
                    oldContractNumber = contractNumber;
                    honorFinale = false;
                    finale = dt.Rows[i]["new"].ObjToString().ToUpper();
                    if (finale == "FINALE")
                    {
                        finaleCount++;
                        if (finaleCount == 1)
                            honorFinale = true;
                    }

                    fname = dt.Rows[i]["firstName"].ObjToString();
                    lname = dt.Rows[i]["lastName"].ObjToString();
                    name = fname + " " + lname;
                    dt.Rows[i]["customer"] = name;

                    //payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                    payment = DailyHistory.getPayment(dt, i);

                    interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                    debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                    credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                    saveRetained = dt.Rows[i]["retained"].ObjToDouble();
                    if (saveRetained < 0)
                    {
                        if (Math.Abs(saveRetained) == credit)
                            saveRetained = 0D;
                    }


                    trust85 = dt.Rows[i]["trust85P"].ObjToDouble();
                    trust100 = dt.Rows[i]["trust100P"].ObjToDouble();
                    lockTrust85 = dt.Rows[i]["lockTrust85"].ObjToString().ToUpper();

                    nn = dt.Rows[i]["new"].ObjToString().ToUpper();
                    if (contractNumber == "B18023LI" && nn == "FINALE")
                    {
                    }

                    payment = G1.RoundValue(payment);
                    debit = G1.RoundValue(debit);
                    credit = G1.RoundValue(credit);


                    amtOfMonthlyPayt = dt.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                    originalDownPayment = DailyHistory.GetDownPayment(contractNumber);

                    oldIssueDate = dt.Rows[i]["issueDate8"].ObjToDateTime();
                    oldIssueDate = DailyHistory.GetIssueDate(oldIssueDate, contractNumber, null);

                    financeMonths = dt.Rows[i]["numberOfPayments"].ObjToDouble();
                    rate = dt.Rows[i]["apr1"].ObjToDouble() / 100.0D;
                    lockInterest = dt.Rows[i]["lockInterest"].ObjToString().ToUpper();
                    if (lockInterest == "Y")
                        rate = dt.Rows[i]["apr"].ObjToDouble() / 100.0D;
                    amtPaid = payment;
                    principal = payment + credit - debit - interest + downpayment;
                    principal = G1.RoundDown(principal);
                    contractValue = DailyHistory.GetContractValuePlus(dt.Rows[i]);

                    calculateTrust100 = true;
                    if (payDate8 < DailyHistory.majorDate)
                        calculateTrust100 = false;
                    else if (creditReason.Trim() == "REVERSAL")
                        calculateTrust100 = false;
                    else if (lockTrust85 == "Y")
                        calculateTrust100 = false;
                    if (edited.ToUpper() == "TRUSTADJ" || edited.ToUpper() == "CEMETERY")
                        calculateTrust100 = false;
                    if ((edited.ToUpper() != "TRUSTADJ" && edited.ToUpper() != "CEMETERY") && debit == 0D && credit == 0D)
                    {
                        //if (calculateTrust100 && !honorFinale)
                        if (calculateTrust100 && finale.ToUpper() != "FINALE")
                        {
                            dt.Rows[i]["trust85P"] = 0D;
                            dt.Rows[i]["trust100P"] = 0D;
                            trust85 = 0D;
                            trust100 = 0D;
                        }
                    }
                    if (payment == 0D && downPayment == 0D && debit == 0D && credit == 0D)
                        calculateTrust100 = false;
                    //if (calculateTrust100 && debit == 0D && credit == 0D && !honorFinale)
                    if (calculateTrust100 && debit == 0D && credit == 0D && finale.ToUpper() != "FINALE")
                    {
                        retained = dt.Rows[i]["retained"].ObjToDouble();
                        method = ImportDailyDeposits.CalcTrust85P(payDate8, amtOfMonthlyPayt, oldIssueDate.ToString("MM/dd/yyyy"), contractValue, originalDownPayment, financeMonths, amtPaid, principal, debit, credit, rate, ref trust85, ref trust100, ref retained);
                        if (saveRetained != 0D)
                            retained = saveRetained;

                        dt.Rows[i]["trust85P"] = trust85;
                        dt.Rows[i]["trust100P"] = trust100;
                        dt.Rows[i]["retained"] = retained;
                    }
                    else
                    {
                        retained = dt.Rows[i]["retained"].ObjToDouble();
                        //if (edited.ToUpper() == "TRUSTADJ" && retained != 0D)
                        if (edited.ToUpper() == "TRUSTADJ" || creditReason.Trim() == "REVERSAL" || edited.ToUpper() == "CEMETERY")
                            dt.Rows[i]["retained"] = retained;
                        else if (saveRetained != 0D)
                            dt.Rows[i]["retained"] = saveRetained;
                        else
                        {
                            retained = ImportDailyDeposits.CalculateRetained(payment, credit, debit, interest, trust100);
                            if (retained == 0D && (edited.ToUpper() == "TRUSTADJ" || edited.ToUpper() == "CEMETERY"))
                            {
                                retained = trust100 * -1D;
                            }
                            dt.Rows[i]["retained"] = retained;
                        }
                    }


                    trust100P = dt.Rows[i]["trust100P"].ObjToDouble();
                    //trust100P = G1.RoundDown(trust100P);
                    dt.Rows[i]["trust100P"] = trust100P;
                    trust85P = dt.Rows[i]["trust85P"].ObjToDouble();
                    dt.Rows[i]["trust85P"] = trust85P;
                    //saveRetained = dt.Rows[i]["retained"].ObjToDouble();

                    //retained = interest + debit - credit;
                    //if (payment > 0D)
                    //    retained = payment - trust100P;

                    //retained = ImportDailyDeposits.CalculateRetained(payment, credit, debit, interest, trust100P);
                    //if (saveRetained != 0D)
                    //    retained = saveRetained;

                    //dt.Rows[i]["retained"] = retained;

                    miniContract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
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
                    if (edited.Trim().ToUpper() == "MANUAL" || edited.Trim().ToUpper() == "TRUSTADJ" || edited.Trim().ToUpper() == "CEMETERY")
                        manual = true;
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(depositNumber) && !manual)
                    {
                        depositLocation = depositNumber.Substring(0, 1).ToUpper();
                        //if (depositLocation == "T")
                        if (PaymentsReport.isLockBox(depositNumber))
                        {
                            fill1 = dt.Rows[i]["fill1"].ObjToString();
                            if (fill1.ToUpper() == "TFBX")
                                dt.Rows[i]["location"] = "TF";
                            else
                                dt.Rows[i]["location"] = "LK";
                        }
                        else if (depositLocation == "A")
                            dt.Rows[i]["location"] = "ACH";
                        else if (depositLocation == "C")
                            dt.Rows[i]["location"] = "CC";
                        if (chkCombineHO.Checked)
                        {
                            loc = dt.Rows[i]["location"].ObjToString();
                            if (loc.Trim().ToUpper() == "HOCC")
                                dt.Rows[i]["location"] = "HO";
                        }
                        if (manual)
                            dt.Rows[i]["depositNumber"] = dt.Rows[i]["userId"].ObjToString();
                    }
                    else if (manual)
                    {
                        if (!String.IsNullOrWhiteSpace(depositNumber)) // ramma zamma
                        {
                            if (depositNumber.Length >= 2)
                            {
                                depositLocation = depositNumber.Substring(0, 1).ToUpper();
                                //if (depositLocation == "T")
                                if (PaymentsReport.isLockBox(depositNumber))
                                {
                                    fill1 = dt.Rows[i]["fill1"].ObjToString();
                                    if (fill1.ToUpper() == "TFBX")
                                        dt.Rows[i]["location"] = "TF";
                                    else
                                        dt.Rows[i]["location"] = "LK";
                                }
                            }
                        }
                    }
                    if (chkCombineHO.Checked)
                    {
                        loc = dt.Rows[i]["location"].ObjToString();
                        if (loc.Trim().ToUpper().IndexOf("HO") >= 0)
                            dt.Rows[i]["location"] = "HO";
                    }

                    try
                    {
                        //payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                        payment = DailyHistory.getPayment(dt, i);

                        expected = dt.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                        months = 0D;
                        if (expected > 0D)
                            months = payment / expected;
                        dt.Rows[i]["monthsPaid"] = Math.Truncate(months);
                        if (foundLI)
                        {
                            dt.Rows[i]["retained"] = newInterest;
                            dt.Rows[i]["interestPaid"] = newInterest;
                            dt.Rows[i]["trust100P"] = newTrust100;
                            dt.Rows[i]["trust85P"] = newTrust85;
                        }
                    }
                    catch ( Exception ex)
                    {

                    }
                }
                catch (Exception ex)
                {
                }
            }

            //Trust85.FindContract(dt, "B22021LI");

            //            SplitLocation(dt);

            //DataRow [] dR = dt.Select ( "location='TFBX'");
            //if ( dR.Length > 0 )
            //{
            //    for (int i = 0; i < dR.Length; i++)
            //        dR[i]["location"] = "TF";
            //}

            DataView tempview = dt.DefaultView;
            tempview.Sort = "userId asc, payDate8 asc";
            dt = tempview.ToTable();

            LoadPaymentTypes(dt);

            DailyHistory.RecalcRetained(dt);

            dt = LoadLiInterest(dt);

            string runOn = cmbRunOn.Text.Trim().ToUpper();
            dt = Trust85.FilterForCemetery(dt, runOn);

            //Trust85.FindContract(dt, "B22021LI");
            DailyHistory.AddAP(dt);
            //Trust85.FindContract(dt, "B22021LI");
            DailyHistory.CleanupVisibility(gridMain);
            gridMain.Columns["dpp"].Visible = false;

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            dgv.RefreshDataSource();
            gridMain.RefreshData();
            dgv.Refresh();
            this.Refresh();

            btnSave.Hide();
            btnSave.Refresh();

            if (saveDate1.Day == 1)
            {
                int dim = DateTime.DaysInMonth(saveDate2.Year, saveDate2.Month);
                if (saveDate2.Day == dim)
                {
                    btnSave.SetBounds(btnRun.Left, btnRun.Bottom + 5, btnSave.Width, btnSave.Height);
                    btnSave.Show();
                    btnSave.Refresh();
                }
            }

            originalDt = dt;
            balanceDt = dt;
            this.Cursor = Cursors.Default;
        }
        /*******************************************************************************************/
        private DataTable LoadLiInterest ( DataTable dt)
        {
            if ( G1.get_column_number ( dt, "LiInterest") < 0 )
                dt.Columns.Add("LiInterest", Type.GetType("System.Double"));

            double interest = 0D;
            string contractNumber = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if ( contractNumber == "B21069LI")
                {
                }
                if (contractNumber.ToUpper().EndsWith("LI"))
                {
                    interest = dt.Rows[i]["interestPaid"].ObjToDouble();
                    if ( previousDateRead )
                        interest = dt.Rows[i]["interestPaid1"].ObjToDouble();
                    dt.Rows[i]["LiInterest"] = interest;
                }
                else
                    dt.Rows[i]["LiInterest"] = 0D;
            }
            return dt;
        }
        /*******************************************************************************************/
        private void LoadPaymentTypes ( DataTable dt)
        {
            string paymentType = "";
            double debit = 0D;
            string debitReason = "";
            double credit = 0D;
            string creditReason = "";
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
                    creditReason = dt.Rows[i]["creditReason"].ObjToString();
                    debitReason = dt.Rows[i]["debitReason"].ObjToString();
                    if (debit != 0D || credit != 0D)
                        dt.Rows[i]["paymentType"] = "Debit/Credit";
                    else if ( !String.IsNullOrWhiteSpace ( creditReason) || !String.IsNullOrWhiteSpace ( debitReason))
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
                if ( contractNumber == "L15077UI")
                {

                }
                payDate8 = dx.Rows[i]["payDate8"].ObjToDateTime();

                //payment = dx.Rows[i]["paymentAmount"].ObjToDouble();
                payment = DailyHistory.getPayment(dx, i);

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
                                //pay1 = dR[k]["paymentAmount"].ObjToDouble();
                                pay1 = DailyHistory.getPayment(dR, k);

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
            if (cmbDateType.Text.ToUpper() == "WEEKLY")
            {
                GetWeeklyDate(this.dateTimePicker2.Value, "FORWARD");
                return;
            }
            else if (cmbDateType.Text.ToUpper() == "DAILY")
            {
                now = now.AddDays(1);
                this.dateTimePicker1.Value = now;
                this.dateTimePicker2.Value = now;
                this.dateTimePicker3.Value = now;
                this.dateTimePicker4.Value = now;
                return;
            }
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker3.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker4.Value = new DateTime(now.Year, now.Month, days);
            DateTime start = now.AddDays(-1);
            DateTime stop = new DateTime(now.Year, now.Month, days - 1);
            this.dateTimePicker1.Value = this.dateTimePicker3.Value;
            this.dateTimePicker2.Value = this.dateTimePicker4.Value;

            //DateTime date = this.dateTimePicker2.Value;
            //date = date.AddDays(1);
            //GetWeeklyDate(date);
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            if (cmbDateType.Text.ToUpper() == "WEEKLY")
            {
                GetWeeklyDate(this.dateTimePicker2.Value, "BACK");
                return;
            }
            else if (cmbDateType.Text.ToUpper() == "DAILY")
            {
                now = now.AddDays(-1);
                this.dateTimePicker1.Value = now;
                this.dateTimePicker2.Value = now;
                this.dateTimePicker3.Value = now;
                this.dateTimePicker4.Value = now;
                return;
            }
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker3.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker4.Value = new DateTime(now.Year, now.Month, days);
            DateTime start = now.AddDays(-1);
            DateTime stop = new DateTime(now.Year, now.Month, days - 1);
            this.dateTimePicker1.Value = this.dateTimePicker3.Value;
            this.dateTimePicker2.Value = this.dateTimePicker4.Value;


            //DateTime date = this.dateTimePicker1.Value;
            //date = date.AddDays(-1);
            //GetWeeklyDate(date);
        }
        /***********************************************************************************************/
        private void GetWeeklyDate(DateTime date, string direction)
        {
            loading = true;
            DateTime idate = date;
            if (direction == "BACK")
            {
                date = date.AddDays(-7);
                this.dateTimePicker2.Value = date;
                date = date.AddDays(-4);
                this.dateTimePicker1.Value = date;
            }
            else
            {
                date = date.AddDays(7);
                this.dateTimePicker2.Value = date;
                date = date.AddDays(-4);
                this.dateTimePicker1.Value = date;

            }
            this.dateTimePicker3.Value = this.dateTimePicker1.Value;
            this.dateTimePicker4.Value = this.dateTimePicker2.Value;
            loading = false;
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
                gridMain.OptionsView.ShowFooter = false;
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
            string contractNumber = dt.Rows[row]["contractNumber"].ObjToString();
            if ( contractNumber == "B18007LI")
            {
            }
            if (!chkIncludeDeleted.Checked)
            {
                string deleted = dt.Rows[row]["fill"].ObjToString();
                if ( deleted.ToUpper() == "D")
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
            if ( workingByDate )
            {
                if ( !String.IsNullOrWhiteSpace ( workWhat ))
                {
                    string location = dt.Rows[row]["location"].ObjToString().ToUpper();
                    if ( workWhat == "LKBX" && location != "LK")
                    {
                        e.Visible = false;
                        e.Handled = true;
                        return;
                    }
                    else if (workWhat == "TFBX" && location != "TF")
                    {
                        e.Visible = false;
                        e.Handled = true;
                        return;
                    }
                }
            }
            if (!chkIncludeDownPayments.Checked)
            {
                //double downPayment = dt.Rows[row]["downPayment"].ObjToDouble();
                double downPayment = DailyHistory.getDownPayment(dt, row);
                //if (previousDateRead)
                //{
                //    downPayment = dt.Rows[row]["downPayment1"].ObjToDouble();
                //}
                if ( downPayment > 0D )
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
        }
        /***********************************************************************************************/
        private void chkIncludeDeleted_CheckedChanged(object sender, EventArgs e)
        {
            if (chkIncludeDeleted.Checked)
                gridMain.Columns["fill"].Visible = true;
            else
                gridMain.Columns["fill"].Visible = true;
        }
        /***********************************************************************************************/
        private void cmbDateType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string dateType = cmbDateType.Text;
            if ( dateType.ToUpper() == "MONTHLY")
            {
                DateTime now = this.dateTimePicker1.Value;
                //now = now.AddMonths(-1);
                now = new DateTime(now.Year, now.Month, 1);
                this.dateTimePicker3.Value = now;
                int days = DateTime.DaysInMonth(now.Year, now.Month);
                this.dateTimePicker4.Value = new DateTime(now.Year, now.Month, days);
                if (now < DailyHistory.majorDate)
                {
                    DateTime start = now.AddDays(-1);
                    DateTime stop = new DateTime(now.Year, now.Month, days - 1);
                    this.dateTimePicker1.Value = this.dateTimePicker3.Value;
                    this.dateTimePicker2.Value = this.dateTimePicker4.Value;
                }
                else
                {
                    this.dateTimePicker1.Value = this.dateTimePicker3.Value;
                    this.dateTimePicker2.Value = this.dateTimePicker2.Value;
                }
                //this.dateTimePicker3.Visible = true;
                //this.dateTimePicker4.Visible = true;
                //lblAllOther.Show();
                //lblAllOtherTo.Show();
                this.Text = this.Text.Replace("Weekly", "Monthly");
            }
            else
            {
                //this.dateTimePicker3.Visible = true;
                //this.dateTimePicker4.Visible = true;
                DateTime now = DateTime.Now;
                now = now.AddMonths(-1);
                now = new DateTime(now.Year, now.Month, 1);
                this.dateTimePicker3.Value = now;
                int days = DateTime.DaysInMonth(now.Year, now.Month);
                this.dateTimePicker4.Value = new DateTime(now.Year, now.Month, days);
                this.dateTimePicker2.Value = this.dateTimePicker4.Value;
                this.dateTimePicker1.Value = this.dateTimePicker2.Value.AddDays(-6);
                lblAllOther.Hide();
                lblAllOtherTo.Hide();
                this.Text = this.Text.Replace("Monthly", "Weekly");
            }
            this.Refresh();
        }
        /***********************************************************************************************/
        private void chkIncludeDownPayments_CheckedChanged(object sender, EventArgs e)
        {
            if (chkIncludeDownPayments.Checked)
            {
                //gridMain.Columns["downPayment"].Visible = true;
                gridMain.Columns["dpp"].Visible = true;
                gridMain.RefreshData();
                dgv.Refresh();
            }
            else
            {
                //gridMain.Columns["downPayment"].Visible = false;
                gridMain.Columns["dpp"].Visible = false;
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
                originalSize = gridMain.Columns["firstName"].AppearanceCell.Font.Size;
                mainFont = gridMain.Columns["firstName"].AppearanceCell.Font;
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
            gridMain.AppearancePrint.FooterPanel.Font = font;
            newFont = font;
            size = scale / 100D * originalHeaderSize;
            font = new Font(HeaderFont.Name, (float)size, FontStyle.Bold);
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                gridMain.Columns[i].AppearanceHeader.Font = font;
            }
            //gridMain.Appearance.HeaderPanel.Font = font;
            //gridMain.AppearancePrint.HeaderPanel.Font = font;

            gridMain.Appearance.HeaderPanel.Font = font;
            gridMain.AppearancePrint.HeaderPanel.Font = font;
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);

            dgv.Refresh();
            this.Refresh();
        }
        private void ScaleCells(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain, double factor )
        {
            if (originalSize == 0D)
            {
                //                originalSize = gridMain.Columns["address1"].AppearanceCell.FontSizeDelta.ObjToDouble();
                originalSize = gridMain.Columns["firstName"].AppearanceCell.Font.Size;
                mainFont = gridMain.Columns["firstName"].AppearanceCell.Font;
                HeaderFont = gridMain.Appearance.HeaderPanel.Font;
                originalHeaderSize = gridMain.Appearance.HeaderPanel.Font.Size;
            }
            //double scale = txtScale.Text.ObjToDouble();
            double scale = factor;
            double size = scale / 100D * originalSize;
            Font font = new Font(mainFont.Name, (float)size);

            //this.SuspendLayout();

            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                gridMain.Columns[i].AppearanceCell.Font = font;
            }
            gridMain.Appearance.GroupFooter.Font = font;
            gridMain.AppearancePrint.FooterPanel.Font = font;
            newFont = font;
            size = scale / 100D * originalHeaderSize;
            font = new Font(HeaderFont.Name, (float)size, FontStyle.Bold);
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                gridMain.Columns[i].AppearanceHeader.Font = font;
            }
            //gridMain.Appearance.HeaderPanel.Font = font;
            //gridMain.AppearancePrint.HeaderPanel.Font = font;

            gridMain.Appearance.HeaderPanel.Font = font;
            gridMain.AppearancePrint.HeaderPanel.Font = font;

            //this.ResumeLayout(false);

            //gridMain.RefreshData();
            //gridMain.RefreshEditor(true);

            //dgv.Refresh();
            //this.Refresh();
        }
        /***********************************************************************************************/
        private void AdjustColumnWidths(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain, double factor)
        {
            int width = 0;
            int newWidth = 0;
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                width = gridMain.Columns[i].Width;
                newWidth = (int)((double)width * factor);
                if ( newWidth > 0 )
                    gridMain.Columns[i].Width = newWidth;
            }
        }
        /***********************************************************************************************/
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
        /***********************************************************************************************/
        private void gridMain_CustomDrawFooterCell(object sender, DevExpress.XtraGrid.Views.Grid.FooterCellCustomDrawEventArgs e)
        {
            if (newFont != null)
                e.Appearance.Font = newFont;
        }
        /***********************************************************************************************/
        private void chkShowCredits_CheckedChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            DataTable dx = null;

            string cmd = "";
            string contractNumber = "";
            double balance = 0D;

            this.Cursor = Cursors.WaitCursor;

            gridMain.Columns["xxtrust"].Visible = true;

            if ( G1.get_column_number ( dt, "xxtrust") < 0 )
                dt.Columns.Add("xxtrust", Type.GetType("System.Double"));

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' and `creditAdjustment` > '0';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    balance = dx.Rows[0]["oldBalance"].ObjToDouble();
                    dt.Rows[i]["xxtrust"] = balance;
                }
            }
            this.Cursor = Cursors.Arrow;
            dgv.DataSource = dt;
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void menuReadData_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            DateTime begin = this.dateTimePicker1.Value;
            DateTime end = this.dateTimePicker2.Value;

            int days = DateTime.DaysInMonth(begin.Year, begin.Month);
            DateTime last = new DateTime(begin.Year, begin.Month, days);
            if (last > end)
            {
                this.Cursor = Cursors.Default;
                return;
            }

            string runDate1 = begin.ToString("yyyy-MM-dd");
            string runDate2 = end.ToString("yyyy-MM-dd");
            string runWhat = cmbRunOn.Text.Trim();

            string contractNumber = txtContract.Text.Trim();

            string cmd = "Select * from `weeklymonthly` where `runDate1` = '" + runDate1 + "' AND `runDate2` = '" + runDate2 + "' AND `runWhat` = '" + runWhat + "' ";
            if (!String.IsNullOrWhiteSpace(contractNumber))
                cmd += " AND `contractNumber` = '" + contractNumber + "' ";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);

            if (G1.get_column_number(dt, "paymentType") < 0)
                dt.Columns.Add("paymentType");
            if (G1.get_column_number(dt, "retained") < 0)
                dt.Columns.Add("retained", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "monthsPaid") < 0)
                dt.Columns.Add("monthsPaid", Type.GetType("System.Double"));


            previousDateRead = true;

            gridMain.Columns["dpp"].Visible = false;
            gridMain.Columns["paymentAmount"].Visible = false;
            gridMain.Columns["ap"].Visible = true;
            gridMain.Columns["ap"].Caption = "Payment";

            //dt = CleanupSavedPayments(dt, runDate1, runDate2 );

            //Trust85.FindContract(dt, "E21091LI");

            G1.NumberDataTable(dt);

            dgv.DataSource = dt;
            //dgv.RefreshDataSource();
            //gridMain.RefreshData();
            //dgv.Refresh();
            this.Refresh();

            originalDt = dt;
            balanceDt = dt;

            menuStrip1.BackColor = Color.LightGreen;

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable CleanupSavedPayments ( DataTable dt, string runDate1, string runDate2 )
        {
            DataView tempview = dt.DefaultView;
            tempview.Sort = "userId asc, payDate8 asc";
            dt = tempview.ToTable();

            Trust85.FindContract(dt, "P21091LI");
            LoadPaymentTypes(dt);

            DailyHistory.RecalcRetained(dt);

            Trust85.FindContract(dt, "E21091LI");
            dt = LoadLiInterest(dt);

            Trust85.FindContract(dt, "E21091LI");
            DailyHistory.AddAP(dt, "downPayment1");

            dt = CleanupFutureReporting(dt, runDate1, runDate2);

            DailyHistory.CleanupVisibility(gridMain);
            gridMain.Columns["dpp"].Visible = false;

            Trust85.FindContract(dt, "E21091LI");

            double downPayment = 0D;
            double payment = 0D;
            double ccFee = 0D;
            string contractNumber = "";
            double interest = 0D;
            double debit = 0D;
            double credit = 0D;
            double expected = 0D;
            double months = 0D;
            bool manual = false;
            string edited = "";
            string depositNumber = "";
            string depositLocation = "";
            string fill1 = "";
            string loc = "";

            for ( int i=(dt.Rows.Count - 1); i>=0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                downPayment = dt.Rows[i]["downPayment1"].ObjToDouble();
                payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                ccFee = dt.Rows[i]["ccFee"].ObjToDouble();
                interest = dt.Rows[i]["interestPaid1"].ObjToDouble();
                dt.Rows[i]["interestPaid"] = interest;
                payment = DailyHistory.getPayment(dt, i);
                expected = dt.Rows[i]["amtOfMonthlyPayt"].ObjToDouble();
                months = 0D;
                if (expected > 0D)
                    months = payment / expected;
                dt.Rows[i]["monthsPaid"] = Math.Truncate(months);

                manual = false;
                edited = dt.Rows[i]["edited"].ObjToString();
                if (edited.Trim().ToUpper() == "MANUAL" || edited.Trim().ToUpper() == "TRUSTADJ" || edited.Trim().ToUpper() == "CEMETERY")
                    manual = true;
                depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                if (!String.IsNullOrWhiteSpace(depositNumber) && !manual)
                {
                    depositLocation = depositNumber.Substring(0, 1).ToUpper();
                    //if (depositLocation == "T")
                    if (PaymentsReport.isLockBox(depositNumber))
                    {
                        fill1 = dt.Rows[i]["fill1"].ObjToString();
                        if (fill1.ToUpper() == "TFBX")
                            dt.Rows[i]["location"] = "TF";
                        else
                            dt.Rows[i]["location"] = "LK";
                    }
                    else if (depositLocation == "A")
                        dt.Rows[i]["location"] = "ACH";
                    else if (depositLocation == "C")
                        dt.Rows[i]["location"] = "CC";
                    if (chkCombineHO.Checked)
                    {
                        loc = dt.Rows[i]["location"].ObjToString();
                        if (loc.Trim().ToUpper() == "HOCC")
                            dt.Rows[i]["location"] = "HO";
                    }
                    if (manual)
                        dt.Rows[i]["depositNumber"] = dt.Rows[i]["userId"].ObjToString();
                }
                else if (manual)
                {
                    if (!String.IsNullOrWhiteSpace(depositNumber)) // ramma zamma
                    {
                        if (depositNumber.Length >= 2)
                        {
                            depositLocation = depositNumber.Substring(0, 1).ToUpper();
                            //if (depositLocation == "T")
                            if (PaymentsReport.isLockBox(depositNumber))
                            {
                                fill1 = dt.Rows[i]["fill1"].ObjToString();
                                if (fill1.ToUpper() == "TFBX")
                                    dt.Rows[i]["location"] = "TF";
                                else
                                    dt.Rows[i]["location"] = "LK";
                            }
                        }
                    }
                }
                if (chkCombineHO.Checked)
                {
                    loc = dt.Rows[i]["location"].ObjToString();
                    if (loc.Trim().ToUpper().IndexOf("HO") >= 0)
                        dt.Rows[i]["location"] = "HO";
                }


                downPayment = dt.Rows[i]["downPayment1"].ObjToDouble();
                ccFee = dt.Rows[i]["ccFee"].ObjToDouble();
                payment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                debit = dt.Rows[i]["debitAdjustment"].ObjToDouble();
                credit = dt.Rows[i]["creditAdjustment"].ObjToDouble();
                if (debit != 0D || credit != 0D)
                    continue;
                if (contractNumber == "B18037LI")
                {
                }
                if (interest != 0D)
                    continue;
                if (downPayment != 0D || payment <= 0D)
                    dt.Rows.RemoveAt(i);
            }

            return dt;
        }
        /****************************************************************************************/
        public static DataTable CleanupFutureReporting(DataTable dt, string date1, string date2)
        {
            DateTime lDate1 = date1.ObjToDateTime();
            DateTime lDate2 = date2.ObjToDateTime();

            int nextMonth = lDate2.Month;
            int issueMonth = 0;

            DateTime payDate = DateTime.Now;
            DateTime issueDate = DateTime.Now;
            double downPayment = 0D;

            string contractNumber = "";

            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                downPayment = dt.Rows[i]["downPayment1"].ObjToDouble();
                if (downPayment <= 0D)
                    continue;
                payDate = dt.Rows[i]["payDate8"].ObjToDateTime();
                issueDate = dt.Rows[i]["issueDate8"].ObjToDateTime();
                issueMonth = issueDate.Month;
                if (issueDate.Year > lDate2.Year)
                    issueMonth += 12;
                if (issueMonth > nextMonth)
                {
                    dt.Rows.RemoveAt(i);
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
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
        /****************************************************************************************/
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
                    if (date.Year < 10)
                        e.DisplayText = "";
                    else
                        e.DisplayText = date.ToString("yyyy-MM-dd");
                    //e.DisplayText = date.ToString("MM/dd/yyyy");
                }
            }
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            DialogResult result = new DialogResult();
            result = MessageBox.Show("Are you sure you want to save this data to the database?", "Save Weekly/Monthly Data Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            try
            {
                    SaveDataMonthlyWeekly(dt);
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private bool SaveDataMonthlyWeekly(DataTable dt)
        {
            this.Cursor = Cursors.WaitCursor;

            DateTime date = this.dateTimePicker2.Value;
            int days = DateTime.DaysInMonth(date.Year, date.Month);

            string runDate1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-01";
            string runDate2 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + days.ToString("D2");

            PleaseWait pleaseForm = null;
            pleaseForm = new PleaseWait("Please Wait!\nSaving Weekly/Monthly Data to Database!");
            pleaseForm.Show();
            pleaseForm.Refresh();

            if (!DeletePreviousData("weeklyMonthly"))
            {
                this.Cursor = Cursors.Default;
                return false;
            }

            DataTable saveDt = dt.Copy();

            if (G1.get_column_number(saveDt, "tmstamp") >= 0)
                saveDt.Columns.Remove("tmstamp");
            if (G1.get_column_number(saveDt, "record") >= 0)
                saveDt.Columns.Remove("record");
            if (G1.get_column_number(saveDt, "runwhat") >= 0)
                saveDt.Columns.Remove("runWhat");
            if (G1.get_column_number(saveDt, "runDate2") >= 0)
                saveDt.Columns.Remove("runDate2");
            if (G1.get_column_number(saveDt, "runDate1") >= 0)
                saveDt.Columns.Remove("runDate1");

            DataColumn ColR3 = saveDt.Columns.Add("runWhat", System.Type.GetType("System.String"));
            ColR3.SetOrdinal(0);// to put the column in position 0;
            DataColumn ColR2 = saveDt.Columns.Add("runDate2", System.Type.GetType("System.String"));
            ColR2.SetOrdinal(0);// to put the column in position 0;
            DataColumn ColR1 = saveDt.Columns.Add("runDate1", System.Type.GetType("System.String"));
            ColR1.SetOrdinal(0);// to put the column in position 0;
            DataColumn Col = saveDt.Columns.Add("record", System.Type.GetType("System.String"));
            Col.SetOrdinal(0);// to put the column in position 0;
            DataColumn Col1 = saveDt.Columns.Add("tmstamp", System.Type.GetType("System.String"));
            Col1.SetOrdinal(0);// to put the column in position 0;

            double dValue = 0D;
            string str = "";
            string runWhat = cmbRunOn.Text.Trim();

            for (int i = 0; i < saveDt.Rows.Count; i++)
            {
                //saveDt.Rows[i]["tmstamp"] = "0000-00-00";
                saveDt.Rows[i]["tmstamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                saveDt.Rows[i]["record"] = "0";
                saveDt.Rows[i]["runDate1"] = runDate1;
                saveDt.Rows[i]["runDate2"] = runDate2;
                saveDt.Rows[i]["runWhat"] = runWhat;
                CleanupData(saveDt, i, "firstName", 80);
                CleanupData(saveDt, i, "lastName", 80);
                CleanupData(saveDt, i, "address1", 80);
                CleanupData(saveDt, i, "address2", 80);
                CleanupData(saveDt, i, "city", 80);
                CleanupData(saveDt, i, "state", 80);
                CleanupData(saveDt, i, "zip1", 80);
                CleanupData(saveDt, i, "notes", 90);
            }

            MySQL.CleanupTable(saveDt);

            string strFile = "/CashRemitted/WeeklyMonthly_P_" + date.ToString("yyyyMMdd") + ".csv";
            string Server = "C:/rag";
            //Create directory if not exist... Make sure directory has required rights..
            if (!Directory.Exists(Server + "/CashRemitted/"))
                Directory.CreateDirectory(Server + "/CashRemitted/");

            G1.GrantDirectoryAccess(Server + "/CashRemitted/");
            if (File.Exists(Server + strFile))
                File.Delete(Server + strFile);

            //If file does not exist then create it and right data into it..
            if (!File.Exists(Server + strFile))
            {
                FileStream fs = new FileStream(Server + strFile, FileMode.Create, FileAccess.Write);
                fs.Close();
                fs.Dispose();
            }

            //Generate csv file from where data read

            try
            {
                DateTime saveDate = this.dateTimePicker2.Value;
                //int days = DateTime.DaysInMonth(saveDate.Year, saveDate.Month);
                //                string mySaveDate = saveDate.Year.ToString("D4") + "-" + saveDate.Month.ToString("D2") + "-" + days.ToString("D2") + " 00:00:00";

                var mySaveDate = G1.DTtoMySQLDT(saveDate);

                //for ( int i=0; i<saveDt.Rows.Count; i++)
                //    saveDt.Rows[i]["payDate8"] = mySaveDate;

                MySQL.CreateCSVfile(saveDt, Server + strFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Creating CSV File to load into Database " + ex.Message.ToString(), "Save Data Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                this.Cursor = Cursors.Default;
                return false;
            }
            try
            {
                Structures.TieDbTable("weeklyMonthly", saveDt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Tieing WeeklyMonthly to DataTable " + ex.Message.ToString(), "Save Data Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                this.Cursor = Cursors.Default;
                return false;
            }
            try
            {
                G1.conn1.Open();
                MySqlBulkLoader bcp1 = new MySqlBulkLoader(G1.conn1);
                bcp1.TableName = "weeklyMonthly"; //Create table into MYSQL database...
                bcp1.FieldTerminator = ",";

                bcp1.LineTerminator = "\r\n";
                bcp1.FileName = Server + strFile;
                bcp1.NumberOfLinesToSkip = 0;
                //bcp1.FieldTerminator = "~";
                bcp1.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Bulk Loading weeklyMonthly to DataTable " + ex.Message.ToString(), "Save Data Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }

            saveDt.Dispose();
            saveDt = null;

            File.Delete(Server + strFile);

            btnSave.Hide();
            btnSave.Refresh();

            pleaseForm.FireEvent1();
            pleaseForm.Dispose();
            pleaseForm = null;

            this.Cursor = Cursors.Default;
            return true;
        }
        /***********************************************************************************************/
        private void CleanupData(DataTable dt, int i, string what, int length)
        {
            try
            {
                dt.Rows[i][what] = G1.try_protect_data(dt.Rows[i][what].ObjToString());
                dt.Rows[i][what] = G1.Truncate(dt.Rows[i][what].ObjToString(), length);
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private bool DeletePreviousData(string table = "")
        {
            bool success = false;

            //if (!CheckForFutureData(allData))
            //    return false;

            if (String.IsNullOrWhiteSpace(table))
                table = "cashRemitted";
            DateTime date = this.dateTimePicker2.Value;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            string date1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-01";
            string date2 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + days.ToString("D2");

            string runWhat = cmbRunOn.Text.Trim();

            string cmd = "Select * from `" + table + "` WHERE `runDate1` >= '" + date1 + "' AND `runDate2` <= '" + date2 + "' AND `runWhat` = '" + runWhat + "';";
            DataTable ddd = G1.get_db_data(cmd);
            if (ddd.Rows.Count <= 0)
                return true;

            cmd = "DELETE from `" + table + "` where `runDate1` >= '" + date1 + "' AND `runDate2` <= '" + date2 + "' AND `runWhat` = '" + runWhat + "' ";

            cmd += ";";
            try
            {
                G1.get_db_data(cmd);
                success = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Delete Previous Data " + ex.Message.ToString());
            }
            return success;
        }
        /***********************************************************************************************/
        private string fullPath = "";
        private string format = "";
        private bool continuousPrint = false;
        /***********************************************************************************************/
        private void generateMassReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string mainReport = "";
            string report = "";
            string locations = "";
            format = "";
            string outputFilname = "";
            string outputDirectory = "";

            fullPath = "";

            string location = "";
            string loc = "";
            bool foundLocations = false;

            string lastReadOldData = "";
            string newReadOldData = "";

            DataTable dt = null;

            string[] Lines = null;

            DateTime date = dateTimePicker2.Value;

            string yyyy = date.Year.ToString("D4");
            string month = G1.ToMonthName(date);

            DialogResult result = MessageBox.Show("Are you sure you want to RUN the Mass Reports for Weekly/Monthly Balance Sheet for " + date.ToString("MM/dd/yyyy") + "?", "Mass Report Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            string cmd = "Select * from `mass_reports` where `mainReport` = 'Weekly Monthly Balance Sheet';";
            DataTable dx = G1.get_db_data(cmd);

            int lastRow = dx.Rows.Count;
            //lastRow = 6;

            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            barImport.Show();
            barImport.Refresh();

            bool gotRiles = false;
            bool lastRiles = false;
            DataTable ddt = (DataTable)dgv.DataSource;
            DataTable tempDt = ddt.Copy();
            ddt = null;
            DataView tempview = tempDt.DefaultView;

            for (int i = 0; i < lastRow; i++)
            {
                try
                {
                    Application.DoEvents();

                    barImport.Value = i + 1;
                    barImport.Refresh();

                    mainReport = dx.Rows[i]["mainReport"].ObjToString();
                    report = dx.Rows[i]["report"].ObjToString();
                    locations = dx.Rows[i]["locations"].ObjToString();
                    format = dx.Rows[i]["format"].ObjToString();
                    outputFilname = dx.Rows[i]["outputFilename"].ObjToString();
                    outputDirectory = dx.Rows[i]["outputDirectory"].ObjToString();

                    outputDirectory = outputDirectory.Replace("2021", yyyy);
                    outputFilname = outputFilname.Replace("yyyy", yyyy);
                    outputFilname = outputFilname.Replace("month", month);

                    fullPath = outputDirectory + "/" + outputFilname;

                    G1.verify_path(outputDirectory);

                    this.Text = mainReport + " " + report + " / " + locations;

                    SMFS.activeSystem = "";
                    gotRiles = false;

                    lastRiles = gotRiles;

                    lastReadOldData = newReadOldData;

                    this.Cursor = Cursors.WaitCursor;

                    dt = tempDt.Copy();
                    if ( report.ToUpper().IndexOf ( "ALPHA") >= 0 )
                    {
                        tempview = dt.DefaultView;
                        tempview.Sort = "lastName asc, firstName asc";
                        dt = tempview.ToTable();
                    }
                    dgv.DataSource = dt;
                    dgv.Refresh();

                    //dt = (DataTable)dgv.DataSource;
                    if (dt == null)
                        break;
                    if (dt.Rows.Count <= 0)
                        continue;

                    continuousPrint = true;
                    printPreviewToolStripMenuItem_Click(null, null);
                    continuousPrint = false;

                    this.Cursor = Cursors.Default;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** " + ex.Message.ToString(), "Mass Report Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }

            dgv.DataSource = tempDt;
            dgv.RefreshDataSource();
            dgv.Refresh();

            barImport.Value = lastRow;
            barImport.Refresh();

            try
            {
                SaveDataMonthlyWeekly(tempDt);
            }
            catch ( Exception ex)
            {
                MessageBox.Show("***ERROR*** Saving Data to Database " + ex.Message.ToString(), "Save Data Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }

            MessageBox.Show("Mass Reports Finished for Weekly/Monthly Balance Sheet!", "Mass Report Finished Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

            AutoRunPaidOutReport();

            barImport.Hide();
            barImport.Refresh();
        }
        /***********************************************************************************************/
    }
}